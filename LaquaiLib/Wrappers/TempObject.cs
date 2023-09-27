using System.Reflection;

using LaquaiLib.Extensions;

namespace LaquaiLib.Wrappers;

/// <summary>
/// Represents a temporary instance of type <typeparamref name="T"/> that is automatically cleared from memory when its wrapper object is disposed.
/// </summary>
public class TempObject<T> : IDisposable
{
    /// <summary>
    /// The instance of <typeparamref name="T"/> wrapped by this <see cref="TempObject{T}"/> instance.
    /// </summary>
    public T? Value { get; private set; }

    /// <summary>
    /// Instantiates a new <see cref="TempObject{T}"/>.
    /// </summary>
    /// <param name="parameters">The parameters to use to find a constructor for <typeparamref name="T"/>. If <see langword="null"/> or <c>0</c>-length, the parameterless constructor is used.</param>
    public TempObject(params object?[]? parameters)
    {
        try
        {
                typeof(T).New(parameters is null || parameters.Length == 0 ? null : parameters);
        }
        catch
        {
            Value = default;
        }
    }
    public TempObject(T value)
    {
        Value = value;
    }

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

    /// <summary>
    /// Finalizes this <see cref="TempObject{T}"/>.
    /// </summary>
    ~TempObject()
    {
        Dispose(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }
    #endregion
}
