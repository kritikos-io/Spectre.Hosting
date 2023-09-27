namespace Kritikos.SpectreCli.Infrastructure;

using System;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

internal sealed class TypeResolver(ServiceProvider provider)
  : ITypeResolver, IDisposable
{
  public object? Resolve(Type? type)
    => type == null
      ? null
      : provider.GetService(type);

  /// <inheritdoc />
  public void Dispose()
    => provider.Dispose();
}
