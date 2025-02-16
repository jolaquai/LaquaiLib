namespace LaquaiLib.Collections;

#pragma warning disable IDE0044 // Add readonly modifier

/// <summary>
/// Implements a dictionary that maps keys and specific orders of those keys to values.
/// </summary>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
/// <remarks>
/// <see langword="struct"/>s used for the keys in this dictionary will be boxed. This incurs an allocation and performance penalty.
/// </remarks>
public class MultiKeyDictionary<TValue>
{
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

    private bool EnsureIsAllocated(int keyCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(keyCount);
        ArgumentOutOfRangeException.ThrowIfZero(keyCount);

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
                Allocate(2, keyCount);
                // Enables fast paths in getter methods since a newly allocated dictionary cannot possible contain the sought key
                return true;
        }

        return false;
    }
    private void Allocate(int capacity, int mostLikelyKeyCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(mostLikelyKeyCount);
        ArgumentOutOfRangeException.ThrowIfZero(mostLikelyKeyCount);

        switch (mostLikelyKeyCount)
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

    public TValue this[params ReadOnlySpan<object> keys]
    {
        get => Get(keys);
        set => Set(keys, value);
    }
    public TValue Get(object[] keys) => Get(keys.AsSpan());
    public TValue Get(params ReadOnlySpan<object> keys)
    {
        ArgumentOutOfRangeException.ThrowIfZero(keys.Length);

        if (EnsureIsAllocated(keys.Length))
        {
            throw new KeyNotFoundException("The specified key combination was not found.");
        }

        switch (keys.Length)
        {
            case 1:
                return _one[keys[0]];
            case 2:
                return _two[(keys[0], keys[1])];
            case 3:
                return _three[(keys[0], keys[1], keys[2])];
            case 4:
                return _four[(keys[0], keys[1], keys[2], keys[3])];
            case 5:
                return _five[(keys[0], keys[1], keys[2], keys[3], keys[4])];
            case 6:
                return _six[(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5])];
            case 7:
                return _seven[(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6])];
            case 8:
                return _eight[(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7])];
            default:
                return _manyLookup[keys];
        }
    }
    public void Set(object[] keys, TValue value) => Set(keys.AsSpan(), value);
    public void Set(ReadOnlySpan<object> keys, TValue value)
    {
        ArgumentOutOfRangeException.ThrowIfZero(keys.Length);

        // Setting doesn't care about whether we were already allocated or not
        EnsureIsAllocated(keys.Length);

        switch (keys.Length)
        {
            case 1:
                _one[keys[0]] = value;
                break;
            case 2:
                _two[(keys[0], keys[1])] = value;
                break;
            case 3:
                _three[(keys[0], keys[1], keys[2])] = value;
                break;
            case 4:
                _four[(keys[0], keys[1], keys[2], keys[3])] = value;
                break;
            case 5:
                _five[(keys[0], keys[1], keys[2], keys[3], keys[4])] = value;
                break;
            case 6:
                _six[(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5])] = value;
                break;
            case 7:
                _seven[(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6])] = value;
                break;
            case 8:
                _eight[(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7])] = value;
                break;
            default:
                _manyLookup[keys] = value;
                break;
        }
    }
}
