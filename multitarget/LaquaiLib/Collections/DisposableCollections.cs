namespace LaquaiLib.Collections;

/// <summary>
/// Derives from <see cref="List{T}"/> to support disposing of all items in the list that implement <see cref="IDisposable"/> when the list itself is disposed.
/// The collection is protected against multiple disposal.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public sealed class DisposableList<T> : List<T>, IDisposable
{
    private int disposed;
    /// <summary>
    /// Disposes of all items in the list that implement <see cref="IDisposable"/> and clears the list for <typeparamref name="T"/>s which are reference types or structs containing references.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return;

        foreach (var item in this)
            if (item is IDisposable disposable)
                disposable.Dispose();
        // This is checked inside Clear again, but we can avoid the entire store for non-ref Ts
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Clear();
    }
}

/// <summary>
/// Derives from <see cref="HashSet{T}"/> to support disposing of all items in the hashset that implement <see cref="IDisposable"/> when the hashset itself is disposed.
/// The collection is protected against multiple disposal.
/// </summary>
/// <typeparam name="T">The type of elements in the hashset.</typeparam>
public sealed class DisposableHashSet<T> : HashSet<T>, IDisposable
{
    private int disposed;
    /// <summary>
    /// Disposes of all items in the hashset that implement <see cref="IDisposable"/> and clears the hashset for <typeparamref name="T"/>s which are reference types or structs containing references.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return;

        foreach (var item in this)
            if (item is IDisposable disposable)
                disposable.Dispose();
        // This is checked inside Clear again, but we can avoid the entire store for non-ref Ts
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Clear();
    }
}

/// <summary>
/// Derives from <see cref="Queue{T}"/> to support disposing of all items in the queue that implement <see cref="IDisposable"/> when the queue itself is disposed.
/// The collection is protected against multiple disposal.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
public sealed class DisposableQueue<T> : Queue<T>, IDisposable
{
    private int disposed;
    /// <summary>
    /// Disposes of all items in the queue that implement <see cref="IDisposable"/> and clears the queue for <typeparamref name="T"/>s which are reference types or structs containing references.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return;

        foreach (var item in this)
            if (item is IDisposable disposable)
                disposable.Dispose();
        // This is checked inside Clear again, but we can avoid the entire store for non-ref Ts
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Clear();
    }
}

/// <summary>
/// Derives from <see cref="Stack{T}"/> to support disposing of all items in the stack that implement <see cref="IDisposable"/> when the stack itself is disposed.
/// The collection is protected against multiple disposal.
/// </summary>
/// <typeparam name="T">The type of elements in the stack.</typeparam>
public sealed class DisposableStack<T> : Stack<T>, IDisposable
{
    private int disposed;
    /// <summary>
    /// Disposes of all items in the stack that implement <see cref="IDisposable"/> and clears the stack for <typeparamref name="T"/>s which are reference types or structs containing references.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return;

        foreach (var item in this)
            if (item is IDisposable disposable)
                disposable.Dispose();
        // This is checked inside Clear again, but we can avoid the entire store for non-ref Ts
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Clear();
    }
}

/// <summary>
/// Derives from <see cref="Dictionary{TKey, TValue}"/> to support disposing of all keys and values in the dictionary that implement <see cref="IDisposable"/> when the dictionary itself is 
/// The collection is protected against multiple disposal.disposed.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
public sealed class DisposableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
{
    private int disposed;
    /// <summary>
    /// Disposes of all items in the dictionary that implement <see cref="IDisposable"/> and clears the dictionary for <typeparamref name="T"/>s which are reference types or structs containing references.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return;

        foreach (var (k, v) in this)
        {
            if (k is IDisposable disposableKey)
                disposableKey.Dispose();
            if (v is IDisposable disposableValue)
                disposableValue.Dispose();
        }
        // This is checked inside Clear again, but we can avoid the entire store for non-ref Ts
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>() || RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            Clear();
    }
}

/// <summary>
/// Derives from <see cref="ConcurrentBag{T}"/> to support disposing of all items in the concurrent bag that implement <see cref="IDisposable"/> when the concurrent bag itself is disposed.
/// The collection is protected against multiple disposal.
/// </summary>
/// <typeparam name="T">The type of elements in the concurrent bag.</typeparam>
public sealed class DisposableConcurrentBag<T> : ConcurrentBag<T>, IDisposable
{
    private int disposed;
    /// <summary>
    /// Disposes of all items in the concurrent bag that implement <see cref="IDisposable"/> and clears the concurrent bag for <typeparamref name="T"/>s which are reference types or structs containing references.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return;

        foreach (var item in this)
            if (item is IDisposable disposable)
                disposable.Dispose();
        // This is checked inside Clear again, but we can avoid the entire store for non-ref Ts
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Clear();
    }
}

/// <summary>
/// Derives from <see cref="ConcurrentQueue{T}"/> to support disposing of all items in the concurrent queue that implement <see cref="IDisposable"/> when the concurrent queue itself is 
/// The collection is protected against multiple disposal.disposed.
/// </summary>
/// <typeparam name="T">The type of elements in the concurrent queue.</typeparam>
public sealed class DisposableConcurrentQueue<T> : ConcurrentQueue<T>, IDisposable
{
    private int disposed;
    /// <summary>
    /// Disposes of all items in the concurrent queue that implement <see cref="IDisposable"/> and clears the concurrent queue for <typeparamref name="T"/>s which are reference types or structs containing references.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return;

        foreach (var item in this)
            if (item is IDisposable disposable)
                disposable.Dispose();
        // This is checked inside Clear again, but we can avoid the entire store for non-ref Ts
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Clear();
    }
}

/// <summary>
/// Derives from <see cref="ConcurrentStack{T}"/> to support disposing of all items in the concurrent stack that implement <see cref="IDisposable"/> when the concurrent stack itself is 
/// The collection is protected against multiple disposal.disposed.
/// </summary>
/// <typeparam name="T">The type of elements in the concurrent stack.</typeparam>
public sealed class DisposableConcurrentStack<T> : ConcurrentStack<T>, IDisposable
{
    private int disposed;
    /// <summary>
    /// Disposes of all items in the concurrent stack that implement <see cref="IDisposable"/> and clears the concurrent stack for <typeparamref name="T"/>s which are reference types or structs containing references.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return;

        foreach (var item in this)
            if (item is IDisposable disposable)
                disposable.Dispose();
        // This is checked inside Clear again, but we can avoid the entire store for non-ref Ts
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Clear();
    }
}

/// <summary>
/// Derives from <see cref="ConcurrentDictionary{TKey, TValue}"/> to support disposing of all keys and values in the concurrent dictionary that implement <see cref="IDisposable"/> when the 
/// The collection is protected against multiple disposal.concurrent dictionary itself is disposed.
/// </summary>
/// <typeparam name="TKey">The type of keys in the concurrent dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the concurrent dictionary.</typeparam>
public sealed class DisposableConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, IDisposable
{
    private int disposed;
    /// <summary>
    /// Disposes of all items in the concurrent dictionary that implement <see cref="IDisposable"/> and clears the concurrent dictionary for <typeparamref name="T"/>s which are reference types or structs containing references.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return;

        foreach (var (k, v) in this)
        {
            if (k is IDisposable disposableKey)
                disposableKey.Dispose();
            if (v is IDisposable disposableValue)
                disposableValue.Dispose();
        }
        // This is checked inside Clear again, but we can avoid the entire store for non-ref Ts
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>() || RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            Clear();
    }
}