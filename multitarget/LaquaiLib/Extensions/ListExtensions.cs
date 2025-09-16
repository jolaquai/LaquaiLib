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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(Index index) => list.RemoveAt(index.GetOffset(list.Count));
    }

    private static class Accessors<T>
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T[] _items(List<T> list);
    }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void KeepOnly(Func<T, bool> predicate) => list.RemoveAll(item => !predicate(item));

        /// <summary>
        /// Retrieves a <see cref="Memory{T}"/> over a portion of the backing array of the specified <see cref="List{T}"/>. By default, only the portion considered valid (as indicated through <see cref="List{T}.Count"/>) is returned.
        /// </summary>
        /// <param name="start">The starting index of the <see cref="Memory{T}"/> to be retrieved. Must resolve to a position within the valid portion of the <see cref="List{T}"/> (as indicated through <see cref="List{T}.Count"/>).</param>
        /// <param name="length">The length of the <see cref="Memory{T}"/> to be retrieved. Together with <paramref name="start"/>, this must resolve to a range that lies entirely within the valid portion of the <see cref="List{T}"/> (as indicated through <see cref="List{T}.Count"/>). Defaults to -1, which is equivalent to retrieving all elements from <paramref name="start"/> to the end of the valid portion of the <see cref="List{T}"/>.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing array of the specified <see cref="List{T}"/>.</returns>
        /// <remarks>
        /// When the <see cref="List{T}"/> undergoes a resize through any means, the <see cref="Memory{T}"/> returned by this method becomes invalid, just like with <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// </remarks>
        public Memory<T> AsMemory(Index start = default, int length = -1)
        {
            if (length == 0)
            {
                return Memory<T>.Empty;
            }

            var offset = start.GetOffset(list.Count);
            if (offset < 0 || offset > list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "The specified start index is out of range.");
            }
            if (length < -1 || length > list.Count - offset)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The specified length is out of range.");
            }

            Memory<T> memory = Accessors<T>._items(list);
            var endIndex = length == -1 ? list.Count : offset + length;
            return memory[offset..endIndex];
        }
        /// <summary>
        /// Retrieves a <see cref="Memory{T}"/> over a portion of the backing array of the specified <see cref="List{T}"/>. <paramref name="range"/> must lie entirely within the valid portion of the <see cref="List{T}"/> (as indicated through <see cref="List{T}.Count"/>).
        /// </summary>
        /// <param name="range">The <see cref="Range"/> that indicates the portion of the backing array to be retrieved.</param>
        /// <returns>A <see cref="Memory{T}"/> over the backing array of the specified <see cref="List{T}"/>.</returns>
        /// <remarks>
        /// When the <see cref="List{T}"/> undergoes a resize through any means, the <see cref="Memory{T}"/> returned by this method becomes invalid, just like with <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// </remarks>
        public Memory<T> AsMemory(Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(list.Count);
            return AsMemory(list, offset, length);
        }
        /// <summary>
        /// Retrieves a <see cref="Span{T}"/> over a portion of the backing array of the specified <see cref="List{T}"/>. By default, only the portion considered valid (as indicated through <see cref="List{T}.Count"/>) is returned.
        /// </summary>
        /// <param name="start">The starting index of the <see cref="Span{T}"/> to be retrieved.</param>
        /// <param name="length">The length of the <see cref="Span{T}"/> to be retrieved.</param>
        /// <returns>A <see cref="Span{T}"/> over the backing array of the specified <see cref="List{T}"/>.</returns>
        /// <remarks>
        /// When the <see cref="List{T}"/> undergoes a resize through any means, the <see cref="Memory{T}"/> returned by this method becomes invalid, just like with <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/>.
        /// </remarks>
        public Span<T> AsSpan(Index start = default, int length = -1)
        {
            if (length == 0)
            {
                return [];
            }

            var offset = start.GetOffset(list.Count);
            if (offset < 0 || offset > list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "The specified start index is out of range.");
            }
            if (length < -1 || length > list.Count - offset)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The specified length is out of range.");
            }

            Span<T> span = Accessors<T>._items(list);
            var endIndex = length == -1 ? list.Count : offset + length;
            return span[offset..endIndex];
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
        /// <returns>A <see cref="Span{T}"/> over the valid portion of the specified <see cref="List{T}"/> after setting its <see cref="List{T}.Count"/> to <paramref name="count"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> SetCount(int count)
        {
            CollectionsMarshal.SetCount(list, count);
            return AsMemory(list);
        }
    }
}
