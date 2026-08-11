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

One span per command execution, named after the command type:

| Attribute | Notes |
| --- | --- |
| `spectre.command.name` | The command name from `CommandContext` |
| `spectre.command.type` | Full name of the command type |
| `spectre.command.exit_code` | The exit code returned, or the host's exit code on failure |
| `error.type` | Full name of the exception type; present only on failure |

The span status is `Ok` for exit code `0`, and `Error` for any non-zero exit code or thrown exception. Failures also carry an `exception` event with `exception.type`, `exception.message`, and `exception.stacktrace`.

Two instruments, tagged with `spectre.command.name`, `spectre.command.type`, `spectre.command.exit_code`, and `error.type` when the run failed:

| Instrument | Type | Unit |
| --- | --- | --- |
| `spectre.command.duration` | Histogram | `s` |
| `spectre.command.executions` | Counter | — |

## How Failures Are Captured

Spectre skips `ICommandInterceptor.InterceptResult` when a command throws, so an interceptor alone would leave the span unstopped and the metrics unrecorded. The instrumentation therefore also implements `ICommandExecutionObserver`, which the host invokes on the exception path.

> [!WARNING]
> This depends on `SpectreConsoleOptions.PropagateExceptions` remaining enabled, which is the default. Turning it off means failed commands emit no telemetry.

## Caveats

> [!TIP]
> Nest your `CommandSettings` class inside its command. The command type is inferred from the settings type's declaring type, so a standalone settings class makes `spectre.command.type` report the settings type instead.

> [!NOTE]
> `spectre.command.name` reports Spectre's internal `__default_command` sentinel when a default command runs without an explicit command name.

[Spectre.Console.Cli]: https://spectreconsole.net/cli/
