using LaquaiLib.UnsafeUtils;

namespace LaquaiLib.Core;

/// <summary>
/// Contains utility methods for the <see cref="string"/> type.
/// </summary>
public static class StringUtility
{
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
