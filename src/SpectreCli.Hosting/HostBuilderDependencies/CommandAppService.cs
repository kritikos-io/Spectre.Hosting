namespace Kritikos.SpectreCli.Hosting.HostBuilderDependencies;

using Microsoft.Extensions.Hosting;

public class CommandAppService(CommandAppWrapper wrapper) : IHostedService
{
  private readonly CommandAppWrapper wrapper = wrapper;

  /// <inheritdoc />
  public Task StartAsync(CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(wrapper.App);

    // Start the command app
    return wrapper.App.RunAsync(wrapper.Args);
  }

  /// <inheritdoc />
  public Task StopAsync(CancellationToken cancellationToken)
    => Task.CompletedTask;
}
