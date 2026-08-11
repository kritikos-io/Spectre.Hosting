namespace Kritikos.SpectreCli.Hosting;

using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Spectre.Console;
using Spectre.Console.Cli;

/// <summary>
/// A <see cref="BackgroundService"/> that executes the configured
/// <see cref="ICommandApp"/> and triggers host shutdown upon completion.
/// </summary>
/// <param name="lifetime">The host application lifetime used to signal shutdown.</param>
/// <param name="app">The Spectre command application to execute.</param>
/// <param name="commandLineArgs">The command-line arguments to forward.</param>
/// <param name="result">The holder receiving the exit code of the run.</param>
/// <param name="options">The error and cancellation policy.</param>
/// <param name="observers">Observers notified when the run terminates with an exception.</param>
/// <param name="console">The console used to render unhandled exceptions.</param>
internal sealed class SpectreConsoleWorker(
  IHostApplicationLifetime lifetime,
  ICommandApp app,
  SpectreConsoleArgs commandLineArgs,
  SpectreConsoleResult result,
  IOptions<SpectreConsoleOptions> options,
  IEnumerable<ICommandExecutionObserver> observers,
  IAnsiConsole? console = null) : BackgroundService
{
  /// <inheritdoc/>
  [SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification = "Terminal handler for the command run: every failure must yield an exit code and reach the observers.")]
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      result.ExitCode = await app.RunAsync(commandLineArgs.Args, stoppingToken).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
      var policy = options.Value;
      var cancelled = ex is OperationCanceledException;
      var exitCode = cancelled ? policy.CancellationExitCode : policy.UnhandledExceptionExitCode;

      result.ExitCode = exitCode;

      foreach (var observer in observers)
      {
        observer.OnCommandFailed(ex, exitCode);
      }

      if (policy.RenderUnhandledExceptions && !cancelled)
      {
        Render(console ?? AnsiConsole.Console, ex);
      }
    }
    finally
    {
      lifetime.StopApplication();
    }
  }

  // Mirrors Spectre's own error rendering, which PropagateExceptions bypasses.
  private static void Render(IAnsiConsole target, Exception exception)
  {
    if (exception is CommandAppException { Pretty: { } pretty })
    {
      target.Write(pretty);
      return;
    }

    target.MarkupLineInterpolated($"[red]Error:[/] {exception.Message}");

    if (exception.InnerException is CommandAppException { Pretty: { } innerPretty })
    {
      target.Write(innerPretty);
    }
  }
}
