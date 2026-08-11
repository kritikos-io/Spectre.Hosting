namespace Kritikos.SpectreCli.Hosting;

using Spectre.Console.Cli;

/// <summary>
/// Extension methods for configuring Spectre.Console.Cli interceptors.
/// </summary>
public static class ConfiguratorExtensions
{
  /// <summary>
  /// Adds an <see cref="ICommandInterceptor"/> to the command pipeline.
  /// Multiple interceptors can be added and are executed in registration order
  /// (reverse order for <see cref="ICommandInterceptor.InterceptResult"/>).
  /// </summary>
  /// <param name="configurator">The Spectre configurator.</param>
  /// <param name="interceptor">The interceptor instance to add.</param>
  /// <returns>The same <paramref name="configurator"/> instance for chaining.</returns>
  public static IConfigurator UseInterceptor(
    this IConfigurator configurator,
    ICommandInterceptor interceptor)
  {
    ArgumentNullException.ThrowIfNull(configurator);
    ArgumentNullException.ThrowIfNull(interceptor);

#pragma warning disable CS0618 // ICommandAppSettings.Interceptor is obsolete — we use it to read back and compose interceptors; consumers use UseInterceptor() instead.
    var current = configurator.Settings.Interceptor;
    configurator.Settings.Interceptor = current switch
    {
      CompositeCommandInterceptor composite => composite.Add(interceptor),
      not null => new CompositeCommandInterceptor([current, interceptor]),
      _ => interceptor,
    };
#pragma warning restore CS0618

    return configurator;
  }
}
