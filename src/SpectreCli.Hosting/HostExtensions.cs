namespace Kritikos.SpectreCli.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

/// <summary>
/// Extension methods for running a Spectre.Console.Cli hosted application.
/// </summary>
public static class HostExtensions
{
  /// <summary>
  /// Runs the host, waits for the Spectre command to complete,
  /// and returns its exit code.
  /// </summary>
  /// <param name="host">The built host.</param>
  /// <param name="cancellationToken">Optional cancellation token.</param>
  /// <returns>
  /// The exit code produced by the Spectre command, or
  /// <see cref="SpectreConsoleOptions.UnhandledExceptionExitCode"/> if the host shut down before
  /// the command completed.
  /// </returns>
  public static async Task<int> RunSpectreConsoleAsync(
    this IHost host,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(host);

    var result = host.Services.GetRequiredService<SpectreConsoleResult>();
    var options = host.Services.GetRequiredService<IOptions<SpectreConsoleOptions>>().Value;

    await host.RunAsync(cancellationToken).ConfigureAwait(false);
    return result.ExitCode ?? options.UnhandledExceptionExitCode;
  }
}
