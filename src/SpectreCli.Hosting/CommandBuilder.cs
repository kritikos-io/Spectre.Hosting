namespace Kritikos.SpectreCli.Hosting;

using Kritikos.SpectreCli.Hosting.HostBuilderDependencies;

using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

public sealed class CommandBuilder
{
  /// <summary>
  /// Builds a <see cref="CommandApp"/> using the provided <typeparamref name="T"/> as the startup configuration.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public static ICommandApp CreateHostedCommandApp<T>()
    where T : ICommandAppServiceInjector, ICommandAppConfiguration
  {
    var host = Host.CreateDefaultBuilder()
      .ConfigureServices(T.ConfigureServices);

    var registrar = new GenericHostBuilderTypeRegistrar(host);
    var app = new CommandApp(registrar);
    app.Configure(T.Configure);

    return app;
  }
}
