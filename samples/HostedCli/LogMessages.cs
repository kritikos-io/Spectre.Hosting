namespace Kritikos.HostedCli;

using Microsoft.Extensions.Logging;

/// <summary>
/// High-performance log messages for the sample CLI.
/// </summary>
internal static partial class LogMessages
{
  [LoggerMessage(Level = LogLevel.Information, Message = "Greeted {Name} {Count} time(s)")]
  public static partial void GreetingDelivered(this ILogger logger, string name, int count);
}
