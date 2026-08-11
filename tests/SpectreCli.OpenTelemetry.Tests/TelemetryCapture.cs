namespace Kritikos.SpectreCli.OpenTelemetry.Tests;

using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <summary>Captures the spans and measurements the library emits during a command run.</summary>
internal sealed class TelemetryCapture : IDisposable
{
  private readonly ActivityListener activityListener;
  private readonly MeterListener meterListener;

  public TelemetryCapture()
  {
    activityListener = new ActivityListener
    {
      ShouldListenTo = source => source.Name == SpectreCliInstrumentation.ActivitySourceName,
      Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
      ActivityStopped = Activities.Add,
    };
    ActivitySource.AddActivityListener(activityListener);

    meterListener = new MeterListener
    {
      InstrumentPublished = (instrument, listener) =>
      {
        if (instrument.Meter.Name == SpectreCliInstrumentation.MeterName)
        {
          listener.EnableMeasurementEvents(instrument);
        }
      },
    };
    meterListener.SetMeasurementEventCallback<long>(
      (instrument, measurement, tags, _) => Record(instrument, measurement, tags));
    meterListener.SetMeasurementEventCallback<double>(
      (instrument, measurement, tags, _) => Record(instrument, measurement, tags));
    meterListener.Start();
  }

  public List<Activity> Activities { get; } = [];

  public List<Measurement> Measurements { get; } = [];

  public void Dispose()
  {
    activityListener.Dispose();
    meterListener.Dispose();
  }

  private void Record<T>(Instrument instrument, T measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags)
    where T : struct
    => Measurements.Add(new Measurement(
      instrument.Name,
      Convert.ToDouble(measurement, System.Globalization.CultureInfo.InvariantCulture),
      tags.ToArray()));

  internal sealed record Measurement(string Instrument, double Value, KeyValuePair<string, object?>[] Tags)
  {
    public object? Tag(string key) => Tags.FirstOrDefault(t => t.Key == key).Value;
  }
}
