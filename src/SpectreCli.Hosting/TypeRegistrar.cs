namespace Kritikos.SpectreCli.Hosting;

using Spectre.Console.Cli;

/// <summary>
/// Bridges the generic host's DI container with Spectre's <see cref="ITypeRegistrar"/>.
/// Accumulates Spectre-internal registrations and delegates type construction
/// to the host's <see cref="IServiceProvider"/> via <c>ActivatorUtilities</c>.
/// </summary>
/// <param name="hostProvider">The host's service provider used for dependency resolution.</param>
internal sealed class TypeRegistrar(IServiceProvider hostProvider) : ITypeRegistrar
{
  private readonly Dictionary<Type, Type> registrations = [];
  private readonly Dictionary<Type, object> instances = [];
  private readonly Dictionary<Type, Func<object>> factories = [];

  /// <inheritdoc/>
  public ITypeResolver Build()
    => new TypeResolver(hostProvider, registrations, instances, factories);

  /// <inheritdoc/>
  public void Register(Type service, Type implementation)
    => registrations[service] = implementation;

  /// <inheritdoc/>
  public void RegisterInstance(Type service, object implementation)
    => instances[service] = implementation;

  /// <inheritdoc/>
  public void RegisterLazy(Type service, Func<object> factory)
    => factories[service] = factory;
}
