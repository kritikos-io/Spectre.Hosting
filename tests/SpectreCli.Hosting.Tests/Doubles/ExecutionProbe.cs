namespace Kritikos.SpectreCli.Hosting.Tests.Doubles;

internal sealed class ExecutionProbe
{
  public bool Executed { get; set; }

  public int ExitCode { get; set; }

  public ScopedProbe? ScopedInstance { get; set; }
}
