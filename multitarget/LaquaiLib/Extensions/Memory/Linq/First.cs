namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.First{TSource}(IEnumerable{TSource})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource First() => source[0];

        /// <inheritdoc cref="Enumerable.First{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource First(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
                if (predicate(source[i]))
                    return source[i];
            throw new InvalidOperationException("Span does not contain any elements that match the predicate.");
        }
    }
}