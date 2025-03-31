using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Collections.LimitedCollections;

/// <summary>
/// Implements a queue data structure with a maximum number of items allowed in it.
/// When the collection is at capacity and it is attempted to enqueue another object, the oldest is removed.
/// </summary>
/// <typeparam name="T">The Type of the items in the collection.</typeparam>
/// <remarks>
/// This Type is thread-safe.
/// </remarks>
public sealed class ConcurrentLimitedQueue<T> : IReadOnlyCollection<T>
{
    private static ArgumentException NeedCapacityGreaterThanInitialItemsException => new ArgumentException($"The passed initial capacity may not be smaller than the number of items passed to create the {nameof(ConcurrentLimitedQueue<>)} with.");

    // there is no way around a lock to prevent the data race between queue.Count and Capacity since both are freely mutable
    private readonly Lock _lock = new Lock();
    private ConcurrentQueue<T> queue;

    private volatile int capacity;
    /// <summary>
    /// The capacity of this <see cref="ConcurrentLimitedQueue{T}"/>.
    /// </summary>
    public int Capacity
    {
        get => capacity;
        private set => capacity = value;
    }

    /// <summary>
    /// Initializes a new empty <see cref="ConcurrentLimitedQueue{T}"/> with the given maximum <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The maximum number of items this <see cref="ConcurrentLimitedQueue{T}"/> can hold before discarding the oldest value.</param>
    private ConcurrentLimitedQueue(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        Capacity = capacity;
    }
    /// <summary>
    /// Initializes a new <see cref="ConcurrentLimitedQueue{T}"/> with the items from the passed <paramref name="enumerable"/>. Its maximum capacity is set to <paramref name="enumerable"/>'s length.
    /// </summary>
    /// <param name="enumerable">The collection to copy the new <see cref="ConcurrentLimitedQueue{T}"/>'s items from.</param>
    public ConcurrentLimitedQueue(IEnumerable<T> enumerable)
    {
        queue = new ConcurrentQueue<T>(enumerable);
        ArgumentOutOfRangeException.ThrowIfZero(queue.Count, nameof(enumerable));
        Capacity = queue.Count;
    }
    /// <summary>
    /// Initializes a new <see cref="ConcurrentLimitedQueue{T}"/> with the items from the passed <paramref name="items"/>. Its maximum capacity is set to <paramref name="items"/>'s length.
    /// </summary>
    /// <param name="items">The <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> to copy the new <see cref="ConcurrentLimitedQueue{T}"/>'s items from.</param>
    public ConcurrentLimitedQueue(params T[] items) : this(items.Length, items) { }
    /// <summary>
    /// Initializes a new <see cref="ConcurrentLimitedQueue{T}"/> with the items from the passed <paramref name="enumerable"/>. Its maximum capacity is set to <paramref name="capacity"/>.
    /// </summary>
    /// <param name="enumerable">The collection to copy the new <see cref="ConcurrentLimitedQueue{T}"/>'s items from.</param>
    /// <param name="capacity">The maximum number of items this <see cref="ConcurrentLimitedQueue{T}"/> can hold before discarding the oldest value.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="capacity"/> is smaller than the number of items in <paramref name="enumerable"/>.</exception>
    public ConcurrentLimitedQueue(int capacity, IEnumerable<T> enumerable) : this(capacity)
    {
        if (enumerable.TryGetNonEnumeratedCount(out var count) && capacity > count)
        {
            throw NeedCapacityGreaterThanInitialItemsException;
        }

        queue = new ConcurrentQueue<T>();
        foreach (var item in enumerable)
        {
            queue.Enqueue(item);

            if (queue.Count > capacity)
            {
                throw NeedCapacityGreaterThanInitialItemsException;
            }
        }
    }
    /// <summary>
    /// Initializes a new <see cref="ConcurrentLimitedQueue{T}"/> with the items from the passed <paramref name="items"/>. Its maximum capacity is set to <paramref name="capacity"/>.
    /// </summary>
    /// <param name="items">The span to copy the new <see cref="ConcurrentLimitedQueue{T}"/>'s items from.</param>
    /// <param name="capacity">The maximum number of items this <see cref="ConcurrentLimitedQueue{T}"/> can hold before discarding the oldest value.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="capacity"/> is smaller than the number of items in <paramref name="items"/>.</exception>
    public ConcurrentLimitedQueue(int capacity, params T[] items) : this(capacity)
    {
        if (Capacity < items.Length)
        {
            throw NeedCapacityGreaterThanInitialItemsException;
        }
        queue = new ConcurrentQueue<T>(items);
    }

    #region Queue methods
    /// <summary>
    /// Adds an item to the end of the <see cref="ConcurrentLimitedQueue{T}"/>, discarding the oldest item if the collection is at maximum capacity.
    /// </summary>
    /// <param name="item">The item to add to the <see cref="ConcurrentLimitedQueue{T}"/>.</param>
    public void Enqueue(T item)
    {
        lock (_lock)
        {
            // Dequeue before Enqueue if the collection is at capacity to prevent resizing the backing store
            if (Capacity <= queue.Count)
            {
                Dequeue();
            }
            queue.Enqueue(item);
        }
    }
    /// <summary>
    /// Attempts to add an item to the end of the <see cref="ConcurrentLimitedQueue{T}"/>.
    /// If this would cause the oldest item to be discarded because the collection is at capacity, the collection remains unchanged.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="ConcurrentLimitedQueue{T}"/>.</param>
    /// <returns>A value indicating whether the collection was modified; <see langword="true"/> if <paramref name="item"/> could be added, otherwise <see langword="false"/>.</returns>
    public bool TryEnqueue(T item)
    {
        lock (_lock)
        {
            if (queue.Count <= Capacity)
            {
                queue.Enqueue(item);
                return true;
            }
            return false;
        }
    }

    // Unfortunately, not even dequeueing is lock-free since other methods could be called in between
    /// <summary>
    /// Removes and returns the object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.
    /// An exception is thrown if the <see cref="ConcurrentLimitedQueue{T}"/> is empty.
    /// </summary>
    /// <returns>The object that is removed from the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.</returns>
    public T Dequeue()
    {
        lock (_lock)
        {
            return queue.TryDequeue(out var result) ? result : throw new InvalidOperationException("The collection is empty.");
        }
    }
    /// <summary>
    /// Attempts to remove and return the object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.
    /// </summary>
    /// <param name="item">An <see langword="out"/> variable that receives the object removed from the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.</param>
    /// <returns>A value indicating whether the collection was modified; <see langword="true"/> if an object could be removed, otherwise <see langword="false"/>.</returns>
    public bool TryDequeue([NotNullWhen(true)] out T item)
    {
        lock (_lock)
        {
            return queue.TryDequeue(out item);
        }
    }
    /// <summary>
    /// Returns the object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/> without removing it.
    /// An exception is thrown if the <see cref="ConcurrentLimitedQueue{T}"/>
    /// </summary>
    /// <returns>The object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.</returns>
    public T Peek()
    {
        lock (_lock)
        {
            return queue.TryPeek(out var result) ? result : throw new InvalidOperationException("The collection is empty.");
        }
    }
    /// <summary>
    /// Attempts to return the object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/> without removing it.
    /// </summary>
    /// <param name="item">An <see langword="out"/> variable that receives the object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.</param>
    /// <returns>A value indicating whether <paramref name="item"/> was assigned; <see langword="true"/> if an object could be returned, otherwise <see langword="false"/>.</returns>
    public bool TryPeek([NotNullWhen(true)] out T item)
    {
        lock (_lock)
        {
            return queue.TryPeek(out item);
        }
    }
    #endregion

    #region IReadOnlyCollection<T>
    /// <summary>
    /// Gets the number of items currently enqueued in the <see cref="ConcurrentLimitedQueue{T}"/>.
    /// </summary>
    public int Count => queue.Count;

    /// <summary>
    /// Gets an enumerator over the items in the <see cref="ConcurrentLimitedQueue{T}"/>.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator<T> GetEnumerator() => queue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
    #region ICollection<T>
    /// <inheritdoc cref="Enqueue(T)"/>
    public void Add(T item) => Enqueue(item);
    /// <summary>
    /// Dequeues all items from the <see cref="LimitedQueue{T}"/>.
    /// </summary>
    public void Clear()
    {
        // Lock here instead of using (Try)Dequeue only since we need to be atomic (other queues mustn't (de)queue while we're working)
        lock (_lock)
        {
            while (queue.TryDequeue(out _))
            {
            }
        }
    }
    /// <summary>
    /// Determines whether the <see cref="LimitedQueue{T}"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="LimitedQueue{T}"/>.</param>
    /// <returns>The <see langword="true"/> if <paramref name="item"/> is found in the <see cref="LimitedQueue{T}"/>, otherwise <see langword="false"/>.</returns>
    public bool Contains(T item)
    {
        lock (_lock)
        {
            return queue.Contains(item);
        }
    }
    /// <summary>
    /// Copies the elements of the queue to a specified array starting at a given index.
    /// </summary>
    /// <param name="array">The destination array where the elements from the queue will be copied.</param>
    /// <param name="arrayIndex">The starting index in the destination array where copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        // Prevent mutation while we're copying to ensure we have a valid snapshot for the copy
        lock (_lock)
        {
            queue.CopyTo(array, arrayIndex);
        }
    }
    /// <summary>
    /// Always returns <see langword="false"/>. Items cannot be directly removed from a <see cref="LimitedQueue{T}"/>;
    /// </summary>
    /// <param name="item">The item to remove from the <see cref="LimitedQueue{T}"/>.</param>
    /// <returns><see langword="false"/> unconditionally.</returns>
    public bool Remove(T item) => false;
    #endregion

    /// <summary>
    /// Resizes the internal backing store of the <see cref="ConcurrentLimitedQueue{T}"/> to the specified <paramref name="size"/>.
    /// The oldest items beyond the new size are discarded.
    /// <para/>This is the only method that will cause new allocations in regards to the backing store.
    /// </summary>
    /// <param name="size">The number of items to allow in the collection.</param>
    public void Resize(int size)
    {
        ArgumentOutOfRangeException.ThrowIfZero(size);
        ArgumentOutOfRangeException.ThrowIfNegative(size);

        lock (_lock)
        {
            if (size == Capacity)
            {
                return;
            }

            Capacity = size;

            if (size > queue.Count)
            {
                var newQueue = new ConcurrentQueue<T>();
                foreach (var item in queue)
                {
                    newQueue.Enqueue(item);
                }
                queue = newQueue;
            }
            else if (size < queue.Count)
            {
                while (queue.Count > size)
                {
                    Dequeue();
                }
            }
        }
    }
}
