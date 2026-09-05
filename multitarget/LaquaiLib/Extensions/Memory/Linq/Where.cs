namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <summary>
        /// Filters the elements of the <see cref="ReadOnlySpan{T}"/> becased on a <paramref name="predicate"/> function and stores all matching elements in a specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="predicate">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a <see langword="bool"/> indicating whether the element should be included in the result.</param>
        /// <param name="destination">A <see cref="Span{T}"/> to store the results of the filtering.</param>
        /// <returns>The number of elements written to in <paramref name="destination"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the filtered elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Where(Func<TSource, bool> predicate, Span<TSource> destination)
        {
            var requiredSpace = 0;
            for (var i = 0; i < source.Length; i++)
                if (predicate(source[i]))
                    requiredSpace++;
            if (destination.Length < requiredSpace)
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            var index = 0;
            for (var i = 0; i < source.Length; i++)
                if (predicate(source[i]))
                    destination[index++] = source[i];
            return requiredSpace;
        }

        /// <summary>
        /// Filters the elements of the <see cref="ReadOnlySpan{T}"/> becased on a <paramref name="predicate"/> function and stores all matching elements in a specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="predicate">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and its index in the source <see cref="ReadOnlySpan{T}"/> and returns a <see langword="bool"/> indicating whether the element should be included in the result.</param>
        /// <param name="destination">A <see cref="Span{T}"/> to store the results of the filtering.</param>
        /// <returns>The number of elements written to in <paramref name="destination"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the filtered elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Where(Func<TSource, int, bool> predicate, Span<TSource> destination)
        {
            var requiredSpace = 0;
            for (var i = 0; i < source.Length; i++)
                if (predicate(source[i], i))
                    requiredSpace++;
            if (destination.Length < requiredSpace)
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            var index = 0;
            for (var i = 0; i < source.Length; i++)
                if (predicate(source[i], i))
                    destination[index++] = source[i];
            return requiredSpace;
        }
    }
}