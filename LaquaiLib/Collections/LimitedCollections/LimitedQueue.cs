namespace LaquaiLib.Classes.Collections.LimitedCollections;

/// <summary>
/// Represents a <see cref="Queue{T}"/> with a maximum number of items allowed in it. When the collection is at capacity and it is attempted to enqueue another object, the oldest is removed.
/// </summary>
/// <typeparam name="T">The Type of the items in the collection.</typeparam>
public class LimitedQueue<T> : Queue<T>
{
    private int _capacity = int.MaxValue;

    /// <summary>
    /// The capacity of this <see cref="LimitedQueue{T}"/>.
    /// </summary>
    public int Capacity {
        get => _capacity;
        set {
            if (value < _capacity)
            {
                Reduce(value);
            }
            _capacity = value;
        }
    }

    /// <summary>
    /// Instantiates a new empty <see cref="LimitedQueue{T}"/> with the default maximum capacity.
    /// </summary>
    public LimitedQueue() : base() { }

    /// <summary>
    /// Instantiates a new <see cref="LimitedQueue{T}"/> with the items from the passed <paramref name="collection"/>. Its maximum capacity is set to <paramref name="collection"/>'s length.
    /// </summary>
    /// <param name="collection">The collection to copy the new <see cref="LimitedQueue{T}"/>'s items from.</param>
    public LimitedQueue(IEnumerable<T> collection) : base(collection) { }

    /// <summary>
    /// Instantiates a new empty <see cref="LimitedQueue{T}"/> with the given maximum <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The maximum number of items this <see cref="LimitedQueue{T}"/> can hold before discarding the oldest value.</param>
    public LimitedQueue(int capacity)
    {
        Capacity = capacity;
    }

    /// <summary>
    /// Forcibly adds an item to the end of the <see cref="LimitedQueue{T}"/>, discarding the oldest item if the collection is at maximum capacity.
    /// </summary>
    /// <param name="item">The item to add to the <see cref="LimitedQueue{T}"/>.</param>
    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        if (Count > Capacity)
        {
            Dequeue();
        }
    }

    /// <summary>
    /// Attempts to add an item to the end of the <see cref="LimitedQueue{T}"/>. If this would cause the oldest item to be discarded because the collection is at capacity, the collection remains unchanged.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="LimitedQueue{T}"/>.</param>
    /// <returns>A value indicating whether the collection was modified; <see langword="true"/> if <paramref name="item"/> could be added, <see langword="false"/> otherwise.</returns>
    public bool TryEnqueue(T item)
    {
        if (Count <= Capacity)
        {
            base.Enqueue(item);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes items from the <see cref="LimitedQueue{T}"/>, discarding the oldest items until the passed <paramref name="length"/> is reached.
    /// </summary>
    /// <param name="length">The number of items to remain in the collection.</param>
    public void Reduce(int length)
    {
        while (Count > length)
        {
            Dequeue();
        }
    }
}
