namespace Kritikos.CommandLine.Logging;

using System.ComponentModel;

using Spectre.Console.Cli;

public abstract class LogCommandSettings : CommandSettings
{
  [CommandOption("--logFile")]
  [Description("Path to log file [[application.log]]")]
  [DefaultValue("application.log")]
  public string LogFile { get; set; } = "application.log";

  [CommandOption("--verbose")]
  [DefaultValue(false)]
  [Description("Enable debug logging [[false]]")]
  public bool? Verbose { get; set; }
}
