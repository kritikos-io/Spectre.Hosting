namespace Kritikos.SpectreCli.Hosting;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Generates deterministic service instance identifiers using UUID v5 (RFC 9562).
/// Produces the same identifier for a given service name on the same machine,
/// ensuring stable telemetry identity across restarts.
/// </summary>
public static class InstanceId
{
  /// <summary>
  /// A well-known namespace UUID used as the base for generating service instance identifiers.
  /// </summary>
  private static readonly Guid Namespace =
    Guid.Parse("8b1e2c3d-4f5a-6b7c-9d0e-f1a2b3c4d5e6");

  /// <summary>
  /// Creates a deterministic UUID v5 from the given <paramref name="serviceName"/>
  /// and the current machine name.
  /// </summary>
  /// <param name="serviceName">The service name to include in the identifier.</param>
  /// <returns>A stable, deterministic <see cref="Guid"/> unique to the service and host.</returns>
  public static Guid CreateDeterministic(string serviceName)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
    return CreateDeterministic(serviceName, Environment.MachineName);
  }

  /// <summary>
  /// Creates a deterministic UUID v5 from the given <paramref name="serviceName"/>
  /// and <paramref name="machineName"/>.
  /// </summary>
  /// <param name="serviceName">The service name to include in the identifier.</param>
  /// <param name="machineName">The machine name to include in the identifier.</param>
  /// <returns>A stable, deterministic <see cref="Guid"/> unique to the service and host combination.</returns>
  public static Guid CreateDeterministic(string serviceName, string machineName)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
    ArgumentException.ThrowIfNullOrWhiteSpace(machineName);
    return CreateVersion5(Namespace, $"{serviceName}:{machineName}");
  }

  /// <summary>
  /// Creates a UUID v5 (SHA-1 name-based) per RFC 9562.
  /// </summary>
  private static Guid CreateVersion5(Guid namespaceId, string name)
  {
    var namespaceBytes = namespaceId.ToByteArray();
    SwapGuidByteOrder(namespaceBytes);

    var nameBytes = Encoding.UTF8.GetBytes(name);

    Span<byte> hash = stackalloc byte[20];
    var inputLength = namespaceBytes.Length + nameBytes.Length;
    var input = inputLength <= 256
      ? stackalloc byte[inputLength]
      : new byte[inputLength];

    namespaceBytes.CopyTo(input);
    nameBytes.CopyTo(input[namespaceBytes.Length..]);

#pragma warning disable CA5350 // SHA-1 is mandated by the UUID v5 specification (RFC 9562) and is not used for cryptographic security here.
    SHA1.HashData(input, hash);
#pragma warning restore CA5350

    // Set version (5) and variant (RFC 9562)
    hash[6] = (byte)((hash[6] & 0x0F) | 0x50); // version 5
    hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // variant 10xx

    var result = hash[..16].ToArray();
    SwapGuidByteOrder(result);
    return new Guid(result);
  }

  /// <summary>
  /// Converts between .NET's mixed-endian Guid layout and the big-endian RFC byte order.
  /// The first three components (4-2-2 bytes) are little-endian in .NET but big-endian in RFC.
  /// </summary>
  private static void SwapGuidByteOrder(Span<byte> bytes)
  {
    (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
    (bytes[1], bytes[2]) = (bytes[2], bytes[1]);
    (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
    (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
  }
}
