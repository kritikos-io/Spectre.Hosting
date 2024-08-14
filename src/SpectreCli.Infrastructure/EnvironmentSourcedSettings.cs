// Copyright (c) Kritikos IO. All rights reserved.

namespace Kritikos.SpectreCli.Infrastructure;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Spectre.Console.Cli;

using ValidationResult = Spectre.Console.ValidationResult;

public abstract class EnvironmentSourcedSettings<TSelf> : CommandSettings
    where TSelf : EnvironmentSourcedSettings<TSelf>
{
  private readonly ILogger logger;

  protected EnvironmentSourcedSettings()
      : this(NullLogger<TSelf>.Instance)
  {
  }

  // ReSharper disable once ContextualLoggerProblem
  protected EnvironmentSourcedSettings(ILogger<TSelf> logger) => this.logger = logger;

  /// <inheritdoc />
  public override ValidationResult Validate()
  {
    var props = this.GetType()
        .GetProperties()
        .Where(x => x.PropertyType == typeof(string));

    foreach (var prop in props)
    {
      var attr = prop.GetCustomAttribute<EnvironmentSourcedAttribute>();
      var isRequired = prop.GetCustomAttribute<RequiredAttribute>() != null;
      var prefix = this.GetType().GetCustomAttribute<EnvironmentSourcePrefixAttribute>()?.Prefix ?? string.Empty;

      if (attr is null)
      {
        continue;
      }

      var key = string.IsNullOrWhiteSpace(attr.VariableName)
          ? $"{prefix}{prop.Name}"
          : $"{prefix}{attr.VariableName}";

      var env = Environment.GetEnvironmentVariable(key);
      if (!string.IsNullOrWhiteSpace(env))
      {
        prop.SetValue(this, env);
      }

      if (prop.GetValue(this) is string || !isRequired)
      {
        continue;
      }

      logger.LogMissingRequiredProperty(prop.Name);
      return ValidationResult.Error($"{prop.Name} is required");
    }

    return ValidationResult.Success();
  }
}
