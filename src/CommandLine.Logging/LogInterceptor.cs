namespace Kritikos.CommandLine.Logging;

using Serilog.Events;

using Spectre.Console.Cli;

public class LogInterceptor : ICommandInterceptor
{
  /// <inheritdoc />
  public virtual void Intercept(CommandContext context, CommandSettings settings)
  {
    if (settings is not LogCommandSettings logCommandSettings)
    {
      return;
    }

    if (logCommandSettings.Verbose ?? false)
    {
      LoggingExtensions.LevelSwitch.MinimumLevel = LogEventLevel.Debug;
    }
  }
}
