using LaquaiLib.UnsafeUtils;

namespace LaquaiLib.Core;

/// <summary>
/// Contains utility methods for the <see cref="string"/> type.
/// </summary>
public static class StringUtility
{
    /// <summary>
    /// Allocates a new <see langword="string"/> with the specified length, then invokes the specified <see cref="SpanAction{T}"/> to fill it.
    /// </summary>
    /// <param name="length">The length of the <see cref="string"/> to create.</param>
    /// <param name="spanAction">A <see cref="SpanAction{T}"/> that takes a <see cref="Span{T}"/> of <see cref="char"/>.</param>
    /// <returns>The created <see cref="string"/>.</returns>
    /// <remarks>
    /// <paramref name="spanAction"/> MUST fill the entire <see cref="Span{T}"/> with valid <see cref="char"/> values, otherwise uninitialized memory will be exposed through the <see cref="string"/>.
    /// </remarks>
    public static string CreateString(int length, SpanAction<char> spanAction)
    {
        var str = AllocString(length);
        var mut = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(str.AsSpan()), str.Length);
        spanAction(mut);
        return str;
    }

    /// <summary>
    /// Allocates an uninitialized string from unmanaged memory.
    /// </summary>
    /// <param name="length">The length of the string to allocate.</param>
    /// <returns>A reference to the allocated string.</returns>
    internal static unsafe string AllocString(int length)
    {
        var buffer = MemoryManager.CAlloc<char>(length + 1);
        buffer[length] = '\0';
        return new string(buffer, 0, length);
    }
}

/// <summary>
/// Encapsulates a method that takes a <see cref="Span{T}"/> of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the elements in the <see cref="Span{T}"/>.</typeparam>
public delegate void SpanAction<T>(Span<T> span);
