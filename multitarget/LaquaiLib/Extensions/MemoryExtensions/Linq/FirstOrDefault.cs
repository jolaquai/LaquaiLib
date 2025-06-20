namespace LaquaiLib.Extensions.MemoryExtensions.Linq;

public static partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault() => source.Length > 0 ? source[0] : default;

        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault(TSource defaultValue) => source.Length > 0 ? source[0] : defaultValue;

        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return default;
        }

        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault(Func<TSource, bool> predicate, TSource defaultValue)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return defaultValue;
        }
    }
}