namespace LaquaiLib.Extensions.MemoryExtensions.Linq;

public static partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.TakeLast{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> TakeLast(int count) => source.Length < count ? source : source[^count..];
    }

    extension<TSource>(Span<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.TakeLast{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> TakeLast(int count) => source.Length < count ? source : source[^count..];
    }
}