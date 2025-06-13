using LaquaiLib.Collections.LimitedCollections;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Queue{T}"/> Type.
/// </summary>
public static class QueueExtensions
{
    extension<T>(Queue<T> queue)
    {
        /// <summary>
        /// Adds items to the end of the <see cref="Queue{T}"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the collection.</typeparam>
        /// <param name="queue">The <see cref="Queue{T}"/> instance to add the items from <paramref name="items"/> to.</param>
        /// <param name="items">The items to add to <paramref name="queue"/>.</param>
        public void EnqueueRange(params ReadOnlySpan<T> items)
        {
            for (var i = 0; i < items.Length; i++)
            {
                queue.Enqueue(items[i]);
            }
        }
        /// <summary>
        /// Adds items from a collection to the end of the <see cref="Queue{T}"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the collection.</typeparam>
        /// <param name="queue">The <see cref="Queue{T}"/> instance to add the items from the <paramref name="collection"/> to.</param>
        /// <param name="collection">A collection of items to add to <paramref name="queue"/>.</param>
        public void EnqueueRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                queue.Enqueue(item);
            }
        }
    }

    extension<T>(LimitedQueue<T> queue)
    {
        /// <summary>
        /// Adds items to the end of the <see cref="Queue{T}"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the collection.</typeparam>
        /// <param name="queue">The <see cref="Queue{T}"/> instance to add the items from <paramref name="items"/> to.</param>
        /// <param name="items">The items to add to <paramref name="queue"/>.</param>
        public void EnqueueRange(params ReadOnlySpan<T> items)
        {
            for (var i = 0; i < items.Length; i++)
            {
                queue.Enqueue(items[i]);
            }
        }
        /// <summary>
        /// Adds items from a collection to the end of the <see cref="Queue{T}"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the collection.</typeparam>
        /// <param name="queue">The <see cref="Queue{T}"/> instance to add the items from the <paramref name="collection"/> to.</param>
        /// <param name="collection">A collection of items to add to <paramref name="queue"/>.</param>
        public void EnqueueRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                queue.Enqueue(item);
            }
        }
    }
}
