using System.Diagnostics;

using LaquaiLib.Extensions;

namespace LaquaiLib.DependencyInjection.ExternalRunner;

/// <summary>
/// Implements <see cref="IExternalRunner"/> abstractly, providing utility functionality to derived types.
/// </summary>
public abstract class ExternalRunner : IExternalRunner
{
    /// <inheritdoc/>
    public abstract bool CanHandle(string path);
    /// <inheritdoc/>
    public abstract Task RunAsync(string path, string[] args, CancellationToken stoppingToken);

    /// <summary>
    /// For the specified <see cref="ProcessStartInfo"/> instance, starts a new process, registering default lifetime management.
    /// </summary>
    protected virtual async Task RunDefaultAsync(ProcessStartInfo psi, CancellationToken stoppingToken)
    {
        var process = Process.Start(psi);

        var stopTask = stoppingToken.WhenCancelled();
        var terminateTask = process.WaitForExitAsync();
        if (await Task.WhenAny(stopTask, terminateTask) != terminateTask)
        {
            process.Kill();
            await terminateTask;
        }
    }
}
