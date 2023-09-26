namespace Kritikos.Spectre.Logging;

using global::Spectre.Console.Cli;

using Serilog.Core;
using Serilog.Events;

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
