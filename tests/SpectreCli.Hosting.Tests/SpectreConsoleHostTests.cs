namespace Kritikos.SpectreCli.Hosting.Tests;

using Kritikos.SpectreCli.Hosting.Tests.Doubles;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console;

[NotInParallel]
public class SpectreConsoleHostTests
{
  [Test]
  public async Task RunSpectreConsoleAsync_SucceedingCommand_ReturnsTheCommandExitCode()
  {
    using var host = BuildHost<SucceedingCommand>();
    var probe = host.Services.GetRequiredService<ExecutionProbe>();

    var exitCode = await host.RunSpectreConsoleAsync();

    await Assert.That(probe.Executed).IsTrue();
    await Assert.That(exitCode).IsEqualTo(42);
  }

  [Test]
  public async Task RunSpectreConsoleAsync_CommandWithScopedDependency_ResolvesALiveInstance()
  {
    using var host = BuildHost<SucceedingCommand>();
    var probe = host.Services.GetRequiredService<ExecutionProbe>();

    await host.RunSpectreConsoleAsync();

    await Assert.That(probe.ScopedInstance).IsNotNull();

    // 42 rather than -99 proves the scope was still alive during execution.
    await Assert.That(probe.ExitCode).IsEqualTo(42);
  }

  [Test]
  public async Task RunSpectreConsoleAsync_AfterTheRun_DisposesTheCommandScope()
  {
    using var host = BuildHost<SucceedingCommand>();
    var probe = host.Services.GetRequiredService<ExecutionProbe>();

    await host.RunSpectreConsoleAsync();

    await Assert.That(probe.ScopedInstance!.Disposed).IsTrue();
  }

  [Test]
  public async Task RunSpectreConsoleAsync_ThrowingCommand_ReturnsUnhandledExceptionExitCode()
  {
    using var host = BuildHost<ThrowingCommand>();

    var exitCode = await host.RunSpectreConsoleAsync();

    await Assert.That(exitCode).IsEqualTo(-1);
  }

  [Test]
  public async Task RunSpectreConsoleAsync_ThrowingCommand_NotifiesObservers()
  {
    using var host = BuildHost<ThrowingCommand>();
    var observer = host.Services.GetServices<ICommandExecutionObserver>()
      .OfType<RecordingObserver>()
      .Single();

    await host.RunSpectreConsoleAsync();

    await Assert.That(observer.Exception).IsTypeOf<InvalidOperationException>();
    await Assert.That(observer.ExitCode).IsEqualTo(-1);
  }

  [Test]
  public async Task RunSpectreConsoleAsync_ThrowingCommand_RendersTheError()
  {
    var output = new StringWriter();
    using var host = BuildHost<ThrowingCommand>(output);

    await host.RunSpectreConsoleAsync();

    await Assert.That(output.ToString()).Contains("command failed");
  }

  [Test]
  public async Task RunSpectreConsoleAsync_RenderingDisabled_WritesNothing()
  {
    var output = new StringWriter();
    using var host = BuildHost<ThrowingCommand>(
      output,
      options => options.RenderUnhandledExceptions = false);

    await host.RunSpectreConsoleAsync();

    await Assert.That(output.ToString()).IsEmpty();
  }

  [Test]
  public async Task RunSpectreConsoleAsync_CustomExceptionExitCode_IsHonoured()
  {
    using var host = BuildHost<ThrowingCommand>(
      configureOptions: options => options.UnhandledExceptionExitCode = 7);

    var exitCode = await host.RunSpectreConsoleAsync();

    await Assert.That(exitCode).IsEqualTo(7);
  }

  [Test]
  public async Task RunSpectreConsoleAsync_CancelledRun_ReturnsCancellationExitCode()
  {
    using var host = BuildHost<BlockingCommand>();
    var probe = host.Services.GetRequiredService<CancellationProbe>();
    using var cts = new CancellationTokenSource();

    var run = host.RunSpectreConsoleAsync(cts.Token);
    await probe.Started.Task;
    await cts.CancelAsync();

    await Assert.That(await run).IsEqualTo(130);
  }

  [Test]
  public async Task RunSpectreConsoleAsync_CancelledRun_DoesNotRenderAnError()
  {
    var output = new StringWriter();
    using var host = BuildHost<BlockingCommand>(output);
    var probe = host.Services.GetRequiredService<CancellationProbe>();
    using var cts = new CancellationTokenSource();

    var run = host.RunSpectreConsoleAsync(cts.Token);
    await probe.Started.Task;
    await cts.CancelAsync();
    await run;

    await Assert.That(output.ToString()).IsEmpty();
  }

  private static IHost BuildHost<TCommand>(
    TextWriter? output = null,
    Action<SpectreConsoleOptions>? configureOptions = null)
    where TCommand : class, Spectre.Console.Cli.ICommand
  {
    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
      DisableDefaults = true,
    });

    builder.Services.AddSingleton<ExecutionProbe>();
    builder.Services.AddSingleton<CancellationProbe>();
    builder.Services.AddScoped<ScopedProbe>();
    builder.Services.AddSingleton<ICommandExecutionObserver, RecordingObserver>();
    builder.Services.AddSingleton<IAnsiConsole>(AnsiConsole.Create(new AnsiConsoleSettings
    {
      Out = new AnsiConsoleOutput(output ?? TextWriter.Null),
    }));
    builder.Services.AddSpectreConsole<TCommand>(args: [], configureOptions: configureOptions);

    return builder.Build();
  }
}
