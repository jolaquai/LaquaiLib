using System.Runtime.CompilerServices;

namespace LaquaiLib.Util;

/// <summary>
/// Disables marshalling of asynchronous continuations back to it for this thread for the duration of the scope.
/// All <see langword="await"/>s or <see cref="TaskAwaiter.GetResult"/> calls will have their continuations run on a thread pool thread instead.
/// Has no effect if the current <see cref="SynchronizationContext"/> is <see langword="null"/>.
/// <para/><b>Warning!</b> Selectively enabling using <see cref="Task.ConfigureAwait(bool)"/> and passing <see langword="true"/> is not possible since the context is removed from <see cref="SynchronizationContext"/>. Use with caution.
/// <para/><b>Warning!</b> This utility is implicitly not thread-safe for... obvious reasons. If not disposed, the previous <see cref="SynchronizationContext"/> will never be restored.
/// </summary>
public ref struct NoSynchronizationContextScope : IDisposable
{
    private readonly SynchronizationContext _previousContext;
    /// <summary>
    /// Initializes a new <see cref="NoSynchronizationContextScope"/>. For the lifetime of the resulting object, <see cref="SynchronizationContext.Current"/> will return <see langword="null"/>.
    /// If not disposed, the previous <see cref="SynchronizationContext"/> will never be restored.
    /// </summary>
    public NoSynchronizationContextScope()
    {
        _previousContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
    }
    /// <inheritdoc/>
    public readonly void Dispose() => SynchronizationContext.SetSynchronizationContext(_previousContext);
}
