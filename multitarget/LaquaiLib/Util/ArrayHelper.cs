using System.Diagnostics;

namespace LaquaiLib.Util;

/// <summary>
/// Contains helper methods for arrays and spans.
/// </summary>
public static class ArrayHelper
{
    // True if indices is not the identity permutation [0, 1, 2, ...] - needs no comparison buffer, unlike SequenceEqual against a materialized range
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PermutationChanged(int[] indices)
    {
        for (var i = 0; i < indices.Length; i++)
            if (indices[i] != i)
                return true;
        return false;
    }

    private static bool Reverse(int[] indices, Array keys)
    {
        Array.Reverse(indices);
        Array.Reverse(keys);
        return PermutationChanged(indices);
    }

    private static void SortGenericImpl<TKey, TValue>(TKey[] keys, IComparer<TKey> comparer, TValue[][] itemsArrays, delegate*<int[], TKey[], bool> inBetween)
    {
        comparer ??= Comparer<TKey>.Default;

        // What's in these is irrelevant if ValidateAndGetKeys returns null so we can skip initialization
        Unsafe.SkipInit(out int keysLength);
        Unsafe.SkipInit(out int[] indices);
        var changed = ValidateAndGetKeys(keys, null, comparer, itemsArrays, ref keysLength, ref indices);
        if (changed is null)
            return;

        if (inBetween is not null)
            unsafe
            {
                if (!inBetween(indices, keys))
                    return;
            }
        else if (!changed.Value)
            return;

        // Since we know all the passed arrays have the same length, we can use the same temp array for all of them in turn
        var temp = GC.AllocateUninitializedArray<TValue>(keysLength);
        for (var i = 0; i < itemsArrays.Length; i++)
        {
            var arr = itemsArrays[i];
            // Copy the current array to the temp array
            Array.Copy(arr, temp, keysLength);
            // Reassign each index using the sorted indices
            for (var j = 0; j < keysLength; j++)
                arr[j] = temp[indices[j]];
        }
    }
    private static void SortNonGenericImpl(Array keys, IComparer comparer, Array[] itemsArrays, delegate*<int[], Array, bool> inBetween)
    {
        comparer ??= Comparer.Default;

        // What's in these is irrelevant if ValidateAndGetKeys returns null so we can skip initialization
        Unsafe.SkipInit(out int keysLength);
        Unsafe.SkipInit(out int[] indices);
        var changed = ValidateAndGetKeys<object>(keys, comparer, null, itemsArrays, ref keysLength, ref indices);
        if (changed is null)
            return;

        if (inBetween is not null)
            unsafe
            {
                if (!inBetween(indices, keys))
                    return;
            }
        else if (!changed.Value)
            return;

        // Since we know all the passed arrays have the same length, we can use the same temp array for all of them in turn
        var temp = new object[keysLength];
        for (var i = 0; i < itemsArrays.Length; i++)
        {
            var arr = itemsArrays[i];
            // Copy the current array to the temp array
            Array.Copy(arr, temp, keysLength);
            // Reassign each index using the sorted indices
            for (var j = 0; j < keysLength; j++)
                arr.SetValue(temp[indices[j]], j);
        }
    }
    // items (TKey[]) and itemsArrays (TValue[][]) are unrelated element types and can't share one reassignment pass the way
    // SortGenericImpl's homogeneous itemsArrays can - so this sorts interimKeys once to get the permutation, then applies it
    // via two separate typed passes. Nothing here boxes: Array.Sort(TCompare[], int[], IComparer<TCompare>) and both
    // reassignment loops stay fully typed.
    private static void SortGenericSelectorImpl<TKey, TCompare, TValue>(TKey[] items, TCompare[] interimKeys, IComparer<TCompare> comparer, TValue[][] itemsArrays, delegate*<int[], TCompare[], bool> inBetween)
    {
        comparer ??= Comparer<TCompare>.Default;

        var keysLength = interimKeys.Length;
        for (var i = 0; i < itemsArrays.Length; i++)
            if (itemsArrays[i].Length != keysLength)
                throw new ArgumentException("The length of the keys array must be equal to the length of all items arrays.");

        var indices = GC.AllocateUninitializedArray<int>(keysLength);
        for (var i = 0; i < keysLength; i++)
            indices[i] = i;

        Array.Sort(interimKeys, indices, comparer);

        if (inBetween is not null)
            unsafe
            {
                if (!inBetween(indices, interimKeys))
                    return;
            }
        else if (!PermutationChanged(indices))
            return;

        // items always needs reassigning here - unlike itemsArrays, it's never optional, so it gets its own temp buffer
        var tempKeys = GC.AllocateUninitializedArray<TKey>(keysLength);
        Array.Copy(items, tempKeys, keysLength);
        for (var j = 0; j < keysLength; j++)
            items[j] = tempKeys[indices[j]];

        if (itemsArrays.Length == 0)
            return;

        var temp = GC.AllocateUninitializedArray<TValue>(keysLength);
        for (var i = 0; i < itemsArrays.Length; i++)
        {
            var arr = itemsArrays[i];
            Array.Copy(arr, temp, keysLength);
            for (var j = 0; j < keysLength; j++)
                arr[j] = temp[indices[j]];
        }
    }
    // null = nothing to sort (itemsArrays empty), false = sort was a no-op, true = permutation changed
    private static bool? ValidateAndGetKeys<TKey>(Array keys, IComparer comparer, IComparer<TKey> genericComparer, Array[] itemsArrays, ref int keysLength, ref int[] indices)
    {
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(itemsArrays);
        if (itemsArrays.Length == 0)
            return null; // Nothing to sort

        keysLength = keys.Length;
        for (var i = 0; i < itemsArrays.Length; i++)
            if (itemsArrays[i].Length != keysLength)
                throw new ArgumentException("The length of the keys array must be equal to the length of all items arrays.");

        indices = GC.AllocateUninitializedArray<int>(keysLength);
        for (var i = 0; i < keysLength; i++)
            indices[i] = i;

        switch (comparer)
        {
            case not null:
                Array.Sort(keys, indices, comparer);
                break;
            default:
                if (keys is TKey[] typedKeys)
                    Array.Sort(typedKeys, indices, genericComparer);
                else
                {
                    Debug.Fail("Keys array is not of the same type as the comparer. We should not be here.");
                    throw new InvalidOperationException("The keys array must be of the same type as the comparer.");
                }

                break;
        }

        return PermutationChanged(indices);
    }

    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the default comparer for <typeparamref name="TKey"/> in ascending order.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<TKey, TValue>(TKey[] keys, params TValue[][] itemsArrays) => Sort<TKey, TValue>(keys, null, itemsArrays);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the specified <paramref name="comparer"/> in ascending order.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<TKey, TValue>(TKey[] keys, IComparer<TKey> comparer, params TValue[][] itemsArrays)
    {
        unsafe
        {
            SortGenericImpl(keys, comparer, itemsArrays, null);
        }
    }

    /// <summary>
    /// According to an array of keys produced using the specified <paramref name="selector"/> function, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the default comparer for <typeparamref name="TCompare"/> in ascending order.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TCompare">The type of the keys produced by the <paramref name="selector"/> function.</typeparam>
    /// <typeparam name="TValue">The type of the items in the arrays to sort.</typeparam>
    /// <param name="items">The array of keys to sort by.</param>
    /// <param name="selector">The function to produce the keys to sort by.</param>
    /// <param name="itemsArray">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<TKey, TCompare, TValue>(TKey[] items, Func<TKey, TCompare> selector, params TValue[][] itemsArray) => Sort<TKey, TCompare, TValue>(items, selector, null, itemsArray);
    /// <summary>
    /// According to an array of keys produced using the specified <paramref name="selector"/> function, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the specified <paramref name="comparer"/> in ascending order.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TCompare">The type of the keys produced by the <paramref name="selector"/> function.</typeparam>
    /// <typeparam name="TValue">The type of the items in the arrays to sort.</typeparam>
    /// <param name="items">The array of keys to sort by.</param>
    /// <param name="selector">The function to produce the keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void Sort<TKey, TCompare, TValue>(TKey[] items, Func<TKey, TCompare> selector, IComparer<TCompare> comparer, params TValue[][] itemsArrays)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(itemsArrays);
        var interimKeys = GC.AllocateUninitializedArray<TCompare>(items.Length);
        for (var i = 0; i < interimKeys.Length; i++)
            interimKeys[i] = selector(items[i]);
        unsafe
        {
            SortGenericSelectorImpl(items, interimKeys, comparer, itemsArrays, null);
        }
    }
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the default comparer for <typeparamref name="TKey"/> in descending order.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortDescending<TKey, TValue>(TKey[] keys, params TValue[][] itemsArrays) => SortDescending<TKey, TValue>(keys, null, itemsArrays);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the specified <paramref name="comparer"/> in descending order.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortDescending<TKey, TValue>(TKey[] keys, IComparer<TKey> comparer, params TValue[][] itemsArrays)
    {
        unsafe
        { SortGenericImpl(keys, comparer, itemsArrays, &Reverse); }
    }

    /// <summary>
    /// According to an array of keys produced using the specified <paramref name="selector"/> function, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the default comparer for <typeparamref name="TCompare"/> in descending order.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TCompare">The type of the keys produced by the <paramref name="selector"/> function.</typeparam>
    /// <typeparam name="TValue">The type of the items in the arrays to sort.</typeparam>
    /// <param name="items">The array of keys to sort by.</param>
    /// <param name="selector">The function to produce the keys to sort by.</param>
    /// <param name="itemsArray">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortDescending<TKey, TCompare, TValue>(TKey[] items, Func<TKey, TCompare> selector, params TValue[][] itemsArray) => SortDescending<TKey, TCompare, TValue>(items, selector, null, itemsArray);
    /// <summary>
    /// According to an array of keys produced using the specified <paramref name="selector"/> function, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the specified <paramref name="comparer"/> in descending order.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TCompare">The type of the keys produced by the <paramref name="selector"/> function.</typeparam>
    /// <typeparam name="TValue">The type of the items in the arrays to sort.</typeparam>
    /// <param name="items">The array of keys to sort by.</param>
    /// <param name="selector">The function to produce the keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void SortDescending<TKey, TCompare, TValue>(TKey[] items, Func<TKey, TCompare> selector, IComparer<TCompare> comparer, params TValue[][] itemsArrays)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(itemsArrays);
        var interimKeys = GC.AllocateUninitializedArray<TCompare>(items.Length);
        for (var i = 0; i < interimKeys.Length; i++)
            interimKeys[i] = selector(items[i]);
        unsafe
        {
            SortGenericSelectorImpl(items, interimKeys, comparer, itemsArrays, &Reverse);
        }
    }

    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays with unspecified types using the default comparer.
    /// Note that this method is significantly slower than the generic version, but does allow for sorting arbitrarily typed arrays.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort(Array keys, params Array[] itemsArrays) => Sort(keys, (IComparer)null, itemsArrays);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays with unspecified types using the specified <paramref name="comparer"/>.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void Sort(Array keys, IComparer comparer, params Array[] itemsArrays)
    {
        unsafe
        { SortNonGenericImpl(keys, comparer, itemsArrays, null); }
    }

    /// <summary>
    /// According to an array of keys produced using the specified <paramref name="selector"/> function, sorts an arbitrary number of arrays using the default comparer for the keys in ascending order.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="selector">The function to produce the keys to sort by.</param>
    /// <param name="itemsArray">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort(Array keys, Func<object, object> selector, params Array[] itemsArray) => Sort(keys, selector, null, itemsArray);
    /// <summary>
    /// According to an array of keys produced using the specified <paramref name="selector"/> function, sorts an arbitrary number of arrays using the specified <paramref name="comparer"/> in ascending order.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="selector">The function to produce the keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void Sort(Array keys, Func<object, object> selector, IComparer comparer, params Array[] itemsArrays)
    {
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(itemsArrays);
        var interimKeys = new object[keys.Length];
        for (var i = 0; i < interimKeys.Length; i++)
            interimKeys[i] = selector(keys.GetValue(i));
        Sort(interimKeys, comparer, [keys, .. itemsArrays]);
    }

    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays with unspecified types using the default comparer.
    /// Note that this method is significantly slower than the generic version, but does allow for sorting arbitrarily typed arrays.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortDescending(Array keys, params Array[] itemsArrays) => SortDescending(keys, (IComparer)null, itemsArrays);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays with unspecified types using the specified <paramref name="comparer"/>.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortDescending(Array keys, IComparer comparer, params Array[] itemsArrays)
    {
        unsafe
        { SortNonGenericImpl(keys, comparer, itemsArrays, &Reverse); }
    }

    /// <summary>
    /// According to an array of keys produced using the specified <paramref name="selector"/> function, sorts an arbitrary number of arrays using the default comparer for the key type in descending order.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="selector">The function to produce the keys to sort by.</param>
    /// <param name="itemsArray">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortDescending(Array keys, Func<object, object> selector, params Array[] itemsArray) => SortDescending(keys, selector, null, itemsArray);
    /// <summary>
    /// According to an array of keys produced using the specified <paramref name="selector"/> function, sorts an arbitrary number of arrays using the specified <paramref name="comparer"/> in descending order.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="selector">The function to produce the keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void SortDescending(Array keys, Func<object, object> selector, IComparer comparer, params Array[] itemsArrays)
    {
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(itemsArrays);
        var interimKeys = new object[keys.Length];
        for (var i = 0; i < interimKeys.Length; i++)
            interimKeys[i] = selector(keys.GetValue(i));
        SortDescending(interimKeys, comparer, [keys, .. itemsArrays]);
    }
}
