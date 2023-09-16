using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LaquaiLib.Classes.Collections;

/// <summary>
/// Represents a fast implementation of a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The Type of the elements in the collection.</typeparam>
public class ObservableCollectionFast<T> : ObservableCollection<T>
{
    #region Fields / Properties
    private bool isSilenced;

    private bool keepOrdered;
    /// <summary>
    /// Whether the <see cref="ObservableCollection{T}"/> should keep itself ordered. When this is <see langword="true"/>, whenever the collection is modified in a way that raises a <see cref="NotifyCollectionChangedAction"/> event, it is sorted using <see cref="Comparer"/>.
    /// Assigning a new <see cref="bool"/> value will cause the <see cref="ObservableCollection{T}"/> to be sorted using the currently set <see cref="Comparer"/> immediately. This also raises a <see cref="NotifyCollectionChangedAction.Reset"/> event.
    /// </summary>
    public bool KeepOrdered
    {
        get => keepOrdered;
        set
        {
            if (keepOrdered != value && value)
            {
                Sort();
            }
            keepOrdered = value;
        }
    }

    private IComparer<T>? comparer;
    /// <summary>
    /// The <see cref="IComparer{T}"/> used to compare elements in the <see cref="ObservableCollection{T}"/> if <see cref="KeepOrdered"/> is <see langword="true"/>.
    /// <para/>
    /// <para/>Assigning a new <see cref="IComparer{T}"/> will cause the <see cref="ObservableCollection{T}"/> to be sorted using the new <see cref="IComparer{T}"/> immediately. This also raises a <see cref="NotifyCollectionChangedAction.Reset"/> event.
    /// </summary>
    public IComparer<T>? Comparer
    {
        get => comparer;
        set
        {
            if (comparer != value && value is not null)
            {
                comparer = value;
                Sort();
            }
        }
    }
    #endregion

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
    public T this[Index index]
    {
        get
        {
            IndexGet?.Invoke(index.GetOffset(Count));
            return base[index];
        }
        set
        {
            IndexSet?.Invoke(index.GetOffset(Count));
            base[index] = value;
            RaiseCollectionChanged();
        }
    }

    /// <summary>
    /// Gets or sets elements within the specified <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The <see cref="Range"/> in which to get or set elements.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the items that were get or set.</returns>
    public IEnumerable<T> this[Range range]
    {
        get
        {
            var (offset, length) = range.GetOffsetAndLength(Count);
            RangeGet?.Invoke(offset, length);
            for (var i = offset; i < offset + length; i++)
            {
                yield return this[i];
            }
        }
        set
        {
            var (offset, length) = range.GetOffsetAndLength(Count);
            RangeSet?.Invoke(offset, length);
            for (var i = offset; i < offset + length; i++)
            {
                this[i] = value.ElementAt(i - offset);
            }
            RaiseCollectionChanged();
        }
    }

    /// <summary>
    /// Gets or sets elements within a range as specified by <paramref name="index"/> and <paramref name="count"/>.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range of elements to get or set.</param>
    /// <param name="count">The number of elements to get or set.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the items that were get or set.</returns>
    public IEnumerable<T> this[int index, int count]
    {
        get
        {
            RangeGet?.Invoke(index, count);
            return this[range: index..(index + count)];
        }
    }
    #endregion

    /// <summary>
    /// Silences the <see cref="ObservableCollection{T}"/>. Changes made to the <see cref="ObservableCollection{T}"/> are not propagated to observers, not even using <see cref="RaiseCollectionChanged(NotifyCollectionChangedEventArgs?)"/>.
    /// </summary>
    public void Silence()
    {
        isSilenced = true;
    }
    /// <summary>
    /// Executes the specified <paramref name="action"/>. For its entire context, the <see cref="ObservableCollection{T}"/> is silenced.
    /// </summary>
    /// <param name="action">The <see cref="Action"/> to execute.</param>
    public void Silence(Action action)
    {
        Silence();
        action();
        Unsilence();
    }
    /// <summary>
    /// Unsilences the <see cref="ObservableCollection{T}"/>. Changes made to the <see cref="ObservableCollection{T}"/> are propagated to observers as normal.
    /// </summary>
    public void Unsilence()
    {
        isSilenced = false;
    }

    /// <summary>
    /// Orders the elements in the <see cref="ObservableCollection{T}"/> using the <see cref="Comparer"/> or the default <see cref="Comparer{T}"/> if <see cref="Comparer"/> is <see langword="null"/>.
    /// </summary>
    public void Sort()
    {
        SortSilent();
        RaiseCollectionChanged();
    }

    /// <summary>
    /// Silently orders the elements in the <see cref="ObservableCollection{T}"/> using the <see cref="Comparer"/> or the default <see cref="Comparer{T}"/> if <see cref="Comparer"/> is <see langword="null"/>. This causes no <see cref="NotifyCollectionChangedAction.Reset"/> event to be fired.
    /// </summary>
    public void SortSilent()
    {
        if (Comparer is not null)
        {
            ClearSilent();
            foreach (var item in Items.OrderBy(x => x, Comparer).ToList())
            {
                AddSilent(item);
            }
        }
        else
        {
            ClearSilent();
            foreach (var item in Items.OrderBy(x => x, Comparer<T>.Default).ToList())
            {
                AddSilent(item);
            }
        }
    }

    /// <summary>
    /// Silently adds the elements of the specified collection to the end of the <see cref="ObservableCollection{T}"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added to the end of the <see cref="ObservableCollection{T}"/>.</param>
    public void AddRangeSilent(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            AddSilent(item);
        }
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added to the end of the <see cref="ObservableCollection{T}"/>.</param>
    public void AddRange(IEnumerable<T> collection)
    {
        RangeAdded?.Invoke(collection);
        foreach (var item in collection)
        {
            AddSilent(item);
        }
        RaiseCollectionChanged();
    }

    /// <inheritdoc cref="ICollection{T}.Add(T)"/>
    public new void Add(T item)
    {
        Added?.Invoke(item);
        AddSilent(item);
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
        RangeRemoved?.Invoke(collection);
        foreach (var item in collection)
        {
            RemoveSilent(item);
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
    /// Silently removes all elements from this <see cref="ObservableCollection{T}"/> as dictated by a <paramref name="selector"/> <see cref="Func{T, TResult}"/>. This causes no <see cref="NotifyCollectionChangedAction.Remove"/> event to be fired.
    /// </summary>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that determines whether an element should be removed.</param>
    public void RemoveSilent(Func<T, bool> selector)
    {
        foreach (var item in Items.Where(selector).ToList())
        {
            RemoveSilent(item);
        }
    }

    /// <summary>
    /// Silently clears this <see cref="ObservableCollection{T}"/>. This causes no <see cref="NotifyCollectionChangedAction.Reset"/> event to be fired.
    /// </summary>
    public void ClearSilent()
    {
        RangeRemoved?.Invoke(Items);
        Items.Clear();
    }

    /// <summary>
    /// Resets this <see cref="ObservableCollection{T}"/> by clearing it (silently) and re-filling it using the specified <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">The collection to fill this <see cref="ObservableCollection{T}"/> with.</param>
    /// <remarks>
    /// The clearing operation itself is silent, but the re-filling operation is not; that is, observers of the <see cref="ObservableCollection{T}"/> will only be notified AFTER the re-filling operation is complete.
    /// </remarks>
    public void Reset(IEnumerable<T> collection)
    {
        Refill?.Invoke(collection);
        ClearSilent();
        AddRange(collection);
    }

    /// <summary>
    /// Removes an element from this <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <param name="item">The item to remove from this <see cref="ObservableCollection{T}"/>.</param>
    public new void Remove(T item)
    {
        Removed?.Invoke(item);
        RemoveSilent(item);
        RaiseCollectionChanged();
    }

    /// <summary>
    /// Removes all elements from this <see cref="ObservableCollection{T}"/> as dictated by a <paramref name="selector"/> <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that determines whether an element should be removed.</param>
    public void Remove(Func<T, bool> selector)
    {
        RangeRemoved?.Invoke(Items.Where(selector));
        RemoveSilent(selector);
        RaiseCollectionChanged();
    }

    /// <summary>
    /// Raises a <see cref="NotifyCollectionChangedAction.Reset"/> event. Changes made to the <see cref="ObservableCollection{T}"/> by any methods with a "Silent" suffix will not be propagated to observers until this method is called.
    /// <paramref name="e"/>The <see cref="NotifyCollectionChangedEventArgs"/> to pass to observers. If <see langword="null"/>, a <see cref="NotifyCollectionChangedEventArgs"/> with <see cref="NotifyCollectionChangedAction.Reset"/> will be passed.
    /// </summary>
    public void RaiseCollectionChanged(NotifyCollectionChangedEventArgs? e = null)
    {
        e ??= new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
        OnCollectionChanged(e);
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
    /// Encapsulates a method that is called after a <see cref="NotifyCollectionChangedAction"/> event is raised for this <see cref="ObservableCollectionFast{T}"/>. It may still modify the collection.
    /// </summary>
    public delegate void PostCollectionChangedNotification();
    /// <summary>
    /// Occurs after a <see cref="NotifyCollectionChangedAction"/> event is raised. It should not modify the collection as changes 
    /// </summary>
    public event PostCollectionChangedNotification PostCollectionChanged;

    /// <summary>
    /// Encapsulates a method that is called before a <see cref="NotifyCollectionChangedAction.Add"/> event is raised for this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    public delegate void AddedNotification(T item);
    /// <summary>
    /// Occurs before a <see cref="NotifyCollectionChangedAction.Add"/> event is raised. It may modify the collection.
    /// </summary>
    public event AddedNotification Added;
    /// <summary>
    /// Encapsulates a method that is called before a <see cref="NotifyCollectionChangedAction.Remove"/> event is raised for this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    /// <param name="item">The item to be removed.</param>
    public delegate void RemovedNotification(T item);
    /// <summary>
    /// Occurs before a <see cref="NotifyCollectionChangedAction.Remove"/> event is raised. It may modify the collection.
    /// </summary>
    public event RemovedNotification Removed;
    /// <summary>
    /// Encapsulates a method that is called before multiple items are added to this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    /// <param name="items">The items to be added.</param>
    public delegate void RangeAddedNotification(IEnumerable<T> items);
    /// <summary>
    /// Occurs before multiple items are added to this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    public event RangeAddedNotification RangeAdded;
    /// <summary>
    /// Encapsulates a method that is called before multiple items are removed from this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    /// <param name="items">The items to be removed.</param>
    public delegate void RangeRemovedNotification(IEnumerable<T> items);
    /// <summary>
    /// Occurs before multiple items are removed from this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    public event RangeRemovedNotification RangeRemoved;
    /// <summary>
    /// Encapsulates a method that is called when an item located at an individual index is accessed. It may modify the collection.
    /// </summary>
    /// <param name="index">The index of the item to be accessed.</param>
    public delegate void IndexGetNotification(int index);
    /// <summary>
    /// Occurs when an item located at an individual index is accessed. It may modify the collection.
    /// </summary>
    public event IndexGetNotification IndexGet;
    /// <summary>
    /// The event that is raised when an item located at an individual index is set. It may modify the collection.
    /// </summary>
    /// <param name="index">The index of the item to be set.</param>
    public delegate void IndexSetNotification(int index);
    /// <summary>
    /// Occurs when an item located at an individual index is set. It may modify the collection.
    /// </summary>
    public event IndexSetNotification IndexSet;
    /// <summary>
    /// Encapsulates a method that is called when multiple items in a specific range are accessed. It may modify the collection.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range of items to be accessed.</param>
    /// <param name="count">The number of items to be accessed.</param>
    public delegate void RangeGetNotification(int index, int count);
    /// <summary>
    /// Occurs when multiple items in a specific range are accessed. It may modify the collection.
    /// </summary>
    public event RangeGetNotification RangeGet;
    /// <summary>
    /// Encapsulates a method that is called when multiple items in a specific range are set. It may modify the collection.
    /// </summary>
    /// <param name="index">The zero-based starting index of the range of items to be set.</param>
    /// <param name="count">The number of items to be set.</param>
    public delegate void RangeSetNotification(int index, int count);
    /// <summary>
    /// Occurs when multiple items in a specific range are set. It may modify the collection.
    /// </summary>
    public event RangeSetNotification RangeSet;
    /// <summary>
    /// Encapsulates a method that is called when the collection is reset by clearing it and refilling it. It may modify the collection.
    /// </summary>
    /// <param name="newContents">The new contents of the collection.</param>
    public delegate void ResetNotification(IEnumerable<T> newContents);
    /// <summary>
    /// Occurs when the collection is reset by clearing it and refilling it. It may modify the collection.
    /// </summary>
    public event ResetNotification Refill;
    /// <summary>
    /// Encapsulates a method that is called when the collection is reset by clearing it. It may modify the collection.
    /// </summary>
    public delegate void ClearNotification();
    /// <summary>
    /// Occurs when the collection is reset by clearing it. It may modify the collection.
    /// </summary>
    public event ClearNotification Empty;

    /// <inheritdoc/>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        PreCollectionChanged?.Invoke();
        if (!isSilenced)
        {
            base.OnCollectionChanged(e);
        }
        PostCollectionChanged?.Invoke();
    }
}
