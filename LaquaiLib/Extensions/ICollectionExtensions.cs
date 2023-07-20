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
        var temp = collection.Where(element => predicate(element)).ToList();
        collection.Clear();
        foreach (var element in temp)
        {
            collection.Add(element);
        }
    }
}
