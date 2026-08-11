namespace Kritikos.SpectreCli.Hosting;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

/// <summary>
/// Bridges the generic host's DI container with Spectre's <see cref="ITypeRegistrar"/>.
/// Accumulates Spectre-internal registrations and delegates type construction
/// to a dedicated <see cref="IServiceScope"/> created for each command run.
/// </summary>
/// <param name="scopeFactory">The host's scope factory used to create the per-run scope.</param>
internal sealed class TypeRegistrar(IServiceScopeFactory scopeFactory) : ITypeRegistrar
{
  private readonly Dictionary<Type, Type> registrations = [];
  private readonly Dictionary<Type, object> instances = [];
  private readonly Dictionary<Type, Func<object>> factories = [];

  /// <inheritdoc/>
  /// <remarks>Spectre calls this once per run and disposes the returned resolver, which owns the scope.</remarks>
  public ITypeResolver Build()
    => new TypeResolver(scopeFactory.CreateScope(), registrations, instances, factories);

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
