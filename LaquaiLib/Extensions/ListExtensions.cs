using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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
    /// Extracts a range of elements from this <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to extract elements from.</param>
    /// <param name="range">A <see cref="Range"/> instance that indicates where the items to be extracted are located in the <paramref name="list"/>.</param>
    public static IEnumerable<T> GetRange<T>(this IList<T> list, Range range)
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

    /// <summary>
    /// Attempts to retrieve the backing store of this <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to retrieve the backing store from.</param>
    /// <param name="backingStore">An <see langword="out"/> variable that receives the backing store of the <paramref name="list"/>.</param>
    /// <returns>The backing store of the <paramref name="list"/>.</returns>
    public static bool TryGetBackingStore<T>(this List<T> list, [NotNullWhen(true)] out T[]? backingStore) => TryGetBackingStore(list, out backingStore, out _, out _);
    /// <summary>
    /// Attempts to retrieve the backing store of this <see cref="List{T}"/>, also providing the <paramref name="count"/> and <paramref name="length"/> of the backing store.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to retrieve the backing store from.</param>
    /// <param name="backingStore">An <see langword="out"/> variable that receives the backing store of the <paramref name="list"/>.</param>
    /// <param name="count">An <see langword="out"/> variable that receives the number of elements from the start of <paramref name="backingStore"/> that lay within <paramref name="list"/>'s <see cref="List{T}.Count"/>.</param>
    /// <param name="length">An <see langword="out"/> variable that receives the length of the <paramref name="backingStore"/>.</param>
    /// <returns>The backing store of the <paramref name="list"/>.</returns>
    public static bool TryGetBackingStore<T>(this List<T> list, [NotNullWhen(true)] out T[]? backingStore, out int count, out int length)
    {
        var fieldInfo = typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)!;
        backingStore = fieldInfo.GetValueOrDefault<T[]>(list);
        count = list.Count;
        length = backingStore?.Length ?? 0;
        return backingStore is not null;
    }
}
