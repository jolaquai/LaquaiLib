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
            RaiseCollectionChanged();
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
            RaiseCollectionChanged();
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
        RaiseCollectionChanged();
    }

    /// <summary>
    /// Silently adds an element to the end of this <see cref="ObservableCollection{T}"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    /// <param name="item">The item to add to the end of this <see cref="ObservableCollection{T}"/>.</param>
    public void AddSilent(T item)
    {
        Items.Add(item);
    }

    /// <summary>
    /// Removes all occurrences of the specified items from the <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <param name="collection">A sequence of values to remove from this <see cref="ObservableCollectionFast{T}"/>.</param>
    public void RemoveRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Items.Remove(item);
        }
        RaiseCollectionChanged();
    }

    /// <summary>
    /// Silently removes an element from this <see cref="ObservableCollection{T}"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    /// <param name="item">The item to remove from this <see cref="ObservableCollection{T}"/>.</param>
    public void RemoveSilent(T item)
    {
        Items.Remove(item);
    }

    /// <summary>
    /// Silently removes all elements from this <see cref="ObservableCollection{T}"/> as dictated by a <paramref name="selector"/> <see cref="Func{T, TResult}"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that determines whether an element should be removed.</param>
    public void RemoveSilent(Func<T, bool> selector)
    {
        foreach (var item in Items.Where(selector).ToList())
        {
            Items.Remove(item);
        }
    }

    /// <summary>
    /// Silently clears this <see cref="ObservableCollection{T}"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    public void ClearSilent()
    {
        Items.Clear();
    }

    /// <summary>
    /// Resets this <see cref="ObservableCollection{T}"/> by clearing and re-filling it using the specified <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">The collection to fill this <see cref="ObservableCollection{T}"/> with.</param>
    /// <remarks>
    /// The clearing operation itself is silent, but the re-filling operation is not; that is, observers of the <see cref="ObservableCollection{T}"/> will only be notified AFTER the re-filling operation is complete.
    /// </remarks>
    public void Reset(IEnumerable<T> collection)
    {
        ClearSilent();
        AddRange(collection);
    }

    /// <summary>
    /// Removes an element from this <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <param name="item">The item to remove from this <see cref="ObservableCollection{T}"/>.</param>
    public new void Remove(T item)
    {
        Items.Remove(item);
        RaiseCollectionChanged();
    }

    /// <summary>
    /// Removes all elements from this <see cref="ObservableCollection{T}"/> as dictated by a <paramref name="selector"/> <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that determines whether an element should be removed.</param>
    public void Remove(Func<T, bool> selector)
    {
        RemoveSilent(selector);
        RaiseCollectionChanged();
    }

    /// <summary>
    /// Raises a <see cref="NotifyCollectionChangedAction.Reset"/> event. Changes made to the <see cref="ObservableCollection{T}"/> by any methods with a "Silent" suffix will not be propagated to observers until this method is called.
    /// </summary>
    public void RaiseCollectionChanged()
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Encapsulates a method that is called before a <see cref="NotifyCollectionChangedAction"/> event is raised for this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    public delegate void PreCollectionChangedNotification();
    /// <summary>
    /// Occurs before a <see cref="NotifyCollectionChangedAction"/> event is raised. It may modify the collection.
    /// </summary>
    public event PreCollectionChangedNotification PreCollectionChanged;
    /// <summary>
    /// Encapsulates a method that is called after a <see cref="NotifyCollectionChangedAction"/> event is raised for this <see cref="ObservableCollectionFast{T}"/>. It should not modify the collection as any changes are not propagated to observers.
    /// </summary>
    public delegate void PostCollectionChangedNotification();
    /// <summary>
    /// Occurs after a <see cref="NotifyCollectionChangedAction"/> event is raised. It should not modify the collection as any changes are not propagated to observers.
    /// </summary>
    public event PostCollectionChangedNotification PostCollectionChanged;

    /// <inheritdoc/>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        PreCollectionChanged?.Invoke();
        base.OnCollectionChanged(e);
        PostCollectionChanged?.Invoke();
    }
}
