namespace Kritikos.SpectreCli.Infrastructure;

using System.Runtime.InteropServices;

/// <summary>
/// A POSIX-compatible handler of cancellation logic for console apps
/// </summary>
internal sealed class PosixCancellationTokenSource : IDisposable
{
  private readonly CancellationTokenSource cancellationSource = new();
  private readonly List<PosixSignalRegistration> signalRegistrations = new();

  public PosixCancellationTokenSource()
  {
    var sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, OnSignal);
    var sigQuit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, OnSignal);
    var sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, OnSignal);

    signalRegistrations.Add(sigInt);
    signalRegistrations.Add(sigQuit);
    signalRegistrations.Add(sigTerm);
  }

  public CancellationToken Token
    => cancellationSource.Token;

  /// <inheritdoc />
  public void Dispose()
  {
    foreach (var signal in signalRegistrations)
    {
      signal.Dispose();
    }
  }

  private void OnSignal(PosixSignalContext ctx)
  {
    ctx.Cancel = true;
    cancellationSource.Cancel();
  }
}
