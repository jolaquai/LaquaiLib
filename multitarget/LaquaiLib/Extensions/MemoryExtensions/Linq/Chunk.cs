using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.Extensions.MemoryExtensions.Linq;

public static partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Chunk{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanChunkEnumerable<TSource> Chunk(int size) => new SpanChunkEnumerable<TSource>(source, size);
    }
}