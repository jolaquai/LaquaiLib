using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LaquaiLib.Wrappers;

/// <summary>
/// Wraps a <see langword="fixed"/> statement in a disposable wrapper. For its lifetime, the pointer pinned by the <see langword="fixed"/> statement will remain valid.
/// Allows 
/// </summary>
/// <typeparam name="T">The type of the value to pin.</typeparam>
public readonly unsafe ref struct PinWrapper<T> : IDisposable
    where T : unmanaged
{
    private readonly GCHandle<T> _handle;

    /// <summary>
    /// Initializes a new <see cref="PinWrapper{T}"/> with the specified value to pin.
    /// </summary>
    /// <param name="fixValue">The value to pin.</param>
    public PinWrapper(T fixValue)
    {
        _handle = new GCHandle<T>(fixValue, GCHandleType.Pinned);
        Pointer = (T*)_handle.AddrOfPinnedObject();
    }
    /// <summary>
    /// Initializes a new <see cref="PinWrapper{T}"/> with the specified existing pointer.
    /// </summary>
    /// <param name="ptr">The pointer to use.</param>
    public PinWrapper(T* ptr)
    {
        _handle = new GCHandle<T>(ptr, GCHandleType.Pinned);
        Pointer = (T*)_handle.AddrOfPinnedObject();
    }
    /// <summary>
    /// Initializes a new <see cref="PinWrapper{T}"/> with the specified <see cref="GCHandle"/>. Must be a pinned handle, otherwise an exception is thrown.
    /// </summary>
    /// <param name="handle">The <see cref="GCHandle"/> to use.</param>
    public PinWrapper(GCHandle handle)
    {
        // This throws internally if the handle is not pinned
        handle.AddrOfPinnedObject();
        _handle = new GCHandle<T>(handle);
        Pointer = (T*)GetSetHandleInternal(ref handle);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_handle")]
    private static extern ref nint GetSetHandleInternal(ref GCHandle handle);

    /// <summary>
    /// Gets the size of the pinned value.
    /// </summary>
    public readonly int Size { get; } = sizeof(T);
    /// <summary>
    /// Gets a pointer to the pinned value.
    /// </summary>
    public readonly T* Pointer { get; }
    /// <summary>
    /// Dereferences the pointer to the pinned value and returns a managed <see langword="ref"/>erence to it.
    /// </summary>
    public readonly ref T Reference => ref System.Runtime.CompilerServices.Unsafe.AsRef<T>(Pointer);
    /// <summary>
    /// Dereferences the pointer to the pinned value and returns its value.
    /// </summary>
    public readonly T Target => *Pointer;

    /// <summary>
    /// Dereferences the pointer to the pinned value and returns a <see langword="ref"/>erence to the value at the specified <paramref name="offset"/>.
    /// </summary>
    /// <param name="offset">The offset to apply to the pointer before dereferencing.</param>
    /// <returns>The <see langword="ref"/>erence to the value at the specified <paramref name="offset"/>.</returns>
    public readonly ref T this[int offset] => ref Pointer[offset];
    /// <summary>
    /// Creates a <see cref="Span{T}"/> of <typeparamref name="T"/> from this handle and forms a slice of it using the specified <see cref="Range"/>.
    /// It is considered undefined behavior to do this with a handle that does not originate from a <see cref="Span{T}"/>-compatible type (for example, <see cref="string"/>/<c><see cref="char"/>*</c> is one such type).
    /// </summary>
    /// <param name="range">The <see cref="Range"/> to use for the slice. Indices cannot be from the end since there is no length to calculate from.</param>
    /// <returns>The created <see cref="Span{T}"/> as described.</returns>
    public readonly Span<T> this[Range range] => new Span<T>(Pointer, range.End.Value - range.Start.Value);

    /// <summary>
    /// Creates a <see cref="Span{T}"/> of <typeparamref name="T"/> from this handle, starting at the pinned pointer up to the specified <paramref name="length"/>.
    /// It is considered undefined behavior to do this with a handle that does not originate from a <see cref="Span{T}"/>-compatible type (for example, <see cref="string"/>/<c><see cref="char"/>*</c> is a type combination that does support this).
    /// </summary>
    /// <param name="length">The length of the <see cref="Span{T}"/> to create.</param>
    /// <returns>The created <see cref="Span{T}"/> as described.</returns>
    public readonly Span<T> AsSpan(int length) => new Span<T>(Pointer, length);

    // This proxies GCHandle.Free
    public readonly void Dispose() => _handle.Dispose();

    /// <inheritdoc/>
    public override readonly string ToString() => $"{nameof(PinWrapper<>)}<{typeof(T).Namespace}.{typeof(T).Name}> at 0x{Convert.ToString((nint)Pointer, 16).ToUpperInvariant()}->{Target}";
}

/// <summary>
/// Contains factory methods for specific types of <see cref="PinWrapper{T}"/>.
/// For general cases, use <see cref="PinWrapper{T}"/> directly.
/// </summary>
public static class PinWrapper
{
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    /// <summary>
    /// Pins a pointer to the specified <see langword="string"/> and wraps it in a <see cref="PinWrapper{T}"/>.
    /// This is equivalent to the statement <c>fixed (char* ptr = str)</c>.
    /// </summary>
    /// <param name="str">The <see langword="string"/> to pin.</param>
    /// <returns>The created <see cref="PinWrapper{T}"/>.</returns>
    public static unsafe PinWrapper<char> Pin(string str) => new PinWrapper<char>(new GCHandle<char>((char*)&str));
    /// <summary>
    /// Pins a pointer to the specified <see cref="Array"/> and wraps it in a <see cref="PinWrapper{T}"/>.
    /// This is equivalent to the statement <c>fixed (T* ptr = array)</c>.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The <see cref="Array"/> to pin.</param>
    /// <returns>The created <see cref="PinWrapper{T}"/>.</returns>
    public static unsafe PinWrapper<T> Pin<T>(T[] array) where T : unmanaged => new PinWrapper<T>(GCHandle.Alloc(array, GCHandleType.Pinned));
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
}