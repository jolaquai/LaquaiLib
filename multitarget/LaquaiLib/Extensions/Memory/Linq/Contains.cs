namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
#if !NET10_0_OR_GREATER
        /// <inheritdoc cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource, IEqualityComparer{TSource})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TSource value, IEqualityComparer<TSource> comparer) => source.IndexOf(value, comparer) > -1;
#endif
    }
}