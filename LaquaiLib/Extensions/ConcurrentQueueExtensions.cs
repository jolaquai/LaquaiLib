using System.Collections.Concurrent;

using LaquaiLib.Classes.Collections.Concurrent;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="ConcurrentQueue{T}"/> Type.
/// </summary>
public static class ConcurrentQueueExtensions
{
    /// <summary>
    /// Adds items to the end of the <see cref="ConcurrentQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the collection.</typeparam>
    /// <param name="queue">The <see cref="ConcurrentQueue{T}"/> instance to add the items from <paramref name="items"/> to.</param>
    /// <param name="item">The first item to add to <paramref name="queue"/>.</param>
    /// <param name="items">Any more items to add to <paramref name="queue"/>.</param>
    public static void Enqueue<T>(this ConcurrentQueue<T> queue, T item, params T[] items)
    {
        queue.Enqueue(item);
        foreach (T i in items)
        {
            queue.Enqueue(i);
        }
    }

    /// <summary>
    /// Adds items from a collection to the end of the <see cref="ConcurrentQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the collection.</typeparam>
    /// <param name="queue">The <see cref="ConcurrentQueue{T}"/> instance to add the items from the <paramref name="collection"/> to.</param>
    /// <param name="collection">A collection of items to add to <paramref name="queue"/>.</param>
    public static void Enqueue<T>(this ConcurrentQueue<T> queue, IEnumerable<T> collection)
    {
        foreach (T item in collection)
        {
            queue.Enqueue(item);
        }
    }

    /// <summary>
    /// Adds items to the end of the <see cref="ConcurrentQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the collection.</typeparam>
    /// <param name="queue">The <see cref="ConcurrentQueue{T}"/> instance to add the items from <paramref name="items"/> to.</param>
    /// <param name="item">The first item to add to <paramref name="queue"/>.</param>
    /// <param name="items">Any more items to add to <paramref name="queue"/>.</param>
    public static void Enqueue<T>(this LimitedConcurrentQueue<T> queue, T item, params T[] items)
    {
        queue.Enqueue(item);
        foreach (T i in items)
        {
            queue.Enqueue(i);
        }
    }

    /// <summary>
    /// Adds items from a collection to the end of the <see cref="ConcurrentQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the collection.</typeparam>
    /// <param name="queue">The <see cref="ConcurrentQueue{T}"/> instance to add the items from the <paramref name="collection"/> to.</param>
    /// <param name="collection">A collection of items to add to <paramref name="queue"/>.</param>
    public static void Enqueue<T>(this LimitedConcurrentQueue<T> queue, IEnumerable<T> collection)
    {
        foreach (T item in collection)
        {
            queue.Enqueue(item);
        }
    }
}
