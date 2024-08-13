#pragma warning disable SA1402
namespace Kritikos.SpectreCli.Infrastructure;

using Spectre.Console.Cli;

/// <summary>
/// Base class for an asynchronous command with cancellation support.
/// </summary>
/// <typeparam name="TCommandSettings">The settings type.</typeparam>
public abstract class CancellableAsyncCommand<TCommandSettings> : AsyncCommand<TCommandSettings>
  where TCommandSettings : CommandSettings
{
  /// <summary>
  /// Executes the command.
  /// </summary>
  /// <param name="context">The command context.</param>
  /// <param name="settings">The settings.</param>
  /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
  /// <returns>An integer indicating whether or not the command executed successfully.</returns>
  public abstract Task<int> ExecuteAsync(
    CommandContext context,
    TCommandSettings settings,
    CancellationToken cancellationToken);

  /// <inheritdoc />
  public override async Task<int> ExecuteAsync(CommandContext context, TCommandSettings settings)
  {
    using var cancellationSource = new PosixCancellationTokenSource();
    var cancellable = ExecuteAsync(context, settings, cancellationSource.Token);
    return await cancellable;
  }
}

/// <summary>
/// Base class for an asynchronous command with cancellation support.
/// </summary>
public abstract class CancellableAsyncCommand : CancellableAsyncCommand<EmptyCommandSettings>
{
  /// <summary>
  /// Executes the command.
  /// </summary>
  /// <param name="context">The command context.</param>
  /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
  /// <returns>An integer indicating whether or not the command executed successfully.</returns>
  public abstract Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken);

  /// <inheritdoc />
  public override Task<int> ExecuteAsync(
    CommandContext context,
    EmptyCommandSettings settings,
    CancellationToken cancellationToken)
    => ExecuteAsync(context, cancellationToken);
}
