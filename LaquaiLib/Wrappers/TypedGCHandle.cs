using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LaquaiLib.Wrappers;

/// <summary>
/// Provides a way to access a managed instance of <typeparamref name="T"/> from unmanaged memory.
/// Any <see langword="unsafe"/> members of this type should only be called when <typeparamref name="T"/> is an <see langword="unmanaged"/> type.
/// </summary>
/// <typeparam name="T">The type of the object to be referenced.</typeparam>
public readonly struct GCHandle<T> : IDisposable
{
    /// <summary>
    /// The wrapped untyped <see cref="GCHandle"/>.
    /// </summary>
    public GCHandle Handle { get; }
    /// <summary>
    /// Gets the object of type <typeparamref name="T"/> this <see cref="GCHandle{T}"/> represents.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the handle has been disposed.</exception>
    public T Target => Handle.IsAllocated ? (T)Handle.Target! : throw new ObjectDisposedException("The handle is disposed.");
    /// <inheritdoc cref="GCHandle.IsAllocated"/>
    public bool IsAllocated => Handle.IsAllocated;

    #region Constructors
    /// <summary>
    /// Initializes a new <see cref="GCHandle{T}"/> that represents the specified instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The object to be referenced.</param>
    /// <param name="type">The <see cref="GCHandleType"/> of the object to be referenced.</param>
    public GCHandle(T value, GCHandleType type = GCHandleType.Normal)
    {
        Handle = GCHandle.Alloc(value, type);
    }
    /// <summary>
    /// Initializes a new <see cref="GCHandle{T}"/> from a handle to a managed object.
    /// </summary>
    /// <param name="ptr">The handle to a managed object.</param>
    /// <param name="type">The <see cref="GCHandleType"/> of the object to be referenced.</param>
    public GCHandle(nint ptr, GCHandleType type = GCHandleType.Pinned)
    {
        unsafe
        {
            Handle = GCHandle.Alloc(*(T*)ptr, type);
        }
    }
    /// <summary>
    /// Initializes a new <see cref="GCHandle{T}"/> from a pointer to an object.
    /// </summary>
    /// <param name="ptr">The pointer to an object.</param>
    /// <param name="type">The <see cref="GCHandleType"/> of the object to be referenced.</param>
    public unsafe GCHandle(T* ptr, GCHandleType type = GCHandleType.Pinned)
    {
        Handle = GCHandle.Alloc(*ptr, type);
    }
    /// <summary>
    /// Initializes a new <see cref="GCHandle{T}"/> typed <typeparamref name="T"/> from an existing untyped handle to a managed object.
    /// </summary>
    /// <param name="handle">The existing untyped handle to a managed object.</param>
    public GCHandle(GCHandle handle)
    {
        Handle = handle;
    }
    #endregion

    /// <inheritdoc cref="GCHandle.AddrOfPinnedObject"/>
    public nint AddrOfPinnedObject() => Handle.AddrOfPinnedObject();
    /// <summary>
    /// Obtains a pointer to the pinned object.
    /// </summary>
    public unsafe T* Pointer => (T*)AddrOfPinnedObject();

    /// <summary>
    /// Returns the untyped <see cref="GCHandle"/> a typed <see cref="GCHandle{T}"/> represents.
    /// </summary>
    /// <param name="handle">The <see cref="GCHandle{T}"/> to convert.</param>
    public static implicit operator GCHandle(GCHandle<T> handle) => handle.Handle;
    /// <summary>
    /// Returns a value indicating whether two typed <see cref="GCHandle{T}"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="GCHandle{T}"/> to compare.</param>
    /// <param name="right">The second <see cref="GCHandle{T}"/> to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are equal, otherwise <see langword="false"/>.</returns>
    public static bool operator ==(GCHandle<T> left, GCHandle<T> right) => left.Handle == right.Handle;
    /// <summary>
    /// Returns a value indicating whether two typed <see cref="GCHandle{T}"/> instances are unequal.
    /// </summary>
    /// <param name="left">The first <see cref="GCHandle{T}"/> to compare.</param>
    /// <param name="right">The second <see cref="GCHandle{T}"/> to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are unequal, otherwise <see langword="false"/>.</returns>
    public static bool operator !=(GCHandle<T> left, GCHandle<T> right) => !(left.Handle == right.Handle);
    /// <summary>
    /// Returns the internal integer representation of the wrapped <see cref="Handle"/>.
    /// </summary>
    /// <param name="handle">The <see cref="GCHandle{T}"/> to convert.</param>
    public static explicit operator nint(GCHandle<T> handle) => (nint)handle.Handle;
    /// <summary>
    /// Returns the internal integer representation of the wrapped <see cref="Handle"/>.
    /// </summary>
    /// <param name="handle">The <see cref="GCHandle{T}"/> to convert.</param>
    public static explicit operator nint?(GCHandle<T> handle) => (nint)handle.Handle;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is GCHandle<T> handle && this == handle;
    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(GCHandle<T>)} at 0x{Convert.ToString(value: (nint)this, 16).ToUpperInvariant()}";

    #region Dispose pattern
    /// <summary>
    /// Releases the unmanaged and optionally the managed resources used by this <see cref="GCHandle{T}"/> instance.
    /// </summary>
    /// <param name="disposing">Whether to release the managed resources used by this <see cref="GCHandle{T}"/> instance.</param>
    private void Dispose(bool disposing)
    {
        if (Handle.IsAllocated)
        {
            if (disposing)
            {
                // Dispose of managed resources (Streams etc.)
            }

            Handle.Free();
        }
    }

    /// <summary>
    /// Releases the managed and unmanaged resources used by this <see cref="GCHandle{T}"/> instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
