namespace Kritikos.SpectreCli.Hosting.Tests;

using Kritikos.SpectreCli.Hosting.Tests.Doubles;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console;
using Spectre.Console.Cli;

[NotInParallel]
public class CommandInterceptorTests
{
  [Test]
  public async Task Run_InterceptorsRegisteredInTheContainer_AreInvoked()
  {
    var log = new List<string>();

    await RunAsync(log);

    await Assert.That(log).Contains("intercept:first");
  }

  // Spectre iterates its interceptor list forward for both passes; it does not nest them.
  [Test]
  public async Task Run_MultipleInterceptors_RunInRegistrationOrderForBothPasses()
  {
    var log = new List<string>();

    await RunAsync(log);

    await Assert.That(log).IsEquivalentTo([
      "intercept:first",
      "intercept:second",
      "result:first",
      "result:second",
    ]);
  }

  private static async Task RunAsync(List<string> log)
  {
    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
    {
      DisableDefaults = true,
    });

    builder.Services.AddSingleton<ExecutionProbe>();
    builder.Services.AddScoped<ScopedProbe>();
    builder.Services.AddSingleton<IAnsiConsole>(AnsiConsole.Create(new AnsiConsoleSettings
    {
      Out = new AnsiConsoleOutput(TextWriter.Null),
    }));
    builder.Services.AddSingleton<ICommandInterceptor>(new RecordingInterceptor("first", log));
    builder.Services.AddSingleton<ICommandInterceptor>(new RecordingInterceptor("second", log));
    builder.Services.AddSpectreConsole<SucceedingCommand>(args: []);

    using var host = builder.Build();
    await host.RunSpectreConsoleAsync();
  }
}
