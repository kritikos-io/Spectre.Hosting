namespace Kritikos.SpectreCli.Hosting;

/// <summary>
/// Carries the exit code from the hosted command run to
/// <see cref="HostExtensions.RunSpectreConsoleAsync"/>, replacing process-global
/// <see cref="Environment.ExitCode"/> state.
/// </summary>
internal sealed class SpectreConsoleResult
{
  /// <summary>
  /// Gets or sets the exit code produced by the command run,
  /// or <see langword="null"/> if the run never completed.
  /// </summary>
  public int? ExitCode { get; set; }
}
