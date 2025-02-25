using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Collections.LimitedCollections;

/// <summary>
/// Implements a queue data structure with a maximum number of items allowed in it.
/// When the collection is at capacity and it is attempted to enqueue another object, the oldest is removed.
/// </summary>
/// <typeparam name="T">The Type of the items in the collection.</typeparam>
public sealed class LimitedQueue<T> : IReadOnlyCollection<T>
{
    private static ArgumentException NeedCapacityGreaterThanInitialItemsException => new ArgumentException($"The passed initial capacity may not be smaller than the number of items passed to create the {nameof(LimitedQueue<>)} with.");

    private Queue<T> queue;

    /// <summary>
    /// The capacity of this <see cref="LimitedQueue{T}"/>.
    /// </summary>
    public int Capacity { get; private set; }

    #region .ctors
    /// <summary>
    /// Initializes a new empty <see cref="LimitedQueue{T}"/> with the given maximum <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The maximum number of items this <see cref="LimitedQueue{T}"/> can hold before discarding the oldest value.</param>
    private LimitedQueue(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        Capacity = capacity;
    }
    /// <summary>
    /// Initializes a new <see cref="LimitedQueue{T}"/> with the items from the passed <paramref name="enumerable"/>. Its maximum capacity is set to <paramref name="enumerable"/>'s length.
    /// </summary>
    /// <param name="enumerable">The collection to copy the new <see cref="LimitedQueue{T}"/>'s items from.</param>
    public LimitedQueue(IEnumerable<T> enumerable)
    {
        queue = new Queue<T>(enumerable);
        ArgumentOutOfRangeException.ThrowIfZero(queue.Count, nameof(enumerable));
        Capacity = queue.Count;
    }
    /// <summary>
    /// Initializes a new <see cref="LimitedQueue{T}"/> with the items from the passed <paramref name="items"/>. Its maximum capacity is set to <paramref name="items"/>'s length.
    /// </summary>
    /// <param name="items">The <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> to copy the new <see cref="LimitedQueue{T}"/>'s items from.</param>
    public LimitedQueue(params T[] items) : this(items.Length, items) { }
    /// <summary>
    /// Initializes a new <see cref="LimitedQueue{T}"/> with the items from the passed <paramref name="enumerable"/>. Its maximum capacity is set to <paramref name="capacity"/>.
    /// </summary>
    /// <param name="enumerable">The collection to copy the new <see cref="LimitedQueue{T}"/>'s items from.</param>
    /// <param name="capacity">The maximum number of items this <see cref="LimitedQueue{T}"/> can hold before discarding the oldest value.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="capacity"/> is smaller than the number of items in <paramref name="enumerable"/>.</exception>
    public LimitedQueue(int capacity, IEnumerable<T> enumerable) : this(capacity)
    {
        if (enumerable.TryGetNonEnumeratedCount(out var count) && capacity > count)
        {
            throw NeedCapacityGreaterThanInitialItemsException;
        }

        queue = new Queue<T>(capacity);
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
    /// Initializes a new <see cref="LimitedQueue{T}"/> with the items from the passed <paramref name="items"/>. Its maximum capacity is set to <paramref name="capacity"/>.
    /// </summary>
    /// <param name="items">The span to copy the new <see cref="LimitedQueue{T}"/>'s items from.</param>
    /// <param name="capacity">The maximum number of items this <see cref="LimitedQueue{T}"/> can hold before discarding the oldest value.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="capacity"/> is smaller than the number of items in <paramref name="items"/>.</exception>
    public LimitedQueue(int capacity, params T[] items) : this(capacity)
    {
        if (Capacity < items.Length)
        {
            throw NeedCapacityGreaterThanInitialItemsException;
        }
        queue = new Queue<T>(items);
    }
    #endregion

    #region Queue methods
    /// <summary>
    /// Adds an item to the end of the <see cref="LimitedQueue{T}"/>, discarding the oldest item if the collection is at maximum capacity.
    /// </summary>
    /// <param name="item">The item to add to the <see cref="LimitedQueue{T}"/>.</param>
    public void Enqueue(T item)
    {
        // Dequeue before Enqueue if the collection is at capacity to prevent resizing the backing store
        if (queue.Count + 1 > Capacity)
        {
            _ = Dequeue();
        }
        queue.Enqueue(item);
    }
    /// <summary>
    /// Attempts to add an item to the end of the <see cref="LimitedQueue{T}"/>.
    /// If this would cause the oldest item to be discarded because the collection is at capacity, the collection remains unchanged.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="LimitedQueue{T}"/>.</param>
    /// <returns>A value indicating whether the collection was modified; <see langword="true"/> if <paramref name="item"/> could be added, otherwise <see langword="false"/>.</returns>
    public bool TryEnqueue(T item)
    {
        if (queue.Count <= Capacity)
        {
            queue.Enqueue(item);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Removes and returns the object at the beginning of the <see cref="LimitedQueue{T}"/>.
    /// An exception is thrown if the <see cref="LimitedQueue{T}"/> is empty.
    /// </summary>
    /// <returns>The object that is removed from the beginning of the <see cref="LimitedQueue{T}"/>.</returns>
    public T Dequeue() => queue.Dequeue();
    /// <summary>
    /// Attempts to remove and return the object at the beginning of the <see cref="LimitedQueue{T}"/>.
    /// </summary>
    /// <param name="item">An <see langword="out"/> variable that receives the object removed from the beginning of the <see cref="LimitedQueue{T}"/>.</param>
    /// <returns>A value indicating whether the collection was modified; <see langword="true"/> if an object could be removed, otherwise <see langword="false"/>.</returns>
    public bool TryDequeue([NotNullWhen(true)] out T item) => queue.TryDequeue(out item);
    /// <summary>
    /// Returns the object at the beginning of the <see cref="LimitedQueue{T}"/> without removing it.
    /// An exception is thrown if the <see cref="LimitedQueue{T}"/>
    /// </summary>
    /// <returns>The object at the beginning of the <see cref="LimitedQueue{T}"/>.</returns>
    public T Peek() => queue.Peek();
    /// <summary>
    /// Attempts to return the object at the beginning of the <see cref="LimitedQueue{T}"/> without removing it.
    /// </summary>
    /// <param name="item">An <see langword="out"/> variable that receives the object at the beginning of the <see cref="LimitedQueue{T}"/>.</param>
    /// <returns>A value indicating whether <paramref name="item"/> was assigned; <see langword="true"/> if an object could be returned, otherwise <see langword="false"/>.</returns>
    public bool TryPeek([NotNullWhen(true)] out T item) => queue.TryPeek(out item);
    #endregion

    #region IReadOnlyCollection<T>
    /// <summary>
    /// Gets the number of items currently enqueued in the <see cref="LimitedQueue{T}"/>.
    /// </summary>
    public int Count => queue.Count;

    /// <summary>
    /// Gets an enumerator over the items in the <see cref="LimitedQueue{T}"/>.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator<T> GetEnumerator() => queue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    /// <summary>
    /// Resizes the internal backing store of the <see cref="LimitedQueue{T}"/> to the specified <paramref name="size"/>.
    /// The oldest items beyond the new size are discarded.
    /// </summary>
    /// <param name="size">The number of items to allow in the collection.</param>
    public void Resize(int size)
    {
        Capacity = size;

        if (size > queue.Count)
        {
            var newQueue = new Queue<T>(size);
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
                _ = Dequeue();
            }
        }
    }
}

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
                _ = Dequeue();
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

    // Dequeueing is implicitly thread-safe since we're just delegating to the ConcurrentQueue
    /// <summary>
    /// Removes and returns the object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.
    /// An exception is thrown if the <see cref="ConcurrentLimitedQueue{T}"/> is empty.
    /// </summary>
    /// <returns>The object that is removed from the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.</returns>
    public T Dequeue() => queue.TryDequeue(out var result) ? result : throw new InvalidOperationException("The collection is empty.");
    /// <summary>
    /// Attempts to remove and return the object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.
    /// </summary>
    /// <param name="item">An <see langword="out"/> variable that receives the object removed from the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.</param>
    /// <returns>A value indicating whether the collection was modified; <see langword="true"/> if an object could be removed, otherwise <see langword="false"/>.</returns>
    public bool TryDequeue([NotNullWhen(true)] out T item) => queue.TryDequeue(out item);
    /// <summary>
    /// Returns the object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/> without removing it.
    /// An exception is thrown if the <see cref="ConcurrentLimitedQueue{T}"/>
    /// </summary>
    /// <returns>The object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.</returns>
    public T Peek() => queue.TryPeek(out var result) ? result : throw new InvalidOperationException("The collection is empty.");
    /// <summary>
    /// Attempts to return the object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/> without removing it.
    /// </summary>
    /// <param name="item">An <see langword="out"/> variable that receives the object at the beginning of the <see cref="ConcurrentLimitedQueue{T}"/>.</param>
    /// <returns>A value indicating whether <paramref name="item"/> was assigned; <see langword="true"/> if an object could be returned, otherwise <see langword="false"/>.</returns>
    public bool TryPeek([NotNullWhen(true)] out T item) => queue.TryPeek(out item);
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

    /// <summary>
    /// Resizes the internal backing store of the <see cref="ConcurrentLimitedQueue{T}"/> to the specified <paramref name="size"/>.
    /// The oldest items beyond the new size are discarded.
    /// </summary>
    /// <param name="size">The number of items to allow in the collection.</param>
    public void Resize(int size)
    {
        ArgumentOutOfRangeException.ThrowIfZero(size);
        ArgumentOutOfRangeException.ThrowIfNegative(size);

        lock (_lock)
        {
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
                    _ = Dequeue();
                }
            }
        }
    }
}
