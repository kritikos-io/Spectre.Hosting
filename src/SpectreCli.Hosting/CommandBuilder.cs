namespace Kritikos.SpectreCli.Hosting;

using Kritikos.SpectreCli.Hosting.HostBuilderDependencies;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

public class CommandBuilder
{
  private Action<IServiceCollection> configureServices;
  private Action<IConfigurator> configureSpectre;

  public static ICommandApp CreateHostedCommandApp<T>(params string[] args)
    where T : ICommandAppServiceInjector, ICommandAppConfiguration
  {
    var host = Host.CreateDefaultBuilder()
      .UseConsoleLifetime()
      .ConfigureServices(T.ConfigureServices);

    var registrar = new GenericHostBuilderTypeRegistrar(host);
    var app = new CommandApp(registrar);
    app.Configure(T.Configure);

    host.ConfigureServices(services =>
    {
      services.AddSingleton(new CommandAppWrapper() { App = app, Args = args });
      services.AddHostedService<CommandAppService>();
    });

    return app;
  }
}
