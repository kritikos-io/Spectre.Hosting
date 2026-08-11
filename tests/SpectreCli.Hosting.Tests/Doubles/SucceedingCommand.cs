namespace Kritikos.SpectreCli.Hosting.Tests.Doubles;

using Spectre.Console.Cli;

internal sealed class SucceedingCommand(ExecutionProbe probe, ScopedProbe scoped) : AsyncCommand<ProbeSettings>
{
  protected override Task<int> ExecuteAsync(
    CommandContext context,
    ProbeSettings settings,
    CancellationToken cancellation)
  {
    probe.Executed = true;
    probe.ScopedInstance = scoped;

    // A disposed scoped dependency means the run's scope was torn down too early.
    probe.ExitCode = scoped.Disposed ? -99 : 42;
    return Task.FromResult(probe.ExitCode);
  }
}
