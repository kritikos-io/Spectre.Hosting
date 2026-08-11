namespace Kritikos.SpectreCli.Hosting;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

/// <summary>
/// Bridges <see cref="IServiceProvider"/> to Spectre's <see cref="ITypeResolver"/>.
/// Resolves instances, lazy factories, and type registrations accumulated by
/// <see cref="TypeRegistrar"/>, falling back to the host's service provider.
/// </summary>
/// <param name="hostProvider">The host's service provider.</param>
/// <param name="registrations">Type-to-type mappings registered by Spectre.</param>
/// <param name="instances">Pre-built instances registered by Spectre.</param>
/// <param name="factories">Lazy factories registered by Spectre.</param>
internal sealed class TypeResolver(
  IServiceProvider hostProvider,
  Dictionary<Type, Type> registrations,
  Dictionary<Type, object> instances,
  Dictionary<Type, Func<object>> factories) : ITypeResolver
{
  /// <inheritdoc/>
  public object? Resolve(Type? type)
  {
    if (type is null)
    {
      return null;
    }

    if (instances.TryGetValue(type, out var instance))
    {
      return instance;
    }

    if (factories.TryGetValue(type, out var factory))
    {
      return factory();
    }

    if (registrations.TryGetValue(type, out var implementation))
    {
      return ActivatorUtilities.CreateInstance(hostProvider, implementation);
    }

    return hostProvider.GetService(type);
  }
}
