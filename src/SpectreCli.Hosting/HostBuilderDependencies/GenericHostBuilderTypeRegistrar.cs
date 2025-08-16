namespace Kritikos.SpectreCli.Hosting.HostBuilderDependencies;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

/// <summary>
/// Provides a type registrar that is integrated with the generic <see cref="IHostBuilder"/>, providing deferred service instantiation.
/// </summary>
/// <param name="builder">The <see cref="IHostBuilder"/> to retrieve services from.</param>
public sealed class GenericHostBuilderTypeRegistrar(IHostBuilder builder)
  : ITypeRegistrar
{
  private readonly IHostBuilder builder = builder;
  private ITypeResolver? cachedTypeResolver;

  /// <inheritdoc />
  public void Register(Type service, Type implementation)
    => builder.ConfigureServices((_, services) => services.AddSingleton(service, implementation));

  /// <inheritdoc />
  public void RegisterInstance(Type service, object implementation)
    => builder.ConfigureServices((_, services) => services.AddSingleton(service, implementation));

  /// <inheritdoc />
  public void RegisterLazy(Type service, Func<object> func)
  {
    ArgumentNullException.ThrowIfNull(func);

    builder.ConfigureServices((_, services) => services.AddSingleton(service, _ => func()));
  }

  /// <inheritdoc />
  public ITypeResolver Build()
  {
    cachedTypeResolver ??= new GenericHostTypeResolver(builder.Build());
    return cachedTypeResolver;
  }
}
