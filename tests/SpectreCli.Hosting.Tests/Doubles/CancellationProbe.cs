namespace Kritikos.SpectreCli.Hosting.Tests.Doubles;

internal sealed class CancellationProbe
{
  public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}
