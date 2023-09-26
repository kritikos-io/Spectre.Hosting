namespace Kritikos.CommandLine.Hosting;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

/// <summary>
/// A builder for <see cref="ICommandApp"/> instances.
/// </summary>
public sealed class CommandBuilder
{
  public CommandBuilder(IServiceCollection? services = null, ITypeRegistrar? registrar = null)
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
  /// <param name="services">An implementation of a service provider for dependency injection.</param>
  /// <param name="registrar">The type registrar to be used.</param>
  /// <typeparam name="T">The <see cref="ICommandAppStartup"/> to use both for registering <see cref="CommandSettings"/> and configuring the resulting <see cref="ICommandApp"/>.</typeparam>
  /// <returns>A properly configured <see cref="ICommandApp"/> with dependency injection.</returns>
  public static ICommandApp CreateDefaultCommandApp<T>(
    IServiceCollection? services = null,
    ITypeRegistrar? registrar = null)
    where T : ICommandAppStartup
    => new CommandBuilder(services, registrar)
      .UseStartup<T>()
      .RegisterSettingsFromAssembly<T>()
      .Build();
}
