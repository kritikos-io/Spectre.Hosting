namespace Kritikos.SpectreCli.Infrastructure;

[AttributeUsage(AttributeTargets.Class)]
public sealed class EnvironmentSourcePrefixAttribute : Attribute
{
  public EnvironmentSourcePrefixAttribute(string prefix) => Prefix = prefix;

  public string Prefix { get; }
}
