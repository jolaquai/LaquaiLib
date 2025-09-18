namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.LongCount{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long LongCount() => source.Length;

        /// <inheritdoc cref="Enumerable.LongCount{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long LongCount(Func<TSource, bool> predicate) => Count(source, predicate);
    }
}