namespace Kritikos.SpectreCli.Hosting;

/// <summary>
/// Controls how the hosted Spectre command application handles failures and cancellation.
/// </summary>
public sealed class SpectreConsoleOptions
{
  /// <summary>
  /// Gets or sets a value indicating whether command exceptions are propagated out of Spectre so
  /// the host can observe them. Defaults to <see langword="true"/>.
  /// </summary>
  /// <remarks>
  /// Spectre evaluates this before its own <c>ExceptionHandler</c>, so configuring a custom handler
  /// through the configurator also requires setting this to <see langword="false"/>. Leaving it
  /// enabled is what allows <see cref="ICommandExecutionObserver"/> implementations to run.
  /// </remarks>
  public bool PropagateExceptions { get; set; } = true;

  /// <summary>
  /// Gets or sets a value indicating whether unhandled command exceptions are rendered to the
  /// console before the host shuts down. Defaults to <see langword="true"/>.
  /// </summary>
  public bool RenderUnhandledExceptions { get; set; } = true;

  /// <summary>
  /// Gets or sets the exit code returned when a command terminates with an exception.
  /// Defaults to <c>-1</c>, matching Spectre's own behaviour.
  /// </summary>
  public int UnhandledExceptionExitCode { get; set; } = -1;

  /// <summary>
  /// Gets or sets the exit code returned when the run is cancelled.
  /// Defaults to <c>130</c>, matching Spectre's own behaviour.
  /// </summary>
  public int CancellationExitCode { get; set; } = 130;
}
