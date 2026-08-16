namespace LaquaiLib.Util;

/// <summary>
/// Contains static methods to find differences in memory regions.
/// </summary>
public static class MemoryDiff
{
    /// <summary>
    /// Unconditionally uses scalar operations to find the first differing index. <c>-1</c> if no difference is found.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DiffImpl<T>(scoped ReadOnlySpan<T> left, scoped ReadOnlySpan<T> right, IEqualityComparer<T> equalityComparer = null)
    {
        var cpl = left.CommonPrefixLength(right, equalityComparer);
        return cpl == left.Length && cpl == right.Length && cpl is not 0 ? -1 : cpl;
    }

    /// <summary>
    /// Finds the index at which the specified <see cref="Span{T}"/>s of <typeparamref name="T"/> differ (that is, where two elements at the same index do not compare equal).
    /// </summary>
    /// <typeparam name="T">The type of the elements in the spans.</typeparam>
    /// <param name="left">The first span to compare.</param>4
    /// <param name="right">The second span to compare.</param>
    /// <param name="startIndex">The index at which to start the comparison. Must be a valid index in both spans.</param>
    /// <param name="equalityComparer">An optional <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements. If <c>null</c>, the default equality comparer for <typeparamref name="T"/> is used.</param>
    /// <returns>The index of the first differing element, or <c>-1</c> if the spans are equal up to the length of the shorter span and have the same length.</returns>
    /// <remarks>
    /// If the spans do not have the same length but compare equal up to the length of the shorter span, the difference index will always be the length of the shorter span.
    /// </remarks>
    public static int FindDifference<T>(scoped ReadOnlySpan<T> left, scoped ReadOnlySpan<T> right, int startIndex = 0, IEqualityComparer<T> equalityComparer = null)
    {
        if (startIndex != 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
            if (startIndex >= left.Length || startIndex >= right.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index must refer to a valid index in both spans.");
            }
            left = left[startIndex..];
            right = right[startIndex..];
        }

        switch (left.Length)
        {
            case 0:
                if (right.Length == 0)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }

            default:
                if (right.Length == 0)
                {
                    return 0;
                }

                break;
        }

        return startIndex + DiffImpl(left, right, equalityComparer);
    }
    /// <summary>
    /// Finds all indices at which the specified <see cref="Span{T}"/>s of <typeparamref name="T"/> differ (that is, where two elements at the same index do not compare equal).
    /// </summary>
    /// <typeparam name="T">The type of the elements in the spans.</typeparam>
    /// <param name="left">The first span to compare.</param>
    /// <param name="right">The second span to compare.</param>
    /// <param name="startIndex">The index at which to start the comparison. Must be a valid index in both spans.</param>
    /// <param name="equalityComparer">An optional <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements. If <c>null</c>, the default equality comparer for <typeparamref name="T"/> is used.</param>
    /// <returns>An array of indices where the spans differ. If the spans are equal, an empty array is returned.</returns>
    /// <remarks>
    /// If the passed spans are of unequal length, all indices beyond the shorter span's length will be considered differing indices.
    /// </remarks>
    public static int[] FindDifferences<T>(scoped ReadOnlySpan<T> left, scoped ReadOnlySpan<T> right, int startIndex = 0, IEqualityComparer<T> equalityComparer = null)
    {
        var diff = FindDifference(left, right, startIndex, equalityComparer);
        if (diff == -1)
        {
            return [];
        }

        var minLength = Math.Min(left.Length, right.Length);
        var maxLength = Math.Max(left.Length, right.Length);
        var diffs = new List<int>(Math.Min(32, minLength >> 2))
        {
            diff
        };

        var pos = diff + 1;
        while (pos < minLength)
        {
            var nextDiff = DiffImpl(left[pos..], right[pos..], equalityComparer);
            if (nextDiff == -1)
            {
                break;
            }

            pos += nextDiff;
            diffs.Add(pos++);
        }

        // Add remaining indices if lengths differ
        while (pos < maxLength)
        {
            diffs.Add(pos++);
        }

        return [.. diffs];
    }
}
