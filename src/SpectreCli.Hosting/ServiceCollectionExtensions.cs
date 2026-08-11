namespace Kritikos.SpectreCli.Hosting;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

/// <summary>
/// Extension methods for registering a Spectre.Console.Cli application
/// with the generic host's dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Registers a <see cref="CommandApp"/> and the necessary infrastructure to run
  /// it as a hosted service, forwarding the supplied <paramref name="args"/> to Spectre.
  /// </summary>
  /// <param name="services">The service collection to register into.</param>
  /// <param name="args">The command-line arguments to forward.</param>
  /// <param name="configure">A callback to configure the Spectre command tree.</param>
  /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
  public static IServiceCollection AddSpectreConsole(
    this IServiceCollection services,
    string[] args,
    Action<IConfigurator> configure)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(args);
    ArgumentNullException.ThrowIfNull(configure);

    services.AddSingleton<ICommandApp>(sp =>
    {
      var registrar = new TypeRegistrar(sp);
      var app = new CommandApp(registrar);
      app.Configure(configure);
      return app;
    });

    services.AddSingleton(new SpectreConsoleArgs(args));
    services.AddHostedService<SpectreConsoleWorker>();

    return services;
  }

  /// <summary>
  /// Registers a <see cref="CommandApp{TDefaultCommand}"/> and the necessary infrastructure
  /// to run it as a hosted service, forwarding the supplied <paramref name="args"/> to Spectre.
  /// </summary>
  /// <typeparam name="TDefaultCommand">The default command type to execute when no command is specified.</typeparam>
  /// <param name="services">The service collection to register into.</param>
  /// <param name="args">The command-line arguments to forward.</param>
  /// <param name="configure">An optional callback to further configure the Spectre command tree.</param>
  /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
  public static IServiceCollection AddSpectreConsole<TDefaultCommand>(
    this IServiceCollection services,
    string[] args,
    Action<IConfigurator>? configure = null)
    where TDefaultCommand : class, ICommand
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(args);

    services.AddSingleton<ICommandApp>(sp =>
    {
      var registrar = new TypeRegistrar(sp);
      var app = new CommandApp<TDefaultCommand>(registrar);

      if (configure is not null)
      {
        app.Configure(configure);
      }

      return app;
    });

    services.AddSingleton(new SpectreConsoleArgs(args));
    services.AddHostedService<SpectreConsoleWorker>();

    return services;
  }
}
