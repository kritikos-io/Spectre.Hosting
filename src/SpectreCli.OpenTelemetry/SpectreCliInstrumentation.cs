namespace Kritikos.SpectreCli.OpenTelemetry;

using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <summary>
/// Central telemetry definitions for the Spectre.Console.Cli OpenTelemetry integration.
/// Consumers must register <see cref="ActivitySourceName"/> with <c>.AddSource()</c>
/// and <see cref="MeterName"/> with <c>.AddMeter()</c> in their OpenTelemetry configuration.
/// </summary>
public static class SpectreCliInstrumentation
{
  /// <summary>
  /// The name used for the <see cref="ActivitySource"/> that emits command execution spans.
  /// </summary>
  public const string ActivitySourceName = "SpectreCli.OpenTelemetry";

  /// <summary>
  /// The name used for the <see cref="Meter"/> that records command execution metrics.
  /// </summary>
  public const string MeterName = "SpectreCli.OpenTelemetry";

  /// <summary>Gets the <see cref="ActivitySource"/> for command execution tracing.</summary>
  internal static ActivitySource ActivitySource { get; } = new(ActivitySourceName);

  /// <summary>Gets the <see cref="Meter"/> for command execution metrics.</summary>
  internal static Meter Meter { get; } = new(MeterName);

  /// <summary>Gets the histogram that records command execution duration in seconds.</summary>
  internal static Histogram<double> CommandDuration { get; } = Meter.CreateHistogram<double>(
    "spectre.command.duration",
    unit: "s",
    description: "Duration of Spectre.Console.Cli command execution");

  /// <summary>Gets the counter that tracks total command executions.</summary>
  internal static Counter<long> CommandExecutions { get; } = Meter.CreateCounter<long>(
    "spectre.command.executions",
    description: "Total number of Spectre.Console.Cli command executions");
}
