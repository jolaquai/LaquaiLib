namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for <see cref="Array"/> Types.
/// </summary>
public static class ArrayExtensions
{
    #region Linq proxy methods
    // Basically, it sucks massive d in my eyes that only something like OfType<T>() is available for multi-dimensional Arrays when Where<T>() or Select<TSource, TResult>() are just as easily possible because OfType<T>() returns an IEnumerable<T>

    /// <summary>
    /// Uses the default order to transform the <see cref="Array"/> of <typeparamref name="T"/> to an <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>. This allows using Linq methods on multi-dimensional <see cref="Array"/>s.
    /// </summary>
    /// <typeparam name="T">The type of the items in the array.</typeparam>
    /// <param name="source">The <see cref="Array"/> to transform.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the transformed elements from the input sequence.</returns>
    public static IEnumerable<T> AsEnumerable<T>(this Array source) => source.OfType<T>();
    #endregion
}