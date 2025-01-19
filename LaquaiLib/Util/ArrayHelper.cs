using System.Numerics;
using System.Security.Cryptography;

namespace LaquaiLib.Util;

/// <summary>
/// Contains helper methods for arrays.
/// </summary>
public static class ArrayHelper
{
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of items arrays using the default comparer for <typeparamref name="TKey"/>.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void Sort<TKey>(TKey[] keys, params object[][] itemsArrays) => Sort(keys, null, itemsArrays);
    /// <summary>
    /// According to an array of <paramref name="keys"/>, sorts an arbitrary number of <typeparamref name="TValue"/> arrays using the specified <paramref name="comparer"/>.
    /// </summary>
    /// <param name="keys">The array of keys to sort by.</param>
    /// <param name="comparer">The comparer to use for sorting the keys.</param>
    /// <param name="itemsArrays">The arrays of items to sort.</param>
    public static void Sort<TKey, TValue>(TKey[] keys, IComparer<TKey> comparer, params TValue[][] itemsArrays)
    {
        comparer ??= Comparer<TKey>.Default;

        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(itemsArrays);
        if (itemsArrays.Length == 0)
        {
            return; // Nothing to sort
        }

        var keysLength = keys.Length;
        if (itemsArrays.Any(a => a.Length != keysLength))
        {
            throw new ArgumentException("The length of the keys array must be equal to the length of all items arrays.");
        }

        // Enumerable.Range is vectorized
        var indices = Enumerable.Range(0, keysLength).ToArray();
        Array.Sort(keys, indices, comparer);

        // If the indices array didn't change (it's SequenceEquals to the original ascending ints), then leave early
        if (indices.SequenceEqual(Enumerable.Range(0, keysLength)))
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
}
