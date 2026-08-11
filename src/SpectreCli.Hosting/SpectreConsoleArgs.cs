namespace Kritikos.SpectreCli.Hosting;

/// <summary>
/// Holds the command-line arguments to be forwarded to the Spectre <see cref="Spectre.Console.Cli.ICommandApp"/>.
/// </summary>
/// <param name="args">The raw command-line arguments.</param>
internal sealed class SpectreConsoleArgs(string[] args)
{
  /// <summary>Gets the raw command-line arguments.</summary>
  public string[] Args { get; } = args;
}
