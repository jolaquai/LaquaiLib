using LaquaiLib.Wrappers;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for the <see cref="IEnumerator{T}"/> type.
/// </summary>
public static class IEnumeratorExtensions
{
    extension<T>(IEnumerator<T> source)
    {
        /// <summary>
        /// Returns the current instance. Allows <see langword="foreach"/> directly over enumerators.
        /// </summary>
        public IEnumerator<T> GetEnumerator() => source;

        /// <summary>
        /// Returns an <see cref="IAsyncEnumerator{T}"/> wrapper around the specified <see cref="IEnumerator{T}"/>.
        /// </summary>
        /// <returns>The <paramref name="source"/> as an <see cref="IAsyncEnumerator{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncEnumerator<T> AsAsynchronous() => new AsyncEnumeratorWrapper<T>(source);
    }
}