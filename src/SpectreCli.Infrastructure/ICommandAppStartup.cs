namespace Kritikos.SpectreCli.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

/// <summary>
/// Provides an interface for initializing services and middleware used by an application.
/// </summary>
public interface ICommandAppStartup
{
  /// <summary>
  /// Register services into the <see cref="IServiceCollection" />.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  public void ConfigureServices(IServiceCollection services);

  /// <summary>
  /// Configures the application.
  /// </summary>
  /// <param name="app">An <see cref="IConfigurator"/> for the app to configure.</param>
  public void Configure(IConfigurator app);
}
