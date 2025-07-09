namespace LaquaiLib.Wrappers;

/// <summary>
/// Provides factory methods for <see cref="UsingWrapper{T}"/> instances.
/// </summary>
public static class UsingWrapper
{
    /// <summary>
    /// Creates a new <see cref="UsingWrapper{T}"/> instance for the given <see cref="IDisposable"/>-implementing instance of <typeparamref name="T"/>.
    /// The registered dispose action just delegates to <see cref="IDisposable.Dispose"/> of the underlying instance.
    /// </summary>
    /// <typeparam name="T">The type of the object to wrap.</typeparam>
    /// <param name="instance">The instance to wrap.</param>
    /// <returns>The created <see cref="UsingWrapper{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UsingWrapper<T> Use<T>(T instance) where T : IDisposable
        => new UsingWrapper<T>(instance, static instance => instance.Dispose());
    /// <summary>
    /// Creates a new <see cref="UsingWrapper{T}"/> instance for the given instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to wrap.</typeparam>
    /// <param name="instance">The instance to wrap.</param>
    /// <param name="dispose">The <see cref="Action{T}"/> that is executed when the <see cref="UsingWrapper{T}"/> is disposed. It is passed the wrapped instance.</param>
    /// <returns>The created <see cref="UsingWrapper{T}"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UsingWrapper<T> Use<T>(T instance, Action<T> dispose) => new UsingWrapper<T>(instance, dispose);
}

/// <summary>
/// Wraps an instance of an object and, when disposed, executes dispose logic.
/// Allows for syntactic sugar (as in, allows you to omit a <see langword="try"/>-<see langword="finally"/> combination and "outsource" it into a <see langword="using"/>).
/// </summary>
/// <typeparam name="T">The type of the object to wrap.</typeparam>
public class UsingWrapper<T> : IDisposable, IAsyncDisposable
{
    private readonly string _instanceType;
    private readonly T _instance;
    private readonly Action<T> _dispose;
    private readonly Func<T, ValueTask> _disposeAsync;

    private bool disposed;

    /// <summary>
    /// Initializes a new <see cref="UsingWrapper{T}"/> for the specified <paramref name="instance"/> of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="instance">The instance to wrap.</param>
    /// <param name="dispose">The <see cref="Action{T}"/> that is executed when the <see cref="UsingWrapper{T}"/> is disposed. It is passed the wrapped instance.</param>
    public UsingWrapper(T instance, Action<T> dispose)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(dispose);

        _instance = instance;
        _instanceType = _instance.GetType().FullName;
        _dispose = dispose;
    }
    /// <summary>
    /// Initializes a new <see cref="UsingWrapper{T}"/> for the specified <paramref name="instance"/> of <typeparamref name="T"/>
    /// </summary>
    /// <param name="instance">The instance to wrap.</param>
    /// <param name="dispose">The asynchronous <see cref="Func{T, TResult}"/> that is executed when the <see cref="UsingWrapper{T}"/> is disposed. It is passed the wrapped instance.</param>
    public UsingWrapper(T instance, Func<T, ValueTask> dispose)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(dispose);

        _instance = instance;
        _instanceType = _instance.GetType().FullName;
        _disposeAsync = dispose;
    }

    /// <summary>
    /// A reference to the wrapped instance of <typeparamref name="T"/>.
    /// If the instance is disposed, an <see cref="ObjectDisposedException"/> is thrown.
    /// </summary>
    public T Instance => !disposed ? _instance : throw new ObjectDisposedException($"UsingWrapper<{_instanceType}>");
    /// <summary>
    /// Executes the registered dispose action on the wrapped instance.
    /// </summary>
    void IDisposable.Dispose()
    {
        if (disposed)
        {
            return;
        }
        GC.SuppressFinalize(this);

        if (_disposeAsync is not null)
        {
            throw new InvalidOperationException($"Cannot call {nameof(IDisposable.Dispose)} on a {nameof(UsingWrapper<>)} that has an asynchronous dispose action registered. Use {nameof(IAsyncDisposable.DisposeAsync)} instead.");
        }

        _dispose(Instance);
        disposed = true;
    }
    /// <summary>
    /// Executes the registered asynchronous dispose action on the wrapped instance.
    /// </summary>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (disposed)
        {
            return;
        }
        GC.SuppressFinalize(this);

        if (_disposeAsync is not null)
        {
            await _disposeAsync(Instance).ConfigureAwait(false);
        }
        else
        {
            _dispose(Instance);
        }
        disposed = true;
    }

    /// <summary>
    /// Converts a <see cref="UsingWrapper{T}"/> instance to the wrapped instance. The wrapper instance is lost if not kept track of separately.
    /// </summary>
    /// <param name="wrapper">The <see cref="UsingWrapper{T}"/> instance to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T(UsingWrapper<T> wrapper) => wrapper.Instance;
}
