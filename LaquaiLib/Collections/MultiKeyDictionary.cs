using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using SrcsUnsafe = System.Runtime.CompilerServices.Unsafe;

namespace LaquaiLib.Collections;

#pragma warning disable IDE0044 // Add readonly modifier

/// <summary>
/// Implements a dictionary that maps keys and specific orders of those keys to values.
/// </summary/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
/// <remarks>
/// <see langword="struct"/>s used for the keys in this dictionary will be boxed. This incurs an allocation and performance penalty.
/// </remarks>
public class MultiKeyDictionary<TValue>
{
    [DoesNotReturn]
    private static void ThrowKeysNotFoundException() => throw new KeyNotFoundException("The specified keys combination was not found.");

    private Dictionary<object, TValue> _one;
    private Dictionary<(object, object), TValue> _two;
    private Dictionary<(object, object, object), TValue> _three;
    private Dictionary<(object, object, object, object), TValue> _four;
    private Dictionary<(object, object, object, object, object), TValue> _five;
    private Dictionary<(object, object, object, object, object, object), TValue> _six;
    private Dictionary<(object, object, object, object, object, object, object), TValue> _seven;
    private Dictionary<(object, object, object, object, object, object, object, object), TValue> _eight;
    // Beyond 8 (which fits into a default ValueTuple), switch to an array-based dictionary instead
    private Dictionary<object[], TValue> _many;
    private Dictionary<object[], TValue>.AlternateLookup<ReadOnlySpan<object>> _manyLookup;

    /// <summary>
    /// Initializes a new <see cref="MultiKeyDictionary{TValue}"/> with no backing storage allocated.
    /// </summary>
    public MultiKeyDictionary() { }
    /// <summary>
    /// Initializes a new <see cref="MultiKeyDictionary{TValue}"/> with the specified capacity for the specified most likely key count.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    /// <param name="mostLikelyKeyCount">The number of keys that will be used most likely.</param>
    public MultiKeyDictionary(int capacity, int mostLikelyKeyCount)
    {
        Allocate(capacity, mostLikelyKeyCount);
    }

    private bool IsAllocated(int keyCount)
    {
        Debug.Assert(keyCount > 0);

        switch (keyCount)
        {
            case 1 when _one is null:
            case 2 when _two is null:
            case 3 when _three is null:
            case 4 when _four is null:
            case 5 when _five is null:
            case 6 when _six is null:
            case 7 when _seven is null:
            case 8 when _eight is null:
            case > 8 when _many is null:
                return true;
            default:
                return false;
        }
    }
    /// <summary>
    /// Checks whether a backing storage for the specified number of keys is allocated and allocates it if not.
    /// </summary>
    /// <returns><see langword="true"/> if the backing storage was allocated by the call to this method, otherwise <see langword="false"/> (i.e. the backing storage was already allocated).</returns>
    private bool TryAllocate(int keyCount)
    {
        Debug.Assert(keyCount > 0);

        if (IsAllocated(keyCount))
        {
            return false;
        }

        switch (keyCount)
        {
            case 1 when _one is null:
            case 2 when _two is null:
            case 3 when _three is null:
            case 4 when _four is null:
            case 5 when _five is null:
            case 6 when _six is null:
            case 7 when _seven is null:
            case 8 when _eight is null:
            case > 8 when _many is null:
                Allocate(0, keyCount);
                // Enables fast paths in getter methods since a newly allocated dictionary cannot possible contain the sought key
                return true;
        }

        return false;
    }
    private void Allocate(int capacity, int keyCount)
    {
        Debug.Assert(capacity >= 0);
        Debug.Assert(keyCount >= 0);

        switch (keyCount)
        {
            case 1:
                _one = new(capacity);
                break;
            case 2:
                _two = new(capacity);
                break;
            case 3:
                _three = new(capacity);
                break;
            case 4:
                _four = new(capacity);
                break;
            case 5:
                _five = new(capacity);
                break;
            case 6:
                _six = new(capacity);
                break;
            case 7:
                _seven = new(capacity);
                break;
            case 8:
                _eight = new(capacity);
                break;
            default:
                _many = new(capacity);
                _manyLookup = _many.GetAlternateLookup<ReadOnlySpan<object>>();
                break;
        }
    }

    /// <summary>
    /// Gets or sets the value associated with the specified keys.
    /// </summary>
    /// <param name="keys">The keys to get or set the value for.</param>
    /// <returns>The value associated with the specified keys.</returns>
    public TValue this[ReadOnlySpan<object> keys]
    {
        get => GetValue(keys);
        set => SetValue(keys, value);
    }
    /// <summary>
    /// Gets the value associated with the specified keys.
    /// </summary>
    /// <param name="keys">The keys to get or set the value for.</param>
    /// <returns>The value associated with the specified keys.</returns>
    public TValue GetValue(object[] keys) => GetValue(keys.AsSpan());
    /// <summary>
    /// Gets the value associated with the specified keys.
    /// </summary>
    /// <param name="keys">The keys to get or set the value for.</param>
    /// <returns>The value associated with the specified keys.</returns>
    public TValue GetValue(ReadOnlySpan<object> keys)
    {
        if (!TryGetValue(keys, out var value))
        {
            ThrowKeysNotFoundException();
        }
        return value;
    }
    /// <summary>
    /// Gets the value associated with the specified keys.
    /// </summary>
    /// <param name="keys">The keys to get or set the value for.</param>
    /// <param name="value">An <see langword="out"/> variable that receives the value associated with the specified keys.</param>
    /// <returns>The value associated with the specified keys.</returns>
    public bool TryGetValue(object[] keys, out TValue value) => TryGetValue(keys.AsSpan(), out value);
    /// <summary>
    /// Gets the value associated with the specified keys.
    /// </summary>
    /// <param name="keys">The keys to get or set the value for.</param>
    /// <param name="value">An <see langword="out"/> variable that receives the value associated with the specified keys.</param>
    /// <returns>The value associated with the specified keys.</returns>
    public bool TryGetValue(ReadOnlySpan<object> keys, out TValue value)
    {
        ArgumentOutOfRangeException.ThrowIfZero(keys.Length);

        if (TryAllocate(keys.Length))
        {
            value = default;
            return false;
        }

        ref var theRef = ref GetRef(keys, false, out var existed);
        if (!existed)
        {
            value = default;
            return false;
        }

        value = theRef;
        return true;
    }
    /// <summary>
    /// Sets the value associated with the specified keys.
    /// </summary>
    /// <param name="keys">The keys to get or set the value for.</param>
    /// <param name="value">The value to associate with the specified keys.</param>
    /// <returns>The value associated with the specified keys.</returns>
    public void SetValue(object[] keys, TValue value) => SetValue(keys.AsSpan(), value);
    /// <summary>
    /// Sets the value associated with the specified keys.
    /// </summary>
    /// <param name="keys">The keys to get or set the value for.</param>
    /// <param name="value">The value to associate with the specified keys.</param>
    /// <returns>The value associated with the specified keys.</returns>
    public void SetValue(ReadOnlySpan<object> keys, TValue value)
    {
        ArgumentOutOfRangeException.ThrowIfZero(keys.Length);

        // Setting doesn't care about whether we were already allocated or not
        TryAllocate(keys.Length);

        ref var theRef = ref GetRef(keys, true, out _);
        theRef = value;
    }
    public void Add(object[] keys, TValue value) => Add(keys.AsSpan(), value);
    public void Add(ReadOnlySpan<object> keys, TValue value)
    {
            
    }

    // This is the workhorse method that does all the accesses, pretty much everything else just delegates to this, then does some post-processing
    /// <summary>
    /// Gets a <see langword="ref"/> into the backing storage of the corresponding dictionary for the specified keys.
    /// If <paramref name="addDefault"/> is <see langword="false"/>, that <see langword="ref"/> may be <see langword="null"/>.
    /// </summary>
    /// <param name="keys">The keys to get the <see langword="ref"/> for.</param>
    /// <param name="addDefault">Whether to add a default value if the key is not found.</param>
    /// <param name="existed">An <see langword="out"/> variable that indicates whether the key-value pair was present in the dictionary.</param>
    /// <returns>A <see langword="ref"/> into the backing storage of the corresponding dictionary for the specified keys.</returns>
    private ref TValue GetRef(ReadOnlySpan<object> keys, bool addDefault, out bool existed)
    {
        Debug.Assert(keys.Length > 0);

        if (addDefault)
        {
            if (TryAllocate(keys.Length);

            switch (keys.Length)
            {
                case 1:
                    ref var theRef = ref CollectionsMarshal.GetValueRefOrNullRef(_one, keys[0]);
                    existed = !SrcsUnsafe.IsNullRef(ref theRef);
                    return ref theRef;
                case 2:
                    theRef = ref CollectionsMarshal.GetValueRefOrNullRef(_two, (keys[0], keys[1]));
                    existed = !SrcsUnsafe.IsNullRef(ref theRef);
                    return ref theRef;
                case 3:
                    theRef = ref CollectionsMarshal.GetValueRefOrNullRef(_three, (keys[0], keys[1], keys[2]));
                    existed = !SrcsUnsafe.IsNullRef(ref theRef);
                    return ref theRef;
                case 4:
                    theRef = ref CollectionsMarshal.GetValueRefOrNullRef(_four, (keys[0], keys[1], keys[2], keys[3]));
                    existed = !SrcsUnsafe.IsNullRef(ref theRef);
                    return ref theRef;
                case 5:
                    theRef = ref CollectionsMarshal.GetValueRefOrNullRef(_five, (keys[0], keys[1], keys[2], keys[3], keys[4]));
                    existed = !SrcsUnsafe.IsNullRef(ref theRef);
                    return ref theRef;
                case 6:
                    theRef = ref CollectionsMarshal.GetValueRefOrNullRef(_six, (keys[0], keys[1], keys[2], keys[3], keys[4], keys[5]));
                    existed = !SrcsUnsafe.IsNullRef(ref theRef);
                    return ref theRef;
                case 7:
                    theRef = ref CollectionsMarshal.GetValueRefOrNullRef(_seven, (keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6]));
                    existed = !SrcsUnsafe.IsNullRef(ref theRef);
                    return ref theRef;
                case 8:
                    theRef = ref CollectionsMarshal.GetValueRefOrNullRef(_eight, (keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7]));
                    existed = !SrcsUnsafe.IsNullRef(ref theRef);
                    return ref theRef;
                default:
                    theRef = ref CollectionsMarshal.GetValueRefOrNullRef(_manyLookup, keys);
                    existed = !SrcsUnsafe.IsNullRef(ref theRef);
                    return ref theRef;
            }
        }
        else
        {
            if (!IsAllocated(keys.Length))
            {
                existed = false;
                return ref SrcsUnsafe.NullRef<TValue>();
            }

            switch (keys.Length)
            {
                case 1:
                    ref var theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_one, keys[0], out existed);
                    return ref theRef;
                case 2:
                    theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_two, (keys[0], keys[1]), out existed);
                    return ref theRef;
                case 3:
                    theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_three, (keys[0], keys[1], keys[2]), out existed);
                    return ref theRef;
                case 4:
                    theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_four, (keys[0], keys[1], keys[2], keys[3]), out existed);
                    return ref theRef;
                case 5:
                    theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_five, (keys[0], keys[1], keys[2], keys[3], keys[4]), out existed);
                    return ref theRef;
                case 6:
                    theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_six, (keys[0], keys[1], keys[2], keys[3], keys[4], keys[5]), out existed);
                    return ref theRef;
                case 7:
                    theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_seven, (keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6]), out existed);
                    return ref theRef;
                case 8:
                    theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_eight, (keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7]), out existed);
                    return ref theRef;
                default:
                    theRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_manyLookup, keys, out existed);
                    return ref theRef;
            }
        }
    }

    /// <summary>
    /// Gets the value associated with the specified keys or adds a new key-value pair if the key combination does not already exist.
    /// </summary>
    /// <param name="keys">The keys of the value to get or add.</param>
    /// <param name="addValue">The value to be added for an absent key combination. If the keys are absent, the return value is this value.</param>
    /// <returns>The value associated with the specified key, if the key is found, otherwise <paramref name="addValue"/>.</returns>
    public TValue GetOrAdd(ReadOnlySpan<object> keys, TValue addValue)
    {
        ref var theRef = ref GetRef(keys, true, out var existed);
        if (!existed)
        {
            return theRef = addValue;
        }
        else
        {
            return theRef;
        }
    }
    /// <summary>
    /// Gets the value associated with the specified keys or adds a new key-value pair to the dictionary if the key combination does not already exist.
    /// </summary>
    /// <param name="keys">The keys of the value to get or add.</param>
    /// <param name="addValueFactory">A factory <see cref="Func{TResult}"/> that produces the value to be added for an absent key combination. If the keys are absent, the return value is the produced value.</param>
    /// <returns>The value associated with the specified key, if the key is found, otherwise the value produced by <paramref name="addValueFactory"/>.</returns>
    public TValue GetOrAdd(ReadOnlySpan<object> keys, Func<TValue> addValueFactory)
    {
        ref var theRef = ref GetRef(keys, true, out var existed);
        if (!existed)
        {
            return theRef = addValueFactory();
        }
        else
        {
            return theRef;
        }
    }
    /// <summary>
    /// Gets the value associated with the specified keys or adds a new key-value pair to the dictionary if the key combination does not exist.
    /// </summary>
    /// <param name="keys">The keys of the value to get or add.</param>
    /// <param name="addValue">The value to add to the dictionary if the key combination does not exist.</param>
    /// <param name="element">An <see langword="out"/> variable that receives the value associated with the specified keys or the added value.</param>
    /// <returns><see langword="true"/> if the key was found in the dictionary, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the <see cref="IDictionary{TKey, TValue}"/> is not mutable.</exception>
    public bool GetOrAdd(ReadOnlySpan<object> keys, TValue addValue, out TValue element)
    {
        ref var theRef = ref GetRef(keys, true, out var existed);
        if (!existed)
        {
            element = theRef = addValue;
        }
        else
        {
            element = theRef;
        }
        return existed;
    }
    /// <summary>
    /// Gets the value associated with the specified keys or adds a new key-value pair produced by a factory to the dictionary if the key combination does not exist.
    /// </summary>
    /// <param name="keys">The keys of the value to get or add.</param>
    /// <param name="addValueFactory">A factory <see cref="Func{TResult}"/> that produces the value to add to the dictionary if the key combination does not exist. This overload is useful when constructing the value is expensive and should only be done when necessary.</param>
    /// <param name="element">An <see langword="out"/> variable that receives the value associated with the specified keys or the added value.</param>
    /// <returns><see langword="true"/> if the key was found in the dictionary, otherwise <see langword="false"/>.</returns>
    public bool GetOrAdd(ReadOnlySpan<object> keys, Func<TValue> addValueFactory, out TValue element)
    {
        ref var theRef = ref GetRef(keys, true, out var existed);
        if (!existed)
        {
            element = theRef = addValueFactory();
        }
        else
        {
            element = theRef;
        }
        return existed;
    }

    /// <summary>
    /// Adds a key-value pair to the dictionary if the key combination does not already exist. Otherwise, a factory <see cref="Func{T, TResult}"/> that produces a new value is invoked with the existing value.
    /// </summary>
    /// <param name="keys">The keys of the element to add or update.</param>
    /// <param name="addValue">The value to be added for an absent key combination.</param>
    /// <param name="updateValueFactory">A factory <see cref="Func{T, TResult}"/> that takes the existing value for a key and produces a new value.</param>
    public void AddOrUpdate(ReadOnlySpan<object> keys, TValue addValue, Func<TValue, TValue> updateValueFactory)
    {
        ref var theRef = ref GetRef(keys, true, out var existed);
        if (!existed)
        {
            theRef = addValue;
        }
        else
        {
            // Validate null only when needed
            ArgumentNullException.ThrowIfNull(updateValueFactory);
            theRef = updateValueFactory(theRef);
        }
    }
    /// <summary>
    /// Adds a key-value pair to the dictionary if the key combination does not already exist. Otherwise, a factory <see cref="Func{T1, T2, TResult}"/> that produces a new value is invoked with the existing value and <paramref name="addValue"/>.
    /// </summary>
    /// <param name="keys">The keys of the element to add or update.</param>
    /// <param name="addValue">The value to be added for an absent key combination.</param>
    /// <param name="updateValueFactory">A factory <see cref="Func{T1, T2, TResult}"/> that takes the existing value for a key and <paramref name="addValue"/> itself and produces a new value. This avoids having to materialize the value twice.</param>
    public void AddOrUpdate(ReadOnlySpan<object> keys, TValue addValue, Func<TValue, TValue, TValue> updateValueFactory)
    {
        ref var theRef = ref GetRef(keys, true, out var existed);
        if (!existed)
        {
            theRef = addValue;
        }
        else
        {
            // Validate null only when needed
            ArgumentNullException.ThrowIfNull(updateValueFactory);
            theRef = updateValueFactory(theRef, addValue);
        }
    }
    /// <summary>
    /// Adds a key-value pair to the dictionary where the value is produced by <paramref name="addValueFactory"/> if the key combination does not already exist. Otherwise, a factory <see cref="Func{T, TResult}"/> that produces a new value is invoked with the existing value.
    /// </summary>
    /// <param name="keys">The keys of the element to add or update.</param>
    /// <param name="addValueFactory">A <see cref="Func{TResult}"/> that produces the value to be added for an absent key combination. It is only invoked if the key combination does not already exist in the <see cref="IDictionary{TKey, TValue}"/>.</param>
    /// <param name="updateValueFactory">A factory <see cref="Func{T1, T2, TResult}"/> that takes the existing value for a key and produces a new value.</param>
    public void AddOrUpdate(ReadOnlySpan<object> keys, Func<TValue> addValueFactory, Func<TValue, TValue> updateValueFactory)
    {
        ref var theRef = ref GetRef(keys, true, out var existed);
        if (!existed)
        {
            theRef = addValueFactory();
        }
        else
        {
            // Validate null only when needed
            ArgumentNullException.ThrowIfNull(updateValueFactory);
            theRef = updateValueFactory(theRef);
        }
    }

    /// <summary>
    /// Returns a <see langword="ref"/> into the storage to where the value was found if the key-value pair was present, otherwise returns a <see langword="null"/> <see langword="ref"/>.
    /// If <paramref name="existed"/> is <see langword="false"/> when control returns to the caller, using the returned <see langword="ref"/> is undefined behavior and will likely result in a <see cref="NullReferenceException"/>.
    /// </summary>
    /// <param name="keys">The keys of the value to get.</param>
    /// <param name="existed">An <see langword="out"/> variable that indicates whether the key-value pair was present.</param>
    /// <returns>A <see langword="ref"/> into the storage to where the value was found if the key-value pair was present, otherwise a <see langword="null"/> <see langword="ref"/>.</returns>
    public ref TValue GetValueRefOrNullRef(ReadOnlySpan<object> keys, out bool existed) => ref GetRef(keys, false, out existed);
    /// <summary>
    /// Checks if the dictionary contains a key-value pair with the specified <paramref name="keys"/>, adds the <see langword="default"/> for <typeparamref name="TValue"/> if not and returns a <see langword="ref"/> into that storage.
    /// </summary>
    /// <param name="keys">The keys of the value to get.</param>
    /// <param name="existed">An <see langword="out"/> variable that indicates whether the key-value pair was present.</param>
    /// <returns>A <see langword="ref"/> to where the value was found.</returns>
    public ref TValue GetValueRefOrAddDefault(ReadOnlySpan<object> keys, out bool existed) => ref GetRef(keys, true, out existed);
    /// <summary>
    /// Checks if the dictionary contains a key-value pair with the specified <paramref name="keys"/>, adds one with the specified <paramref name="value"/> if not and returns a <see langword="ref"/> into that storage.
    /// </summary>
    /// <param name="keys">The keys of the value to get or add.</param>
    /// <param name="value">The value to add if the key combination does not exist.</param>
    /// <param name="existed">An <see langword="out"/> variable that indicates whether the key-value pair was present.</param>
    /// <returns>A <see langword="ref"/> to where the value was found.</returns>
    public ref TValue GetValueRefOrAdd(ReadOnlySpan<object> keys, TValue value, out bool existed)
    {
        ref var theRef = ref GetRef(keys, true, out existed);
        if (!existed)
        {
            theRef = value;
        }
        return ref theRef;
    }
    /// <summary>
    /// Checks if the dictionary contains a key-value pair with the specified <paramref name="keys"/>, adds one with the value produced by <paramref name="valueFactory"/> if not and returns a <see langword="ref"/> into that storage.
    /// </summary>
    /// <param name="keys">The keys of the value to get or add.</param>
    /// <param name="valueFactory">A factory <see cref="Func{TResult}"/> that produces the value to add if the key combination does not exist.</param>
    /// <param name="existed">An <see langword="out"/> variable that indicates whether the key-value pair was present.</param>
    /// <returns>A <see langword="ref"/> to where the value was found.</returns>
    public ref TValue GetValueRefOrAdd(ReadOnlySpan<object> keys, Func<TValue> valueFactory, out bool existed)
    {
        ref var theRef = ref GetRef(keys, true, out existed);
        if (!existed)
        {
            theRef = valueFactory();
        }
        return ref theRef;
    }
}
