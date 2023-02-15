namespace Kritikos.CommandLine.Hosting
{
  using Microsoft.Extensions.DependencyInjection;

  using Spectre.Console.Cli;

  public interface ICommandAppStartup
  {
    public void ConfigureServices(IServiceCollection services);

    public void Configure(IConfigurator appConfiguration);
  }
}
