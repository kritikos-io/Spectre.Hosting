namespace Kritikos.CommandLine.Logging;

using Serilog.Core;
using Serilog.Events;

public class LoggingEnricher : ILogEventEnricher
{
  public const string LogFilePathPropertyName = "LogFilePath";
  private string? cachedLogFilePath;
  private LogEventProperty? cachedLogFilePathProperty;

  public static string Path { get; internal set; } = string.Empty;

  /// <inheritdoc />
  public void Enrich(LogEvent? logEvent, ILogEventPropertyFactory propertyFactory)
  {
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

    logEvent?.AddPropertyIfAbsent(logFilePathProperty);
  }
}
