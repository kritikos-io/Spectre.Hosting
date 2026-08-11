namespace Kritikos.SpectreCli.Hosting;

/// <summary>
/// Observes command runs that terminate with an exception.
/// </summary>
/// <remarks>
/// Spectre skips <see cref="Spectre.Console.Cli.ICommandInterceptor.InterceptResult"/> when a
/// command throws, so interceptors alone cannot see failures. Every implementation registered in
/// dependency injection is notified, in registration order, before the host shuts down. Requires
/// <see cref="SpectreConsoleOptions.PropagateExceptions"/> to remain enabled.
/// </remarks>
public interface ICommandExecutionObserver
{
  /// <summary>
  /// Called when a command run terminates with an exception.
  /// </summary>
  /// <param name="exception">The exception that terminated the run.</param>
  /// <param name="exitCode">The exit code the host will return.</param>
  void OnCommandFailed(Exception exception, int exitCode);
}
