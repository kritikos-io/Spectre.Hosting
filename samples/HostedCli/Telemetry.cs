namespace Kritikos.HostedCli;

using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <summary>
/// Central telemetry definitions for the sample CLI application.
/// </summary>
internal static class Telemetry
{
  /// <summary>The service name used for OpenTelemetry resource identification.</summary>
  public const string ServiceName = "HostedCli";

  /// <summary>Gets the <see cref="ActivitySource"/> for distributed tracing.</summary>
  public static ActivitySource ActivitySource { get; } = new(ServiceName);

  /// <summary>Gets the <see cref="Meter"/> for metrics collection.</summary>
  public static Meter Meter { get; } = new(ServiceName);

  /// <summary>Gets a counter tracking the total number of greetings delivered.</summary>
  public static Counter<long> GreetingsCounter { get; } = Meter.CreateCounter<long>(
    "cli.greetings",
    description: "Total number of greetings delivered");
}
