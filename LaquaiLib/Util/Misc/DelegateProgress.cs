namespace LaquaiLib.Util.Misc;

/// <summary>
/// Contains static factory methods for creating <see cref="DelegateProgress{T}"/> instances.
/// </summary>
public static class DelegateProgress
{
    /// <summary>
    /// Creates a new instance of <see cref="DelegateProgress{T}"/> using the specified delegate.
    /// </summary>
    /// <typeparam name="T">The type of progress value.</typeparam>
    /// <param name="action">The delegate to proxy <see cref="IProgress{T}.Report(T)"/> invocations to.</param>
    /// <returns>The created <see cref="DelegateProgress{T}"/> instance.</returns>
    public static DelegateProgress<T> Create<T>(Action<T> action) => new DelegateProgress<T>(action);
}

/// <summary>
/// Implements <see cref="IProgress{T}"/> using a delegate.
/// </summary>
/// <typeparam name="T">The type of progress value.</typeparam>
/// <param name="action">The delegate to proxy <see cref="Report(T)"/> invocations to.</param>
public class DelegateProgress<T>(Action<T> action) : IProgress<T>
{
    /// <summary>
    /// Reports the specified <paramref name="progress"/> value.
    /// </summary>
    /// <param name="progress">The value to report.</param>
    public void Report(T progress) => action(progress);
}