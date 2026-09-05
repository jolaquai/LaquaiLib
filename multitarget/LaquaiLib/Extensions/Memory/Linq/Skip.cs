namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Skip{TSource}(IEnumerable{TSource}, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> Skip(int count) => source[count..];
    }

    extension<TSource>(in Span<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Skip{TSource}(IEnumerable{TSource}, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> Skip(int count) => source[count..];
    }
}