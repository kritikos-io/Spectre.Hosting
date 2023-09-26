namespace Kritikos.CommandLine.Logging;

using Serilog.Core;
using Serilog.Events;

using Spectre.Console.Cli;

public class LogInterceptor : ICommandInterceptor
{
  public static readonly LoggingLevelSwitch LogLevel = new();

  /// <inheritdoc />
  public virtual void Intercept(CommandContext context, CommandSettings settings)
  {
    if (settings is not LogCommandSettings logCommandSettings)
    {
      return;
    }

    LogLevel.MinimumLevel = logCommandSettings.Verbose
      ? LogEventLevel.Verbose
      : LogEventLevel.Warning;
  }
}
