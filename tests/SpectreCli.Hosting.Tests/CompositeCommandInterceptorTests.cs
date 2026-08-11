namespace Kritikos.SpectreCli.Hosting.Tests;

using Kritikos.SpectreCli.Hosting.Tests.Doubles;

using Spectre.Console.Cli;

public class CompositeCommandInterceptorTests
{
  [Test]
  public async Task Intercept_MultipleInterceptors_RunsInRegistrationOrder()
  {
    var log = new List<string>();
    var composite = new CompositeCommandInterceptor(
      [new RecordingInterceptor("a", log), new RecordingInterceptor("b", log)]);

    composite.Intercept(Context(), new ProbeSettings());

    await Assert.That(log).IsEquivalentTo(["intercept:a", "intercept:b"]);
  }

  [Test]
  public async Task InterceptResult_MultipleInterceptors_RunsInReverseOrder()
  {
    var log = new List<string>();
    var composite = new CompositeCommandInterceptor(
      [new RecordingInterceptor("a", log), new RecordingInterceptor("b", log)]);

    var result = 0;
    composite.InterceptResult(Context(), new ProbeSettings(), ref result);

    await Assert.That(log).IsEquivalentTo(["result:b", "result:a"]);
  }

  [Test]
  public async Task Add_ExistingComposite_ReturnsNewInstanceLeavingOriginalUnchanged()
  {
    var log = new List<string>();
    var original = new CompositeCommandInterceptor([new RecordingInterceptor("a", log)]);

    var extended = original.Add(new RecordingInterceptor("b", log));
    original.Intercept(Context(), new ProbeSettings());

    await Assert.That(extended).IsNotSameReferenceAs(original);
    await Assert.That(log).IsEquivalentTo(["intercept:a"]);
  }

  [Test]
  public async Task Add_ExistingComposite_AppendsToTheEndOfTheChain()
  {
    var log = new List<string>();
    var composite = new CompositeCommandInterceptor([new RecordingInterceptor("a", log)])
      .Add(new RecordingInterceptor("b", log));

    composite.Intercept(Context(), new ProbeSettings());

    await Assert.That(log).IsEquivalentTo(["intercept:a", "intercept:b"]);
  }

  [Test]
  public async Task Intercept_NoInterceptors_DoesNothing()
  {
    var composite = new CompositeCommandInterceptor([]);

    composite.Intercept(Context(), new ProbeSettings());
    var result = 0;
    composite.InterceptResult(Context(), new ProbeSettings(), ref result);

    await Assert.That(result).IsEqualTo(0);
  }

  private static CommandContext Context()
    => new([], new FakeRemainingArguments(), "cmd", data: null);

  private sealed class FakeRemainingArguments : IRemainingArguments
  {
    public ILookup<string, string?> Parsed { get; } =
      Array.Empty<string>().ToLookup(static x => x, static _ => (string?)null);

    public IReadOnlyList<string> Raw { get; } = [];
  }
}
