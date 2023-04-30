using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Office.Interop.Word;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a temporarily allocated region of unmanaged memory that is automatically freed when its wrapper object is disposed.
/// </summary>
public unsafe class TempAlloc : IDisposable
{
    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> with the given size as represented by a 32-bit integer.
    /// </summary>
    /// <param name="bytes">The amount of bytes to allocate.</param>
    public TempAlloc(int bytes)
    {
        _address = Marshal.AllocHGlobal(bytes);
        _size = bytes;
    }

    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> with the given size as represented by a 32-bit integer, optionally clearing any previous data.
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
        return new TempAlloc(Marshal.SizeOf<T>(), clear);
    }

    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> for <paramref name="count"/> instances of the given <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to allocate memory for.</typeparam>
    /// <param name="count">The amount of <typeparamref name="T"/> instances to allocate memory for.</param>
    public static TempAlloc Create<T>(int count)
        where T : struct
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
        }
        return new TempAlloc(Marshal.SizeOf<T>() * count);
    }

    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> for <paramref name="count"/> instances of the given <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to allocate memory for.</typeparam>
    /// <param name="count">The amount of <typeparamref name="T"/> instances to allocate memory for.</param>
    /// <param name="clear">A value indicating whether any previous data in the allocated memory region should be cleared.</param>
    public static TempAlloc Create<T>(int count, bool clear)
        where T : struct
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");
        }
        return new TempAlloc(Marshal.SizeOf<T>() * count, clear);
    }

    /// <summary>
    /// Instantiates a new <see cref="TempAlloc"/> for the
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the <paramref name="value"/> to allocate memory for.</typeparam>
    /// <param name="value">A value of type <typeparamref name="T"/> to allocate memory for. The existing value is copied to the newly allocated memory region.</param>
    public static TempAlloc Create<T>(T value)
        where T : struct
    {
        var alloc = new TempAlloc(Marshal.SizeOf<T>());
        Marshal.StructureToPtr(value, alloc._address, true);
        return alloc;
    }

    private nint _address;
    private int _size;

    /// <summary>
    /// The address of the memory region this <see cref="TempAlloc"/> wraps.
    /// </summary>
    public nint Address {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, _address);
            return _address;
        }
    }
    /// <summary>
    /// The size of the memory region this <see cref="TempAlloc"/> wraps in bytes.
    /// </summary>
    public int Size {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, _size);
            return _size;
        }
    }
    /// <summary>
    /// The size of the memory region this <see cref="TempAlloc"/> wraps in bits.
    /// </summary>
    public int Bits {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, _size);
            return _size * 8;
        }
    }
    /// <summary>
    /// A <see cref="Span{T}"/> of <see cref="byte"/> that represents the memory region this <see cref="TempAlloc"/> wraps.
    /// </summary>
    public Span<byte> Data {
        get {
            ObjectDisposedException.ThrowIf(IsDisposed, _size);
            unsafe
            {
                return new Span<byte>((byte*)_address, _size);
            }
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
    /// Attempts to cast the contents of the memory region this <see cref="TempAlloc"/> wraps to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to cast the contents of the memory region to.</typeparam>
    /// <returns>The entire contents of the memory region this <see cref="TempAlloc"/> wraps as an instance of <typeparamref name="T"/>.</returns>
    public T As<T>() => As<T>(0, _size);

    /// <summary>
    /// Attempts to cast the content of a slice of the memory region this <see cref="TempAlloc"/> wraps to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to cast the contents of the memory region to.</typeparam>
    /// <param name="offset">The offset at which to start the slice.</param>
    /// <param name="length">The length of the slice.</param>
    /// <returns>The contents of the slice of the memory region this <see cref="TempAlloc"/> wraps as an instance of <typeparamref name="T"/>.</returns>
    public T As<T>(int offset, int length)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, _address);

        var block = Data.Slice(offset, length).ToArray();
        try
        {
            fixed (byte* bytePtr = block)
            {
                // Special case for strings since they're not blittable
                if (typeof(T) == typeof(string))
                {
                    unsafe
                    {
                        if (new string((sbyte*)bytePtr, 0, length) is T value)
                        {
                            return value;
                        }
                    }
                }

                // Convert the pointer to a pointer to the target type, and dereference it to get a reference to the instance.
                return Marshal.PtrToStructure<T>((nint)bytePtr);
            }
        }
        catch (Exception inner)
        {
            throw new InvalidCastException($"Could not cast the contents of the memory region at address '{_address + offset:X}' with length '{length}' to type '{typeof(T).Name}'.", inner);
        }
    }

    /// <summary>
    /// Clears the memory region this <see cref="TempAlloc"/> wraps (sets all bytes to zero).
    /// </summary>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_address == nint.Zero && _size == -1, _address);
        Data.Clear();
    }

    /// <summary>
    /// Searches for the first occurrence of a given <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/> in the memory region this <see cref="TempAlloc"/> wraps and replaces it with memory represented by another <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/>.
    /// </summary>
    /// <param name="search">The sequence of bytes to find in <see cref="Data"/>.</param>
    /// <param name="replacement">The sequence of bytes to replace the first occurrence of <paramref name="search"/> with. The length of this sequence need not be equal to the length of the <paramref name="search"/> sequence.</param>
    /// <param name="shift">Whether to shift the bytes to the right of the replacement after it has been made.
    /// <para/><list type="bullet">
    /// <item/>If <c><paramref name="replacement"/>.Length &gt; <paramref name="search"/>.Length</c>, <list type="bullet">
    ///     <item/>and <c><paramref name="shift"/></c>, the bytes that would be overwritten by the <paramref name="replacement"/> are shifted right to fully accomodate the replacement.
    ///     <item/>and <c>!<paramref name="shift"/></c>, the <paramref name="replacement"/> bytes overwrite any bytes after <paramref name="search"/>.
    /// </list>
    /// <item/>If <c><paramref name="replacement"/>.Length &lt; <paramref name="search"/>.Length</c>, <list type="bullet">
    ///     <item/>and <c><paramref name="shift"/></c>, the bytes remaining in the space of <paramref name="search"/> after the replacement is made are discarded and the bytes to the right of it are shifted left to fill the space. The size of the memory region is decreased by <c>(<paramref name="search"/>.Length - <paramref name="replacement"/>.Length)</c>.
    ///     <item/>and <c>!<paramref name="shift"/></c>, the bytes remaining in the space of <paramref name="search"/> after the replacement is made are left untouched. The size of the memory region does not change.
    /// </list>
    /// </list>
    /// </param>
    /// <returns>A value that indicates whether a replacement was made.</returns>
    public bool Replace(ReadOnlySpan<byte> search, ReadOnlySpan<byte> replacement, bool shift = false)
    {
        if (search == replacement)
        {
            return false;
        }
        if (replacement == ReadOnlySpan<byte>.Empty && !shift)
        {
            throw new ArgumentException("The replacement cannot be empty if the remaining bytes are not shifted over the search bytes.", nameof(replacement));
        }

        if (replacement.Length >= search.Length)
        {
            // Use ReplaceLonger as that method reallocates the memory region to the longer length it needs, if necessary
            // I.e. if their lengths are equal, all the logic in a hypothetical 'ReplaceEqual' call would be the same as in ReplaceLonger
            return ReplaceLonger(search, replacement, shift);
        }
        else // if (replacement.Length < search.Length)
        {
            return ReplaceShorter(search, replacement, shift);
        }
    }
    
    /// <summary>
    /// Searches for all occurrences of a given <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/> in the memory region this <see cref="TempAlloc"/> wraps and replaces them with memory represented by another <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/>.
    /// </summary>
    /// <param name="search">The sequence of bytes to find in <see cref="Data"/>.</param>
    /// <param name="replacement">The sequence of bytes to replace the occurrences of <paramref name="search"/> with. The length of this sequence need not be equal to the length of the <paramref name="search"/> sequence.</param>
    /// <param name="shift">Whether to shift the bytes to the right of the replacement after it has been made.
    /// <para/><list type="bullet">
    /// <item/>If <c><paramref name="replacement"/>.Length &gt; <paramref name="search"/>.Length</c>, <list type="bullet">
    ///     <item/>and <c><paramref name="shift"/></c>, the bytes that would be overwritten by the <paramref name="replacement"/> are shifted right to fully accomodate the replacement.
    ///     <item/>and <c>!<paramref name="shift"/></c>, the <paramref name="replacement"/> bytes overwrite any bytes after <paramref name="search"/>.
    /// </list>
    /// <item/>If <c><paramref name="replacement"/>.Length &lt; <paramref name="search"/>.Length</c>, <list type="bullet">
    ///     <item/>and <c><paramref name="shift"/></c>, the bytes remaining in the space of <paramref name="search"/> after the replacement is made are discarded and the bytes to the right of it are shifted left to fill the space. The size of the memory region is decreased by <c>(<paramref name="search"/>.Length - <paramref name="replacement"/>.Length)</c>.
    ///     <item/>and <c>!<paramref name="shift"/></c>, the bytes remaining in the space of <paramref name="search"/> after the replacement is made are left untouched. The size of the memory region does not change.
    /// </list>
    /// </list>
    /// </param>
    /// <returns>The number of replacements made.</returns>
    public int ReplaceAll(ReadOnlySpan<byte> search, ReadOnlySpan<byte> replacement, bool shift = false)
    {
        if (search == replacement)
        {
            return 0;
        }
        if (replacement == ReadOnlySpan<byte>.Empty && !shift)
        {
            throw new ArgumentException("The replacement cannot be empty if the remaining bytes are not shifted over the search bytes.", nameof(replacement));
        }

        var c = 0;
        while (Replace(search, replacement, shift))
        {
            c++;
        }
        return c;
    }

    private bool ReplaceLonger(ReadOnlySpan<byte> search, ReadOnlySpan<byte> replacement, bool shift)
    {
        var data = Data;

        if (data.IndexOf(search) is var location and >= 0)
        {
            // If the replacement is larger than the space between 'location' and the end of the data, reallocate the memory region to accomodate the replacement
            if (replacement.Length > data.Length - location)
            {
                var newLength = data.Length + (replacement.Length - (data.Length - location));
                if (Reallocate(newLength))
                {
                    data = Data;
                }
            }

            // The space between 'location' and the end of the data is now as larger or larger than 'replacement'
            // 1. If 'shift', shift the bytes after 'location + replacement.Length' to the right by 'search.Length - replacement.Length' bytes, otherwise skip this step (because the bytes are overwritten instead)
            // 2. Replace 'data[location..location + replacement.Length]' with the 'replacement' bytes
            if (shift)
            {
                // Shift the data after 'location + replacement.Length' to the right by 'search.Length - replacement.Length' bytes
                var shiftAmount = search.Length - replacement.Length;
                for (var i = data.Length - 1; i >= location + replacement.Length; i--)
                {
                    data[i] = data[i - shiftAmount];
                }
            }

            // Replace 'data[location..location + search.Length]' with the 'replacement' bytes
            for (var i = location; i < location + replacement.Length; i++)
            {
                data[i] = replacement[i - location];
            }

            return true;
        }

        return false;
    }

    private bool ReplaceShorter(ReadOnlySpan<byte> search, ReadOnlySpan<byte> replacement, bool shift)
    {
        var data = Data;

        if (data.IndexOf(search) is var location and >= 0)
        {
            // If '!shift', the bytes that are not overwritten by the replacement are left completely untouched
            // Otherwise, those bytes are discarded and everything after 'location + search.Length' is shifted to the left by 'search.Length - replacement.Length' bytes
            if (shift)
            {
                // Shift the data after 'location + search.Length' to the left by 'search.Length - replacement.Length' bytes
                var shiftAmount = search.Length - replacement.Length;
                for (var i = location + search.Length; i < data.Length; i++)
                {
                    data[i - shiftAmount] = data[i];
                }
                Reallocate(data.Length - shiftAmount);
                data = Data;
            }

            if (replacement.Length > 0)
            {
                for (var i = location; i < location + replacement.Length; i++)
                {
                    data[i] = replacement[i];
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Serializes the contents of the memory region this <see cref="TempAlloc"/> wraps to a <see cref="string"/> of hexadecimal characters, grouped into 4-byte words.
    /// </summary>
    /// <returns>The string as described.</returns>
    public string ToHexString()
    {
        var data = Data;
        var sb = new StringBuilder();
        for (var i = 0; i < _size; i += 4)
        {
            var slice = data[i..(i + 4 > _size ? _size : i + 4)];
            sb.Append(Convert.ToHexString(slice));
            sb.Append(' ');
        }
        return sb.ToString().Trim(' ');
    }

    /// <summary>
    /// Serializes the contents of the memory region this <see cref="TempAlloc"/> wraps to a <see cref="string"/> of binary characters, grouped into 32-bit words.
    /// </summary>
    /// <returns>The string as described.</returns>
    public string ToBinaryString()
    {
        var data = Data;
        var sb = new StringBuilder();
        for (var i = _size; i > 0; i -= 4)
        {
            var slice = i - 4 < 0 ? data[..i] : data.Slice(i - 4, 4);
            foreach (var b in slice)
            {
                sb.Insert(0, System.Convert.ToString(b, toBase: 2).PadLeft(8, '0'));
            }
            sb.Insert(0, ' ');
        }
        return sb.ToString().Trim(' ');
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

    /// <summary>
    /// In addition to performing application-defined tasks associated with freeing, releasing, or resetting unmanaged resources, clears the entire contents of the memory region this <see cref="TempAlloc"/> wraps (sets all bytes to zero).
    /// </summary>
    public void DisposeSecure()
    {
        Clear();
        Dispose();
    }
    #endregion
}
