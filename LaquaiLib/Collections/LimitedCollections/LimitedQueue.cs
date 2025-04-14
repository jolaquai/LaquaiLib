using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LaquaiLib.Collections.LimitedCollections;

/// <summary>
/// Implements a queue data structure with a maximum number of items allowed in it.
/// When the collection is at capacity and it is attempted to enqueue another object, the oldest is removed.
/// </summary>
/// <typeparam name="T">The Type of the items in the collection.</typeparam>
public sealed class LimitedQueue<T> : ICollection<T>
{
    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    internal static extern ref T[] _array(Queue<T> queue);
    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    internal static extern ref int _size(Queue<T> queue);

    private static ArgumentException NeedCapacityGreaterThanInitialItemsException => new ArgumentException($"The passed initial capacity may not be smaller than the number of items passed to create the {nameof(LimitedQueue<>)} with.");
    private static ArgumentOutOfRangeException InitializationLengthMustNotBeZero => new ArgumentOutOfRangeException("The passed collection must not be empty.");

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
    public LimitedQueue(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        Capacity = capacity;
        queue = new Queue<T>(capacity);
    }
    /// <summary>
    /// Initializes a new <see cref="LimitedQueue{T}"/> with the items from the passed <paramref name="enumerable"/>. Its maximum capacity is set to <paramref name="enumerable"/>'s length.
    /// </summary>
    /// <param name="enumerable">The collection to copy the new <see cref="LimitedQueue{T}"/>'s items from.</param>
    public LimitedQueue(IEnumerable<T> enumerable)
    {
        if (enumerable.TryGetNonEnumeratedCount(out var count))
        {
            if (count == 0)
            {
                throw InitializationLengthMustNotBeZero;
            }
            Capacity = count;
            queue = new Queue<T>(count);
            foreach (var item in enumerable)
            {
                queue.Enqueue(item);
            }
        }
        else
        {
            var array = enumerable.ToArray();
            if (array.Length == 0)
            {
                throw InitializationLengthMustNotBeZero;
            }
            Capacity = array.Length;
            queue = new Queue<T>(array.Length);
            for (var i = 0; i < array.Length; i++)
            {
                queue.Enqueue(array[i]);
            }
        }
    }
    /// <summary>
    /// Initializes a new <see cref="LimitedQueue{T}"/> with the items from the passed <paramref name="items"/>. Its maximum capacity is set to <paramref name="items"/>'s length.
    /// </summary>
    /// <param name="items">The <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> to copy the new <see cref="LimitedQueue{T}"/>'s items from.</param>
    public LimitedQueue(ReadOnlySpan<T> items) : this(items.Length, items) { }
    /// <summary>
    /// Initializes a new <see cref="LimitedQueue{T}"/> with the items from the passed <paramref name="enumerable"/>. Its maximum capacity is set to <paramref name="capacity"/>.
    /// </summary>
    /// <param name="enumerable">The collection to copy the new <see cref="LimitedQueue{T}"/>'s items from.</param>
    /// <param name="capacity">The maximum number of items this <see cref="LimitedQueue{T}"/> can hold before discarding the oldest value.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="capacity"/> is smaller than the number of items in <paramref name="enumerable"/>.</exception>
    public LimitedQueue(int capacity, IEnumerable<T> enumerable) : this(capacity)
    {
        if (enumerable.TryGetNonEnumeratedCount(out var count) && count > capacity)
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
    /// Initializes a new <see cref="LimitedQueue{T}"/> with the items from the passed <paramref name="span"/>. Its maximum capacity is set to <paramref name="capacity"/>.
    /// </summary>
    /// <param name="span">The span to copy the new <see cref="LimitedQueue{T}"/>'s items from.</param>
    /// <param name="capacity">The maximum number of items this <see cref="LimitedQueue{T}"/> can hold before discarding the oldest value.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="capacity"/> is smaller than the number of items in <paramref name="span"/>.</exception>
    public LimitedQueue(int capacity, T[] span) : this(capacity, span.AsSpan()) { }
    /// <summary>
    /// Initializes a new <see cref="LimitedQueue{T}"/> with the items from the passed <paramref name="span"/>. Its maximum capacity is set to <paramref name="capacity"/>.
    /// </summary>
    /// <param name="span">The span to copy the new <see cref="LimitedQueue{T}"/>'s items from.</param>
    /// <param name="capacity">The maximum number of items this <see cref="LimitedQueue{T}"/> can hold before discarding the oldest value.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="capacity"/> is smaller than the number of items in <paramref name="span"/>.</exception>
    public LimitedQueue(int capacity, ReadOnlySpan<T> span) : this(capacity)
    {
        if (Capacity < span.Length)
        {
            throw NeedCapacityGreaterThanInitialItemsException;
        }
        queue = new Queue<T>(capacity);
        for (var i = 0; i < span.Length; i++)
        {
            queue.Enqueue(span[i]);
        }
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
        if (queue.Count > 0 && queue.Count >= Capacity)
        {
            Dequeue();
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
        if (queue.Count < Capacity)
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
    /// An exception is thrown if the <see cref="LimitedQueue{T}"/> is empty.
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

    public bool IsReadOnly { get; }

    /// <summary>
    /// Gets an enumerator over the items in the <see cref="LimitedQueue{T}"/>.
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
        while (TryDequeue(out _))
        {
        }
    }
    /// <summary>
    /// Determines whether the <see cref="LimitedQueue{T}"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="LimitedQueue{T}"/>.</param>
    /// <returns>The <see langword="true"/> if <paramref name="item"/> is found in the <see cref="LimitedQueue{T}"/>, otherwise <see langword="false"/>.</returns>
    public bool Contains(T item) => queue.Contains(item);
    /// <summary>
    /// Copies the elements of the queue to a specified array starting at a given index.
    /// </summary>
    /// <param name="array">The destination array where the elements from the queue will be copied.</param>
    /// <param name="arrayIndex">The starting index in the destination array where copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex) => queue.CopyTo(array, arrayIndex);
    /// <summary>
    /// Always returns <see langword="false"/>. Items cannot be directly removed from a <see cref="LimitedQueue{T}"/>;
    /// </summary>
    /// <param name="item">The item to remove from the <see cref="LimitedQueue{T}"/>.</param>
    /// <returns><see langword="false"/> unconditionally.</returns>
    public bool Remove(T item) => false;
    #endregion

    /// <summary>
    /// Resizes the internal backing store of the <see cref="LimitedQueue{T}"/> to the specified <paramref name="size"/>.
    /// The oldest items beyond the new size are discarded.
    /// </summary>
    /// <param name="size">The number of items to allow in the collection.</param>
    public void Resize(int size)
    {
        if (size > queue.Count)
        {
            Capacity = size;
            var newQueue = new Queue<T>(size);
            foreach (var item in queue)
            {
                newQueue.Enqueue(item);
            }
            queue = newQueue;
        }
        else if (queue.Count > size)
        {
            Capacity = size;
            do
            {
                Dequeue();
            } while (queue.Count > size);
        }
    }
}
