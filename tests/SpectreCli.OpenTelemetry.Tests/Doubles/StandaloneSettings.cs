namespace Kritikos.SpectreCli.OpenTelemetry.Tests.Doubles;

using Spectre.Console.Cli;

/// <summary>Settings declared outside a command, which Spectre allows but the interceptor cannot attribute.</summary>
internal sealed class StandaloneSettings : CommandSettings;
