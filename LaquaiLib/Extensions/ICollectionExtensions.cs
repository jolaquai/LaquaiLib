namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="ICollection{T}"/> Type.
/// </summary>
public static class ICollectionExtensions
{
    /// <summary>
    /// Replaces the contents of this <see cref="ICollection{T}"/> with only the elements that match the given <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="ICollection{T}"/>.</typeparam>
    /// <param name="collection">The <see cref="ICollection{T}"/> to be modified.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements to keep.</param>
    public static void KeepOnly<T>(this ICollection<T> collection, Predicate<T> predicate)
    {
        if (collection.IsReadOnly)
        {
            throw new NotSupportedException("The collection is read-only.");
        }
        if (collection.Count == 0)
        {
            return;
        }

        var temp = collection.Where(element => predicate(element)).ToArray();
        collection.Clear();
        foreach (var element in temp)
        {
            collection.Add(element);
        }
    }
    /// <summary>
    /// Attempts to create a <see cref="Span{T}"/> over the backing store of this <see cref="ICollection{T}"/>. This may fail if the backing store cannot be retrieved or is not an array.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the <see cref="ICollection{T}"/>.</typeparam>
    /// <param name="collection">The <see cref="ICollection{T}"/> to create a <see cref="Span{T}"/> over.</param>
    /// <param name="start">The index at which to start the <see cref="Span{T}"/>.</param>
    /// <param name="length">The length of the <see cref="Span{T}"/>. If <see langword="null"/>, the <see cref="Span{T}"/> will extend to the end of the <paramref name="collection"/>.</param>
    /// <returns>A <see cref="Span{T}"/> over the backing store of the <paramref name="collection"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="start"/> or <paramref name="length"/> is out of range.</exception>
    /// <exception cref="ArgumentException"></exception>
    public static Span<T> SpanOver<T>(this ICollection<T> collection, int start = 0, int length = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (collection is T[] arr)
        {
            return arr.AsSpan(start, length);
        }
        if (collection is List<T> list && list.TryGetBackingStore(out var backingStore, out _, out var arrLength))
        {
            if (start + length > arrLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The length is out of range with respect to the backing store.");
            }

            return backingStore.AsSpan(start, length);
        }

        throw new ArgumentException($"The type of the specified {nameof(collection)} is not supported (if you know the backing store of the type should be retrievable, please open an issue on GitHub).", nameof(collection));
    }
}
