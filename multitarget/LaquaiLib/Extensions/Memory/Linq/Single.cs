namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Single{TSource}(IEnumerable{TSource})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Single() => source.Length != 1 ? throw new InvalidOperationException("Span does not contain exactly one element.") : source[0];

        /// <inheritdoc cref="Enumerable.Single{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Single(Func<TSource, bool> predicate)
        {
            TSource result = default;
            var found = false;
            for (var i = 0; i < source.Length; i++)
                if (predicate(source[i]))
                {
                    if (found)
                        throw new InvalidOperationException("Span contains more than one element that matches the predicate.");
                    result = source[i];
                    found = true;
                }
            return !found ? throw new InvalidOperationException("Span does not contain any elements that match the predicate.") : result;
        }
    }
}