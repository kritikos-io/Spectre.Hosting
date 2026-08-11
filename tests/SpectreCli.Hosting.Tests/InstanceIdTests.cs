namespace Kritikos.SpectreCli.Hosting.Tests;

public class InstanceIdTests
{
  // RFC 9562 A.4: UUID v5 of namespace 6ba7b810-9dad-11d1-80b4-00c04fd430c8 over "www.example.com".
  [Test]
  public async Task CreateVersion5_Rfc9562TestVector_MatchesSpecification()
  {
    var result = InstanceId.CreateVersion5(
      Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"),
      "www.example.com");

    await Assert.That(result).IsEqualTo(Guid.Parse("2ed6657d-e927-568b-95e1-2665a8aea6a2"));
  }

  [Test]
  public async Task CreateDeterministic_SameInputs_ReturnsSameGuid()
  {
    var first = InstanceId.CreateDeterministic("svc", "host");
    var second = InstanceId.CreateDeterministic("svc", "host");

    await Assert.That(first).IsEqualTo(second);
  }

  [Test]
  public async Task CreateDeterministic_DifferentMachineName_ReturnsDifferentGuid()
  {
    var first = InstanceId.CreateDeterministic("svc", "host-a");
    var second = InstanceId.CreateDeterministic("svc", "host-b");

    await Assert.That(first).IsNotEqualTo(second);
  }

  [Test]
  public async Task CreateDeterministic_DifferentServiceName_ReturnsDifferentGuid()
  {
    var first = InstanceId.CreateDeterministic("svc-a", "host");
    var second = InstanceId.CreateDeterministic("svc-b", "host");

    await Assert.That(first).IsNotEqualTo(second);
  }

  [Test]
  public async Task CreateDeterministic_AnyInput_SetsVersion5AndRfcVariant()
  {
    var bytes = InstanceId.CreateDeterministic("svc", "host").ToByteArray();

    // Version nibble and variant bits live in the big-endian positions 6 and 8.
    await Assert.That(bytes[7] >> 4).IsEqualTo(5);
    await Assert.That(bytes[8] & 0xC0).IsEqualTo(0x80);
  }

  [Test]
  public async Task CreateDeterministic_UsesMachineName_MatchesExplicitOverload()
  {
    var implicitMachine = InstanceId.CreateDeterministic("svc");
    var explicitMachine = InstanceId.CreateDeterministic("svc", Environment.MachineName);

    await Assert.That(implicitMachine).IsEqualTo(explicitMachine);
  }

  [Test]
  [Arguments(null)]
  [Arguments("")]
  [Arguments("   ")]
  public async Task CreateDeterministic_MissingServiceName_Throws(string? serviceName)
    => await Assert.That(() => InstanceId.CreateDeterministic(serviceName!))
      .Throws<ArgumentException>();

  [Test]
  [Arguments(null)]
  [Arguments("")]
  [Arguments("   ")]
  public async Task CreateDeterministic_MissingMachineName_Throws(string? machineName)
    => await Assert.That(() => InstanceId.CreateDeterministic("svc", machineName!))
      .Throws<ArgumentException>();
}
