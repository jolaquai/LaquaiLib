using System.Runtime.InteropServices;

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
    public static void Remove<T>(this IList<T> list, Index index) => list.Remove(index.GetOffset(list.Count));
    /// <summary>
    /// Removes elements in a specified <paramref name="range"/> from this <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to be modified.</param>
    /// <param name="range">A <see cref="Range"/> instance that indicates where the items to be removed are located in the <paramref name="list"/>.</param>
    public static void RemoveRange<T>(this List<T> list, Range range)
    {
        var (offset, length) = range.GetOffsetAndLength(list.Count);
        list.RemoveRange(offset, length);
    }

    /// <summary>
    /// Removes all elements from this <see cref="List{T}"/> that match the conditions defined by the specified <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to be modified.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements to keep.</param>
    public static void KeepOnly<T>(this List<T> list, Predicate<T> predicate) => list.RemoveAll(item => !predicate(item));

    /// <summary>
    /// Retrieves a <see cref="Span{T}"/> over a portion of the backing array of the specified <paramref name="list"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to retrieve the backing array from.</param>
    /// <param name="start">The starting index of the <see cref="Span{T}"/> to be retrieved.</param>
    /// <param name="length">The length of the <see cref="Span{T}"/> to be retrieved.</param>
    /// <returns>A <see cref="Span{T}"/> over the backing array of the specified <paramref name="list"/>.</returns>
    public static Span<T> AsSpan<T>(this List<T> list, Index start = default, int length = -1)
    {
        var span = CollectionsMarshal.AsSpan(list);
        var offset = start.GetOffset(span.Length);
        return length == -1 ? span[offset..] : span[offset..(offset + length)];
    }
    /// <summary>
    /// Retrieves a <see cref="Span{T}"/> over a portion of the backing array of the specified <paramref name="list"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to retrieve the backing array from.</param>
    /// <param name="range">The <see cref="Range"/> that indicates the portion of the backing array to be retrieved.</param>
    /// <returns>A <see cref="Span{T}"/> over the backing array of the specified <paramref name="list"/>.</returns>
    public static Span<T> AsSpan<T>(this List<T> list, Range range)
    {
        // Do it this way instead of using the range in Span's indexer, because that would need to actually calculate stuff, and since the Span's length and the List<T>'s Count are both known here, the compiler is probably gonna be able to optimize this better
        var (offset, length) = range.GetOffsetAndLength(list.Count);
        return list.AsSpan()[offset..(offset + length)];
    }

    // this should be used as cautiously as CollectionsMarshal.SetCount itself since it may expose garbage data if the entire Span isn't filled
    /// <summary>
    /// Increases the capacity of the <paramref name="list"/> so it can hold at least <paramref name="count"/> elements in addition to its current <see cref="List{T}.Count"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to be modified.</param>
    /// <param name="count">The number of elements to reserve additional space for.</param>
    /// <param name="startAt">The index to consider the start of empty space in the <paramref name="list"/>. Defaults to its current <see cref="List{T}.Count"/>.</param>
    /// <returns>A <see cref="Span{T}"/> over the requested space in <paramref name="list"/>.</returns>
    public static Span<T> ExpandBy<T>(this List<T> list, int count, int startAt = -1)
    {
        if (startAt == -1)
        {
            startAt = list.Count;
        }
        if (startAt + count > list.Capacity)
        {
            CollectionsMarshal.SetCount(list, startAt + count);
        }
        return list.AsSpan(startAt, count);
    }
}
