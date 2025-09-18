namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        // These are safe to invoke when source and destination point to the same location (if the types are compatible)
        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into a new form and stores the results in a specified destination <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a transformed element.</param>
        /// <param name="destination">A <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the projected elements.</exception>
        /// <returns>The number of elements written to in <paramref name="destination"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Select<TResult>(Func<TSource, TResult> selector, Span<TResult> destination)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }
            for (var i = 0; i < source.Length; i++)
            {
                destination[i] = selector(source[i]);
            }
            return source.Length;
        }

        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into a new form while incorporating the element's index and stores the results in a specified destination <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and its index in the source <see cref="ReadOnlySpan{T}"/> and returns a transformed element.</param>
        /// <param name="destination">A <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the projected elements.</exception>
        /// <returns>The number of elements written to in <paramref name="destination"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Select<TResult>(Func<TSource, int, TResult> selector, Span<TResult> destination)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }
            for (var i = 0; i < source.Length; i++)
            {
                destination[i] = selector(source[i], i);
            }
            return source.Length;
        }
    }
}