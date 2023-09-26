namespace Kritikos.CommandLine.Logging;

using Serilog.Core;
using Serilog.Events;

public class LoggingEnricher : ILogEventEnricher
{
  public const string LogFilePathPropertyName = "LogFilePath";

  private string _cachedLogFilePath;
  private LogEventProperty? _cachedLogFilePathProperty;

  public static string Path { get; } = string.Empty;

  /// <inheritdoc />
  public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
  {
    ArgumentNullException.ThrowIfNull(logEvent);
    ArgumentNullException.ThrowIfNull(propertyFactory);

    LogEventProperty logFilePathProperty;

    if (_cachedLogFilePathProperty != null && Path.Equals(_cachedLogFilePath, StringComparison.Ordinal))
    {
      logFilePathProperty = _cachedLogFilePathProperty;
    }
    else
    {
      _cachedLogFilePath = Path;
      _cachedLogFilePathProperty = logFilePathProperty = propertyFactory.CreateProperty(LogFilePathPropertyName, Path);
    }

    logEvent.AddPropertyIfAbsent(logFilePathProperty);
  }
}
