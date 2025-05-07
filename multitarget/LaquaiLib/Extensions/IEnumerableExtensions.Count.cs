namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        /// <summary>
        /// Determines if a sequence contains less than the specified number of elements.
        /// </summary>
        /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
        /// <param name="source">The input sequence.</param>
        /// <param name="n">The number of elements to check for.</param>
        /// <returns><see langword="true"/> if the input sequence contains less than <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
        public bool HasLessThan(int n) => (source.TryGetNonEnumeratedCount(out var count) ? count : source.Count()) < n;
        /// <summary>
        /// Determines if a sequence contains less than or exactly the specified number of elements.
        /// </summary>
        /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
        /// <param name="source">The input sequence.</param>
        /// <param name="n">The number of elements to check for.</param>
        /// <returns><see langword="true"/> if the input sequence contains less than or exactly <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
        public bool HasLessThanOr(int n) => (source.TryGetNonEnumeratedCount(out var count) ? count : source.Count()) <= n;
        /// <summary>
        /// Determines if a sequence contains exactly the specified number of elements.
        /// An attempt is made to avoid enumerating the input sequence, but if this fails, it is done regardless.
        /// </summary>
        /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
        /// <param name="source">The input sequence.</param>
        /// <param name="n">The number of elements to check for.</param>
        /// <returns>The result of the check.</returns>
        public bool HasExactly(int n) => (source.TryGetNonEnumeratedCount(out var count) ? count : source.Count()) == n;
        /// <summary>
        /// Determines if a sequence contains more than or exactly the specified number of elements.
        /// </summary>
        /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
        /// <param name="source">The input sequence.</param>
        /// <param name="n">The number of elements to check for.</param>
        /// <returns><see langword="true"/> if the input sequence contains more than or exactly <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
        public bool HasMoreThanOr(int n) => (source.TryGetNonEnumeratedCount(out var count) ? count : source.Count()) >= n;
        /// <summary>
        /// Determines if a sequence contains more than the specified number of elements.
        /// </summary>
        /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
        /// <param name="source">The input sequence.</param>
        /// <param name="n">The number of elements to check for.</param>
        /// <returns><see langword="true"/> if the input sequence contains more than <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
        public bool HasMoreThan(int n) => (source.TryGetNonEnumeratedCount(out var count) ? count : source.Count()) > n;

        /// <summary>
        /// Builds a <see cref="Dictionary{TKey, TValue}"/>, mapping the item to the number of times it appears in the input sequence.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the input sequence.</typeparam>
        /// <param name="source">The input sequence.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing items.</param>
        /// <returns>The resulting <see cref="Dictionary{TKey, TValue}"/>.</returns>
        public Dictionary<T, int> Counts(IEqualityComparer<T> comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            var counts = new Dictionary<T, int>(comparer);

            foreach (var item in source)
            {
                CollectionsMarshal.GetValueRefOrAddDefault(counts, item, out _)++;
            }
            return counts;
        }
        /// <summary>
        /// Builds a <see cref="Dictionary{TKey, TValue}"/>, mapping the item to the number of times it appears as counted using a <paramref name="selector"/> function in the input sequence.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the input sequence.</typeparam>
        /// <param name="source">The input sequence.</param>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that produces the keys to count.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing items.</param>
        /// <returns>The resulting <see cref="Dictionary{TKey, TValue}"/>.</returns>
        public Dictionary<T, int> CountsBy<TSelect>(Func<T, TSelect> selector, IEqualityComparer<TSelect> comparer = null)
        {
            ArgumentNullException.ThrowIfNull(selector);

            comparer ??= EqualityComparer<TSelect>.Default;
            var counts = new Dictionary<TSelect, (T, int)>(comparer);

            foreach (var item in source)
            {
                var key = selector(item);
                ref var theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(counts, key, out _);
                theRef.Item1 ??= item;
                theRef.Item2++;
            }
            return counts.ToDictionary(t => t.Value.Item1, t => t.Value.Item2);
        }
    }
}