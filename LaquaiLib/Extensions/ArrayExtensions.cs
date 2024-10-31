namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for <see cref="Array"/> Types.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Reinterprets the reference to <paramref name="source"/> as <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>.
    /// This allows using Linq methods on multi-dimensional <see cref="Array"/>s.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="source">The <see cref="Array"/> to transform.</param>
    /// <returns>The reinterpreted reference to <paramref name="source"/>.</returns>
    public static IEnumerable<T> CastEnumerable<T>(this Array source) => System.Runtime.CompilerServices.Unsafe.As<IEnumerable<T>>(source);

    /// <summary>
    /// Splits the specified <paramref name="array"/> into two new <see cref="Array"/>s based on the given <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> to split.</param>
    /// <param name="true">The <see cref="Array"/> that will contain all elements that match the given <paramref name="predicate"/>.</param>
    /// <param name="false">The <see cref="Array"/> that will contain all elements that do not match the given <paramref name="predicate"/>.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that checks each element for a condition.</param>
    /// <remarks>
    /// <paramref name="true"/> and <paramref name="false"/>'s lengths are not checked against <paramref name="array"/>'s length. If they are too small, an <see cref="IndexOutOfRangeException"/> will be thrown by the runtime.
    /// </remarks>
    public static void Split<T>(T[] array, T[] @true, T[] @false, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentNullException.ThrowIfNull(@true);
        ArgumentNullException.ThrowIfNull(@false);
        ArgumentNullException.ThrowIfNull(predicate);

        var trueIndex = 0;
        var falseIndex = 0;
        for (var i = 0; i < array.Length; i++)
        {
            if (predicate(array[i]))
            {
                @true[trueIndex] = array[i];
                trueIndex++;
            }
            else
            {
                @false[falseIndex] = array[i];
                falseIndex++;
            }
        }
    }

    /// <summary>
    /// Attempts to retrieve the element at the specified index from the array if that index is valid for the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The array to retrieve the element from.</param>
    /// <param name="i">The index of the element to retrieve.</param>
    /// <param name="value">An <see langword="out"/> variable that receives the element at the specified index if it is valid.</param>
    /// <returns><see langword="true"/> if the index was valid and the element could be retrieved, otherwise <see langword="false"/>.</returns>
    public static bool TryGetAt<T>(this T[] array, int i, out T value)
    {
        if (i < array.Length && i >= 0)
        {
            value = array[i];
            return true;
        }
        value = default;
        return false;
    }
    /// <summary>
    /// Attempts to retrieve the element at specified index from the array if that index is valid for the array, otherwise returns the specified default value.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The array to retrieve the element from.</param>
    /// <param name="i">The index of the element to retrieve.</param>
    /// <param name="defaultValue">The default value to return if the index is invalid.</param>
    /// <returns>The element at the specified index if it is valid, otherwise the specified default value.</returns>
    public static T GetAtOrDefault<T>(this T[] array, int i, T defaultValue = default)
    {
        if (array.GetLowerBound(0) <= i && i <= array.GetUpperBound(0))
        {
            return array[i];
        }
        return defaultValue;
    }
}
