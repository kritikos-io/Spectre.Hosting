namespace Kritikos.CommandLine.Hosting;

using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

public class ConsoleHostedAppService : BackgroundService
{
  private readonly ICommandApp app;

  public ConsoleHostedAppService(ICommandApp app) => this.app = app;

  /// <inheritdoc />
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var result = await app.RunAsync(Environment.GetCommandLineArgs().Skip(1));
    Environment.Exit(result);
  }
}
