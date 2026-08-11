namespace Kritikos.SpectreCli.Hosting.Tests.Doubles;

using Spectre.Console.Cli;

internal sealed class BlockingCommand(CancellationProbe probe) : AsyncCommand<ProbeSettings>
{
  protected override async Task<int> ExecuteAsync(
    CommandContext context,
    ProbeSettings settings,
    CancellationToken cancellation)
  {
    probe.Started.TrySetResult();
    await Task.Delay(Timeout.Infinite, cancellation).ConfigureAwait(false);
    return 0;
  }
}
