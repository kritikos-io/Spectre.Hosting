namespace Kritikos.SpectreCli.Hosting;

using Microsoft.Extensions.Hosting;

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
  /// <returns>The exit code produced by the Spectre command.</returns>
  public static async Task<int> RunSpectreConsoleAsync(
    this IHost host,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(host);

    await host.RunAsync(cancellationToken).ConfigureAwait(false);
    return Environment.ExitCode;
  }
}
