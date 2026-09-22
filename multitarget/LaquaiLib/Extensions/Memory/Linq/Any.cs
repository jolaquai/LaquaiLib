namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Any{TSource}(IEnumerable{TSource})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any() => source.Length > 0;

        /// <inheritdoc cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
                if (predicate(source[i]))
                    return true;
            return false;
        }
    }
}