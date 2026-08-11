namespace Kritikos.SpectreCli.Hosting;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

/// <summary>
/// Bridges <see cref="IServiceProvider"/> to Spectre's <see cref="ITypeResolver"/>.
/// Resolves instances, lazy factories, and type registrations accumulated by
/// <see cref="TypeRegistrar"/>, falling back to the owned service scope.
/// </summary>
/// <remarks>
/// Owns the <see cref="IServiceScope"/> it resolves from; Spectre disposes the resolver once the
/// command run completes, tearing the scope down with it. Instances constructed here through
/// <see cref="ActivatorUtilities"/> are not tracked by the container, so this type tracks and
/// disposes them itself. Spectre's disposal path is synchronous, so only <see cref="IDisposable"/>
/// is honoured. Instances and factories registered by Spectre remain owned by Spectre.
/// </remarks>
/// <param name="scope">The service scope owned by this resolver.</param>
/// <param name="registrations">Type-to-type mappings registered by Spectre.</param>
/// <param name="instances">Pre-built instances registered by Spectre.</param>
/// <param name="factories">Lazy factories registered by Spectre.</param>
internal sealed class TypeResolver(
  IServiceScope scope,
  Dictionary<Type, Type> registrations,
  Dictionary<Type, object> instances,
  Dictionary<Type, Func<object>> factories) : ITypeResolver, IDisposable
{
  private readonly List<IDisposable> tracked = [];

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
      var created = ActivatorUtilities.CreateInstance(scope.ServiceProvider, implementation);
      if (created is IDisposable createdDisposable)
      {
        tracked.Add(createdDisposable);
      }

      return created;
    }

    return scope.ServiceProvider.GetService(type);
  }

  /// <inheritdoc/>
  public void Dispose()
  {
    for (var i = tracked.Count - 1; i >= 0; i--)
    {
      tracked[i].Dispose();
    }

    tracked.Clear();
    scope.Dispose();
  }
}
