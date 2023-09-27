using System.Collections;

using LaquaiLib.Extensions;

namespace LaquaiLib.Classes.Collections;

/// <summary>
/// Represents a two-way lookup table where entries can be looked up by either key or value and are guaranteed to be unique.
/// Automatic enumeration is supported in the forward direction using standard <see cref="IEnumerable{T}"/> methods.
/// For (manual-only) reverse enumeration, use <see cref="GetReverseEnumerator"/>.
/// </summary>
public class TwoWayLookup<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    private readonly Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
    private readonly Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

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
    /// Adds a new entry to the lookup table. An exception is thrown if either the key or the value already exists or if the type parameters do not match <typeparamref name="T1"/> and <typeparamref name="T2"/>.
    /// </summary>
    /// <typeparam name="TFirst">The type of the key.</typeparam>
    /// <typeparam name="TSecond">The type of the value.</typeparam>
    /// <param name="key">The key of the entry.</param>
    /// <param name="value">The value of the entry.</param>
    /// <remarks>For the love of all things holy, avoid using this method. The 9000 generic type parameters make it a nightmare to use and slow as all hell. Additionally, it's incredibly inefficient because values of the generic types of this method cannot be directly cast to the generic types of the <see cref="TwoWayLookup{T1, T2}"/>.</remarks>
    /// <exception cref="ArgumentException">Thrown if the types <typeparamref name="TFirst"/> and <typeparamref name="TSecond"/> do not match the <see cref="TwoWayLookup{T1, T2}"/>'s type parameters in any order.</exception>
    /// <exception cref="ArgumentException">Thrown if the type <typeparamref name="TFirst"/> matches both <typeparamref name="T1"/> and <typeparamref name="T2"/>, but the latter are different types.</exception>
    /// <exception cref="ArgumentException">Thrown if the type <typeparamref name="TSecond"/> matches both <typeparamref name="T1"/> and <typeparamref name="T2"/>, but the latter are different types.</exception>
    public void Add<TFirst, TSecond>(TFirst key, TSecond value)
    {
        // Dynamically choose the correct method to call based on the type parameters
        if (typeof(TFirst).CanCastTo(typeof(T1)))
        {
            if (typeof(TSecond).CanCastTo(typeof(T2)))
            {
                AddForward(key.Cast<T1>(), value.Cast<T2>());
            }
            else if (typeof(TSecond).CanCastTo(typeof(T1)))
            {
                throw new ArgumentException($"Since type of parameter '{nameof(key)}' '{typeof(TFirst).FullName}' is assignable to the first type parameter of the TwoWayLookup '{typeof(T1).FullName}' and the second ('{typeof(T2).FullName}') is different from the first, the type of parameter '{nameof(value)}' '{typeof(TSecond).FullName}' must not also be assignable to the first type parameter of the TwoWayLookup.", nameof(value));
            }
            else
            {
                throw new ArgumentException($"Type of parameter '{nameof(value)}' '{typeof(TSecond).FullName}' is not assignable to either type parameter of the TwoWayLookup ('{typeof(T1).FullName}' or '{typeof(T2).FullName}')", nameof(value));
            }
        }
        else if (typeof(TFirst).CanCastTo(typeof(T2)))
        {
            if (typeof(TSecond).CanCastTo(typeof(T1)))
            {
                AddReverse(key.Cast<T2>(), value.Cast<T1>());
            }
            else if (typeof(TSecond).CanCastTo(typeof(T2)))
            {
                throw new ArgumentException($"Since type of parameter '{nameof(key)}' '{typeof(TFirst).FullName}' is assignable to the second type parameter of the TwoWayLookup '{typeof(T2).FullName}' and the first ('{typeof(T1).FullName}') is different from the second, the type of parameter '{nameof(value)}' '{typeof(TSecond).FullName}' must not also be assignable to the second type parameter of the TwoWayLookup.", nameof(value));
            }
            else
            {
                throw new ArgumentException($"Type of parameter '{nameof(value)}' '{typeof(TSecond).FullName}' is not assignable to either type parameter of the TwoWayLookup ('{typeof(T1).FullName}' or '{typeof(T2).FullName}')", nameof(value));
            }
        }
        else
        {
            throw new ArgumentException($"Type of parameter '{nameof(key)}' '{typeof(TFirst).FullName}' is not assignable to either type parameter of the TwoWayLookup ('{typeof(T1).FullName}' or '{typeof(T2).FullName}')", nameof(key));
        }
    }

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
            if (typeof(TFirst) is T1)
            {
                if (typeof(TSecond) is T2)
                {
                    return TryAddForward((T1)(object)key, (T2)(object)value);
                }
                else if (typeof(TSecond) is T1)
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else if (typeof(TFirst) is T2)
            {
                if (typeof(TSecond) is T1)
                {
                    return TryAddReverse((T2)(object)key, (T1)(object)value);
                }
                else if (typeof(TSecond) is T2)
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
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

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() => ((IEnumerable<KeyValuePair<T1, T2>>)_forward).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_forward).GetEnumerator();
    /// <summary>
    /// Returns an enumerator that iterates through the reverse collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the reverse collection.</returns>
    public IEnumerator<KeyValuePair<T2, T1>> GetReverseEnumerator() => ((IEnumerable<KeyValuePair<T2, T1>>)_reverse).GetEnumerator();
}
