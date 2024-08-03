using System.Runtime.InteropServices;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Span{T}"/> of <see langword="byte"/> type.
/// </summary>
public static class SpanByteExtensions
{
    /// <summary>
    /// Formats the <see langword="byte"/>s of the specified <paramref name="data"/> instance into the <paramref name="span"/> at the specified <paramref name="index"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="data"/> instance.</typeparam>
    /// <param name="span">The target <see cref="Span{T}"/> of <see langword="byte"/>.</param>
    /// <param name="data">The <paramref name="data"/> instance to format into the <paramref name="span"/>.</param>
    /// <param name="index">The index at which to start writing the <paramref name="data"/> instance into the <paramref name="span"/>.</param>
    /// <returns>
    /// A slice of the input span that begins immediately after the last byte written. It may have length 0.
    /// This can be used to chain calls to this method.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if the target <paramref name="span"/> cannot accomodate the specified <paramref name="data"/> instance.</exception>
    public static Span<byte> FormatInto<T>(this Span<byte> span, T data, int index = 0)
        where T : unmanaged
    {
        if (index > 0)
        {
            span = span[index..];
        }

        var size = Marshal.SizeOf(data);
        if (span.Length < size)
        {
            throw new ArgumentException($"The target {nameof(span)} cannot accomodate the specified {nameof(data)} instance (need {size} bytes, have {span.Length}).");
        }

        // Cool thing is, Span pointer magic does all of what we need to do here
        unsafe
        {
            var bytes = MemoryMarshal.AsBytes(new ReadOnlySpan<T>(&data, 1));
            bytes.CopyTo(span);
        }

        return span[size..];
    }
}
