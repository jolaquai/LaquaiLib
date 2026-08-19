namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault() => source.Length > 0 ? source[^1] : default;

        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, TSource)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault(TSource defaultValue) => source.Length > 0 ? source[^1] : defaultValue;

        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault(Func<TSource, bool> predicate)
        {
            for (var i = source.Length - 1; i >= 0; i--)
                if (predicate(source[i]))
                    return source[i];
            return default;
        }

        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault(Func<TSource, bool> predicate, TSource defaultValue)
        {
            for (var i = source.Length - 1; i >= 0; i--)
                if (predicate(source[i]))
                    return source[i];
            return defaultValue;
        }
    }
}