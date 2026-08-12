# Kritikos.SpectreCli.OpenTelemetry

OpenTelemetry traces and metrics for [Spectre.Console.Cli] commands hosted with [Kritikos.SpectreCli.Hosting](../SpectreCli.Hosting/README.md), including commands that terminate with an exception.

## Getting Started

```bash
dotnet add package Kritikos.SpectreCli.OpenTelemetry
```

```csharp
using Kritikos.SpectreCli.Hosting;
using Kritikos.SpectreCli.OpenTelemetry;

builder.Services.AddSpectreCliInstrumentation();
builder.Services.AddSpectreConsole<GreetCommand>(args);

builder.Services.AddOpenTelemetry()
  .WithTracing(tracing => tracing
    .AddSource(SpectreCliInstrumentation.ActivitySourceName)
    .AddOtlpExporter())
  .WithMetrics(metrics => metrics
    .AddMeter(SpectreCliInstrumentation.MeterName)
    .AddOtlpExporter());
```

> [!IMPORTANT]
> `AddSpectreCliInstrumentation` only registers the instrumentation. Nothing is exported until you add `SpectreCliInstrumentation.ActivitySourceName` and `SpectreCliInstrumentation.MeterName` to your OpenTelemetry configuration.

## Emitted Telemetry

One span per command execution, named after the command type when it can be determined and after the command name otherwise:

| Attribute | Notes |
| --- | --- |
| `spectre.command.name` | The command name, or `(default)` for a default command invoked without one |
| `spectre.command.type` | Full name of the command type. **Omitted** when it cannot be determined |
| `spectre.command.exit_code` | The exit code returned, or the host's exit code on failure |
| `error.type` | Full name of the exception type; present only on failure |

The span status is `Ok` for exit code `0`, and `Error` for any non-zero exit code or thrown exception. Failures also carry an `exception` event with `exception.type`, `exception.message`, and `exception.stacktrace`.

Two instruments, carrying the same attributes as the span:

| Instrument | Type | Unit |
| --- | --- | --- |
| `spectre.command.duration` | Histogram | `s` |
| `spectre.command.executions` | Counter | — |

`spectre.command.duration` ships explicit bucket boundaries covering a CLI's realistic range — a few milliseconds of startup through a multi-minute job — rather than the SDK's HTTP-oriented defaults. Exporters that honour instrument advice pick these up automatically.

> [!NOTE]
> `spectre.command.type` is deliberately absent rather than wrong when the command type cannot be
> determined. Spectre exposes no supported way for an interceptor to learn the executing command's
> type — the public `ICommandInfo` carries no `CommandType` — so it is inferred from the settings
> class's declaring type. Nest your settings inside the command and the attribute is always present.

### Process and runtime attributes

These are **resource** attributes in OpenTelemetry, not span attributes, so this package does not stamp them on every span. Add them once to the resource instead:

```csharp
builder.Services.AddOpenTelemetry()
  .ConfigureResource(resource => resource
    .AddService(serviceName: "my-cli", serviceVersion: version)
    .AddProcessRuntimeDetector());
```

> [!TIP]
> `InstanceId.CreateDeterministic` from the hosting package produces a stable `service.instance.id`,
> so repeated runs on the same machine share an identity.

## How Failures Are Captured

Spectre skips `ICommandInterceptor.InterceptResult` when a command throws, so an interceptor alone would leave the span unstopped and the metrics unrecorded. The instrumentation therefore also implements `ICommandExecutionObserver`, which the host invokes on the exception path.

> [!WARNING]
> This depends on `SpectreConsoleOptions.PropagateExceptions` remaining enabled, which is the default. Turning it off means failed commands emit no telemetry.

## Caveats

> [!IMPORTANT]
> The `spectre.command.*` attribute and instrument names are **not stable** while these packages are
> pre-1.0. They are not covered by any OpenTelemetry semantic convention, so they may be renamed in a
> minor release. `error.type` and the `exception` event follow the OpenTelemetry conventions and are
> expected to remain as they are.

> [!TIP]
> Nest your `CommandSettings` class inside its command. The command type is inferred from the settings
> type's declaring type, so a standalone settings class means `spectre.command.type` is omitted.

[Spectre.Console.Cli]: https://spectreconsole.net/cli/
