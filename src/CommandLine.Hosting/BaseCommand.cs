// ReSharper disable ContextualLoggerProblem - CRT Pattern requirement
// ReSharper disable MemberCanBePrivate.Global - Required for consumers
// ReSharper disable SuggestBaseTypeForParameterInConstructor - Contextual Logging requirement
// ReSharper disable UnusedAutoPropertyAccessor.Global - Required for consumers

#pragma warning disable SA1402
namespace Kritikos.CommandLine.Hosting;

using Microsoft.Extensions.Logging;

using Spectre.Console.Cli;

public abstract class BaseCommand<TCommand, TSettings> : Command<TSettings>
  where TCommand : BaseCommand<TCommand, TSettings>
  where TSettings : CommandSettings
{
  protected BaseCommand(ILogger<TCommand> logger) => Logger = logger;

  protected ILogger Logger { get; }
}

public abstract class BaseCommand<TCommand> : BaseCommand<TCommand, EmptyCommandSettings>
  where TCommand : BaseCommand<TCommand>
{
  protected BaseCommand(ILogger<TCommand> logger)
    : base(logger)
  {
  }
}
