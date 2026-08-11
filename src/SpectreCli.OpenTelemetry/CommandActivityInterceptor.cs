namespace Kritikos.SpectreCli.OpenTelemetry;

using System.Diagnostics;

using Kritikos.SpectreCli.Hosting;

using Spectre.Console.Cli;

/// <summary>
/// A <see cref="ICommandInterceptor"/> that automatically creates
/// <see cref="Activity"/> spans and records metrics for every command execution,
/// including runs that terminate with an exception.
/// </summary>
internal sealed class CommandActivityInterceptor : ICommandInterceptor, ICommandExecutionObserver
{
  private long startTimestamp;
  private Activity? activity;
  private CommandContext? context;
  private CommandSettings? settings;
  private bool completed;

  /// <inheritdoc/>
  public void Intercept(CommandContext context, CommandSettings settings)
  {
    startTimestamp = Stopwatch.GetTimestamp();
    this.context = context;
    this.settings = settings;

    var commandType = ResolveCommandType(settings);

    activity = SpectreCliInstrumentation.ActivitySource.StartActivity(
      commandType.Name,
      ActivityKind.Internal);

    activity?.SetTag("spectre.command.name", context.Name);
    activity?.SetTag("spectre.command.type", commandType.FullName);
  }

  /// <inheritdoc/>
  public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
    => Complete(context, settings, result, exception: null);

  /// <inheritdoc/>
  public void OnCommandFailed(Exception exception, int exitCode)
  {
    ArgumentNullException.ThrowIfNull(exception);

    // A failure before Intercept ran (argument parsing, configuration) has no command to report on.
    if (context is null || settings is null)
    {
      return;
    }

    Complete(context, settings, exitCode, exception);
  }

  private static Type ResolveCommandType(CommandSettings settings)
    => settings.GetType().DeclaringType ?? settings.GetType();

  private void Complete(CommandContext context, CommandSettings settings, int result, Exception? exception)
  {
    if (completed)
    {
      return;
    }

    completed = true;

    var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
    var errorType = exception?.GetType().FullName;
    var commandType = ResolveCommandType(settings);

    activity?.SetTag("spectre.command.exit_code", result);

    if (exception is not null)
    {
      activity?.SetTag("error.type", errorType);
      activity?.AddEvent(new ActivityEvent(
        "exception",
        tags: new ActivityTagsCollection
        {
          { "exception.type", errorType },
          { "exception.message", exception.Message },
          { "exception.stacktrace", exception.ToString() },
        }));
      activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
    }
    else if (result != 0)
    {
      activity?.SetStatus(ActivityStatusCode.Error, $"Command exited with code {result}");
    }
    else
    {
      activity?.SetStatus(ActivityStatusCode.Ok);
    }

    activity?.Stop();

    var tags = new TagList
    {
      { "spectre.command.name", context.Name },
      { "spectre.command.type", commandType.Name },
      { "spectre.command.exit_code", result },
    };

    if (errorType is not null)
    {
      tags.Add("error.type", errorType);
    }

    SpectreCliInstrumentation.CommandDuration.Record(elapsed.TotalSeconds, tags);
    SpectreCliInstrumentation.CommandExecutions.Add(1, tags);
  }
}
