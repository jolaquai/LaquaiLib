using LaquaiLib.Extensions;

namespace LaquaiLib.Wrappers;

/// <summary>
/// Represents a temporary instance of type <typeparamref name="T"/> that is automatically cleared from memory when its wrapper object is disposed.
/// </summary>
public ref struct TempObject<T> : IDisposable
{
    /// <summary>
    /// The instance of <typeparamref name="T"/> wrapped by this <see cref="TempObject{T}"/> instance.
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// Initializes a new <see cref="TempObject{T}"/> by using the specified <paramref name="parameters"/> to find and invoke a constructor for type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="parameters">The parameters to use to find a constructor for <typeparamref name="T"/>. If <c>0</c>-length, the parameterless constructor is used.</param>
    public TempObject(params object[] parameters)
    {
        try
        {
            Value = (T)typeof(T).New(parameters.Length == 0 ? null : parameters);
        }
        catch
        {
            Value = default;
        }
    }
    /// <summary>
    /// Initializes a new <see cref="TempObject{T}"/> by using the specified <paramref name="value"/>.
    /// The calling scope should not hold its own references to <paramref name="value"/> except by accessing <see cref="Value"/>.
    /// </summary>
    /// <param name="value">A reference to the instance of <typeparamref name="T"/> to wrap.</param>
    public TempObject(T value) => Value = value;

    #region Dispose pattern
    /// <summary>
    /// Whether this <see cref="TempObject{T}"/> has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            Value = default;
        }

        // Dispose of unmanaged resources (native allocated memory etc.)

        IsDisposed = true;
    }

    /// <inheritdoc/>
    public void Dispose() => Dispose(true);
    #endregion
}
