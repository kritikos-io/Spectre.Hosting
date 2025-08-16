namespace Kritikos.SpectreCli.Hosting.EnvironmentSettings;

using System.ComponentModel;

using Spectre.Console.Cli;

public sealed class EnvironmentVariableAttribute(string name) : ParameterValueProviderAttribute
{
  private readonly string name = name;

  /// <inheritdoc />
  public override bool TryGetValue(CommandParameterContext context, out object? result)
  {
    result = null;
    if (context.Value is not null)
    {
      return false;
    }

    var targetType = context.Parameter.ParameterType;
    var envValue = Environment.GetEnvironmentVariable(name);

    if (targetType == typeof(string))
    {
      result = envValue;
      return result != null;
    }

    var converter = TypeDescriptor.GetConverter(targetType);
    if (!converter.CanConvertFrom(context.Parameter.ParameterType) || string.IsNullOrWhiteSpace(envValue))
    {
      return false;
    }

    result = converter.ConvertFrom(envValue);
    return result != null;
  }
}
