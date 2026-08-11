namespace Kritikos.SpectreCli.OpenTelemetry.Tests.Doubles;

using Spectre.Console.Cli;

internal sealed class SucceedingCommand : AsyncCommand<SucceedingCommand.Settings>
{
  protected override Task<int> ExecuteAsync(
    CommandContext context,
    Settings settings,
    CancellationToken cancellation)
    => Task.FromResult(0);

  internal sealed class Settings : CommandSettings;
}
