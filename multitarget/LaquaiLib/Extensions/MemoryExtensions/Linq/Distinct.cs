namespace LaquaiLib.Extensions.MemoryExtensions.Linq;

public static partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Distinct{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Distinct() => Distinct(source, EqualityComparer<TSource>.Default);

        /// <inheritdoc cref="Enumerable.Distinct{TSource}(IEnumerable{TSource}, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Distinct(IEqualityComparer<TSource> comparer) => ToHashSet(source, comparer);

        /// <summary>
        /// Filters the source <see cref="ReadOnlySpan{T}"/> for distinct elements and stores them in the specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="destination">The destination <see cref="Span{T}"/> to store the distinct elements.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> implementation to use for comparing elements.</param>
        /// <returns>The number of distinct elements written to the <paramref name="destination"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="destination"/> is shorter than the source <see cref="ReadOnlySpan{T}"/> (this is enforced because all elements could already be distinct).</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Distinct(Span<TSource> destination, IEqualityComparer<TSource> comparer = null)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }

            comparer ??= EqualityComparer<TSource>.Default;
            var destIndex = 0;
            // Around 10^7 elements is the point where the performance of using a HashSet becomes better than using a simple loop to check for duplicates
            // This and this method alone is unfortunately the only place where we can avoid HashSet
            if (source.Length < 10_000_000)
            {
                for (var i = 0; i < source.Length; i++)
                {
                    if (destIndex == 0 || destination[..destIndex].IndexOf(source[i], comparer) < 0)
                    {
                        destination[destIndex++] = source[i];
                    }
                }
                return destIndex;
            }

            var hashSet = new HashSet<TSource>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                if (hashSet.Add(source[i]))
                {
                    destination[destIndex++] = source[i];
                }
            }
            return destIndex;
        }
    }
}