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

        if (collection is List<T> list)
        {
            ListExtensions.KeepOnly(list, predicate);
            return;
        }

        var newItems = collection.Where(item => predicate(item)).ToArray();
        collection.Clear();
        for (var i = 0; i < newItems.Length; i++)
        {
            collection.Add(newItems[i]);
        }
    }
}
