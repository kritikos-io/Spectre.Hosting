namespace Kritikos.SpectreCli.OpenTelemetry.Tests;

using System.Diagnostics;

using Kritikos.SpectreCli.Hosting;
using Kritikos.SpectreCli.OpenTelemetry.Tests.Doubles;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console;

[NotInParallel]
public class CommandActivityInterceptorTests
{
  [Test]
  public async Task Run_SucceedingCommand_StopsTheSpanWithOkStatus()
  {
    using var capture = new TelemetryCapture();

    await RunAsync<SucceedingCommand>();

    var activity = capture.Activities.Single();
    await Assert.That(activity.DisplayName).IsEqualTo(nameof(SucceedingCommand));
    await Assert.That(activity.Status).IsEqualTo(ActivityStatusCode.Ok);
    await Assert.That(activity.GetTagItem("spectre.command.exit_code")).IsEqualTo(0);
    await Assert.That(activity.GetTagItem("error.type")).IsNull();
  }

  [Test]
  public async Task Run_SucceedingCommand_RecordsDurationAndExecutionCount()
  {
    using var capture = new TelemetryCapture();

    await RunAsync<SucceedingCommand>();

    await Assert.That(capture.Measurements.Select(m => m.Instrument))
      .IsEquivalentTo(["spectre.command.duration", "spectre.command.executions"]);
    await Assert.That(capture.Measurements.Single(m => m.Instrument == "spectre.command.executions").Value)
      .IsEqualTo(1);
  }

  [Test]
  public async Task Run_NonZeroExitCode_StopsTheSpanWithErrorStatusButNoErrorType()
  {
    using var capture = new TelemetryCapture();

    var exitCode = await RunAsync<NonZeroExitCommand>();

    var activity = capture.Activities.Single();
    await Assert.That(exitCode).IsEqualTo(3);
    await Assert.That(activity.Status).IsEqualTo(ActivityStatusCode.Error);
    await Assert.That(activity.GetTagItem("spectre.command.exit_code")).IsEqualTo(3);
    await Assert.That(activity.GetTagItem("error.type")).IsNull();
  }

  [Test]
  public async Task Run_ThrowingCommand_StopsTheSpanWithErrorTypeSet()
  {
    using var capture = new TelemetryCapture();

    await RunAsync<ThrowingCommand>();

    var activity = capture.Activities.Single();
    await Assert.That(activity.Status).IsEqualTo(ActivityStatusCode.Error);
    await Assert.That(activity.GetTagItem("error.type"))
      .IsEqualTo(typeof(InvalidOperationException).FullName);
    await Assert.That(activity.GetTagItem("spectre.command.exit_code")).IsEqualTo(-1);
  }

  [Test]
  public async Task Run_ThrowingCommand_RecordsAnExceptionEvent()
  {
    using var capture = new TelemetryCapture();

    await RunAsync<ThrowingCommand>();

    var exceptionEvent = capture.Activities.Single().Events.Single(e => e.Name == "exception");
    await Assert.That(exceptionEvent.Tags.First(t => t.Key == "exception.message").Value)
      .IsEqualTo("boom");
  }

  [Test]
  public async Task Run_ThrowingCommand_StillRecordsMetricsWithErrorType()
  {
    using var capture = new TelemetryCapture();

    await RunAsync<ThrowingCommand>();

    var executions = capture.Measurements.Single(m => m.Instrument == "spectre.command.executions");
    await Assert.That(executions.Value).IsEqualTo(1);
    await Assert.That(executions.Tag("error.type")).IsEqualTo(typeof(InvalidOperationException).FullName);
    await Assert.That(capture.Measurements.Any(m => m.Instrument == "spectre.command.duration")).IsTrue();
  }

  // Pins roadmap 2.3: settings declared outside a command make the span report the settings type.
  [Test]
  public async Task Run_SettingsNotNestedInTheCommand_MisreportsTheCommandType()
  {
    using var capture = new TelemetryCapture();

    await RunAsync<StandaloneSettingsCommand>();

    var activity = capture.Activities.Single();
    await Assert.That(activity.DisplayName).IsEqualTo(nameof(StandaloneSettings));
    await Assert.That(activity.GetTagItem("spectre.command.type"))
      .IsEqualTo(typeof(StandaloneSettings).FullName);
  }

  private static Task<int> RunAsync<TCommand>()
    where TCommand : class, Spectre.Console.Cli.ICommand
  {
    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
      DisableDefaults = true,
    });

    builder.Services.AddSingleton<IAnsiConsole>(AnsiConsole.Create(new AnsiConsoleSettings
    {
      Out = new AnsiConsoleOutput(TextWriter.Null),
    }));
    builder.Services.AddSpectreCliInstrumentation();
    builder.Services.AddSpectreConsole<TCommand>(args: []);

    var host = builder.Build();
    return host.RunSpectreConsoleAsync();
  }
}
