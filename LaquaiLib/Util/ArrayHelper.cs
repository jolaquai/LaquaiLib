using System.Collections;
using System.Diagnostics;

namespace LaquaiLib.Util;

/// <summary>
/// Contains helper methods for arrays.
/// </summary>
public static class ArrayHelper
{
    private static bool Reverse(int[] indices, Array keys)
    {
        Array.Reverse(indices);
        Array.Reverse(keys);

        if (indices.AsSpan().SequenceEqual(Enumerable.Range(0, indices.Length).ToArray()))
        {
            return false;
        }
        return true;
    }

    private static void SortGenericImpl<TKey, TValue>(TKey[] keys, IComparer<TKey> comparer, TValue[][] itemsArrays, Func<int[], TKey[], bool> inBetween)
    {
        comparer ??= Comparer<TKey>.Default;

        // What's in these is irrelevant if ValidateAndGetKeys returns false so we can skip initialization
        System.Runtime.CompilerServices.Unsafe.SkipInit(out int keysLength);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out int[] indices);
        if (!ValidateAndGetKeys(keys, null, comparer, itemsArrays, ref keysLength, ref indices))
        {
            return;
        }

        if (inBetween?.Invoke(indices, keys) is false)
        {
            return;
        }

        // Since we know all the passed arrays have the same length, we can use the same temp array for all of them in turn
        var temp = new TValue[keysLength];
        for (var i = 0; i < itemsArrays.Length; i++)
        {
            // Copy the current array to the temp array
            Array.Copy(itemsArrays[i], temp, keysLength);
            // Reassign each index using the sorted indices
            for (var j = 0; j < keysLength; j++)
            {
                itemsArrays[i][j] = temp[indices[j]];
            }
        }
    }
    private static void SortNonGenericImpl(Array keys, IComparer comparer, Array[] itemsArrays, Func<int[], Array, bool> inBetween)
    {
        comparer ??= Comparer.Default;

        // What's in these is irrelevant if ValidateAndGetKeys returns false so we can skip initialization
        System.Runtime.CompilerServices.Unsafe.SkipInit(out int keysLength);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out int[] indices);
        if (!ValidateAndGetKeys<object>(keys, comparer, null, itemsArrays, ref keysLength, ref indices))
        {
            return;
        }

        if (inBetween?.Invoke(indices, keys) is false)
        {
            return;
        }

        // Since we know all the passed arrays have the same length, we can use the same temp array for all of them in turn
        var temp = new object[keysLength];
        for (var i = 0; i < itemsArrays.Length; i++)
        {
            // Copy the current array to the temp array
            Array.Copy(itemsArrays[i], temp, keysLength);
            // Reassign each index using the sorted indices
            for (var j = 0; j < keysLength; j++)
            {
                System.Runtime.CompilerServices.Unsafe.As<Array>(itemsArrays.GetValue(i)).SetValue(temp[indices[j]], j);
            }
        }
    }
    private static bool ValidateAndGetKeys<TKey>(Array keys, IComparer comparer, IComparer<TKey> genericComparer, Array[] itemsArrays, ref int keysLength, ref int[] indices)
    {
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(itemsArrays);
        if (itemsArrays.Length == 0)
        {
            return false; // Nothing to sort
        }

        var keysLengthLocal = keysLength = keys.Length;
        if (itemsArrays.Any(a => a.Length != keysLengthLocal))
        {
            throw new ArgumentException("The length of the keys array must be equal to the length of all items arrays.");
        }

        indices = Enumerable.Range(0, keysLength).ToArray();
        var originalIndices = new int[keysLength];
        indices.CopyTo(originalIndices);

        if (comparer is not null)
        {
            Array.Sort(keys, indices, comparer);
        }
        else if (keys is TKey[] typedKeys)
        {
            Array.Sort(typedKeys, indices, genericComparer);
        }
        else
        {
            Debug.Fail("Keys array is not of the same type as the comparer. We should not be here.");
            throw new InvalidOperationException("The keys array must be of the same type as the comparer.");
        }

        // If the indices array didn't change (it's SequenceEquals to the original ascending ints), then leave early
        // Force this to use the span implementation since its significantly faster than LINQ
        if (indices.AsSpan().SequenceEqual(originalIndices))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays using the default comparer for <typeparamref name="TKey"/>.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void Sort<TKey, TValue>(TKey[] keys, params TValue[][] itemsArrays) => Sort<TKey, TValue>(keys, null, itemsArrays);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the specified <paramref name="comparer"/>.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void Sort<TKey, TValue>(TKey[] keys, IComparer<TKey> comparer, params TValue[][] itemsArrays) => SortGenericImpl(keys, comparer, itemsArrays, null);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays with unspecified types using the default comparer.
    /// Note that this method is significantly slower than the generic version.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void Sort(Array keys, params Array[] itemsArrays) => Sort(keys, null, itemsArrays);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays with unspecified types using the specified <paramref name="comparer"/>.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void Sort(Array keys, IComparer comparer, params Array[] itemsArrays) => SortNonGenericImpl(keys, comparer, itemsArrays, null);

    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays using the default comparer for <typeparamref name="TKey"/>.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void SortDescending<TKey, TValue>(TKey[] keys, params TValue[][] itemsArrays) => SortDescending<TKey, TValue>(keys, null, itemsArrays);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the specified <paramref name="comparer"/>.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void SortDescending<TKey, TValue>(TKey[] keys, IComparer<TKey> comparer, params TValue[][] itemsArrays)
        => SortGenericImpl(keys, comparer, itemsArrays, Reverse);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays with unspecified types using the default comparer.
    /// Note that this method is significantly slower than the generic version.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void SortDescending(Array keys, params Array[] itemsArrays) => SortDescending(keys, null, itemsArrays);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays with unspecified types using the specified <paramref name="comparer"/>.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void SortDescending(Array keys, IComparer comparer, params Array[] itemsArrays)
        => SortNonGenericImpl(keys, comparer, itemsArrays, Reverse);
}
