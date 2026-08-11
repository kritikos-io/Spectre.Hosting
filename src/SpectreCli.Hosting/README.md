# Kritikos.SpectreCli.Hosting

Runs a [Spectre.Console.Cli] application as a hosted service in the .NET Generic Host, backed by `Microsoft.Extensions.DependencyInjection`.

## Getting Started

```bash
dotnet add package Kritikos.SpectreCli.Hosting
```

```csharp
using Kritikos.SpectreCli.Hosting;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSpectreConsole<GreetCommand>(args);

var app = builder.Build();
return await app.RunSpectreConsoleAsync();
```

Use the non-generic overload to configure a command tree instead of a single default command:

```csharp
builder.Services.AddSpectreConsole(args, config =>
{
  config.AddCommand<GreetCommand>("greet");
  config.AddBranch("db", db => db.AddCommand<MigrateCommand>("migrate"));
});
```

## Capabilities

1. Commands and their dependencies are resolved from the host container.
1. Each run executes inside its own `IServiceScope`, so scoped services such as a `DbContext` work correctly. The scope is created when Spectre builds its type resolver and disposed when the run ends, on both the success and the failure path.
1. Instances the bridge constructs through `ActivatorUtilities` are tracked and disposed, which the container cannot do on its own.
1. `RunSpectreConsoleAsync` returns the command's exit code without touching process-global `Environment.ExitCode`.
1. Command failures are surfaced to `ICommandExecutionObserver` implementations, which `ICommandInterceptor` cannot observe.
1. `UseInterceptor` composes multiple `ICommandInterceptor`s where Spectre allows only one.
1. `InstanceId.CreateDeterministic` produces a stable RFC 9562 UUID v5 for `service.instance.id`.

## Configuration

| `SpectreConsoleOptions` | Default | Effect |
| --- | --- | --- |
| `PropagateExceptions` | `true` | Lets command exceptions escape Spectre so the host can observe them |
| `RenderUnhandledExceptions` | `true` | Renders the error to the console before shutdown |
| `UnhandledExceptionExitCode` | `-1` | Exit code when a command throws |
| `CancellationExitCode` | `130` | Exit code when the run is cancelled |

```csharp
builder.Services.AddSpectreConsole<GreetCommand>(
  args,
  configureOptions: options => options.CancellationExitCode = 2);
```

Register an `IAnsiConsole` in the container to control where unhandled exceptions are rendered; otherwise `AnsiConsole.Console` is used.

## Usage Examples

### Observing failures

```csharp
internal sealed class FailureLogger(ILogger<FailureLogger> logger) : ICommandExecutionObserver
{
  public void OnCommandFailed(Exception exception, int exitCode)
    => logger.LogError(exception, "Command failed with exit code {ExitCode}", exitCode);
}

builder.Services.AddSingleton<ICommandExecutionObserver, FailureLogger>();
```

### Composing interceptors

```csharp
builder.Services.AddSpectreConsole(args, config => config
  .UseInterceptor(new TimingInterceptor())
  .UseInterceptor(new AuditInterceptor()));
```

`Intercept` runs in registration order; `InterceptResult` runs in reverse. Container-managed interceptors do not need this — Spectre 0.55 resolves `IEnumerable<ICommandInterceptor>` through the type resolver, so `services.AddSingleton<ICommandInterceptor, T>()` is enough.

## Caveats

> [!WARNING]
> Spectre evaluates `PropagateExceptions` *before* its own `ExceptionHandler`. If you configure a handler through `IConfigurator`, also set `PropagateExceptions = false`, which in turn disables `ICommandExecutionObserver` notifications.

> [!NOTE]
> Only `IDisposable` is honoured when the run's scope is torn down. Spectre's disposal path is synchronous, so types implementing solely `IAsyncDisposable` are not disposed.

> [!NOTE]
> Calling `AddSpectreConsole` more than once registers duplicate services. Call it exactly once.

[Spectre.Console.Cli]: https://spectreconsole.net/cli/
