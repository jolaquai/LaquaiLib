using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LaquaiLib.Classes.Collections;

/// <summary>
/// Represents a fast implementation of a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The Type of the elements in the collection.</typeparam>
public class ObservableCollectionFast<T> : ObservableCollection<T>
{
    #region Constructors
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
    /// Instantiates a new <see cref="ObservableCollection{T}"/> that contains the specified items and has a capacity equal to the number of items.
    /// </summary>
    /// <param name="items">The items for the list to contain.</param>
    public ObservableCollectionFast(params T[] items) : base()
    {
        AddRange(items);
    }
    #endregion

    #region Indexers
    /// <summary>
    /// Gets or sets the element at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">An <see cref="Index"/> instance that identifies the location of the element to get or set.</param>
    /// <returns>The element at the specified <paramref name="index"/>.</returns>
    public T this[Index index] {
        get => base[index];
        set {
            base[index] = value;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, value, index.GetOffset(Count)));
        }
    }

    /// <summary>
    /// Gets or sets elements within the specified <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The <see cref="Range"/> in which to get or set elements.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the items that were get or set.</returns>
    public IEnumerable<T> this[Range range] {
        get {
            var (offset, length) = range.GetOffsetAndLength(Count);
            for (var i = offset; i < offset + length; i++)
            {
                yield return this[i];
            }
        }
        set {
            var (offset, length) = range.GetOffsetAndLength(Count);
            for (var i = offset; i < offset + length; i++)
            {
                this[i] = value.ElementAt(i - offset);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                action: NotifyCollectionChangedAction.Replace,
                changedItems: value.ToList(),
                startingIndex: offset));
        }
    }
    #endregion

    /// <summary>
    /// Adds the elements of the specified collection to the end of the <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added to the end of the <see cref="ObservableCollection{T}"/>.</param>
    public void AddRange(IEnumerable<T> collection)
    {
        var startingIndex = Count;
        foreach (var item in collection)
        {
            Items.Add(item);
        }
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                action: NotifyCollectionChangedAction.Add,
                changedItems: collection.ToList(),
                startingIndex: startingIndex
            )
        );
    }
}
