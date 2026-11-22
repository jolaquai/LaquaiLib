namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.SkipLast{TSource}(IEnumerable{TSource}, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> SkipLast(int count) => source[..^count];
    }

    extension<TSource>(in Span<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.SkipLast{TSource}(IEnumerable{TSource}, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> SkipLast(int count) => source[..^count];
    }
}