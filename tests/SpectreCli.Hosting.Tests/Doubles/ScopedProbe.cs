namespace Kritikos.SpectreCli.Hosting.Tests.Doubles;

internal sealed class ScopedProbe : IDisposable
{
  public bool Disposed { get; private set; }

  public void Dispose() => Disposed = true;
}
