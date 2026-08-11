namespace Kritikos.SpectreCli.Hosting.Tests.Doubles;

internal sealed class RecordingObserver : ICommandExecutionObserver
{
  public Exception? Exception { get; private set; }

  public int ExitCode { get; private set; }

  public void OnCommandFailed(Exception exception, int exitCode)
  {
    Exception = exception;
    ExitCode = exitCode;
  }
}
