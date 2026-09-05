using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Chunk{TSource}(IEnumerable{TSource}, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanChunkEnumerable<TSource> Chunk(int size) => new SpanChunkEnumerable<TSource>(source, size);
    }
}