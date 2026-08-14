using LaquaiLib.Util.Misc;
using LaquaiLib.Wrappers;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerator{T}"/> Type.
/// </summary>
public static class IEnumeratorExtensions
{
    extension<T>(IEnumerator<T> source)
    {
        /// <summary>
        /// Consumes the specified <see cref="IEnumerator{T}"/> starting at its current position, yielding each element.
        /// </summary>
        /// <returns>The elements of the <paramref name="source"/> as an <see cref="IEnumerable{T}"/>.</returns>
        public IEnumerable<T> AsEnumerable()
        {
            while (source.MoveNext())
            {
                yield return source.Current;
            }
        }

        /// <summary>
        /// Returns an <see cref="IAsyncEnumerator{T}"/> wrapper around the specified <see cref="IEnumerator{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the <see cref="IEnumerator{T}"/>.</typeparam>
        /// <returns>The <paramref name="source"/> as an <see cref="IAsyncEnumerator{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncEnumerator<T> AsAsynchronous() => new AsyncEnumeratorWrapper<T>(source);
    }
}