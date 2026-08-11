namespace Kritikos.SpectreCli.OpenTelemetry;

using Kritikos.SpectreCli.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Spectre.Console.Cli;

/// <summary>
/// Extension methods for enabling OpenTelemetry instrumentation of Spectre.Console.Cli commands.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Enables automatic OpenTelemetry instrumentation for all Spectre commands.
  /// Creates an <see cref="System.Diagnostics.Activity"/> span per command execution
  /// and records <c>spectre.command.duration</c> / <c>spectre.command.executions</c> metrics,
  /// including for runs that terminate with an exception.
  /// </summary>
  /// <param name="services">The service collection to register into.</param>
  /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
  /// <remarks>
  /// <para>
  /// Register the library's telemetry sources in your OpenTelemetry configuration:
  /// </para>
  /// <code>
  /// .WithTracing(t =&gt; t.AddSource(SpectreCliInstrumentation.ActivitySourceName))
  /// .WithMetrics(m =&gt; m.AddMeter(SpectreCliInstrumentation.MeterName))
  /// </code>
  /// <para>
  /// Failure reporting relies on <see cref="SpectreConsoleOptions.PropagateExceptions"/>
  /// remaining enabled.
  /// </para>
  /// </remarks>
  public static IServiceCollection AddSpectreCliInstrumentation(this IServiceCollection services)
  {
    ArgumentNullException.ThrowIfNull(services);

    services.TryAddSingleton<CommandActivityInterceptor>();

    // Spectre resolves IEnumerable<ICommandInterceptor> through the type resolver, which falls
    // back to the host container; both roles must share the one instance holding the span.
    services.TryAddEnumerable(ServiceDescriptor.Singleton<ICommandInterceptor, CommandActivityInterceptor>(
      sp => sp.GetRequiredService<CommandActivityInterceptor>()));
    services.TryAddEnumerable(ServiceDescriptor.Singleton<ICommandExecutionObserver, CommandActivityInterceptor>(
      sp => sp.GetRequiredService<CommandActivityInterceptor>()));

    return services;
  }
}
