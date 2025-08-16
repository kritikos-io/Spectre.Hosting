#pragma warning disable CA1819
namespace Kritikos.SpectreCli.Hosting.HostBuilderDependencies;

using Spectre.Console.Cli;

public class CommandAppWrapper
{
  public string[] Args { get; set; } = [];

  public ICommandApp? App { get; set; }
}
