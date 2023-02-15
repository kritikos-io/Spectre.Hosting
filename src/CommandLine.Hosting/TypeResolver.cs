namespace Kritikos.CommandLine.Hosting;

using System;

using Spectre.Console.Cli;

internal sealed class TypeResolver : ITypeResolver
{
  private readonly IServiceProvider provider;

  public TypeResolver(IServiceProvider provider)
    => this.provider = provider;

  public object? Resolve(Type? type)
    => type == null
      ? null
      : provider.GetService(type);
}
