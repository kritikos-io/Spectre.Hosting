namespace Kritikos.SpectreCli.Hosting.Tests.Doubles;

using Spectre.Console.Cli;

internal sealed class ThrowingCommand : AsyncCommand<ProbeSettings>
{
  protected override Task<int> ExecuteAsync(
    CommandContext context,
    ProbeSettings settings,
    CancellationToken cancellation)
    => throw new InvalidOperationException("command failed");
}
