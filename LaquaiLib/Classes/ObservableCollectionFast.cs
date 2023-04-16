using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LaquaiLib.Classes;

/// <summary>
/// Represents a fast implementation of a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The Type of the elements in the collection.</typeparam>
public class ObservableCollectionFast<T> : ObservableCollection<T>
{
    /// <summary>
    /// Instantiates a new <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public ObservableCollectionFast() : base()
    {
    }

    /// <summary>
    /// Instantiates a new <see cref="ObservableCollection{T}"/> that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">The collection from which the elements are copied.</param>
    public ObservableCollectionFast(IEnumerable<T> collection) : base(collection)
    {
    }

    /// <summary>
    /// Instantiates a new <see cref="ObservableCollection{T}"/> that contains elements copied from the specified list.
    /// </summary>
    /// <param name="list">The list from which the elements are copied.</param>
    public ObservableCollectionFast(List<T> list) : base(list)
    {
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added to the end of the <see cref="ObservableCollection{T}"/>.</param>
    public void AddRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Items.Add(item);
        }
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection.ToList()));
    }
}
