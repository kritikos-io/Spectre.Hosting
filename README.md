# Spectre.Hosting

An opinionated set of extensions and infrastructure for the excelent [Spectre.Console.Cli][1], mainly covering the
integration with NET Generic Host, Dependency Injection and logging via [Serilog][2].

## Installation

```Add-Package Kritikos.Spectre.Hosting```

## Usage

### Generic Host

Runs the `CommandApp` as a HostedService in a .NET Generic Host, providing dependency injection, integration with the
Console Runtime, and logging to event viewer/system journal via `Microsoft.Extensions.Hosting.Systemd`
and `Microsoft.Extensions.Hosting.WindowsServices`.
Internally enables both logging functionality and dependency injection.

```csharp
public class Startup : ICommandAppStartup
{
  private readonly IConfiguration configuration;
  private readonly IHostEnvironment environment;

  public Startup(IConfiguration configuration, IHostEnvironment environment)
  {
    this.configuration = configuration;
    this.environment = environment;
  }

  public void ConfigureServices(IServiceCollection services)
  {
    // inject required services
  }

   public void Configure(IConfigurator appConfiguration)
  {
    // configure Spectre.Console.Cli CommandApp
    ArgumentNullException.ThrowIfNull(appConfiguration);
    appConfiguration
      .SetApplicationName("devops-cli")
      .CaseSensitivity(CaseSensitivity.None);

    appConfiguration.AddCommand<FooCommand>("foo");
  }
}
```

```csharp
var host = Host.CreateDefaultBuilder(args)
  .ConfigureCommandLineDefaults<Startup>()
  .Build();

await host.RunAsync();
```

### Dependency Injection

Provide a proper `IServiceCollection` implementation to `TypeRegistrar` and use it when constructing your `CommandApp`:

```csharp
var services = new ServiceCollection();
var app = new CommandApp(new TypeRegistrar(services));
```

### Logging

Predefined logging settings include settable verbosity at run time and logging to a file writer.
To use:

- Inherit `LogCommandSettings` on your own command settings class
- Pass the `LogInterceptor` to your CommandApp as an interceptor
- Call `AddSpectreInterception()` to your Serilog LoggerConfiguration

```csharp
internal class GitBranchNamingSettings : LogCommandSettings
{
  [CommandArgument(0, "[branch]")]
  [DefaultValue(null)]
  public string? BranchName { get; init; } = null;
}
```

```csharp
var app = new CommandApp()
app
  .Configure(c =>
  {
    c.SetInterceptor(new LogInterceptor());
  });
```

```csharp
Log.Logger = new LoggerConfiguration()
  .AddSpectreInterception()
  .CreateLogger();
```

Alternatively, create your own implementations of the above classes.

[1]: https://spectreconsole.net/cli/

[2]: https://github.com/serilog/serilog
