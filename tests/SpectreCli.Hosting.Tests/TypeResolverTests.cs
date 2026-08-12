namespace Kritikos.SpectreCli.Hosting.Tests;

using Kritikos.SpectreCli.Hosting.Tests.Doubles;

using Microsoft.Extensions.DependencyInjection;

public class TypeResolverTests
{
  [Test]
  public async Task Resolve_NullType_ReturnsNull()
  {
    using var provider = new ServiceCollection().BuildServiceProvider();
    using var resolver = Build(provider);

    await Assert.That(resolver.Resolve(null)).IsNull();
  }

  [Test]
  public async Task Resolve_RegisteredInstance_ReturnsTheSameInstance()
  {
    using var provider = new ServiceCollection().BuildServiceProvider();
    var registrar = Registrar(provider);
    var instance = new ScopedProbe();
    registrar.RegisterInstance(typeof(ScopedProbe), instance);

    using var resolver = (TypeResolver)registrar.Build();

    await Assert.That(resolver.Resolve(typeof(ScopedProbe))).IsSameReferenceAs(instance);
  }

  [Test]
  public async Task Resolve_RegisteredLazyFactory_InvokesTheFactory()
  {
    using var provider = new ServiceCollection().BuildServiceProvider();
    var registrar = Registrar(provider);
    var invocations = 0;
    registrar.RegisterLazy(typeof(ScopedProbe), () =>
    {
      invocations++;
      return new ScopedProbe();
    });

    using var resolver = (TypeResolver)registrar.Build();
    resolver.Resolve(typeof(ScopedProbe));

    await Assert.That(invocations).IsEqualTo(1);
  }

  [Test]
  public async Task Resolve_RegisteredType_ConstructsViaActivatorUtilities()
  {
    using var provider = new ServiceCollection().BuildServiceProvider();
    var registrar = Registrar(provider);
    registrar.Register(typeof(ScopedProbe), typeof(ScopedProbe));

    using var resolver = (TypeResolver)registrar.Build();

    await Assert.That(resolver.Resolve(typeof(ScopedProbe))).IsTypeOf<ScopedProbe>();
  }

  [Test]
  public async Task Resolve_UnregisteredType_FallsBackToTheHostContainer()
  {
    var services = new ServiceCollection();
    var expected = new ExecutionProbe();
    services.AddSingleton(expected);
    using var provider = services.BuildServiceProvider();

    using var resolver = Build(provider);

    await Assert.That(resolver.Resolve(typeof(ExecutionProbe))).IsSameReferenceAs(expected);
  }

  [Test]
  public async Task Resolve_UnknownType_ReturnsNull()
  {
    using var provider = new ServiceCollection().BuildServiceProvider();
    using var resolver = Build(provider);

    await Assert.That(resolver.Resolve(typeof(ExecutionProbe))).IsNull();
  }

  [Test]
  public async Task Resolve_ScopedDependency_ResolvesWithoutPromotingToSingleton()
  {
    var services = new ServiceCollection();
    services.AddScoped<ScopedProbe>();
    using var provider = services.BuildServiceProvider(new ServiceProviderOptions
    {
      ValidateScopes = true,
    });

    var registrar = Registrar(provider);
    using var first = (TypeResolver)registrar.Build();
    using var second = (TypeResolver)registrar.Build();

    // Distinct scopes must hand out distinct instances.
    await Assert.That(first.Resolve(typeof(ScopedProbe)))
      .IsNotSameReferenceAs(second.Resolve(typeof(ScopedProbe)));
  }

  [Test]
  public async Task Dispose_ScopedDependency_DisposesTheOwnedScope()
  {
    var services = new ServiceCollection();
    services.AddScoped<ScopedProbe>();
    using var provider = services.BuildServiceProvider();

    var resolver = Build(provider);
    var probe = (ScopedProbe)resolver.Resolve(typeof(ScopedProbe))!;
    resolver.Dispose();

    await Assert.That(probe.Disposed).IsTrue();
  }

  [Test]
  public async Task Dispose_TypeRegisteredWithSpectre_DisposesTheActivatedInstance()
  {
    using var provider = new ServiceCollection().BuildServiceProvider();
    var registrar = Registrar(provider);
    registrar.Register(typeof(ScopedProbe), typeof(ScopedProbe));

    var resolver = (TypeResolver)registrar.Build();
    var probe = (ScopedProbe)resolver.Resolve(typeof(ScopedProbe))!;
    resolver.Dispose();

    await Assert.That(probe.Disposed).IsTrue();
  }

  [Test]
  public async Task Dispose_CalledTwice_DoesNotThrow()
  {
    using var provider = new ServiceCollection().BuildServiceProvider();
    var resolver = Build(provider);

    resolver.Dispose();

    await Assert.That(resolver.Dispose).ThrowsNothing();
  }

  [Test]
  public async Task Build_CalledTwice_ReturnsIndependentResolvers()
  {
    using var provider = new ServiceCollection().BuildServiceProvider();
    var registrar = Registrar(provider);

    using var first = (TypeResolver)registrar.Build();
    using var second = (TypeResolver)registrar.Build();

    await Assert.That(first).IsNotSameReferenceAs(second);
  }

  [Test]
  public async Task Build_RegistrationAddedAfterwards_DoesNotAffectAnExistingResolver()
  {
    using var provider = new ServiceCollection().BuildServiceProvider();
    var registrar = Registrar(provider);

    using var resolver = (TypeResolver)registrar.Build();
    registrar.RegisterInstance(typeof(ExecutionProbe), new ExecutionProbe());

    await Assert.That(resolver.Resolve(typeof(ExecutionProbe))).IsNull();
  }

  private static TypeRegistrar Registrar(IServiceProvider provider)
    => new(provider.GetRequiredService<IServiceScopeFactory>());

  private static TypeResolver Build(IServiceProvider provider)
    => (TypeResolver)Registrar(provider).Build();
}
