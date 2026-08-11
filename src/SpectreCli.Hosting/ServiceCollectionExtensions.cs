namespace Kritikos.SpectreCli.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
  /// <remarks>
  /// Each command run executes inside its own <see cref="IServiceScope"/>, so commands and their
  /// dependencies may safely take scoped services. The scope is created when Spectre builds its
  /// type resolver and disposed once the run completes — one scope per process run.
  /// </remarks>
  /// <param name="services">The service collection to register into.</param>
  /// <param name="args">The command-line arguments to forward.</param>
  /// <param name="configure">A callback to configure the Spectre command tree.</param>
  /// <param name="configureOptions">An optional callback to configure the error and cancellation policy.</param>
  /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
  public static IServiceCollection AddSpectreConsole(
    this IServiceCollection services,
    string[] args,
    Action<IConfigurator> configure,
    Action<SpectreConsoleOptions>? configureOptions = null)
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(args);
    ArgumentNullException.ThrowIfNull(configure);

    AddInfrastructure(services, args, configureOptions);

    services.AddSingleton<ICommandApp>(sp =>
    {
      var app = new CommandApp(CreateRegistrar(sp));
      app.Configure(ApplyPolicy(sp, configure));
      return app;
    });

    return services;
  }

  /// <summary>
  /// Registers a <see cref="CommandApp{TDefaultCommand}"/> and the necessary infrastructure
  /// to run it as a hosted service, forwarding the supplied <paramref name="args"/> to Spectre.
  /// </summary>
  /// <remarks>
  /// Each command run executes inside its own <see cref="IServiceScope"/>, so commands and their
  /// dependencies may safely take scoped services. The scope is created when Spectre builds its
  /// type resolver and disposed once the run completes — one scope per process run.
  /// </remarks>
  /// <typeparam name="TDefaultCommand">The default command type to execute when no command is specified.</typeparam>
  /// <param name="services">The service collection to register into.</param>
  /// <param name="args">The command-line arguments to forward.</param>
  /// <param name="configure">An optional callback to further configure the Spectre command tree.</param>
  /// <param name="configureOptions">An optional callback to configure the error and cancellation policy.</param>
  /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
  public static IServiceCollection AddSpectreConsole<TDefaultCommand>(
    this IServiceCollection services,
    string[] args,
    Action<IConfigurator>? configure = null,
    Action<SpectreConsoleOptions>? configureOptions = null)
    where TDefaultCommand : class, ICommand
  {
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(args);

    AddInfrastructure(services, args, configureOptions);

    services.AddSingleton<ICommandApp>(sp =>
    {
      var app = new CommandApp<TDefaultCommand>(CreateRegistrar(sp));
      app.Configure(ApplyPolicy(sp, configure));
      return app;
    });

    return services;
  }

  private static void AddInfrastructure(
    IServiceCollection services,
    string[] args,
    Action<SpectreConsoleOptions>? configureOptions)
  {
    var options = services.AddOptions<SpectreConsoleOptions>();
    if (configureOptions is not null)
    {
      options.Configure(configureOptions);
    }

    services.AddSingleton(new SpectreConsoleArgs(args));
    services.AddSingleton<SpectreConsoleResult>();
    services.AddHostedService<SpectreConsoleWorker>();
  }

  private static TypeRegistrar CreateRegistrar(IServiceProvider sp)
    => new(sp.GetRequiredService<IServiceScopeFactory>());

  private static Action<IConfigurator> ApplyPolicy(IServiceProvider sp, Action<IConfigurator>? configure)
    => configurator =>
    {
      // Applied first so a consumer's own configuration can still override the defaults.
      configurator.Settings.PropagateExceptions =
        sp.GetRequiredService<IOptions<SpectreConsoleOptions>>().Value.PropagateExceptions;
      configure?.Invoke(configurator);
    };
}
