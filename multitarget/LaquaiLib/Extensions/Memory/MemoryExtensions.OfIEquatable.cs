using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.Extensions;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

public static partial class MemoryExtensions
{
    extension<T>(in ReadOnlySpan<T> span) where T : IEquatable<T>
    {
        /// <summary>
        /// Returns a <see cref="SpanSplitEnumerable{T}"/> that enumerates the segments of a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/>s that are separated by any of the <typeparamref name="T"/>s specified by <paramref name="splits"/>.
        /// </summary>
        /// <param name="splits">The <see langword="t"/>s to use as delimiters.</param>
        /// <returns>The created <see cref="SpanSplitEnumerable{T}"/>.</returns>
        /// <remarks><typeparamref name="T"/> must implement <see cref="IEquatable{T}"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanSplitEnumerable<T> EnumerateSplits(ReadOnlySpan<T> splits) => new SpanSplitEnumerable<T>(span, splits);
        /// <summary>
        /// Returns a <see cref="SpanSplitEnumerable{T}"/> that enumerates the segments of a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/>s that are separated by the specified <paramref name="sequence"/>.
        /// </summary>
        /// <param name="sequence">The sequence of <typeparamref name="T"/>s to use as a delimiter.</param>
        /// <returns>The created <see cref="SpanSplitEnumerable{T}"/>.</returns>
        /// <remarks><typeparamref name="T"/> must implement <see cref="IEquatable{T}"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanSplitBySequenceEnumerable<T> EnumerateSplitsBySequence(ReadOnlySpan<T> sequence) => new SpanSplitBySequenceEnumerable<T>(span, sequence);
    }
}
