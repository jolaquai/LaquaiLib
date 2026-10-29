namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        // Imma be honest, I stole these right out of System.Linq
        /// <summary>
        /// Determines whether a <see cref="ReadOnlySpan{T}"/> contains exactly one element and returns that element if so, otherwise returns the specified <paramref name="defaultValue"/>.
        /// This behaves exactly like <see cref="SingleOrDefault{TSource}(in ReadOnlySpan{TSource}, TSource)"/> without throwing exceptions.
        /// </summary>
        /// <param name="defaultValue">The value to return if the source <see cref="ReadOnlySpan{T}"/> contains no elements or more than one element.</param>
        /// <returns>The single element in the source <see cref="ReadOnlySpan{T}"/>, or <paramref name="defaultValue"/> if the sequence contains no or more than one element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource OnlyOrDefault(TSource defaultValue = default) => source.Length != 1 ? defaultValue : source[0];
        /// <summary>
        /// Determines whether a <see cref="ReadOnlySpan{T}"/> contains exactly one element that satisfies a <paramref name="predicate"/> and returns that element if so, otherwise returns the specified <paramref name="defaultValue"/>.
        /// This behaves exactly like <see cref="SingleOrDefault{TSource}(in ReadOnlySpan{TSource}, Func{TSource, bool}, TSource)"/> without throwing exceptions.
        /// </summary>
        /// <param name="predicate">The condition to check for.</param>
        /// <param name="defaultValue">The value to return if the source <see cref="ReadOnlySpan{T}"/> contains no elements or more than one element.</param>
        /// <returns>The single element in the source <see cref="ReadOnlySpan{T}"/> that satisfies the <paramref name="predicate"/>, or <paramref name="defaultValue"/> if the sequence contains no or more than one element that satisfies the <paramref name="predicate"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource OnlyOrDefault(Func<TSource, bool> predicate, TSource defaultValue = default)
        {
            var result = defaultValue;
            var found = false;
            for (var i = 0; i < source.Length; i++)
                if (predicate(source[i]))
                {
                    if (found)
                        return defaultValue;
                    result = source[i];
                    found = true;
                }
            return found ? result : defaultValue;
        }
    }
}