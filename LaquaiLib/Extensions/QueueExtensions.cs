using LaquaiLib.Classes.Collections;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Queue{T}"/> Type.
/// </summary>
public static class QueueExtensions
{
    /// <summary>
    /// Adds items to the end of the <see cref="Queue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the collection.</typeparam>
    /// <param name="queue">The <see cref="Queue{T}"/> instance to add the items from <paramref name="items"/> to.</param>
    /// <param name="item">The first item to add to <paramref name="queue"/>.</param>
    /// <param name="items">Any more items to add to <paramref name="queue"/>.</param>
    public static void Enqueue<T>(this Queue<T> queue, T item, params T[] items)
    {
        queue.Enqueue(item);
        foreach (var i in items)
        {
            queue.Enqueue(i);
        }
    }

    /// <summary>
    /// Adds items from a collection to the end of the <see cref="Queue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the collection.</typeparam>
    /// <param name="queue">The <see cref="Queue{T}"/> instance to add the items from the <paramref name="collection"/> to.</param>
    /// <param name="collection">A collection of items to add to <paramref name="queue"/>.</param>
    public static void Enqueue<T>(this Queue<T> queue, IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            queue.Enqueue(item);
        }
    }

    /// <summary>
    /// Adds items to the end of the <see cref="Queue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the collection.</typeparam>
    /// <param name="queue">The <see cref="Queue{T}"/> instance to add the items from <paramref name="items"/> to.</param>
    /// <param name="item">The first item to add to <paramref name="queue"/>.</param>
    /// <param name="items">Any more items to add to <paramref name="queue"/>.</param>
    public static void Enqueue<T>(this LimitedQueue<T> queue, T item, params T[] items)
    {
        queue.Enqueue(item);
        foreach (var i in items)
        {
            queue.Enqueue(i);
        }
    }

    /// <summary>
    /// Adds items from a collection to the end of the <see cref="Queue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the collection.</typeparam>
    /// <param name="queue">The <see cref="Queue{T}"/> instance to add the items from the <paramref name="collection"/> to.</param>
    /// <param name="collection">A collection of items to add to <paramref name="queue"/>.</param>
    public static void Enqueue<T>(this LimitedQueue<T> queue, IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            queue.Enqueue(item);
        }
    }
}
