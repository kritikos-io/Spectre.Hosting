﻿namespace Kritikos.Spectre.Infrastructure;

using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;

using global::Spectre.Console.Cli;

using Kritikos.Spectre.Logging;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

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
  public static CommandBuilder RegisterSettingsFromAssemblyContaining<T>(this CommandBuilder builder)
  {
    ArgumentNullException.ThrowIfNull(builder);
    var assembly = typeof(T).Assembly;

    return builder.RegisterSettingsFromAssembly(assembly);
  }

  /// <summary>
  /// Registers all &lt;see cref="CommandSettings"/&gt; implementations found in the <paramref name="assembly"/>.
  /// </summary>
  /// <param name="builder">The instance of <see cref="CommandBuilder"/> to configure.</param>
  /// <param name="assembly">The <see cref="Assembly"/> containing wanted <see cref="CommandSettings"/>.</param>
  /// <returns>The configured <paramref name="builder"/>.</returns>
  public static CommandBuilder RegisterSettingsFromAssembly(this CommandBuilder builder, Assembly assembly)
  {
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

  /// <summary>
  /// Adds logging to the <see cref="ICommandApp"/> instance.
  /// </summary>
  /// <param name="builder">The instance of <see cref="CommandBuilder"/> to configure.</param>
  /// <returns>The configured <paramref name="builder"/>.</returns>
  public static CommandBuilder AddFileLogging(this CommandBuilder builder)
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
