namespace Kritikos.SpectreCli.OpenTelemetry.Tests.Doubles;

using Spectre.Console.Cli;

internal sealed class ThrowingCommand : AsyncCommand<ThrowingCommand.Settings>
{
  protected override Task<int> ExecuteAsync(
    CommandContext context,
    Settings settings,
    CancellationToken cancellation)
    => throw new InvalidOperationException("boom");

  internal sealed class Settings : CommandSettings;
}
