namespace Kritikos.SpectreCli.Hosting;

using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

/// <summary>
/// A <see cref="BackgroundService"/> that executes the configured
/// <see cref="ICommandApp"/> and triggers host shutdown upon completion.
/// </summary>
/// <param name="lifetime">The host application lifetime used to signal shutdown.</param>
/// <param name="app">The Spectre command application to execute.</param>
/// <param name="commandLineArgs">The command-line arguments to forward.</param>
internal sealed class SpectreConsoleWorker(
  IHostApplicationLifetime lifetime,
  ICommandApp app,
  SpectreConsoleArgs commandLineArgs) : BackgroundService
{
  /// <inheritdoc/>
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    Environment.ExitCode = await app.RunAsync(commandLineArgs.Args, stoppingToken).ConfigureAwait(false);
    lifetime.StopApplication();
  }
}
