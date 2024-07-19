using System.Collections;

namespace LaquaiLib.Collections;

/// <summary>
/// Represents a two-way lookup table where entries can be looked up by either key or value and are guaranteed to be unique.
/// Automatic enumeration is supported in the forward direction using standard <see cref="IEnumerable{T}"/> methods.
/// For (manual-only) reverse enumeration, use <see cref="GetReverseEnumerator"/>.
/// </summary>
public class TwoWayLookup<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    private readonly Dictionary<T1, T2> _forward = [];
    private readonly Dictionary<T2, T1> _reverse = [];

    /// <summary>
    /// Adds a new entry to the lookup table by the first type parameter <typeparamref name="T1"/>. An exception is thrown if either the key or the value already exists.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    public void AddForward(T1 key, T2 value)
    {
        _forward.Add(key, value);
        _reverse.Add(value, key);
    }
    /// <summary>
    /// Adds a new entry to the lookup table by the second type parameter <typeparamref name="T2"/>. An exception is thrown if either the key or the value already exists.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    public void AddReverse(T2 key, T1 value)
    {
        _reverse.Add(key, value);
        _forward.Add(value, key);
    }
    /// <summary>
    /// Attempts to add a new entry to the lookup table by the first type parameter <typeparamref name="T1"/>.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    /// <returns><see langword="true"/> if the key-value pair could be added, otherwise <see langword="false"/>.</returns>
    public bool TryAddForward(T1 key, T2 value)
    {
        try
        {
            AddForward(key, value);
            return true;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Attempts to add a new entry to the lookup table by the second type parameter <typeparamref name="T2"/>.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    /// <returns><see langword="true"/> if the key-value pair could be added, otherwise <see langword="false"/>.</returns>
    public bool TryAddReverse(T2 key, T1 value)
    {
        try
        {
            AddReverse(key, value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Adds a new entry to the lookup table.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    public void Add(T1 key, T2 value) => AddForward(key, value);
    /// <inheritdoc/>
    public void Add(T2 key, T1 value) => AddReverse(key, value);
    /// <summary>
    /// Attempts to add a new entry to the lookup table.
    /// </summary>
    /// <typeparam name="TFirst">The type of the key.</typeparam>
    /// <typeparam name="TSecond">The type of the value.</typeparam>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    /// <remarks>For the love of all things holy, avoid using this method. The 9000 generic type parameters make it a nightmare to use and slow as all hell. Not just that, but the fact that this is specifically designed to fail silently without any indication of what's wrong, possibly with the type parameters, makes it even worse.</remarks>
    public bool TryAdd<TFirst, TSecond>(TFirst key, TSecond value)
    {
        // Dynamically choose the correct method to call based on the type parameters
        try
        {
            return typeof(TFirst) is T1 && typeof(TSecond) is T2
                ? TryAddForward((T1)(object)key, (T2)(object)value)
                : typeof(TFirst) is T2 && typeof(TSecond) is T1 && TryAddReverse((T2)(object)key, (T1)(object)value);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieves an entry from the lookup table by its key. An exception is thrown if there is no entry with the given key.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <returns>The value associated with the given key.</returns>
    public T2 GetForward(T1 key) => _forward[key];
    /// <summary>
    /// Retrieves an entry from the lookup table by its value. An exception is thrown if there is no entry with the given value.
    /// </summary>
    /// <param name="value">The value of the entry.</param>
    /// <returns>The key associated with the given value.</returns>
    public T1 GetReverse(T2 value) => _reverse[value];
    /// <summary>
    /// Attempts to retrieve an entry from the lookup table by its key.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">An <c>out</c> <typeparamref name="T2"/> variable that receives the retrieved value.</param>
    /// <returns><see langword="true"/> if there was a value associated with the key, otherwise <see langword="false"/>.</returns>
    public bool TryGetForward(T1 key, out T2 value)
    {
        try
        {
            value = _forward[key];
            return true;
        }
        catch
        {
            value = default!;
            return false;
        }
    }
    /// <summary>
    /// Attempts to retrieve an entry from the lookup table by its value.
    /// </summary>
    /// <param name="value">The value of the entry.</param>
    /// <param name="key">An <c>out</c> <typeparamref name="T1"/> variable that receives the retrieved key.</param>
    /// <returns><see langword="true"/> if there was a key associated with the value, otherwise <see langword="false"/>.</returns>
    public bool TryGetReverse(T2 value, out T1 key)
    {
        try
        {
            key = _reverse[value];
            return true;
        }
        catch
        {
            key = default!;
            return false;
        }
    }
    /// <summary>
    /// Removes an entry from the lookup table by its key. An exception is thrown if there is no entry with the given key.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    public void RemoveForward(T1 key)
    {
        var rev = _forward[key];
        _reverse.Remove(rev);
        _forward.Remove(key);
    }
    /// <summary>
    /// Removes an entry from the lookup table by its value. An exception is thrown if there is no entry with the given value.
    /// </summary>
    /// <param name="value">The value of the entry.</param>
    public void RemoveReverse(T2 value)
    {
        var forw = _reverse[value];
        _forward.Remove(forw);
        _reverse.Remove(value);
    }
    /// <summary>
    /// Attempts to remove an entry from the lookup table by its key.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <returns><see langword="true"/> if there was a value associated with the key that could be removed, otherwise <see langword="false"/>.</returns>
    public bool TryRemoveForward(T1 key)
    {
        try
        {
            RemoveForward(key);
            return true;
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Attempts to remove an entry from the lookup table by its value.
    /// </summary>
    /// <param name="value">The value of the entry.</param>
    /// <returns><see langword="true"/> if there was a key associated with the value that could be removed, otherwise <see langword="false"/>.</returns>
    public bool TryRemoveReverse(T2 value)
    {
        try
        {
            RemoveReverse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Removes all entries from the lookup table.
    /// </summary>
    public void Clear()
    {
        _forward.Clear();
        _reverse.Clear();
    }

    /// <summary>
    /// Gets an enumerator that, by default, iterates through the forward collection as <see cref="KeyValuePair{TKey, TValue}"/>s.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the forward collection.</returns>
    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() => (_forward as IEnumerable<KeyValuePair<T1, T2>>).GetEnumerator();
    /// <summary>
    /// Returns an enumerator that iterates through the forward collection as <see cref="KeyValuePair{TKey, TValue}"/>s.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the forward collection.</returns>
    IEnumerator<KeyValuePair<T1, T2>> IEnumerable<KeyValuePair<T1, T2>>.GetEnumerator() => ((IEnumerable<KeyValuePair<T1, T2>>)_forward).GetEnumerator();
    /// <summary>
    /// Returns an enumerator that iterates through the reverse collection as <see cref="KeyValuePair{TKey, TValue}"/>s.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the reverse collection.</returns>
    public IEnumerator<KeyValuePair<T2, T1>> GetReverseEnumerator() => ((IEnumerable<KeyValuePair<T2, T1>>)_reverse).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_forward).GetEnumerator();
}