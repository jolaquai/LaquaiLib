using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a temporarily allocated region of unmanaged memory that is automatically freed when its wrapper object is disposed.
/// </summary>
public class TempAlloc : IDisposable
{
    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> with the given size.
    /// </summary>
    /// <param name="bytes">The amount of bytes to allocate.</param>
    public TempAlloc(int bytes)
    {
        _address = Marshal.AllocHGlobal(bytes);
        _size = bytes;
    }

    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> with the given size, optionally clearing any previous data.
    /// </summary>
    /// <param name="bytes">The amount of bytes to allocate.</param>
    /// <param name="clear">A value indicating whether any previous data in the allocated memory region should be cleared.</param>
    public TempAlloc(int bytes, bool clear)
    {
        _address = Marshal.AllocHGlobal(bytes);
        _size = bytes;
        if (clear)
        {
            Data.Clear();
        }
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

    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> as a wrapper around existing allocated memory.
    /// </summary>
    /// <param name="address">The address to the memory region to wrap with this <see cref="TempAlloc"/>.</param>
    /// <param name="size">The size of the memory region at <paramref name="address"/>.</param>
    /// <param name="clear">A value indicating whether any previous data in the allocated memory region should be cleared.</param>
    public TempAlloc(nint address, int size, bool clear)
    {
        _address = address;
        _size = size;
        if (clear)
        {
            Data.Clear();
        }
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
    /// The size of the memory region this <see cref="TempAlloc"/> wraps in bits.
    /// </summary>
    public int Bits {
        get {
            ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _size);
            return _size * 8;
        }
    }
    /// <summary>
    /// A <see cref="Span{T}"/> of <see cref="byte"/> that represents the memory region this <see cref="TempAlloc"/> wraps.
    /// </summary>
    public Span<byte> Data {
        get {
            ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _size);
            unsafe
            {
                return new Span<byte>((void*)_address, _size);
            }
        }
    }
    /// <summary>
    /// Gets or sets a byte of data at the given index in the memory region this <see cref="TempAlloc"/> wraps.
    /// </summary>
    /// <param name="i">The index of the byte to get or set.</param>
    /// <returns>The byte at the given index.</returns>
    public byte this[int i] {
        get {
            ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _size);
            unsafe
            {
                return ((byte*)_address)[i];
            }
        }
        set {
            ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _size);
            unsafe
            {
                ((byte*)_address)[i] = value;
            }
        }
    }
    /// <summary>
    /// Whether this <see cref="TempAlloc"/> has been disposed.
    /// </summary>
    public bool IsDisposed => _address == nint.Zero && _size == -1;

    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> for exactly one instance of the given <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to allocate memory for.</typeparam>
    public static TempAlloc Create<T>()
        where T : struct
    {
        return new TempAlloc(Marshal.SizeOf<T>());
    }

    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> for exactly one instance of the given <see cref="Type"/>, optionally clearing any previous data.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to allocate memory for.</typeparam>
    /// <param name="clear">A value indicating whether any previous data in the allocated memory region should be cleared.</param>
    public static TempAlloc Create<T>(bool clear)
        where T : struct
    {
        var alloc = new TempAlloc(Marshal.SizeOf<T>(), clear);
        return alloc;
    }

    /// <summary>
    /// Resizes the memory region this <see cref="TempAlloc"/> wraps.
    /// </summary>
    /// <param name="bytes">The new size of the memory region in bytes.</param>
    /// <returns>A value indicating whether the <see cref="Address"/> of the memory region this <see cref="TempAlloc"/> wraps has changed. If <c>true</c>, reading from any previous addresses is considered undefined behavior.</returns>
    public bool Reallocate(int bytes)
    {
        ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _address);

        var newAddress = Marshal.ReAllocHGlobal(_address, bytes);
        _size = bytes;
        if (newAddress != _address)
        {
            _address = newAddress;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to cast the contents of the memory region this <see cref="TempAlloc"/> wraps to the given <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to cast the contents of the memory region to.</typeparam>
    /// <returns>The entire contents of the memory region this <see cref="TempAlloc"/> wraps as an instance of <typeparamref name="T"/>.</returns>
    public T Cast<T>()
    {
        ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _address);

        return Marshal.PtrToStructure<T>(_address) is T value
            ? value
            : throw new InvalidCastException($"Could not cast the contents of the memory region at address '{_address:X}' to type '{typeof(T).Name}'.");
    }

    /// <summary>
    /// Attempts to cast the content of a slice of the memory region this <see cref="TempAlloc"/> wraps to the given <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to cast the contents of the memory region to.</typeparam>
    /// <param name="offset">The offset at which to start the slice.</param>
    /// <param name="length">The length of the slice.</param>
    /// <returns>The contents of the slice of the memory region this <see cref="TempAlloc"/> wraps as an instance of <typeparamref name="T"/>.</returns>
    public T Cast<T>(int offset, int length)
    {
        ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _address);

        unsafe
        {
            var block = Data.Slice(offset, length);
            fixed (byte* ptr = block)
            {
                return Marshal.PtrToStructure<T>((nint)ptr) is T value
                    ? value
                    : throw new InvalidCastException($"Could not cast the contents of the memory region at address '{_address + offset:X}' with length '{length}' to type '{typeof(T).Name}'.");
            }
        }
    }

    /// <summary>
    /// Clears the memory region this <see cref="TempAlloc"/> wraps.
    /// </summary>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _address);
        Data.Clear();
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

    /// <summary>
    /// Finalizes this <see cref="TempAlloc"/>.
    /// </summary>
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
