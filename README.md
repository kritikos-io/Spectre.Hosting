# Spectre.Hosting

Run a [Spectre.Console.Cli] application inside the .NET Generic Host, with real dependency injection, a per-run service scope, correct exit codes, and OpenTelemetry instrumentation.

Spectre.Console.Cli ships its own `ITypeRegistrar`/`ITypeResolver` abstraction so it can be bridged to any container. These packages implement that bridge against `Microsoft.Extensions.DependencyInjection` and then fill the gaps the bridge alone leaves open: scoped lifetimes, deterministic disposal, exception-aware telemetry, and an exit code that survives back to `Main`.

| Package | Purpose |
| --- | --- |
| [Kritikos.SpectreCli.Hosting](src/SpectreCli.Hosting/README.md) | Host integration: DI, per-run scope, exit codes, error and cancellation policy |
| [Kritikos.SpectreCli.OpenTelemetry](src/SpectreCli.OpenTelemetry/README.md) | Traces and metrics for every command execution, including failures |

Both packages target `net8.0` and `net10.0`.

## Getting Started

```bash
dotnet add package Kritikos.SpectreCli.Hosting
```

```csharp
using Kritikos.SpectreCli.Hosting;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSpectreConsole<GreetCommand>(args);

var app = builder.Build();
return await app.RunSpectreConsoleAsync();
```

Commands are resolved from the container, so they take constructor dependencies like any other service:

```csharp
internal sealed class GreetCommand(ILogger<GreetCommand> logger) : AsyncCommand<GreetCommand.Settings>
{
  protected override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellation)
  {
    logger.LogInformation("Greeting {Name}", settings.Name);
    AnsiConsole.MarkupLineInterpolated($"Hello, {settings.Name}!");
    return Task.FromResult(0);
  }

  internal sealed class Settings : CommandSettings
  {
    [CommandArgument(0, "<name>")]
    public string Name { get; init; } = string.Empty;
  }
}
```

> [!IMPORTANT]
> Since Spectre.Console.Cli 0.55, `ExecuteAsync` on `AsyncCommand<TSettings>` is `protected`, not `public`.

## Features

1. **Real dependency injection.** Commands, settings, and anything Spectre resolves come from the host container, including `IOptions<T>`, `ILogger<T>`, and typed `HttpClient`s.
1. **One service scope per run.** Commands may depend on scoped services such as a `DbContext`. The scope is created when Spectre builds its type resolver and disposed when the run ends, on both the success and the failure path.
1. **Deterministic disposal.** Types Spectre asks the bridge to construct are tracked and disposed, which the container cannot do for `ActivatorUtilities`-created instances.
1. **Honest exit codes.** The command's exit code is carried back through `RunSpectreConsoleAsync` rather than process-global `Environment.ExitCode`, so a host that shuts down early no longer reports success.
1. **Observable failures.** Exceptions propagate out of Spectre to the host, where they reach every registered `ICommandExecutionObserver` before the process ends.
1. **Interceptors from the container.** Register as many `ICommandInterceptor`s as you like; they are resolved from DI along with everything else.
1. **OpenTelemetry out of the box.** A span and two metrics per command execution, with `error.type` and an `exception` event when a command throws.

## Configuration

`AddSpectreConsole` accepts an optional callback over `SpectreConsoleOptions`:

```csharp
builder.Services.AddSpectreConsole<GreetCommand>(
  args,
  configureOptions: options => options.CancellationExitCode = 2);
```

| Property | Default | Effect |
| --- | --- | --- |
| `PropagateExceptions` | `true` | Lets command exceptions escape Spectre so the host can observe them |
| `RenderUnhandledExceptions` | `true` | Renders the error to the console before shutdown |
| `UnhandledExceptionExitCode` | `-1` | Exit code when a command throws |
| `CancellationExitCode` | `130` | Exit code when the run is cancelled |

The last two match Spectre's own defaults, so enabling the host-level hook does not change observable behaviour.

> [!WARNING]
> Spectre evaluates `PropagateExceptions` *before* its own `ExceptionHandler`. If you configure a handler through `IConfigurator`, also set `PropagateExceptions = false` — otherwise the exception is rethrown and your handler never runs. Doing so also disables `ICommandExecutionObserver` notifications.

## Usage Examples

### Scoped dependencies

```csharp
builder.Services.AddDbContext<CatalogContext>();          // scoped by default
builder.Services.AddSpectreConsole<ImportCommand>(args);  // resolved inside the run's scope
```

### Reacting to failures

```csharp
internal sealed class FailureLogger(ILogger<FailureLogger> logger) : ICommandExecutionObserver
{
  public void OnCommandFailed(Exception exception, int exitCode)
    => logger.LogError(exception, "Command failed with exit code {ExitCode}", exitCode);
}

builder.Services.AddSingleton<ICommandExecutionObserver, FailureLogger>();
```

`ICommandInterceptor.InterceptResult` is skipped when a command throws, so an interceptor alone cannot see failures. This is the hook that can.

### Interceptors

Spectre 0.55 resolves `IEnumerable<ICommandInterceptor>` through the type resolver, so plain DI registration is all that is needed:

```csharp
builder.Services.AddSingleton<ICommandInterceptor, TimingInterceptor>();
builder.Services.AddSingleton<ICommandInterceptor, AuditInterceptor>();
```

Both `Intercept` and `InterceptResult` run in registration order — Spectre iterates its interceptor list forward for each pass rather than nesting them.

### Telemetry

```bash
dotnet add package Kritikos.SpectreCli.OpenTelemetry
```

```csharp
builder.Services.AddSpectreCliInstrumentation();
builder.Services.AddOpenTelemetry()
  .WithTracing(tracing => tracing.AddSource(SpectreCliInstrumentation.ActivitySourceName))
  .WithMetrics(metrics => metrics.AddMeter(SpectreCliInstrumentation.MeterName));
```

See the [OpenTelemetry package readme](src/SpectreCli.OpenTelemetry/README.md) for the spans, metrics, and attributes that are emitted, and [`samples/HostedCli`](samples/HostedCli) for a runnable end-to-end example.

## Building

```bash
dotnet build Spectre.Hosting.slnx
dotnet test Spectre.Hosting.slnx
```

> [!CAUTION]
> The Microsoft.Testing.Platform runner forwards unrecognised arguments to the test host. Passing MSBuild-style flags such as `--nologo` to `dotnet test` yields a misleading `Zero tests ran` result instead of an error.

Build artifacts land in the `artifacts` folder rather than per-project `bin`/`obj`, and package versions are managed centrally in `Directory.Packages.props`.

## Caveats

> [!IMPORTANT]
> The host shuts down as soon as the command completes, successfully or not. This package is built for a one-shot CLI process and cannot be combined with a long-running service, such as an ASP.NET Core backend, in the same host.

> [!NOTE]
> One scope is created per *process run*, not per command invocation, because a CLI process executes exactly one command.

> [!NOTE]
> The bridge disposes instances implementing `IDisposable`. Types implementing only `IAsyncDisposable` are not disposed, because Spectre's disposal path is synchronous.

> [!TIP]
> Prefer nesting your `CommandSettings` class inside its command. Spectre allows standalone settings classes, but telemetry infers the command type from the settings type's declaring type, so `spectre.command.type` is omitted for them.

[Spectre.Console.Cli]: https://spectreconsole.net/cli/
