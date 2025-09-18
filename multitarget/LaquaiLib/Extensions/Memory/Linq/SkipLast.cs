namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.SkipLast{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> SkipLast(int count) => source[..^count];
    }

    extension<TSource>(Span<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.SkipLast{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> SkipLast(int count) => source[..^count];
    }
}