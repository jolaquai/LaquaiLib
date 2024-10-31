using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for all Types.
/// </summary>
public static class AnyExtensions
{
    /// <summary>
    /// Checks whether a number of objects are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>.
    /// Comparing to an empty collection is considered equal.
    /// </summary>
    /// <typeparam name="T">The Type of the objects to compare.</typeparam>
    /// <param name="source">The first object to use for the comparison.</param>
    /// <param name="other">The remaining objects to use for the comparison.</param>
    /// <returns><see langword="true"/> if all passed objects are equal, otherwise <see langword="false"/>.</returns>
    public static bool AllEqual<T>(this T source, params ReadOnlySpan<T> other)
    {
        if (other.Length == 0) return true;

        for (var i = 0; i < other.Length; i++)
        {
            var elem = other[i];
            if (source is null && elem is not null) return false;
            if (!source.Equals(elem)) return false;
        }
        return true;
    }
    /// <summary>
    /// Checks whether a number of objects are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>.
    /// Comparing to an empty collection is considered equal.
    /// </summary>
    /// <typeparam name="T">The Type of the objects to compare.</typeparam>
    /// <param name="source">The first object to use for the comparison.</param>
    /// <param name="enumerable">The objects to use for the comparison. Enumeration will cease if an object is encountered that is not equal to <paramref name="source"/>.</param>
    /// <returns><see langword="true"/> if all passed objects are equal, otherwise <see langword="false"/>.</returns>
    public static bool AllEqual<T>(this T source, IEnumerable<T> enumerable)
    {
        // Carry the extra bool around to check for an empty enumerable
        var compared = false;
        foreach (var elem in enumerable)
        {
            compared = true;
            if (source is null && elem is not null) return false;
            else if (!source.Equals(elem)) return false;
        }
        return compared;
    }

    /// <summary>
    /// Invokes a <paramref name="transform"/> function on a <paramref name="source"/> and any <paramref name="other"/> objects and checks whether the results are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>. In this case, <paramref name="transform"/> is never invoked.
    /// </summary>
    /// <typeparam name="T">The Type of the input objects.</typeparam>
    /// <typeparam name="TCompare">The Type of the results <paramref name="transform"/> yields.</typeparam>
    /// <param name="source">The first object to use for the comparison.</param>
    /// <param name="transform">The transform function to invoke on each object before performing the comparison.</param>
    /// <param name="other">The remaining objects to use for the comparison.</param>
    /// <returns><see langword="true"/> if all the results produced by <paramref name="transform"/> are all equal, otherwise <see langword="false"/>.</returns>
    public static bool EqualBy<T, TCompare>(this T source, Func<T, TCompare> transform, params ReadOnlySpan<T> other)
    {
        if (other.Length == 0) return true;

        var sourceTransformed = transform(source);
        for (var i = 0; i < other.Length; i++)
        {
            var elem = other[i];
            if (sourceTransformed is null && elem is not null) return false;
            if (!sourceTransformed.Equals(elem)) return false;
        }
        return true;
    }
    /// <summary>
    /// Invokes a <paramref name="transform"/> function on a <paramref name="source"/> and any other objects and checks whether the results are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the input objects.</typeparam>
    /// <typeparam name="TCompare">The Type of the results <paramref name="transform"/> yields.</typeparam>
    /// <param name="source">The first object to use for the comparison.</param>
    /// <param name="transform">The transform function to invoke on each object before performing the comparison.</param>
    /// <param name="enumerable">The remaining objects to use for the comparison.</param>
    /// <returns><see langword="true"/> if all the results produced by <paramref name="transform"/> are all equal, otherwise <see langword="false"/>.</returns>
    public static bool EqualBy<T, TCompare>(this T source, Func<T, TCompare> transform, IEnumerable<T> enumerable)
    {
        // Carry the extra bool around to check for an empty enumerable
        var compared = false;
        var sourceTransformed = transform(source);
        foreach (var elem in enumerable)
        {
            compared = true;
            var elemTransformed = transform(elem);
            if (sourceTransformed is null && elemTransformed is not null) return false;
            else if (!sourceTransformed.Equals(elemTransformed)) return false;
        }
        return compared;
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
    public static async Task<T> With<T>(this T source, Func<T, Task> action)
    {
        await action(source).ConfigureAwait(false);
        return source;
    }

    /// <summary>
    /// Casts an instance of <typeparamref name="TFrom"/> to <typeparamref name="TFrom"/>.
    /// </summary>
    /// <typeparam name="TFrom">The type to cast <paramref name="obj"/> to.</typeparam>
    /// <typeparam name="TTo">The type of <paramref name="obj"/>.</typeparam>
    /// <param name="obj">The <typeparamref name="TFrom"/> instance to cast.</param>
    /// <returns>An instance of <typeparamref name="TFrom"/> that has been produced by casting <paramref name="obj"/>.</returns>
    public static TTo Cast<TFrom, TTo>(this TFrom obj)
        where TFrom : allows ref struct
        where TTo : allows ref struct
    {
        // Use As for reference types, additionally checking for assignment compatibility
        if (default(TFrom) is null || default(TTo) is null)
        {
            if (typeof(TTo).IsAssignableFrom(typeof(TFrom)))
            {
                return System.Runtime.CompilerServices.Unsafe.As<TFrom, TTo>(ref obj);
            }
            else
            {
                throw new InvalidCastException($"Cannot cast {typeof(TFrom)} to {typeof(TTo)}.");
            }
        }
        unsafe
        {
            if (sizeof(TFrom) == sizeof(TTo))
            {
                // If the sizes are equal, just reinterpret the bits
                // As far as casting unrelated struct types between each other, this is the same as using As
                return System.Runtime.CompilerServices.Unsafe.BitCast<TFrom, TTo>(obj);
            }
        }
        // We can't use As here, otherwise the "cast" will always succeed
        throw new InvalidCastException($"Cannot cast {typeof(TFrom)} to {typeof(TTo)}.");
    }
    /// <summary>
    /// Changes the type of an instance of <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>, even for reference types.
    /// </summary>
    /// <typeparam name="TFrom">The type of <paramref name="obj"/>.</typeparam>
    /// <typeparam name="TTo">The type to cast <paramref name="obj"/> to.</typeparam>
    /// <param name="obj">The <see cref="object"/> to cast.</param>
    /// <returns><paramref name="obj"/> reinterpreted as an instance of <typeparamref name="TTo"/> or the <see langword="default"/> value of <typeparamref name="TTo"/> if the cast failed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTo As<TFrom, TTo>(this TFrom obj)
        where TFrom : allows ref struct
        where TTo : allows ref struct
    {
        return System.Runtime.CompilerServices.Unsafe.As<TFrom, TTo>(ref obj);
    }
}
