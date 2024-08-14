namespace Kritikos.SpectreCli.Infrastructure;

using Microsoft.Extensions.Logging;

internal static partial class LoggerExtensions
{
  private const string MissingRequiredPropertyMessage = "Missing required property {PropertyName}";
}

internal static partial class LoggerExtensions
{
  [LoggerMessage(LogLevel.Critical, MissingRequiredPropertyMessage)]
  public static partial void LogMissingRequiredProperty(this ILogger logger, string propertyName);
}
