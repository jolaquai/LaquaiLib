using System.Runtime.CompilerServices;

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
/// Allows usage of any object in a <see langword="using"/> block or statement for syntactic sugar or to make sure the desired actions aren't forgotten.
/// </summary>
/// <typeparam name="T">The type of the object to wrap.</typeparam>
public struct UsingWrapper<T> : IDisposable
{
    private readonly string _instanceType;
    private readonly T _instance;
    private readonly Action<T> _dispose;

    private bool disposed;

    /// <summary>
    /// Initializes a new <see cref="UsingWrapper{T}"/> for the specified <paramref name="instance"/> of <typeparamref name="T"/>, optionally registering the <paramref name="dispose"/> action to the <see cref="AppDomain.ProcessExit"/> event.
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
    /// A reference to the wrapped instance of <typeparamref name="T"/>.
    /// If the instance is disposed, an <see cref="ObjectDisposedException"/> is thrown.
    /// </summary>
    public readonly T Instance => !disposed ? _instance : throw new ObjectDisposedException($"UsingWrapper<{_instanceType}>");
    /// <summary>
    /// Executes the registered dispose action on the wrapped instance.
    /// </summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _dispose(Instance);
        disposed = true;
    }

    /// <summary>
    /// Converts a <see cref="UsingWrapper{T}"/> instance to the wrapped instance. The wrapper instance is lost if not kept track of separately
    /// </summary>
    /// <param name="wrapper">The <see cref="UsingWrapper{T}"/> instance to convert.</param>
    public static implicit operator T(UsingWrapper<T> wrapper) => wrapper.Instance;
}
