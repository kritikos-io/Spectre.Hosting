namespace Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using Kritikos.CommandLine.Hosting;

using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

public static class CommandExtensions
{
  public static IHostBuilder ConfigureCommandLineDefaults<T>(this IHostBuilder builder)
    where T : ICommandAppStartup
  {
    var startupType = typeof(T);
    builder.ConfigureServices(services =>
    {
      if (!typeof(ICommandAppStartup).IsAssignableFrom(startupType.GetTypeInfo()))
      {
        throw new ArgumentException("Startup type must implement ICommandAppStartup!", startupType.Name);
      }

      services
        .AddSingleton(typeof(ICommandAppStartup), startupType)
        .AddCommandApp()
        .AddHostedService<ConsoleHostedAppService>();
    });

    return builder
      .UseSystemd()
      .UseWindowsService()
      .UseConsoleLifetime();
  }

  private static IServiceCollection AddCommandApp(this IServiceCollection services)
  {
    using (var baseServices = services.BuildServiceProvider())
    {
      var serviceDelegateProvider = baseServices.GetRequiredService<ICommandAppStartup>();
      serviceDelegateProvider.ConfigureServices(services);
    }

    var app = new CommandApp(new TypeRegistrar(services));
    return services.AddSingleton<ICommandApp, CommandApp>(sp =>
    {
      var startup = sp.GetRequiredService<ICommandAppStartup>();
      app.Configure(startup.Configure);
      return app;
    });
  }
}
