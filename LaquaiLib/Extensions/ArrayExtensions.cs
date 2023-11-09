using System.Linq;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for <see cref="Array"/> Types.
/// </summary>
public static class ArrayExtensions
{
    #region Linq proxying
    /// <summary>
    /// Uses the default order to transform the <see cref="Array"/> of <typeparamref name="T"/> to an <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>. This allows using Linq methods on multi-dimensional <see cref="Array"/>s.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="source">The <see cref="Array"/> to transform.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the transformed elements from the input sequence.</returns>
    public static IEnumerable<T> AsEnumerable<T>(this Array source) => source.OfType<T>();
    #endregion

    #region Efficient Linq proxying using static Array methods instead of Enumerable methods
    /// <summary>
    /// Determines if any element of the <see cref="Array"/> matches the given <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">They Type of the items in the array.</typeparam>
    /// <param name="source">The <see cref="Array"/> to check.</param>
    /// <param name="predicate">A <see cref="Predicate{T}"/> that checks each element for a condition.</param>
    /// <returns><see langword="true"/> if any element matches the given <paramref name="predicate"/>, otherwise <see langword="false"/>.</returns>
    public static bool ArrayAny<T>(this T[] source, Predicate<T> predicate) => Array.Exists(source, x => predicate(x));
    /// <summary>
    /// Determines if all elements of the <see cref="Array"/> match the given <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="source">The <see cref="Array"/> to check.</param>
    /// <param name="predicate">A <see cref="Predicate{T}"/> that checks each element for a condition.</param>
    /// <returns><see langword="true"/> if all elements match the given <paramref name="predicate"/>, otherwise <see langword="false"/>.</returns>
    public static bool ArrayAll<T>(this T[] source, Predicate<T> predicate) => Array.TrueForAll(source, x => predicate(x));
    /// <summary>
    /// Filters the elements of the <see cref="Array"/> based on a <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="source">The <see cref="Array"/> to filter.</param>
    /// <param name="predicate">A <see cref="Predicate{T}"/> that checks each element for a condition.</param>
    /// <returns>A new <see cref="Array"/> of <typeparamref name="T"/> that contains all elements of the input sequence that satisfy the condition.</returns>
    public static T[] ArrayWhere<T>(this T[] source, Predicate<T> predicate) => Array.FindAll(source, x => predicate(x));
    /// <summary>
    /// Projects each element of the <see cref="Array"/> into a new form.
    /// </summary>
    /// <typeparam name="TSource">The Type of the items in the array.</typeparam>
    /// <typeparam name="TResult">The Type of the value returned by <paramref name="selector"/>.</typeparam>
    /// <param name="source">The <see cref="Array"/> to transform.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that transforms each element from <typeparamref name="TSource"/> to <typeparamref name="TResult"/>.</param>
    /// <returns>A new <see cref="Array"/> of <typeparamref name="TResult"/> that contains the transformed elements from the input sequence.</returns>
    public static TResult[] ArraySelect<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector) => Array.ConvertAll(source, x => selector(x));
    #endregion
}
