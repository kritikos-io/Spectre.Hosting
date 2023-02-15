namespace Kritikos.CommandLine.Hosting;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

internal sealed class TypeRegistrar : ITypeRegistrar
{
  private readonly IServiceCollection provider;

  public TypeRegistrar(IServiceCollection provider)
    => this.provider = provider;

  public ITypeResolver Build()
    => new TypeResolver(provider.BuildServiceProvider());

  public void Register(Type service, Type implementation)
    => provider.AddSingleton(service, implementation);

  public void RegisterInstance(Type service, object implementation)
    => provider.AddSingleton(service, implementation);

  public void RegisterLazy(Type service, Func<object> factory)
    => provider.AddSingleton(service, _ => factory());
}
