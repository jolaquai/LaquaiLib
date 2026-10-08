namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.TakeLast{TSource}(IEnumerable{TSource}, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> TakeLast(int count) => source.Length < count ? source : source[^count..];
    }

    extension<TSource>(in Span<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.TakeLast{TSource}(IEnumerable{TSource}, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> TakeLast(int count) => source.Length < count ? source : source[^count..];
    }
}