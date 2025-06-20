namespace LaquaiLib.Extensions.Memory.Linq;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault() => SingleOrDefault(source, default(TSource));

        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault(TSource defaultValue) => source.Length switch
        {
            0 => defaultValue,
            > 1 => throw new InvalidOperationException("Span contains more than one element."),
            _ => source[0]
        };

        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault(Func<TSource, bool> predicate) => SingleOrDefault(source, predicate, default);

        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault(Func<TSource, bool> predicate, TSource defaultValue)
        {
            var result = defaultValue;
            var found = false;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    if (found)
                    {
                        throw new InvalidOperationException("Span contains more than one element that matches the predicate.");
                    }
                    result = source[i];
                    found = true;
                }
            }
            return found ? result : defaultValue;
        }
    }
}