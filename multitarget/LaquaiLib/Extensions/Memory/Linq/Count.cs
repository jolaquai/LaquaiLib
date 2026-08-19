namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count() => source.Length;

        /// <inheritdoc cref="Enumerable.Count{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count(Func<TSource, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
                if (predicate(source[i]))
                    count++;
            return count;
        }
    }
}