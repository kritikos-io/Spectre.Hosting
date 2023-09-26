# Spectre.Hosting

An opinionated set of extensions and infrastructure for the excelent [Spectre.Console.Cli][1], mainly covering the
integration with Dependency Injection and logging via [Serilog][2].

## Usage

### CommandBuilder and Extensions

Creates a `CommandApp` with a fluent builder pattern, with the following options provided:

- Using `ICommandAppStartup` for configuration:
  ```csharp
  public class CommandStartup : ICommandStartup
  { ... }

  var app = CommandBuilder.CreateDefaultBuilder()
    .UseStartup<CommandStartup>;
  ```
- Make all `CommandSettings` available for dependency injection:
  ```csharp
  public class CommandStartup : ICommandStartup
  { ... }

  var app = CommandBuilder.CreateDefaultBuilder()
    .RegisterSettingsFromAssemblyContaining<CommandStartup>();

    // OR

  var app = CommandBuilder.CreateDefaultBuilder()
    .RegisterSettingsFromAssembly(typeof(CommandStartup).Assembly);
  ```
- Adds logging to file via [Serilog][2]:
  ```csharp
  // Additionally, make sure your CommandSettings inherit from LogCommandSettings
  // in order to include the verbosity level and the path to the log file
  var app = CommandBuilder.CreateDefaultBuilder()
    .AddFileLogging();
  ```

For ease of use, the extension method `CommandBuilder.CreateDefaultCommandApp<T>()` is provided, where `T` is an implementation of `ICommandStartup`. This method will ensure you get a command app with all the above options enabled. A sample implementation follows:


```csharp
public class Startup : ICommandAppStartup
{
  public void ConfigureServices(IServiceCollection services)
  {
    services.AddRefitClient<IAzureDevOpsApi>(_=>new RefitSettings(), string.Empty);
  }

   public void Configure(IConfigurator appConfiguration)
  {
    ArgumentNullException.ThrowIfNull(appConfiguration);
    appConfiguration
      .SetApplicationName("devops-cli")
      .CaseSensitivity(CaseSensitivity.None);

    appConfiguration.AddCommand<FooCommand>("foo");
  }
}
```

### Dependency Injection

Provide a proper `IServiceCollection` implementation to `TypeRegistrar` and use it when constructing your `CommandApp`:

```csharp
var services = new ServiceCollection();
var app = new CommandApp(new TypeRegistrar(services));
```

### Logging

Predefined logging settings include settable verbosity at run time and logging to a file writer.

Alternatively, create your own implementations of the above classes.

[1]: https://spectreconsole.net/cli/

[2]: https://github.com/serilog/serilog
