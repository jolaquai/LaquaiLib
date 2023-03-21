using System;
using System.Runtime.InteropServices;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a temporarily allocated region of unmanaged memory that is automatically freed when its wrapper object is disposed.
/// </summary>
public class TempAlloc : IDisposable
{
    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> with the given size.
    /// <param name="bytes">The amount of bytes to allocate.</param>
    /// </summary>
    public TempAlloc(int bytes)
    {
        _address = Marshal.AllocHGlobal(bytes);
        _size = bytes;
    }

    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> for exactly one instance of the given <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to allocate memory for.</param>
    public TempAlloc(Type type)
    {
        _address = Marshal.AllocHGlobal(Marshal.SizeOf(type));
        _size = Marshal.SizeOf(type);
    }

    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> as a wrapper around existing allocated memory.
    /// </summary>
    /// <param name="address">The address to the memory region to wrap with this <see cref="TempAlloc"/>.</param>
    /// <param name="size">The size of the memory region at <paramref name="address"/>.</param>
    public TempAlloc(nint address, int size)
    {
        _address = address;
        _size = size;
    }

    private nint _address;
    private int _size;

    /// <summary>
    /// The address of the memory region this <see cref="TempAlloc"/> wraps.
    /// </summary>
    public nint Address {
        get {
            ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _address);
            return _address;
        }
    }
    /// <summary>
    /// The size of the memory region this <see cref="TempAlloc"/> wraps in bytes.
    /// </summary>
    public int Size {
        get {
            ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _size);
            return _size;
        }
    }
    /// <summary>
    /// Whether this <see cref="TempAlloc"/> has been disposed.
    /// </summary>
    public bool IsDisposed => _address == nint.Zero && _size == -1;

    /// <summary>
    /// Resizes the memory region this <see cref="TempAlloc"/> wraps.
    /// </summary>
    /// <param name="bytes">The new size of the memory region in bytes.</param>
    /// <returns>A value indicating whether the <see cref="Address"/> of the memory region this <see cref="TempAlloc"/> wraps has changed. If <c>true</c>, reading from any previous addresses is considered undefined behavior.</returns>
    public bool Reallocate(int bytes)
    {
        ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _address);

        nint newAddress = Marshal.ReAllocHGlobal(_address, bytes);
        _size = bytes;
        if (newAddress != _address)
        {
            _address = newAddress;
            return true;
        }
        return false;
    }

    #region Dispose pattern
    private void Dispose(bool disposing)
    {
        if (disposing)
        { }

        Marshal.FreeHGlobal(_address);
        _address = nint.Zero;
        _size = -1;
    }

    ~TempAlloc()
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
