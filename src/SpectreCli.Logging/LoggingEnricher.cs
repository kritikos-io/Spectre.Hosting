namespace Kritikos.SpectreCli.Logging;

using Serilog.Core;
using Serilog.Events;

public class LoggingEnricher : ILogEventEnricher
{
  public const string LogFilePathPropertyName = "LogFilePath";

  private string cachedLogFilePath = string.Empty;
  private LogEventProperty? cachedLogFilePathProperty;

  private static string Path { get; } = string.Empty;

  /// <inheritdoc />
  public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
  {
    ArgumentNullException.ThrowIfNull(logEvent);
    ArgumentNullException.ThrowIfNull(propertyFactory);

    LogEventProperty logFilePathProperty;

    if (cachedLogFilePathProperty != null && Path.Equals(cachedLogFilePath, StringComparison.Ordinal))
    {
      logFilePathProperty = cachedLogFilePathProperty;
    }
    else
    {
      cachedLogFilePath = Path;
      cachedLogFilePathProperty = logFilePathProperty = propertyFactory.CreateProperty(LogFilePathPropertyName, Path);
    }

    logEvent.AddPropertyIfAbsent(logFilePathProperty);
  }
}
