namespace Kritikos.CommandLine.Hosting;

using System.Collections.Immutable;
using System.Globalization;

using Kritikos.CommandLine.Logging;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

using Spectre.Console.Cli;

public static class CommandBuilderExtensions
{
  /// <summary>
  /// Registers a <see cref="ICommandAppStartup"/> instance to configure the <see cref="ICommandApp"/> to be created.
  /// </summary>
  /// <param name="builder">The instance of <see cref="CommandBuilder"/> to configure.</param>
  /// <typeparam name="T">The <see cref="ICommandAppStartup"/> to use in configuring.</typeparam>
  /// <returns>The configured <paramref name="builder"/>.</returns>
  public static CommandBuilder UseStartup<T>(this CommandBuilder builder)
    where T : ICommandAppStartup
  {
    ArgumentNullException.ThrowIfNull(builder);
    var startup = typeof(T);

    builder.Services.AddSingleton(typeof(ICommandAppStartup), startup);
    return builder;
  }

  /// <summary>
  /// Registers all <see cref="CommandSettings"/> implementations found in the assembly of <typeparamref name="T"/>.
  /// </summary>
  /// <param name="builder">The instance of <see cref="CommandBuilder"/> to configure.</param>
  /// <typeparam name="T">The <see cref="ICommandAppStartup"/> to load <see cref="CommandSettings"/> from.</typeparam>
  /// <returns>The configured <paramref name="builder"/>.</returns>
  public static CommandBuilder RegisterSettingsFromAssembly<T>(this CommandBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);
    var assembly = typeof(T).Assembly;
    var settings = assembly
      .GetTypes()
      .Where(x => typeof(CommandSettings).IsAssignableFrom(x))
      .ToImmutableList();

    foreach (var setting in settings)
    {
      builder.Services.AddSingleton(setting);
    }

    return builder;
  }

  public static CommandBuilder AddLogging(this CommandBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    var loggerConfiguration = new LoggerConfiguration()
      .MinimumLevel.ControlledBy(LogInterceptor.LogLevel)
      .Enrich.With<LoggingEnricher>()
      .WriteTo.Map(
        LoggingEnricher.LogFilePathPropertyName,
        (logFilePath, wt) => wt.File($"{logFilePath}", formatProvider: CultureInfo.InvariantCulture),
        1);

    builder.Services.AddLogging(configure => configure
      .AddSerilog(loggerConfiguration.CreateLogger()));
    builder.HasConfiguredLogging = true;

    return builder;
  }

  /// <summary>
  /// Creates a <see cref="ICommandApp"/> instance from the <see cref="CommandBuilder"/>.
  /// </summary>
  /// <param name="builder">The <see cref="CommandBuilder"/> to use in creating the application.</param>
  /// <returns>A properly configured <see cref="ICommandApp"/> with dependency injection.</returns>
  /// <remarks>The service provider will be built twice for this.</remarks>
  public static ICommandApp Build(this CommandBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);
    var app = new CommandApp(builder.Registrar);

    // ReSharper disable once ConvertToUsingDeclaration
    using (var services = builder.Services.BuildServiceProvider())
    {
      var startup = services.GetService<ICommandAppStartup>();
      if (startup is null)
      {
        return app;
      }

      startup.ConfigureServices(builder.Services);
      app.Configure(startup.Configure);
    }

    if (builder.HasConfiguredLogging)
    {
      app.Configure(configuration => configuration.SetInterceptor(new LogInterceptor()));
    }

    return app;
  }
}
