namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="List{T}"/> Type.
/// </summary>
public static class ListExtensions
{
    extension<T>(IList<T> list)
    {
        /// <summary>
        /// Removes the element at the specified <paramref name="index"/> from this <see cref="List{T}"/>.
        /// </summary>
        /// <param name="index">An <see cref="Index"/> instance that indicates where the item to be removed is located in the <see cref="IList{T}"/>.</param>
        public void RemoveAt(Index index) => list.RemoveAt(index.GetOffset(list.Count));
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern T[] _items<T>(this List<T> list);

    extension<T>(List<T> list)
    {
        /// <summary>
        /// Removes elements in a specified <paramref name="range"/> from this <see cref="List{T}"/>.
        /// </summary>
        /// <param name="range">A <see cref="Range"/> instance that indicates where the items to be removed are located in the <see cref="List{T}"/>.</param>
        public void RemoveRange(Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(list.Count);
            list.RemoveRange(offset, length);
        }

        /// <summary>
        /// Removes all elements from this <see cref="List{T}"/> that do not match the conditions defined by the specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements to keep.</param>
        public void KeepOnly(Func<T, bool> predicate) => list.RemoveAll(item => !predicate(item));

        /// <summary>
        /// Retrieves a <see cref="Memory{T}"/> over a portion of the backing array of the specified <see cref="List{T}"/>.
        /// </summary>
        /// <param name="start">The starting index of the <see cref="Memory{T}"/> to be retrieved.</param>
        /// <param name="length">The length of the <see cref="Memory{T}"/> to be retrieved.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing array of the specified <see cref="List{T}"/>.</returns>
        /// <remarks>
        /// When the <see cref="List{T}"/> undergoes a resize through any means, the <see cref="Memory{T}"/> returned by this method becomes invalid, just like with <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// </remarks>
        public Memory<T> AsMemory(Index start = default, int length = -1)
        {
            Memory<T> memory = list._items();
            var offset = start.GetOffset(memory.Length);
            return length == -1 ? memory[offset..] : memory[offset..(offset + length)];
        }
        /// <summary>
        /// Retrieves a <see cref="Memory{T}"/> over a portion of the backing array of the specified <see cref="List{T}"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> that indicates the portion of the backing array to be retrieved.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing array of the specified <see cref="List{T}"/>.</returns>
        /// <remarks>
        /// When the <see cref="List{T}"/> undergoes a resize through any means, the <see cref="Memory{T}"/> returned by this method becomes invalid, just like with <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// </remarks>
        public Memory<T> AsMemory(Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(list.Count);
            return list.AsMemory()[offset..(offset + length)];
        }
        /// <summary>
        /// Retrieves a <see cref="Span{T}"/> over a portion of the backing array of the specified <see cref="List{T}"/>.
        /// </summary>
        /// <param name="list">The <see cref="List{T}"/> to retrieve the backing array from.</param>
        /// <param name="start">The starting index of the <see cref="Span{T}"/> to be retrieved.</param>
        /// <param name="length">The length of the <see cref="Span{T}"/> to be retrieved.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing array of the specified <see cref="List{T}"/>.</returns>
        /// <remarks>
        /// When the <see cref="List{T}"/> undergoes a resize through any means, the <see cref="Memory{T}"/> returned by this method becomes invalid, just like with <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// </remarks>
        public Span<T> AsSpan(Index start = default, int length = -1)
        {
            var span = CollectionsMarshal.AsSpan(list);
            var offset = start.GetOffset(span.Length);
            return length == -1 ? span[offset..] : span[offset..(offset + length)];
        }
        /// <summary>
        /// Retrieves a <see cref="Span{T}"/> over a portion of the backing array of the specified <see cref="List{T}"/>.
        /// </summary>
        /// <param name="list">The <see cref="List{T}"/> to retrieve the backing array from.</param>
        /// <param name="range">The <see cref="Range"/> that indicates the portion of the backing array to be retrieved.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing array of the specified <see cref="List{T}"/>.</returns>
        /// <remarks>
        /// When the <see cref="List{T}"/> undergoes a resize through any means, the <see cref="Memory{T}"/> returned by this method becomes invalid, just like with <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// </remarks>
        public Span<T> AsSpan(Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(list.Count);
            return list.AsSpan()[offset..(offset + length)];
        }

        /// <summary>
        /// Sets the <see cref="List{T}.Count"/> of the specified <see cref="List{T}"/> to the specified <paramref name="count"/>.
        /// This is done through <see cref="CollectionsMarshal.SetCount{T}(List{T}, int)"/> and should be used as cautiously as that method.
        /// </summary>
        /// <param name="count">The new <see cref="List{T}.Count"/> of the specified <see cref="List{T}"/>.</param>
        public void SetCount(int count) => CollectionsMarshal.SetCount(list, count);
        /// <summary>
        /// Increases the capacity of the <see cref="List{T}"/> so it can hold at least <paramref name="count"/> elements in addition to its current <see cref="List{T}.Count"/>.
        /// </summary>
        /// <param name="count">The number of elements to reserve additional space for.</param>
        /// <param name="startAt">The index to consider the start of empty space in the <see cref="List{T}"/>. Defaults to its current <see cref="List{T}.Count"/>.</param>
        /// <returns>A <see cref="Span{T}"/> over the requested space in <see cref="List{T}"/>.</returns>
        public Span<T> ExpandBy(int count, int startAt = -1)
        {
            if (count == 0)
            {
                return default;
            }
            if (startAt == -1)
            {
                startAt = list.Count;
            }
            if (startAt + count > list.Count)
            {
                CollectionsMarshal.SetCount(list, startAt + count);
            }
            return list.AsSpan(startAt, count);
        }
    }
}
