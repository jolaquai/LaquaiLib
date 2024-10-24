using System.Runtime.InteropServices;

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
    /// Attempts to retrieve a <see langword="ref"/> to the element at the specified index in the <paramref name="collection"/>.
    /// An <see cref="InvalidOperationException"/> is thrown if unsuccessful.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the <see cref="ICollection{T}"/>.</typeparam>
    /// <param name="collection">The <see cref="ICollection{T}"/> to retrieve the element from.</param>
    /// <param name="index">The index of the element to retrieve.</param>
    /// <returns>A <see langword="ref"/> to the element at the specified index in the <paramref name="collection"/>.</returns>
    public static ref T RefAt<T>(this ICollection<T> collection, int index)
    {
        if (collection is T[] arr)
        {
            return ref arr[index];
        }
        if (collection is List<T> list)
        {
            var span = CollectionsMarshal.AsSpan(list);
            return ref span[index];
        }

        throw new InvalidOperationException($"The type of the specified {nameof(collection)} is not supported.");
    }
}
