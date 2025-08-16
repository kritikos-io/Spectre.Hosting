namespace Kritikos.SpectreCli.Hosting.HostBuilderDependencies;

using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

internal sealed class GenericHostTypeResolver(IHost provider)
  : ITypeResolver, IDisposable
{
  private readonly IHost host = provider ?? throw new ArgumentNullException(nameof(provider));

  public object? Resolve(Type? type)
    => type != null
      ? host.Services.GetService(type)
      : null;

  public void Dispose() => host.Dispose();
}
