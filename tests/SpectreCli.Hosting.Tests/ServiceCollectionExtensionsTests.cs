namespace Kritikos.SpectreCli.Hosting.Tests;

using Kritikos.SpectreCli.Hosting.Tests.Doubles;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class ServiceCollectionExtensionsTests
{
  [Test]
  public async Task AddSpectreConsole_CalledTwice_RegistersASingleHostedService()
  {
    var services = new ServiceCollection();

    services.AddSpectreConsole<SucceedingCommand>(args: []);
    services.AddSpectreConsole<SucceedingCommand>(args: []);

    await Assert.That(services.Count(d => d.ServiceType == typeof(IHostedService))).IsEqualTo(1);
  }

  [Test]
  public async Task AddSpectreConsole_CalledTwice_RegistersASingleCommandApp()
  {
    var services = new ServiceCollection();

    services.AddSpectreConsole<SucceedingCommand>(args: []);
    services.AddSpectreConsole<SucceedingCommand>(args: []);

    await Assert.That(services.Count(d => d.ServiceType == typeof(Spectre.Console.Cli.ICommandApp)))
      .IsEqualTo(1);
  }

  [Test]
  public async Task AddSpectreConsole_CalledTwice_KeepsTheFirstArguments()
  {
    var services = new ServiceCollection();

    services.AddSpectreConsole<SucceedingCommand>(["first"]);
    services.AddSpectreConsole<SucceedingCommand>(["second"]);

    using var provider = services.BuildServiceProvider();
    await Assert.That(provider.GetRequiredService<SpectreConsoleArgs>().Args).IsEquivalentTo(["first"]);
  }

  [Test]
  public async Task AddSpectreConsole_NullArgs_Throws()
  {
    var services = new ServiceCollection();

    await Assert.That(() => services.AddSpectreConsole<SucceedingCommand>(args: null!))
      .Throws<ArgumentNullException>();
  }

  [Test]
  public async Task AddSpectreConsole_NullConfigure_Throws()
  {
    var services = new ServiceCollection();

    await Assert.That(() => services.AddSpectreConsole(args: [], configure: null!))
      .Throws<ArgumentNullException>();
  }
}
