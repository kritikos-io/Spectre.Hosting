namespace Kritikos.Spectre.Infrastructure;

using System;

using global::Spectre.Console.Cli;

using Microsoft.Extensions.DependencyInjection;

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
