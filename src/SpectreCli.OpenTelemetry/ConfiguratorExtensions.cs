namespace Kritikos.SpectreCli.OpenTelemetry;

using Kritikos.SpectreCli.Hosting;

using Spectre.Console.Cli;

/// <summary>
/// Extension methods for configuring OpenTelemetry instrumentation on Spectre.Console.Cli commands.
/// </summary>
public static class ConfiguratorExtensions
{
  /// <summary>
  /// Enables automatic OpenTelemetry instrumentation for all Spectre commands.
  /// Creates an <see cref="System.Diagnostics.Activity"/> span per command execution
  /// and records <c>spectre.command.duration</c> / <c>spectre.command.executions</c> metrics.
  /// </summary>
  /// <param name="configurator">The Spectre configurator to attach the interceptor to.</param>
  /// <returns>The same <paramref name="configurator"/> instance for chaining.</returns>
  /// <remarks>
  /// <para>
  /// Register the library's telemetry sources in your OpenTelemetry configuration:
  /// </para>
  /// <code>
  /// .WithTracing(t =&gt; t.AddSource(SpectreCliInstrumentation.ActivitySourceName))
  /// .WithMetrics(m =&gt; m.AddMeter(SpectreCliInstrumentation.MeterName))
  /// </code>
  /// </remarks>
  public static IConfigurator UseCommandInstrumentation(this IConfigurator configurator)
    => configurator.UseInterceptor(new CommandActivityInterceptor());
}
