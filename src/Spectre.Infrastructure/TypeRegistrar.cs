namespace Kritikos.CommandLine.Hosting;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

internal sealed class TypeRegistrar(IServiceCollection services)
  : ITypeRegistrar, IDisposable
{
  private readonly List<IDisposable> builtProviders = new();

  public ITypeResolver Build()
  {
    var provider = services.BuildServiceProvider();
    builtProviders.Add(provider);
    return new TypeResolver(provider);
  }

  public void Register(Type service, Type implementation)
    => services.AddSingleton(service, implementation);

  public void RegisterInstance(Type service, object implementation)
    => services.AddSingleton(service, implementation);

  public void RegisterLazy(Type service, Func<object> factory)
    => services.AddSingleton(service, _ => factory());

  /// <inheritdoc />
  public void Dispose()
  {
    foreach (var disposable in builtProviders)
    {
      disposable.Dispose();
    }
  }
}
