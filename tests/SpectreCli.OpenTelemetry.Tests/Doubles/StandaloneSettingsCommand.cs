namespace Kritikos.SpectreCli.OpenTelemetry.Tests.Doubles;

using Spectre.Console.Cli;

internal sealed class StandaloneSettingsCommand : AsyncCommand<StandaloneSettings>
{
  protected override Task<int> ExecuteAsync(
    CommandContext context,
    StandaloneSettings settings,
    CancellationToken cancellation)
    => Task.FromResult(0);
}
