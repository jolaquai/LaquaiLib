using System.Diagnostics;

namespace LaquaiLib.DependencyInjection.ExternalRunner;

/// <summary>
/// Implements <see cref="ExternalRunner"/> for executable (<c>.exe</c>) files.
/// </summary>
public class ExecutableRunner : ExternalRunner
{
    /// <inheritdoc/>
    public override bool CanHandle(string path) => Path.GetExtension(path).Equals(".exe", StringComparison.OrdinalIgnoreCase);
    /// <inheritdoc/>
    public override Task RunAsync(string path, string[] args, CancellationToken stoppingToken)
    {
        var psi = new ProcessStartInfo()
        {
            FileName = path,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        for (var i = 0; i < args.Length; i++)
        {
            psi.ArgumentList.Add(args[i]);
        }
        return RunDefaultAsync(psi, stoppingToken);
    }
}
