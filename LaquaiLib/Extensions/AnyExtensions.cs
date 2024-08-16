using System.Diagnostics.CodeAnalysis;

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
    public static bool AllEqual<T>(this T source, params ReadOnlySpan<T> other)
    {
        var enumerated = other.ToArray();
        if (enumerated.Length == 0)
        {
            throw new ArgumentException("Cannot compare to an empty array.", nameof(other));
        }

        // Enumerate and use for to avoid the IEnumerator
        for (var i = 0; i < enumerated.Length; i++)
        {
            var elem = enumerated[i];
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
    public static bool EqualBy<T, TCompare>(this T source, Func<T, TCompare> transform, params ReadOnlySpan<T> other)
    {
        if (other.Length == 0)
        {
            throw new ArgumentException("Cannot compare to an empty array.", nameof(other));
        }
        var enumerated = other.ToArray();
        if (source is null || enumerated.Any(o => o is null))
        {
            return source is null && enumerated.All(o => o is null);
        }

        return transform(source)?.AllEqual(enumerated.Select(transform).ToArray()) is true;
    }

    /// <summary>
    /// Checks whether a given input object is <see langword="null"/>. If not, it is marked to the compiler as non-<see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the input object.</typeparam>
    /// <param name="source">The input object.</param>
    /// <returns><see langword="true"/> if <paramref name="source"/> is <see langword="null"/>, otherwise <see langword="false"/>.</returns>
    public static bool IsNull<T>([NotNullWhen(false)] this T source) => source is null;

    /// <summary>
    /// Invokes an <paramref name="action"/> that is passed the <paramref name="source"/> object.
    /// </summary>
    /// <typeparam name="T">The Type of the object to execute the <paramref name="action"/> on.</typeparam>
    /// <param name="source">The object to execute the <paramref name="action"/> on.</param>
    /// <param name="action">The action to execute on the <paramref name="source"/> object.</param>
    /// <returns>A reference to <paramref name="source"/> itself after <paramref name="action"/> has returned.</returns>
    /// <remarks>
    /// While not tremendously useful, this method can be used to effectively limit variable scopes or chain calls to the same object like when using a builder pattern.
    /// </remarks>
    public static T With<T>(this T source, Action<T> action)
    {
        action(source);
        return source;
    }
    /// <summary>
    /// Invokes an asynchronous <paramref name="action"/> that is passed the <paramref name="source"/> object.
    /// </summary>
    /// <typeparam name="T">The Type of the object to execute the <paramref name="action"/> on.</typeparam>
    /// <param name="source">The object to execute the <paramref name="action"/> on.</param>
    /// <param name="action">The action to execute on the <paramref name="source"/> object.</param>
    /// <returns>A reference to <paramref name="source"/> itself after <paramref name="action"/> has returned.</returns>
    /// <remarks>
    /// While not tremendously useful, this method can be used to effectively limit variable scopes or chain calls to the same object like when using a builder pattern.
    /// </remarks>
    public static async ValueTask<T> With<T>(this T source, Func<T, ValueTask> action)
    {
        await action(source).ConfigureAwait(false);
        return source;
    }
}
