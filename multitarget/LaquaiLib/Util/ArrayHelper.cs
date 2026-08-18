using System.Diagnostics;
using System.Buffers;

namespace LaquaiLib.Util;

/// <summary>
/// Contains helper methods for arrays and spans.
/// </summary>
public static class ArrayHelper
{
    internal class NonGenericComparer : IComparer
    {
        private Func<object, object, int> _compare;
        public int Compare(object x, object y) => _compare(x, y);
        internal static NonGenericComparer Create<T>(IComparer<T> comparer) => new NonGenericComparer
        {
            _compare = (x, y) => comparer.Compare((T)x, (T)y)
        };
    }

    private static bool Reverse(int[] indices, Array keys)
    {
        Array.Reverse(indices);
        Array.Reverse(keys);

        if (indices.AsSpan().SequenceEqual(Enumerable.Range(0, indices.Length).ToArray()))
            return false;
        return true;
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
            // Copy the current array to the temp array
            Array.Copy(itemsArrays[i], temp, keysLength);
            // Reassign each index using the sorted indices
            for (var j = 0; j < keysLength; j++)
                itemsArrays[i][j] = temp[indices[j]];
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
            // Copy the current array to the temp array
            Array.Copy(itemsArrays[i], temp, keysLength);
            // Reassign each index using the sorted indices
            for (var j = 0; j < keysLength; j++)
                Unsafe.As<Array>(itemsArrays.GetValue(i)).SetValue(temp[indices[j]], j);
        }
    }
    // null = nothing to sort (itemsArrays empty), false = sort was a no-op, true = permutation changed
    private static bool? ValidateAndGetKeys<TKey>(Array keys, IComparer comparer, IComparer<TKey> genericComparer, Array[] itemsArrays, ref int keysLength, ref int[] indices)
    {
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(itemsArrays);
        if (itemsArrays.Length == 0)
            return null; // Nothing to sort

        var keysLengthLocal = keysLength = keys.Length;
        if (itemsArrays.Any(a => a.Length != keysLengthLocal))
            throw new ArgumentException("The length of the keys array must be equal to the length of all items arrays.");

        indices = [.. Enumerable.Range(0, keysLength)];
        var originalIndices = GC.AllocateUninitializedArray<int>(keysLength);
        indices.CopyTo(originalIndices);

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

        // Force this to use the span implementation since its significantly faster than LINQ
        return !indices.AsSpan().SequenceEqual(originalIndices);
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
        Sort(interimKeys, NonGenericComparer.Create(comparer ?? Comparer<TCompare>.Default), [items, .. itemsArrays]);
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
        SortDescending(interimKeys, NonGenericComparer.Create(comparer ?? Comparer<TCompare>.Default), [items, .. itemsArrays]);
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
    /// According to an array of keys produced using the specified <paramref name="selector"/> function, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the default comparer for <typeparamref name="TCompare"/> in ascending order.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="selector">The function to produce the keys to sort by.</param>
    /// <param name="itemsArray">The arrays of items to sort.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort(Array keys, Func<object, object> selector, params Array[] itemsArray) => Sort(keys, selector, null, itemsArray);
    /// <summary>
    /// According to an array of keys produced using the specified <paramref name="selector"/> function, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the specified <paramref name="comparer"/> in ascending order.
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
