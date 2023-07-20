namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="List{T}"/> Type.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Removes the element at the specified <paramref name="index"/> from this <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to be modified.</param>
    /// <param name="index">An <see cref="Index"/> instance that indicates where the item to be removed is located in the <paramref name="list"/>.</param>
    public static void Remove<T>(this List<T> list, Index index) => list.Remove(index.GetOffset(list.Count));

    /// <summary>
    /// Removes elements in a specified <paramref name="range"/> from this <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to be modified.</param>
    /// <param name="range">A <see cref="Range"/> instance that indicates where the items to be removed are located in the <paramref name="list"/>.</param>
    public static void RemoveRange<T>(this List<T> list, Range range) => new Action<(int, int)>(tuple => list.RemoveRange(tuple.Item1, tuple.Item2))(range.GetOffsetAndLength(list.Count));

    /// <summary>
    /// Extracts a range of elements from this <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to extract elements from.</param>
    /// <param name="range">A <see cref="Range"/> instance that indicates where the items to be extracted are located in the <paramref name="list"/>.</param>
    public static IEnumerable<T> GetRange<T>(this List<T> list, Range range)
    {
        var (offset, length) = range.GetOffsetAndLength(list.Count);
        for (var i = offset; i < offset + length; i++)
        {
            yield return list[i];
        }
    }

    /// <summary>
    /// Removes all elements from this <see cref="List{T}"/> that match the conditions defined by the specified <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to be modified.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements to keep.</param>
    public static void KeepOnly<T>(this List<T> list, Predicate<T> predicate) => list.RemoveAll(item => !predicate(item));
}
