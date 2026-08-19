namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Last{TSource}(IEnumerable{TSource})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Last() => source[^1];

        /// <inheritdoc cref="Enumerable.Last{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Last(Func<TSource, bool> predicate)
        {
            for (var i = source.Length - 1; i >= 0; i--)
                if (predicate(source[i]))
                    return source[i];
            throw new InvalidOperationException("Span does not contain any elements that match the predicate.");
        }
    }
}