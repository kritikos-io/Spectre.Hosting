namespace Kritikos.SpectreCli.Hosting;

using Spectre.Console.Cli;

/// <summary>
/// An <see cref="ICommandInterceptor"/> that delegates to an ordered list of interceptors,
/// enabling multiple interceptors to be composed where Spectre only allows one.
/// </summary>
/// <param name="interceptors">The interceptors to invoke in order.</param>
internal sealed class CompositeCommandInterceptor(IReadOnlyList<ICommandInterceptor> interceptors)
  : ICommandInterceptor
{
  /// <inheritdoc/>
  public void Intercept(CommandContext context, CommandSettings settings)
  {
    for (var i = 0; i < interceptors.Count; i++)
    {
      interceptors[i].Intercept(context, settings);
    }
  }

  /// <inheritdoc/>
  public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
  {
    for (var i = interceptors.Count - 1; i >= 0; i--)
    {
      interceptors[i].InterceptResult(context, settings, ref result);
    }
  }

  /// <summary>
  /// Returns a new <see cref="CompositeCommandInterceptor"/> with the given
  /// <paramref name="interceptor"/> appended to the chain.
  /// </summary>
  /// <param name="interceptor">The interceptor to add.</param>
  /// <returns>A new composite with the additional interceptor.</returns>
  internal CompositeCommandInterceptor Add(ICommandInterceptor interceptor)
    => new([.. interceptors, interceptor]);
}
