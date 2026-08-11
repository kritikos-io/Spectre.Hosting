namespace Kritikos.SpectreCli.OpenTelemetry;

using System.Diagnostics;

using Spectre.Console.Cli;

/// <summary>
/// A <see cref="ICommandInterceptor"/> that automatically creates
/// <see cref="Activity"/> spans and records metrics for every command execution.
/// </summary>
internal sealed class CommandActivityInterceptor : ICommandInterceptor
{
  private long startTimestamp;

  /// <inheritdoc/>
  public void Intercept(CommandContext context, CommandSettings settings)
  {
    startTimestamp = Stopwatch.GetTimestamp();

    var commandType = settings.GetType().DeclaringType ?? settings.GetType();

    var activity = SpectreCliInstrumentation.ActivitySource.StartActivity(
      commandType.Name,
      ActivityKind.Internal);

    activity?.SetTag("spectre.command.name", context.Name);
    activity?.SetTag("spectre.command.type", commandType.FullName);
  }

  /// <inheritdoc/>
  public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
  {
    var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
    var activity = Activity.Current;

    activity?.SetTag("spectre.command.exit_code", result);

    if (result != 0)
    {
      activity?.SetStatus(ActivityStatusCode.Error, $"Command exited with code {result}");
    }
    else
    {
      activity?.SetStatus(ActivityStatusCode.Ok);
    }

    activity?.Stop();

    var commandType = settings.GetType().DeclaringType ?? settings.GetType();

    var tags = new TagList
    {
      { "spectre.command.name", context.Name },
      { "spectre.command.type", commandType.Name },
      { "spectre.command.exit_code", result },
    };

    SpectreCliInstrumentation.CommandDuration.Record(elapsed.TotalSeconds, tags);
    SpectreCliInstrumentation.CommandExecutions.Add(1, tags);
  }
}
