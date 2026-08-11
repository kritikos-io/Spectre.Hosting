using System.Diagnostics.Metrics;
using System.Reflection;

using Kritikos.HostedCli;
using Kritikos.SpectreCli.Hosting;
using Kritikos.SpectreCli.OpenTelemetry;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
var version = typeof(Program).Assembly
  .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
  ?.InformationalVersion;
builder.Services.AddOpenTelemetry()
  .ConfigureResource(resource => resource.AddService(
    serviceName: Telemetry.ServiceName,
    serviceVersion: version,
    serviceInstanceId: InstanceId.CreateDeterministic(Telemetry.ServiceName).ToString()))
  .WithTracing(tracing => tracing
    .AddSource(Telemetry.ServiceName)
    .AddSource(SpectreCliInstrumentation.ActivitySourceName)
    .AddHttpClientInstrumentation()
    .AddConsoleExporter()
    .AddOtlpExporter(ConfigureOtlp))
  .WithMetrics(metrics => metrics
    .AddMeter(Telemetry.ServiceName)
    .AddMeter(SpectreCliInstrumentation.MeterName)
    .AddHttpClientInstrumentation()
    .AddView(instrument => instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
      ? new Base2ExponentialBucketHistogramConfiguration()
      : null)
    .AddOtlpExporter(ConfigureOtlp))
  .WithLogging(logging => logging
    .AddConsoleExporter()
    .AddOtlpExporter(ConfigureOtlp));

builder.Services.AddSpectreConsole<GreetCommand>(args, c => c.UseCommandInstrumentation());

var app = builder.Build();
return await app.RunSpectreConsoleAsync();

static void ConfigureOtlp(OtlpExporterOptions options)
{
  options.Endpoint = new Uri("http://127.0.0.1:4317");
  options.Protocol = OtlpExportProtocol.Grpc;
}
