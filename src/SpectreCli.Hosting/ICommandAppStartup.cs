namespace Kritikos.SpectreCli.Hosting;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

public interface ICommandAppStartup : ICommandAppServiceInjector, ICommandAppConfiguration
{
  // void ConfigureServices(IServiceCollection services);
  //
  // void Configure(IConfigurator app);
}

public interface ICommandAppServiceInjector
{
  static abstract void ConfigureServices(IServiceCollection services);
}

public interface ICommandAppConfiguration
{
  static abstract void Configure(IConfigurator app);
}

internal class EmptyCommandAppStartup : ICommandAppStartup
{
  /// <inheritdoc />
  public static void ConfigureServices(IServiceCollection services) { }

  /// <inheritdoc />
  public static void Configure(IConfigurator app) { }
}
