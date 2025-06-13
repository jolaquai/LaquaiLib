#if !NET10_0_OR_GREATER

namespace LaquaiLib.Extensions;

// Retain only for .NET 9 since .NET 10 introduces these methods in System.Linq
public static partial class IEnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        /// <summary>
        /// Produces the set difference of two sequences according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="source">The first sequence to compare.</param>
        /// <param name="other">The second sequence to compare.</param>
        /// <param name="keySelector">The <see cref="Func{T, TResult}"/> that is passed each element of the source sequence and returns the key to use for comparison.</param>
        /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
        /// <remarks>Basically just another <see cref="Enumerable.ExceptBy{TSource, TKey}(IEnumerable{TSource}, IEnumerable{TKey}, Func{TSource, TKey})"/> overload that... actually makes sense.</remarks>
        public IEnumerable<T> ExceptBy<TKey>(IEnumerable<T> other, Func<T, TKey> keySelector)
        {
            var keys = new HashSet<TKey>(other.Select(keySelector));
            foreach (var element in source)
            {
                if (keys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Shuffles the elements in the input sequence using <see cref="Random.Shared"/>.
        /// </summary>
        /// <remarks>
        /// If the calling code already has an instance of <see cref="Random"/>, it should use the <see cref="Shuffle{T}(IEnumerable{T}, Random)"/> overload.
        /// </remarks>
        /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
        /// <param name="source">The input sequence.</param>
        /// <returns>A shuffled sequence of the elements in the input sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> Shuffle() => source.Shuffle(System.Random.Shared);
        /// <summary>
        /// Shuffles the elements in the input sequence, using a specified <see cref="Random"/> instance.
        /// </summary>
        /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
        /// <param name="source">The input sequence.</param>
        /// <param name="random">The <see cref="Random"/> instance to use for shuffling.</param>
        /// <returns>A shuffled sequence of the elements in the input sequence.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> Shuffle(Random random) => source.OrderBy(_ => random.Next());
    }
}
#endif
