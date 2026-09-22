namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <summary>
        /// Merges two <see cref="ReadOnlySpan{T}"/>s into another <see cref="Span{T}"/> by applying a result selector function to each pair of elements.
        /// </summary>
        /// <typeparam name="TSecond">The type of the elements in the second <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <param name="second">The second <see cref="ReadOnlySpan{T}"/> to merge with the source <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="resultSelector">A <see cref="Func{T1, T2, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and the corresponding element of the second <see cref="ReadOnlySpan{T}"/>, and returns a transformed element.</param>
        /// <param name="destination">The destination <see cref="Span{T}"/> to store the results of the merge.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the results.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Zip<TSecond, TResult>(ReadOnlySpan<TSecond> second, Func<TSource, TSecond, TResult> resultSelector, Span<TResult> destination)
        {
            var minLen = Math.Min(source.Length, second.Length);
            if (minLen > destination.Length)
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            var i = 0;
            for (; i < source.Length && i < second.Length; i++)
                destination[i] = resultSelector(source[i], second[i]);
            return minLen;
        }

        /// <summary>
        /// Merges two <see cref="ReadOnlySpan{T}"/>s into another <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TSecond">The type of the elements in the second <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <param name="second">The second <see cref="ReadOnlySpan{T}"/> to merge with the source <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="destination">The destination <see cref="Span{T}"/> to store the results of the merge.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the results.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Zip<TSecond>(ReadOnlySpan<TSecond> second, Span<(TSource, TSecond)> destination)
        {
            var minLen = Math.Min(source.Length, second.Length);
            if (minLen > destination.Length)
                throw new ArgumentException("Destination span is too short.", nameof(destination));

            var i = 0;
            for (; i < source.Length && i < second.Length; i++)
                destination[i] = (source[i], second[i]);
            return minLen;
        }

        /// <summary>
        /// Merges three <see cref="ReadOnlySpan{T}"/>s into another <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TSecond">The type of the elements in the second <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <typeparam name="TThird">The type of the elements in the third <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <param name="second">The second <see cref="ReadOnlySpan{T}"/> to merge with the source <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="third">The third <see cref="ReadOnlySpan{T}"/> to merge with the source <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="destination">The destination <see cref="Span{T}"/> to store the results of the merge.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the results.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Zip<TSecond, TThird>(ReadOnlySpan<TSecond> second, ReadOnlySpan<TThird> third, Span<(TSource, TSecond, TThird)> destination)
        {
            var minLen = Math.Min(source.Length, Math.Min(second.Length, third.Length));
            if (minLen > destination.Length)
                throw new ArgumentException("Destination span is too short.", nameof(destination));

            var i = 0;
            for (; i < source.Length && i < second.Length && i < third.Length; i++)
                destination[i] = (source[i], second[i], third[i]);
            return minLen;
        }
    }
}