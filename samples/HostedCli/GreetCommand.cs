namespace Kritikos.HostedCli;

using System.ComponentModel;
using System.Net.Http;

using Microsoft.Extensions.Logging;

using Spectre.Console;
using Spectre.Console.Cli;

/// <summary>
/// A sample command that greets a user, demonstrating host-injected services.
/// </summary>
/// <param name="logger">The logger instance provided by the host.</param>
/// <param name="httpClientFactory">The HTTP client factory provided by the host.</param>
internal sealed class GreetCommand(ILogger<GreetCommand> logger, IHttpClientFactory httpClientFactory) : AsyncCommand<GreetCommand.Settings>
{
  /// <inheritdoc/>
  protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellation)
  {
    using var activity = Telemetry.ActivitySource.StartActivity("Greet");
    activity?.SetTag("greet.name", settings.Name);
    activity?.SetTag("greet.count", settings.Count);

    for (var i = 0; i < settings.Count; i++)
    {
      AnsiConsole.MarkupLine($"[green]Hello, {settings.Name.EscapeMarkup()}![/]");
    }

    using var client = httpClientFactory.CreateClient();
    using var response = await client.GetAsync(new Uri("https://httpbin.org/get"), cancellation).ConfigureAwait(false);
    AnsiConsole.MarkupLine($"[blue]HTTP {(int)response.StatusCode}[/] from httpbin.org");

    Telemetry.GreetingsCounter.Add(settings.Count, new KeyValuePair<string, object?>("name", settings.Name));
    logger.GreetingDelivered(settings.Name, settings.Count);
    return 0;
  }

  /// <summary>Command-line settings for <see cref="GreetCommand"/>.</summary>
  internal sealed class Settings : CommandSettings
  {
    /// <summary>Gets the name to greet.</summary>
    [CommandArgument(0, "<name>")]
    [Description("The name to greet")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the number of times to repeat the greeting.</summary>
    [CommandOption("-c|--count")]
    [Description("Number of times to greet")]
    [DefaultValue(1)]
    public int Count { get; init; } = 1;
  }
}
