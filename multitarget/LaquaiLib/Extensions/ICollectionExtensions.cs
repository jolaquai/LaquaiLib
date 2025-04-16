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
    public static void KeepOnly<T>(this ICollection<T> collection, Func<T, bool> predicate)
    {
        if (collection.IsReadOnly)
        {
            throw new NotSupportedException("The collection is read-only.");
        }
        if (collection.Count == 0)
        {
            return;
        }

        switch (collection)
        {
            case HashSet<T> hashSet:
            {
                _ = hashSet.RemoveWhere(item => !predicate(item));
                return;
            }
            case ISet<T> set:
            {
                var items = set.ToArray();
                for (var i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    if (!predicate(item))
                    {
                        set.Remove(item);
                    }
                }
                return;
            }
            case List<T> list:
            {
                ListExtensions.KeepOnly(list, predicate);
                return;
            }
            case IList<T> ilist:
            {
                for (var i = ilist.Count - 1; i >= 0; i--)
                {
                    if (!predicate(ilist[i]))
                    {
                        ilist.RemoveAt(i);
                    }
                }
                return;
            }
            default:
            {
                var array = collection.ToArray();
                for (var i = 0; i < array.Length; i++)
                {
                    var item = array[i];
                    if (!predicate(item))
                    {
                        collection.Remove(item);
                    }
                }
                return;
            }
        }
    }
}
