namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerator{T}"/> Type.
/// </summary>
public static class IEnumeratorExtensions
{
    /// <summary>
    /// Consumes the specified <see cref="IEnumerator{T}"/> starting at its current position, yielding each element.
    /// </summary>
    /// <param name="source">The <see cref="IEnumerator{T}"/> to iterate over.</param>
    /// <returns>The elements of the <paramref name="source"/> as an <see cref="IEnumerable{T}"/>.</returns>
    public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> source)
    {
        while (source.MoveNext())
        {
            yield return source.Current;
        }
    }
}
