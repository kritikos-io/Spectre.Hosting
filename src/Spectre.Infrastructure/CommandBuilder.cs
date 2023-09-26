namespace Kritikos.Spectre.Infrastructure;

using global::Spectre.Console.Cli;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A builder for <see cref="ICommandApp"/> instances.
/// </summary>
public sealed class CommandBuilder
{
  private CommandBuilder(IServiceCollection? services = null, ITypeRegistrar? registrar = null)
  {
    Services = services ?? new ServiceCollection();
    Registrar = registrar ?? new TypeRegistrar(Services);
  }

  internal IServiceCollection Services { get; }

  internal ITypeRegistrar Registrar { get; }

  internal bool HasConfiguredLogging { get; set; }

  /// <summary>
  /// Creates a new <see cref="ICommandApp"/> instance with default options.
  /// </summary>
  /// <typeparam name="T">The <see cref="ICommandAppStartup"/> to use both for registering <see cref="CommandSettings"/> and configuring the resulting <see cref="ICommandApp"/>.</typeparam>
  /// <returns>A properly configured <see cref="ICommandApp"/> with dependency injection.</returns>
  public static ICommandApp CreateDefaultCommandApp<T>()
    where T : ICommandAppStartup
    => new CommandBuilder()
      .UseStartup<T>()
      .RegisterSettingsFromAssemblyContaining<T>()
      .Build();

  /// <summary>
  /// Creates a new <see cref="CommandBuilder"/> allowing further customization before building.
  /// </summary>
  /// <param name="services">An implementation of a service provider for dependency injection.</param>
  /// <param name="registrar">The type registrar to be used.</param>
  /// <returns>A <see cref="CommandBuilder"/> instance.</returns>
  public static CommandBuilder CreateDefaultBuilder(
    IServiceCollection? services = null,
    ITypeRegistrar? registrar = null)
    => new(services, registrar);
}
