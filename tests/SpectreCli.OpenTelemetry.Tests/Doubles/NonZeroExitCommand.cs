namespace Kritikos.SpectreCli.OpenTelemetry.Tests.Doubles;

using Spectre.Console.Cli;

internal sealed class NonZeroExitCommand : AsyncCommand<NonZeroExitCommand.Settings>
{
  protected override Task<int> ExecuteAsync(
    CommandContext context,
    Settings settings,
    CancellationToken cancellation)
    => Task.FromResult(3);

  internal sealed class Settings : CommandSettings;
}
