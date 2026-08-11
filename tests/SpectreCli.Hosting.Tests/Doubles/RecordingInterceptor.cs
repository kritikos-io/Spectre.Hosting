namespace Kritikos.SpectreCli.Hosting.Tests.Doubles;

using Spectre.Console.Cli;

internal sealed class RecordingInterceptor(string name, List<string> log) : ICommandInterceptor
{
  public void Intercept(CommandContext context, CommandSettings settings)
    => log.Add($"intercept:{name}");

  public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
    => log.Add($"result:{name}");
}
