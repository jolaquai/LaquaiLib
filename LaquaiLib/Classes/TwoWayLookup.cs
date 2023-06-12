using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Classes;

/// <summary>
/// Represents a two-way lookup table where entries can be looked up by either key or value and are guaranteed to be unique. Enumeration is supported in the forward direction using standard <see cref="IEnumerable{T}"/> methods. For reverse enumeration, use <see cref="GetReverseEnumerator"/>.
/// </summary>
public class TwoWayLookup<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    private readonly Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
    private readonly Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

    /// <summary>
    /// Adds a new entry to the lookup table. An exception is thrown if either the key or the value already exists.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    public void Add(T1 key, T2 value)
    {
        _forward.Add(key, value);
        _reverse.Add(value, key);
    }

    /// <summary>
    /// Attempts to add a new entry to the lookup table.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    /// <returns><c>true</c> if the key-value pair could be added, otherwise <c>false</c>.</returns>
    public bool TryAdd(T1 key, T2 value)
    {
        try
        {
            Add(key, value);
            return true;
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
    public T2 GetForward(T1 key)
    {
        return _forward[key];
    }

    /// <summary>
    /// Retrieves an entry from the lookup table by its value. An exception is thrown if there is no entry with the given value.
    /// </summary>
    /// <param name="value">The value of the entry.</param>
    /// <returns>The key associated with the given value.</returns>
    public T1 GetReverse(T2 value)
    {
        return _reverse[value];
    }

    /// <summary>
    /// Attempts to retrieve an entry from the lookup table by its key.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">An <c>out</c> <typeparamref name="T2"/> variable that receives the retrieved value.</param>
    /// <returns><c>true</c> if there was a value associated with the key, otherwise <c>false</c>.</returns>
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
    /// <returns><c>true</c> if there was a key associated with the value, otherwise <c>false</c>.</returns>
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
    /// <returns><c>true</c> if there was a value associated with the key that could be removed, otherwise <c>false</c>.</returns>
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
    /// <returns><c>true</c> if there was a key associated with the value that could be removed, otherwise <c>false</c>.</returns>
    public bool TryGetReverse(T2 value)
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

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() => ((IEnumerable<KeyValuePair<T1, T2>>)_forward).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_forward).GetEnumerator();
    /// <summary>
    /// Returns an enumerator that iterates through the reverse collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the reverse collection.</returns>
    public IEnumerator<KeyValuePair<T2, T1>> GetReverseEnumerator() => ((IEnumerable<KeyValuePair<T2, T1>>)_reverse).GetEnumerator();
}