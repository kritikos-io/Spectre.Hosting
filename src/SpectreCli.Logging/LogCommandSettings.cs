namespace Kritikos.SpectreCli.Logging;

using System.ComponentModel;

using Spectre.Console.Cli;

public abstract class LogCommandSettings : CommandSettings
{
  [CommandOption("--logFile")]
  [Description("Path to log file [application.log]")]
  [DefaultValue("")]
  public string LogFile { get; set; } = string.Empty;

  [CommandOption("--verbose")]
  [DefaultValue(false)]
  [Description("Enable debug logging [false]")]
  public bool Verbose { get; set; } = false;
}
