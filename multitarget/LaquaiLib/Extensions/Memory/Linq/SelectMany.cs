namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into an <see cref="IEnumerable{T}"/> of <typeparamref name="TResult"/> and stores those elements in the specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns an <see cref="IEnumerable{T}"/> of projected elements.</param>
        /// <param name="destination">The <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <returns>The number of elements written to <paramref name="destination"/>.</returns>
        /// <remarks>
        /// This and the other overloads of this method group should only be used with spans owned and controlled by the caller to ensure no unexpected results occur.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SelectMany<TResult>(Func<TSource, IEnumerable<TResult>> selector, Span<TResult> destination)
        {
            var destIndex = 0;
            for (var i = 0; i < source.Length; i++)
                foreach (var item in selector(source[i]))
                {
                    if (destIndex >= destination.Length)
                        return destIndex; // Last assigned index

                    destination[destIndex++] = item;
                }
            return destIndex;
        }

        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into an <see cref="IEnumerable{T}"/> of <typeparamref name="TResult"/> and stores those elements in the specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="selector">A <see cref="Func{T1, T2, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and its index in the source <see cref="ReadOnlySpan{T}"/> and returns an <see cref="IEnumerable{T}"/> of projected elements.</param>
        /// <param name="destination">The <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <returns>The number of elements written to <paramref name="destination"/>.</returns>
        /// <remarks>
        /// This and the other overloads of this method group should only be used with spans owned and controlled by the caller to ensure no unexpected results occur.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SelectMany<TResult>(Func<TSource, int, IEnumerable<TResult>> selector, Span<TResult> destination)
        {
            var destIndex = 0;
            for (var i = 0; i < source.Length; i++)
                foreach (var item in selector(source[i], i))
                {
                    if (destIndex >= destination.Length)
                        return destIndex; // Last assigned index

                    destination[destIndex++] = item;
                }
            return destIndex;
        }

        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into an <see cref="IEnumerable{T}"/> of <typeparamref name="TCollection"/>, which is then projected into an <see cref="IEnumerable{T}"/> of <typeparamref name="TResult"/>, and stores those elements in the specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The type of the elements in the collection returned by <paramref name="collectionSelector"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="collectionSelector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns an <see cref="IEnumerable{T}"/> of <typeparamref name="TCollection"/> of projected elements.</param>
        /// <param name="resultSelector">A <see cref="Func{T1, T2, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and, in turn, each corresponding element from the <see cref="IEnumerable{T}"/> returned by <paramref name="collectionSelector"/>, and returns the projected elements of type <typeparamref name="TResult"/>.</param>
        /// <param name="destination">The <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <returns>The number of elements written to <paramref name="destination"/>.</returns>
        /// <remarks>
        /// This and the other overloads of this method group should only be used with spans owned and controlled by the caller to ensure no unexpected results occur.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SelectMany<TCollection, TResult>(Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector, Span<TResult> destination)
        {
            var destIndex = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var collection = collectionSelector(source[i]);
                foreach (var item in collection)
                {
                    if (destIndex >= destination.Length)
                        return destIndex;
                    destination[destIndex++] = resultSelector(source[i], item);
                }
            }
            return destIndex;
        }

        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into an <see cref="IEnumerable{T}"/> of <typeparamref name="TCollection"/>, which is then projected into an <see cref="IEnumerable{T}"/> of <typeparamref name="TResult"/>, and stores those elements in the specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The type of the elements in the collection returned by <paramref name="collectionSelector"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="collectionSelector">A <see cref="Func{T1, T2, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and its index in the source <see cref="ReadOnlySpan{T}"/> and returns an <see cref="IEnumerable{T}"/> of <typeparamref name="TCollection"/> of projected elements.</param>
        /// <param name="resultSelector">A <see cref="Func{T1, T2, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and, in turn, each corresponding element from the <see cref="IEnumerable{T}"/> returned by <paramref name="collectionSelector"/>, and returns the projected elements of type <typeparamref name="TResult"/>.</param>
        /// <param name="destination">The <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <returns>The number of elements written to <paramref name="destination"/>.</returns>
        /// <remarks>
        /// This and the other overloads of this method group should only be used with spans owned and controlled by the caller to ensure no unexpected results occur.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SelectMany<TCollection, TResult>(Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector, Span<TResult> destination)
        {
            var destIndex = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var collection = collectionSelector(source[i], i);
                foreach (var item in collection)
                {
                    if (destIndex >= destination.Length)
                        return destIndex;
                    destination[destIndex++] = resultSelector(source[i], item);
                }
            }
            return destIndex;
        }
    }
}