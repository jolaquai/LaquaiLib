namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> Type.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <summary>
    /// Determines if a sequence contains less than the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="n">The number of elements to check for.</param>
    /// <returns><see langword="true"/> if the input sequence contains less than <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
    public static bool HasLessThan<T>(this IEnumerable<T> source, int n) => !source.Skip(n).Any();
    /// <summary>
    /// Determines if a sequence contains less than or exactly the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="n">The number of elements to check for.</param>
    /// <returns><see langword="true"/> if the input sequence contains less than or exactly <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
    public static bool HasLessThanOr<T>(this IEnumerable<T> source, int n) => source.HasLessThan(n + 1);
    /// <summary>
    /// Determines if a sequence contains exactly the specified number of elements.
    /// An attempt is made to avoid enumerating the input sequence, but if this fails, it is done regardless.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="n">The number of elements to check for.</param>
    /// <returns>The result of the check.</returns>
    public static bool HasExactly<T>(this IEnumerable<T> source, int n) => source.TryGetNonEnumeratedCount(out var count) ? count == n : source.Count() == n;
    /// <summary>
    /// Determines if a sequence contains more than or exactly the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="n">The number of elements to check for.</param>
    /// <returns><see langword="true"/> if the input sequence contains more than or exactly <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
    public static bool HasMoreThanOr<T>(this IEnumerable<T> source, int n) => !source.HasLessThan(n - 1);
    /// <summary>
    /// Determines if a sequence contains more than the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="n">The number of elements to check for.</param>
    /// <returns><see langword="true"/> if the input sequence contains more than <paramref name="n"/> elements, otherwise <see langword="false"/>.</returns>
    public static bool HasMoreThan<T>(this IEnumerable<T> source, int n) => !source.HasLessThan(n);
}
