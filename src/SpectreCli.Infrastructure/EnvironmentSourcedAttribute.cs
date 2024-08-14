namespace Kritikos.SpectreCli.Infrastructure;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EnvironmentSourcedAttribute : Attribute
{
  public EnvironmentSourcedAttribute()
      : this(string.Empty)
  {
  }

  public EnvironmentSourcedAttribute(string variableName) => VariableName = variableName;

  public string? VariableName { get; }
}
