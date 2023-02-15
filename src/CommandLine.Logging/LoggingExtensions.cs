namespace Kritikos.CommandLine.Logging;

using System.Globalization;

using Serilog;
using Serilog.Core;

public static class LoggingExtensions
{
  public static LoggingLevelSwitch LevelSwitch { get; }
    = new LoggingLevelSwitch();

  public static LoggerConfiguration AddSpectreInterception(this LoggerConfiguration loggerConfiguration)
    => (loggerConfiguration ?? throw new ArgumentNullException(nameof(loggerConfiguration)))
      .Enrich.With<LoggingEnricher>()
      .MinimumLevel.ControlledBy(LevelSwitch)
      .WriteTo.Map(
        LoggingEnricher.LogFilePathPropertyName,
        (logFilePath, writer) => writer.File($"{logFilePath}", formatProvider: CultureInfo.InvariantCulture),
        1);
}
