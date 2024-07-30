
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents a fast implementation of a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The Type of the elements in the collection.</typeparam>
public class ObservableCollectionFast<T> : INotifyCollectionChanged, ICollection<T>, IEnumerable<T>
{
    #region Fields / Properties
    protected List<T> _items;

    /// <summary>
    /// Whether the <see cref="ObservableCollection{T}"/> is silenced. No registered events are raised, not even ones manually triggered using <see cref="RaiseCollectionChanged(NotifyCollectionChangedEventArgs?)"/>.
    /// </summary>
    private bool IsSilenced
    {
        get; set;
    }

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

    private Func<T, bool>? filter;
    public Func<T, bool>? Filter
    {
        get => filter;
        set
        {
            if (filter != value)
            {
                filter = value;
                RaiseCollectionChanged();
            }
        }
    }

    public int Count => _items.Count;
    bool ICollection<T>.IsReadOnly => false;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public ObservableCollectionFast()
    {
        _items = [];
    }
    /// <summary>
    /// Initializes a new <see cref="ObservableCollection{T}"/> that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">The collection from which the elements are copied.</param>
    public ObservableCollectionFast(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        _items = new List<T>(collection);
    }
    /// <summary>
    /// Initializes a new <see cref="ObservableCollection{T}"/> that contains elements copied from the specified span.
    /// </summary>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> from which the elements are copied.</param>
    public ObservableCollectionFast(ReadOnlySpan<T> span)
    {
        _items = new List<T>(span.ToArray());
    }
    /// <summary>
    /// Initializes a new <see cref="ObservableCollection{T}"/> that contains the specified items and has a capacity equal to the number of items.
    /// </summary>
    /// <param name="items">The items for the list to contain.</param>
    public ObservableCollectionFast(params T[] items)
    {
        _items = new List<T>(items);
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
        get => _items[index];
        set
        {
            _items[index] = value;
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
            for (var i = offset; i < offset + length; i++)
            {
                yield return this[i];
            }
        }
        set
        {
            var (offset, length) = range.GetOffsetAndLength(Count);
            var materialized = value as T[] ?? value.ToArray();
            for (var i = offset; i < offset + length; i++)
            {
                this[i] = materialized[i - offset];
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
    public IEnumerable<T> this[int index, int count] => this[range: index..(index + count)];
    #endregion

    #region Silencing
    /// <summary>
    /// Executes the specified <paramref name="action"/>. For its entire context, the <see cref="ObservableCollection{T}"/> is silenced, then the previous state is restored (that is, if the <see cref="ObservableCollection{T}"/> was silenced before, this is the same as calling <paramref name="action"/> directly).
    /// </summary>
    /// <param name="action">The <see cref="Action"/> to execute.</param>
    public void Silenced(Action action)
    {
        var old = IsSilenced;
        IsSilenced = true;
        action?.Invoke();
        IsSilenced = old;
    }
    /// <summary>
    /// Executes the specified <paramref name="action"/>. For its entire context, the <see cref="ObservableCollection{T}"/> is unsilenced, then the previous state is restored (that is, if the <see cref="ObservableCollection{T}"/> was unsilenced before, this is the same as calling <paramref name="action"/> directly).
    /// </summary>
    /// <param name="action">The <see cref="Action"/> to execute.</param>
    public void Unsilenced(Action action)
    {
        var old = IsSilenced;
        IsSilenced = false;
        action?.Invoke();
        IsSilenced = old;
    }
    #endregion

    #region Sorting
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
    public void SortSilent() => _items = new List<T>(_items.OrderBy(x => x, Comparer ?? Comparer<T>.Default));
    #endregion

    #region Add*
    /// <inheritdoc cref="ICollection{T}.Add(T)"/>
    public void Add(T item)
    {
        AddSilent(item);
        RaiseCollectionChanged();
    }
    /// <summary>
    /// Silently adds the elements of the specified collection to the end of the <see cref="ObservableCollection{T}"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    /// <param name="collection">The collection whose elements should be added to the end of the <see cref="ObservableCollection{T}"/>.</param>
    public void AddRangeSilent(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

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
        ArgumentNullException.ThrowIfNull(collection);

        foreach (var item in collection)
        {
            AddSilent(item);
        }
        RaiseCollectionChanged();
    }
    /// <summary>
    /// Silently adds an element to the end of this <see cref="ObservableCollection{T}"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    /// <param name="item">The item to add to the end of this <see cref="ObservableCollection{T}"/>.</param>
    public void AddSilent(T item) => _items.Add(item);
    #endregion
    #region Insert
    /// <summary>
    /// Inserts the specified <paramref name="item"/> into the collection at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The zero-based index at which the <paramref name="item"/> should be inserted.</param>
    /// <param name="item">The object to insert.</param>
    public void Insert(int index, T item)
    {
        InsertSilent(index, item);
        RaiseCollectionChanged();
    }
    /// <summary>
    /// Silently inserts the specified <paramref name="item"/> into the collection at the specified <paramref name="index"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    /// <param name="index">The zero-based index at which the <paramref name="item"/> should be inserted.</param>
    /// <param name="item">The object to insert.</param>
    public void InsertSilent(int index, T item) => _items.Insert(index, item);
    /// <summary>
    /// Silently inserts the specified <paramref name="items"/> into the collection at the specified <paramref name="index"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    /// <param name="index">The zero-based index at which the <paramref name="items"/> should be inserted.</param>
    /// <param name="items">The objects to insert.</param>
    public void InsertRangeSilent(int index, params T[] items) => _items.InsertRange(index, items);
    /// <summary>
    /// Silently inserts the specified <paramref name="items"/> into the collection at the specified <paramref name="index"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    /// <param name="index">The zero-based index at which the <paramref name="items"/> should be inserted.</param>
    /// <param name="items">The objects to insert.</param>
    public void InsertRangeSilent(int index, IEnumerable<T> items) => _items.InsertRange(index, items);
    /// <summary>
    /// Silently inserts the specified <paramref name="items"/> into the collection at the specified <paramref name="index"/>. This causes no <see cref="NotifyCollectionChangedAction.Add"/> event to be fired.
    /// </summary>
    /// <param name="index">The zero-based index at which the <paramref name="items"/> should be inserted.</param>
    /// <param name="items">The objects to insert.</param>
    public void InsertRangeSilent(int index, ReadOnlySpan<T> items) => _items.InsertRange(index, items);
    /// <summary>
    /// Inserts the specified <paramref name="items"/> into the collection at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The zero-based index at which the <paramref name="items"/> should be inserted.</param>
    /// <param name="items">The objects to insert.</param>
    public void InsertRange(int index, params T[] items)
    {
        InsertRangeSilent(index, items);
        RaiseCollectionChanged();
    }
    /// <summary>
    /// Inserts the specified <paramref name="items"/> into the collection at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The zero-based index at which the <paramref name="items"/> should be inserted.</param>
    /// <param name="items">The objects to insert.</param>
    public void InsertRange(int index, IEnumerable<T> items)
    {
        InsertRangeSilent(index, items);
        RaiseCollectionChanged();
    }
    /// <summary>
    /// Inserts the specified <paramref name="items"/> into the collection at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The zero-based index at which the <paramref name="items"/> should be inserted.</param>
    /// <param name="items">The objects to insert.</param>
    public void InsertRange(int index, ReadOnlySpan<T> items)
    {
        InsertRangeSilent(index, items);
        RaiseCollectionChanged();
    }
    #endregion

    #region Clearing
    /// <summary>
    /// Silently clears this <see cref="ObservableCollection{T}"/>. This causes no <see cref="NotifyCollectionChangedAction.Reset"/> event to be fired.
    /// </summary>
    public void ClearSilent() => _items.Clear();
    /// <inheritdoc/>
    public void Clear()
    {
        ClearSilent();
        RaiseCollectionChanged();
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
        ClearSilent();
        AddRange(collection);
    }
    #endregion

    #region Remove*
    /// <summary>
    /// Removes an element from this <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <param name="item">The item to remove from this <see cref="ObservableCollection{T}"/>.</param>
    public bool Remove(T item)
    {
        var ret = RemoveSilent(item);
        RaiseCollectionChanged();
        return ret;
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
    /// Removes all occurrences of the specified items from the <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <param name="collection">A sequence of values to remove from this <see cref="ObservableCollectionFast{T}"/>.</param>
    public void RemoveRange(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

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
    public bool RemoveSilent(T item) => _items.Remove(item);
    /// <summary>
    /// Silently removes all elements from this <see cref="ObservableCollection{T}"/> as dictated by a <paramref name="selector"/> <see cref="Func{T, TResult}"/>. This causes no <see cref="NotifyCollectionChangedAction.Remove"/> event to be fired.
    /// </summary>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that determines whether an element should be removed.</param>
    public void RemoveSilent(Func<T, bool> selector) => _items.RemoveAll(new Predicate<T>(selector));
    #endregion

    #region ICollection<T>
    /// <inheritdoc/>
    public bool Contains(T item) => _items.Contains(item);
    /// <summary>
    /// Searches for the specified <paramref name="item"/> in the collection using the specified <paramref name="comparer"/>, skipping the first <paramref name="startIndex"/> elements.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default comparer.</param>
    /// <param name="startIndex">The index at which to start the search.</param>
    /// <returns>The index of the first occurrence of the specified <paramref name="item"/> in the collection, or -1 if the <paramref name="item"/> is not found.</returns>
    public int IndexOf(T? item, IEqualityComparer<T>? comparer = null, int startIndex = 0)
    {
        if (item is null)
        {
            return -1;
        }
        var i = startIndex;
        foreach (var t in ((IEnumerable<T>)(Filter is not null ? this : _items)).Skip(startIndex))
        {
            if (comparer is not null ? comparer.Equals(t, item) : t.Equals(item))
            {
                return i;
            }
            i++;
        }
        return -1;
    }
    /// <summary>
    /// Searches for the specified <paramref name="item"/> in the collection in reverse using the specified <paramref name="comparer"/>, skipping the last <paramref name="startIndex"/> elements.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default comparer.</param>
    /// <param name="startIndex">The index at which to start the search, or <c>-1</c> to start with the last element.</param>
    /// <returns>The index of the first occurrence of the specified <paramref name="item"/> in the collection, or -1 if the <paramref name="item"/> is not found.</returns>
    public int LastIndexOf(T? item, IEqualityComparer<T>? comparer = null, int startIndex = -1)
    {
        if (item is null)
        {
            return -1;
        }
        var i = startIndex == -1 ? _items.Count : startIndex;
        foreach (var t in ((IEnumerable<T>)(Filter is not null ? this : _items)).Reverse().Skip(startIndex))
        {
            if (comparer is not null ? comparer.Equals(t, item) : t.Equals(item))
            {
                return i;
            }
            i--;
        }
        return -1;
    }
    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in new FilterableEnumerator<T>(_items, Filter))
        {
            yield return item;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Events
    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Raises a <see cref="NotifyCollectionChangedAction.Reset"/> event. Changes made to the <see cref="ObservableCollection{T}"/> by any methods with a "Silent" suffix will not be propagated to observers until this method is called.
    /// </summary>
    /// <paramref name="e"/>The <see cref="NotifyCollectionChangedEventArgs"/> to pass to observers. If <see langword="null"/>, a <see cref="NotifyCollectionChangedEventArgs"/> with <see cref="NotifyCollectionChangedAction.Reset"/> will be passed.
    public void RaiseCollectionChanged(NotifyCollectionChangedEventArgs? e = null) => OnCollectionChanged(e);
    /// <inheritdoc/>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs? e = null)
    {
        e ??= new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

        if (KeepOrdered)
        {
            SortSilent();
        }

        if (!IsSilenced)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
    #endregion
}
