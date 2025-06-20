namespace LaquaiLib.Extensions.Memory.Linq;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.TryGetNonEnumeratedCount{TSource}(IEnumerable{TSource}, out int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNonEnumeratedCount(out int count) => (count = source.Length) > -1;
    }
}