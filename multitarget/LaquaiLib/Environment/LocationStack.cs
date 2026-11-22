namespace LaquaiLib.Env;

/// <summary>
/// Implements a process-wide stack of working directories.
/// The stack is seeded with <see cref="Environment.CurrentDirectory"/> as of first use of this type and that entry is never removed.
/// All access is serialized. <see cref="Push(string)"/> and <see cref="Pop"/> block while an <see cref="In(string, Func{ValueTask}, CancellationToken)"/> scope is active on another flow of execution.
/// </summary>
public static class LocationStack
{
    private static readonly SemaphoreSlim _sema = new(1, 1);
    private static readonly Stack<string> _stack = new([Environment.CurrentDirectory]);
    private static readonly AsyncLocal<Scope> _current = new();
    private static Scope _top;

    private sealed class Scope(Scope parent)
    {
        public readonly Scope Parent = parent;
    }

    /// <summary>
    /// Restores the previous location, makes it the process's current directory and returns it.
    /// If only the initial location remains, it is re-applied and returned; it is never removed.
    /// </summary>
    /// <returns>The full path of the location that is current after the call (not the one that was popped).</returns>
    /// <exception cref="InvalidOperationException">Called from within an <see cref="In(string, Func{ValueTask}, CancellationToken)"/> action, which would deadlock.</exception>
    /// <exception cref="DirectoryNotFoundException">The location to restore no longer exists. The popped entry is not pushed back.</exception>
    public static string Pop()
    {
        ThrowIfInScope();
        _sema.Wait();
        try
        {
            return PopCore();
        }
        finally
        {
            _sema.Release();
        }
    }
    private static string PopCore()
    {
        if (_stack.Count > 1)
            _stack.Pop();
        return Environment.CurrentDirectory = _stack.Peek();
    }

    /// <summary>
    /// Pushes <paramref name="location"/> onto the stack and makes it the process's current directory.
    /// </summary>
    /// <param name="location">The directory to enter. Relative paths are resolved against the current directory.</param>
    /// <exception cref="ArgumentException"><paramref name="location"/> is <see langword="null"/>, empty or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory does not exist.</exception>
    /// <exception cref="InvalidOperationException">Called from within an <see cref="In(string, Func{ValueTask}, CancellationToken)"/> action, which would deadlock.</exception>
    public static void Push(string location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        ThrowIfInScope();
        _sema.Wait();
        try
        {
            PushCore(location);
        }
        finally
        {
            _sema.Release();
        }
    }
    private static void PushCore(string location)
    {
        var full = Path.GetFullPath(location);
        if (!Directory.Exists(full))
            throw new DirectoryNotFoundException($"Could not find a part of the path '{full}'.");
        Environment.CurrentDirectory = full;
        _stack.Push(full);
    }

    /// <summary>
    /// Pushes <paramref name="location"/>, runs <paramref name="action"/> and restores the previous location afterwards, even if the action throws.
    /// The stack is exclusively held for the duration of the action, so concurrent calls are serialized.
    /// Calls to this method from within <paramref name="action"/>, including from async continuations and child tasks, are nested: they reuse the held exclusive access and stack their location on top.
    /// Nested scopes must not overlap (e.g. by running sibling <c>In</c> calls concurrently, or letting one outlive its parent); that is detected and throws <see cref="InvalidOperationException"/>.
    /// <see cref="Push(string)"/> and <see cref="Pop"/> from within <paramref name="action"/> throw <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="location">The directory to run in. Relative paths are resolved against the current directory.</param>
    /// <param name="action">The action to run.</param>
    /// <param name="cancellationToken">Cancels waiting for exclusive access. Does not affect a running action.</param>
    /// <exception cref="ArgumentException"><paramref name="location"/> is <see langword="null"/>, empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory does not exist. The action is not run.</exception>
    /// <exception cref="InvalidOperationException">Nested scopes overlapped.</exception>
    public static async ValueTask In(string location, Func<ValueTask> action, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        ArgumentNullException.ThrowIfNull(action);
        var parent = _current.Value;
        if (parent is null)
            await _sema.WaitAsync(cancellationToken).ConfigureAwait(false);
        var scope = new Scope(parent);
        try
        {
            Enter(scope);
            _current.Value = scope;
            try
            {
                PushCore(location);
                try
                {
                    await action().ConfigureAwait(false);
                }
                finally
                {
                    PopCore();
                }
            }
            finally
            {
                _current.Value = parent;
                Exit(scope);
            }
        }
        finally
        {
            if (parent is null)
                _sema.Release();
        }
    }

    /// <inheritdoc cref="In(string, Func{ValueTask}, CancellationToken)"/>
    /// <typeparam name="T">The result type of <paramref name="action"/>.</typeparam>
    /// <returns>The result of <paramref name="action"/>.</returns>
    public static async ValueTask<T> In<T>(string location, Func<ValueTask<T>> action, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        ArgumentNullException.ThrowIfNull(action);
        var parent = _current.Value;
        if (parent is null)
            await _sema.WaitAsync(cancellationToken).ConfigureAwait(false);
        var scope = new Scope(parent);
        try
        {
            Enter(scope);
            _current.Value = scope;
            try
            {
                PushCore(location);
                try
                {
                    return await action().ConfigureAwait(false);
                }
                finally
                {
                    PopCore();
                }
            }
            finally
            {
                _current.Value = parent;
                Exit(scope);
            }
        }
        finally
        {
            if (parent is null)
                _sema.Release();
        }
    }

    private static void ThrowIfInScope()
    {
        if (_current.Value is not null)
            throw new InvalidOperationException("The location stack is held by an enclosing In scope; reentrant access would deadlock.");
    }

    private static void Enter(Scope scope)
    {
        if (scope.Parent is null)
            _top = scope;
        else if (Interlocked.CompareExchange(ref _top, scope, scope.Parent) != scope.Parent)
            throw new InvalidOperationException("Concurrent use of the location stack within an In scope. Nested In calls must not overlap.");
    }
    private static void Exit(Scope scope)
    {
        if (Interlocked.CompareExchange(ref _top, scope.Parent, scope) != scope)
            throw new InvalidOperationException("An In scope ended while a nested In scope was still active.");
    }
}
