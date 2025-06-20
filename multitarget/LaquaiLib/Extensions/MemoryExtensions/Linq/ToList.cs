namespace LaquaiLib.Extensions.MemoryExtensions.Linq;

public static partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<TSource> ToList()
        {
            var list = new List<TSource>();
            list.SetCount(source.Length);
            var span = list.AsSpan();
            source.CopyTo(span);
            return list;
        }
    }
}