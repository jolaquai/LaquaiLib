using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using SrcsUnsafe = System.Runtime.CompilerServices.Unsafe;

namespace LaquaiLib.Collections;

/// <summary>
/// Implements a dictionary that maps keys and specific orders of those keys to values.
/// </summary>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
/// <remarks>
/// <see langword="struct"/>s used for the keys in this dictionary will be boxed. This incurs an allocation and performance penalty.
/// <para/>The <see langword="string"/> keys <c>"foo"</c> and <c>"bar"</c> are, used by themselves, entirely independent of the key combinations <c>("foo", "bar")</c> and <c>("bar", "foo")</c>.
/// </remarks>
public class MultiKeyDictionary<TValue>
{
    private class ObjectArraySpanComparer([DisallowNull] IEqualityComparer<object> inner) : IEqualityComparer<object>, IAlternateEqualityComparer<ReadOnlySpan<object>, object[]>
    {
        private readonly IEqualityComparer<object> _inner = inner;

        private static ObjectArraySpanComparer instance;
        public static ObjectArraySpanComparer GetInstance(IEqualityComparer<object> innerComparer = null) => instance ??= new ObjectArraySpanComparer(innerComparer ?? EqualityComparer<object>.Default);

        public object[] Create(ReadOnlySpan<object> alternate) => alternate.ToArray();
        public bool Equals(ReadOnlySpan<object> alternate, object[] other) => alternate.SequenceEqual(other, _inner);
        public bool Equals(object x, object y) => _inner.Equals(x, y);
        public int GetHashCode(ReadOnlySpan<object> alternate)
        {
            // Stolen directly from Array.cs
            HashCode hashCode = default;

            for (var i = alternate.Length >= 8 ? alternate.Length - 8 : 0; i < alternate.Length; i++)
            {
                hashCode.Add(_inner.GetHashCode(alternate[i]));
            }

            return hashCode.ToHashCode();
        }
        public int GetHashCode([DisallowNull] object obj) => obj.GetHashCode();
    }

    [DoesNotReturn] private static void ThrowKeysNotFoundException() => throw new KeyNotFoundException("The specified key combination was not found.");
    [DoesNotReturn] private static void ThrowKeysAlreadyExistsException() => throw new InvalidOperationException("The specified key combination already exists in the dictionary.");

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
    private int KeySum => (_one?.Count ?? 0) + (_two?.Count ?? 0) + (_three?.Count ?? 0) + (_four?.Count ?? 0) + (_five?.Count ?? 0) + (_six?.Count ?? 0) + (_seven?.Count ?? 0) + (_eight?.Count ?? 0) + (_many?.Count ?? 0);

    /// <summary>
    /// Initializes a new <see cref="MultiKeyDictionary{TValue}"/> with no backing storage allocated.
    /// </summary>
    public MultiKeyDictionary() { }
    /// <summary>
    /// Initializes a new <see cref="MultiKeyDictionary{TValue}"/> with the specified capacity for the specified most likely key count.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    /// <param name="mostLikelyKeyCount">The number of keys that will be used most likely.</param>
    public MultiKeyDictionary(int capacity, int mostLikelyKeyCount) => Allocate(capacity, mostLikelyKeyCount);

    private bool IsAllocated(int keyCount)
    {
        Debug.Assert(keyCount > 0);

        switch (keyCount)
        {
            case 1 when _one is not null:
            case 2 when _two is not null:
            case 3 when _three is not null:
            case 4 when _four is not null:
            case 5 when _five is not null:
            case 6 when _six is not null:
            case 7 when _seven is not null:
            case 8 when _eight is not null:
            case > 8 when _many is not null:
                return true;
            default:
                return false;
        }
    }
    /// <summary>
    /// Checks whether a backing storage for the specified number of keys is allocated and allocates it if not.
    /// </summary>
    /// <param name="keyCount">The number of keys to allocate the backing storage for.</param>
    /// <returns><see langword="false"/> if the backing storage needed to be allocated by the call to this method, otherwise <see langword="true"/> (i.e. the backing storage was already allocated).</returns>
    /// <remarks>
    /// Multiple calls to this method have no effect after the backing storage has been allocated. To clear the backing storage associated with the specified <paramref name="keyCount"/>, use <see cref="Clear(int)"/>.
    /// </remarks>
    private bool TryAllocate(int keyCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(keyCount);
        ArgumentOutOfRangeException.ThrowIfZero(keyCount);

        if (IsAllocated(keyCount))
        {
            return true;
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
                // Enables fast paths in getter methods since a newly allocated dictionary cannot possibly contain the sought key
                return false;
            default:
                Debug.Fail("Impossible key count");
                throw new ArgumentException("Impossible key count", nameof(keyCount));
        }
    }
    private void Allocate(int capacity, int keyCount)
    {
        Debug.Assert(capacity >= 0);
        Debug.Assert(keyCount >= 0);

        switch (keyCount)
        {
            case 1 when _one is null:
                _one = new(capacity);
                break;
            case 2 when _two is null:
                _two = new(capacity);
                break;
            case 3 when _three is null:
                _three = new(capacity);
                break;
            case 4 when _four is null:
                _four = new(capacity);
                break;
            case 5 when _five is null:
                _five = new(capacity);
                break;
            case 6 when _six is null:
                _six = new(capacity);
                break;
            case 7 when _seven is null:
                _seven = new(capacity);
                break;
            case 8 when _eight is null:
                _eight = new(capacity);
                break;
            case > 8 when _many is null:
                // Need a custom comparer for this unfortunately
                _many = new(capacity, ObjectArraySpanComparer.GetInstance(EqualityComparer<object>.Default));
                _manyLookup = _many.GetAlternateLookup<ReadOnlySpan<object>>();
                break;
        }
    }

    /// <summary>
    /// Gets or sets the value associated with the specified keys.
    /// </summary>
    /// <param name="keys">The keys to get or set the value for.</param>
    /// <returns>The value associated with the specified keys.</returns>
    public TValue this[params ReadOnlySpan<object> keys]
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
    /// <inheritdoc cref="GetValue(object[])"/>
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
    /// <inheritdoc cref="TryGetValue(object[], out TValue)"/>
    public bool TryGetValue(ReadOnlySpan<object> keys, out TValue value)
    {
        ArgumentOutOfRangeException.ThrowIfZero(keys.Length);

        if (!TryAllocate(keys.Length))
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
    /// <inheritdoc cref="SetValue(object[], TValue)"/>
    public void SetValue(ReadOnlySpan<object> keys, TValue value)
    {
        ArgumentOutOfRangeException.ThrowIfZero(keys.Length);

        // Setting doesn't care about whether we were already allocated or not
        TryAllocate(keys.Length);

        ref var theRef = ref GetRef(keys, true, out _);
        theRef = value;
    }
    /// <summary>
    /// Sets the value associated with the specified keys if that combination does not already exist.
    /// If it does, an <see cref="ArgumentException"/> is thrown.
    /// </summary>
    /// <param name="keys">The keys to set the value for.</param>
    /// <param name="value">The value to associate with the specified keys.</param>
    public void Add(object[] keys, TValue value) => Add(keys.AsSpan(), value);
    /// <inheritdoc cref="Add(object[], TValue)"/>
    public void Add(ReadOnlySpan<object> keys, TValue value)
    {
        if (!TryAdd(keys, value))
        {
            ThrowKeysAlreadyExistsException();
        }
    }
    /// <summary>
    /// Sets the value associated with the specified keys if that combination does not already exist.
    /// </summary>
    /// <param name="keys">The keys to set the value for.</param>
    /// <param name="value">The value to associate with the specified keys.</param>
    /// <returns><see langword="true"/> if the call to this method added the key-value pair, otherwise <see langword="false"/> (that is, the key combination already existed).</returns>
    public bool TryAdd(object[] keys, TValue value) => TryAdd(keys.AsSpan(), value);
    /// <inheritdoc cref="TryAdd(object[], TValue)"/>
    public bool TryAdd(ReadOnlySpan<object> keys, TValue value)
    {
        ref var theRef = ref GetRef(keys, true, out var existed);
        Debug.Assert(!SrcsUnsafe.IsNullRef(ref theRef));
        if (existed)
        {
            return false;
        }
        theRef = value;
        return true;
    }

    /// <summary>
    /// Removes all values from the backing storage.
    /// </summary>
    public void Clear()
    {
        _one?.Clear();
        _two?.Clear();
        _three?.Clear();
        _four?.Clear();
        _five?.Clear();
        _six?.Clear();
        _seven?.Clear();
        _eight?.Clear();
        _many?.Clear();
    }
    /// <summary>
    /// Clears the backing storage associated with the specified number of keys.
    /// </summary>
    /// <param name="keyCount">The number of keys to clear the backing storage for.</param>
    public void Clear(int keyCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(keyCount);
        ArgumentOutOfRangeException.ThrowIfZero(keyCount);

        if (!IsAllocated(keyCount))
        {
            return;
        }

        switch (keyCount)
        {
            case 1:
                _one.Clear();
                break;
            case 2:
                _two.Clear();
                break;
            case 3:
                _three.Clear();
                break;
            case 4:
                _four.Clear();
                break;
            case 5:
                _five.Clear();
                break;
            case 6:
                _six.Clear();
                break;
            case 7:
                _seven.Clear();
                break;
            case 8:
                _eight.Clear();
                break;
            default:
                _many.Clear();
                break;
        }
    }

    // This is the workhorse method that does all the accesses, pretty much everything else just delegates to this, then does some post-processing
    /// <summary>
    /// Gets a <see langword="ref"/> into the backing storage of the corresponding dictionary for the specified keys.
    /// If <paramref name="addDefault"/> is <see langword="false"/>, that <see langword="ref"/> may be <see langword="null"/>.
    /// </summary>
    /// <param name="keys">The keys to get the <see langword="ref"/> for.</param>
    /// <param name="addDefault">Whether to add a default value if the key is not found (or even allocate the entire dictionary if it is <see langword="null"/>).</param>
    /// <param name="existed">An <see langword="out"/> variable that indicates whether the accessed dictionary was allocated and the key was present in the dictionary.</param>
    /// <returns>A <see langword="ref"/> into the backing storage of the corresponding dictionary for the specified keys.</returns>
    private ref TValue GetRef(ReadOnlySpan<object> keys, bool addDefault, out bool existed)
    {
        Debug.Assert(keys.Length > 0);

        if (addDefault)
        {
            _ = TryAllocate(keys.Length);

            // Unfortunately no fast path for this case
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
        else
        {
            // Fast path if the dictionary is not even allocated
            if (!IsAllocated(keys.Length))
            {
                existed = false;
                return ref SrcsUnsafe.NullRef<TValue>();
            }

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

    /// <summary>
    /// Creates an array of all keys in the dictionary (that is, the array is a shallow copy of the keys).
    /// </summary>
    public object[] Keys
    {
        get
        {
            var keys = new object[KeySum];
            var index = 0;
            if (_one is not null)
            {
                foreach (var key in _one.Keys)
                {
                    keys[index++] = key;
                }
            }
            if (_two is not null)
            {
                foreach (var key in _two.Keys)
                {
                    keys[index++] = key;
                }
            }
            if (_three is not null)
            {
                foreach (var key in _three.Keys)
                {
                    keys[index++] = key;
                }
            }
            if (_four is not null)
            {
                foreach (var key in _four.Keys)
                {
                    keys[index++] = key;
                }
            }
            if (_five is not null)
            {
                foreach (var key in _five.Keys)
                {
                    keys[index++] = key;
                }
            }
            if (_six is not null)
            {
                foreach (var key in _six.Keys)
                {
                    keys[index++] = key;
                }
            }
            if (_seven is not null)
            {
                foreach (var key in _seven.Keys)
                {
                    keys[index++] = key;
                }
            }
            if (_eight is not null)
            {
                foreach (var key in _eight.Keys)
                {
                    keys[index++] = key;
                }
            }
            if (_many is not null)
            {
                foreach (var key in _many.Keys)
                {
                    keys[index++] = key;
                }
            }
            return keys;
        }
    }
    /// <summary>
    /// Enumerates all values in the dictionary, regardless of the number of keys used to store them.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of all values in the dictionary.</returns>
    public IEnumerable<TValue> Values
    {
        get
        {
            if (_one is not null)
            {
                foreach (var value in _one.Values)
                {
                    yield return value;
                }
            }
            if (_two is not null)
            {
                foreach (var value in _two.Values)
                {
                    yield return value;
                }
            }
            if (_three is not null)
            {
                foreach (var value in _three.Values)
                {
                    yield return value;
                }
            }
            if (_four is not null)
            {
                foreach (var value in _four.Values)
                {
                    yield return value;
                }
            }
            if (_five is not null)
            {
                foreach (var value in _five.Values)
                {
                    yield return value;
                }
            }
            if (_six is not null)
            {
                foreach (var value in _six.Values)
                {
                    yield return value;
                }
            }
            if (_seven is not null)
            {
                foreach (var value in _seven.Values)
                {
                    yield return value;
                }
            }
            if (_eight is not null)
            {
                foreach (var value in _eight.Values)
                {
                    yield return value;
                }
            }
            if (_many is not null)
            {
                foreach (var value in _many.Values)
                {
                    yield return value;
                }
            }
        }
    }
}
