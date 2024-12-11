namespace LaquaiLib.DependencyInjection.ExternalRunner;

/// <summary>
/// Supports <see cref="ExternalRunnerBuilder"/> through the <see cref="IOptions{TOptions}"/> pattern.
/// </summary>
public class ExternalRunnerOptions
{
    /// <summary>
    /// Gets a <see cref="HashSet{T}"/> that holds external objects to run.
    /// </summary>
    public HashSet<(string, string[])> Externals { get; } = [];
}