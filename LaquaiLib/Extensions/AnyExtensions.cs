namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for all Types.
/// </summary>
public static class AnyExtensions
{
    /// <summary>
    /// Checks whether a number of objects are all equal to each other. If any of the passed objects are <c>null</c>, all others must also be <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The Type of the objects to compare.</typeparam>
    /// <param name="source">The first object to use for the comparison.</param>
    /// <param name="other">The remaining objects to use for the comparison.</param>
    /// <returns><c>true</c> if all passed objects are equal, otherwise <c>false</c>.</returns>
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
    /// Invokes a <paramref name="transform"/> function on a <paramref name="source"/> and any <paramref name="other"/> objects and checks whether the results are all equal to each other. If any of the passed objects are <c>null</c>, all others must also be <c>null</c>. In this case, <paramref name="transform"/> is never invoked.
    /// </summary>
    /// <typeparam name="T">The Type of the input objects.</typeparam>
    /// <typeparam name="TCompare">The Type of the results <paramref name="transform"/> yields.</typeparam>
    /// <param name="source">The first object to use for the comparison..</param>
    /// <param name="transform">The transform function to invoke on each object before performing the comparison.</param>
    /// <param name="other">The remaining objects to use for the comparison..</param>
    /// <returns><c>true</c> if all the results produced by <paramref name="transform"/> are all equal, otherwise <c>false</c>.</returns>
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
}

