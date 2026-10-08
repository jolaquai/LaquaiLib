namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Take{TSource}(IEnumerable{TSource}, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> Take(int count) => source.Length < count ? source : source[..count];

        /// <inheritdoc cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Range)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> Take(Range range) => source[range];
    }

    extension<TSource>(in Span<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Take{TSource}(IEnumerable{TSource}, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> Take(int count) => source.Length < count ? source : source[..count];

        /// <inheritdoc cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Range)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> Take(Range range) => source[range];
    }
}