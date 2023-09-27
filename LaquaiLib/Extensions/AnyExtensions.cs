﻿using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for all Types.
/// </summary>
public static class AnyExtensions
{
    /// <summary>
    /// Checks whether a number of objects are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the objects to compare.</typeparam>
    /// <param name="source">The first object to use for the comparison.</param>
    /// <param name="other">The remaining objects to use for the comparison.</param>
    /// <returns><see langword="true"/> if all passed objects are equal, otherwise <see langword="false"/>.</returns>
    public static bool AllEqual<T>(this T source, params T[] other)
    {
        if (source is null || other.Any(o => o is null))
        {
            return source is null && other.All(o => o is null);
        }

        if (other.Length == 0)
        {
            throw new ArgumentException("Cannot compare to an empty array.", nameof(other));
        }

        foreach (var elem in other)
        {
            if (!source.Equals(elem))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Invokes a <paramref name="transform"/> function on a <paramref name="source"/> and any <paramref name="other"/> objects and checks whether the results are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>. In this case, <paramref name="transform"/> is never invoked.
    /// </summary>
    /// <typeparam name="T">The Type of the input objects.</typeparam>
    /// <typeparam name="TCompare">The Type of the results <paramref name="transform"/> yields.</typeparam>
    /// <param name="source">The first object to use for the comparison..</param>
    /// <param name="transform">The transform function to invoke on each object before performing the comparison.</param>
    /// <param name="other">The remaining objects to use for the comparison..</param>
    /// <returns><see langword="true"/> if all the results produced by <paramref name="transform"/> are all equal, otherwise <see langword="false"/>.</returns>
    public static bool EqualBy<T, TCompare>(this T source, Func<T, TCompare> transform, params T[] other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (other.Length == 0)
        {
            throw new ArgumentException("Cannot compare to an empty array.", nameof(other));
        }
        if (source is null || other.Any(o => o is null))
        {
            return source is null && other.All(o => o is null);
        }

        var src = transform(source);
        foreach (var elem in other.Select(transform))
        {
            if (!src!.Equals(elem))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks whether a given input object is <see langword="null"/>. If not, it is marked to the compiler as non-<see langword="null"/> for the remainder of the scope.
    /// </summary>
    /// <typeparam name="T">The Type of the input object.</typeparam>
    /// <param name="source">The input object.</param>
    /// <returns><see langword="true"/> if <paramref name="source"/> is <see langword="null"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsNull<T>([NotNullWhen(false)] this T source)
    {
        return source is null;
    }
}

