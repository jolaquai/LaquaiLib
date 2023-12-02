#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace LaquaiLib.Classes.Collections;

/// <summary>
/// Represents a fast implementation of a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The Type of the elements in the collection.</typeparam>
[CollectionBuilder(typeof(ObservableCollectionFastBuilder), nameof(ObservableCollectionFastBuilder.Create))]
public class ObservableCollectionFast<T> : ObservableCollection<T>
{
    #region Fields / Properties
    /// <summary>
    /// Whether the <see cref="ObservableCollection{T}"/> is silenced. No registered events are raised, not even ones manually triggered using <see cref="RaiseCollectionChanged(NotifyCollectionChangedEventArgs?)"/>.
    /// </summary>
    private bool IsSilenced { get; set; }

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
    public ObservableCollectionFast()
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
    /// Instantiates a new <see cref="ObservableCollection{T}"/> that contains elements copied from the specified span.
    /// </summary>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> from which the elements are copied.</param>
    public ObservableCollectionFast(ReadOnlySpan<T> span) : base(span.ToArray())
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
            if (!IsSilenced)
            {
                IndexGet?.Invoke(this, new IndexGetEventArgs(index.GetOffset(Count)));
            }
            return base[index];
        }
        set
        {
            if (!IsSilenced)
            {
                IndexSet?.Invoke(this, new IndexSetEventArgs(index.GetOffset(Count)));
            }
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
            if (!IsSilenced)
            {
                RangeGet?.Invoke(this, new RangeGetEventArgs(offset, length));
            }
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
            if (!IsSilenced)
            {
                RangeSet?.Invoke(this, new RangeSetEventArgs(offset, length));
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
            if (!IsSilenced)
            {
                RangeGet?.Invoke(this, new RangeGetEventArgs(index, count));
            }
            return this[range: index..(index + count)];
        }
    }
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
    #endregion

    #region Add*
    /// <inheritdoc cref="ICollection{T}.Add(T)"/>
    public new void Add(T item)
    {
        if (!IsSilenced)
        {
            Added?.Invoke(this, new AddedEventArgs<T>(item));
        }
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

        if (!IsSilenced)
        {
            RangeAdded?.Invoke(this, new RangeAddedEventArgs<T>(collection));
        }
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
    public void AddSilent(T item)
    {
        Items.Add(item);
    }
    #endregion

    #region Clearing
    /// <summary>
    /// Silently clears this <see cref="ObservableCollection{T}"/>. This causes no <see cref="NotifyCollectionChangedAction.Reset"/> event to be fired.
    /// </summary>
    public void ClearSilent()
    {
        if (!IsSilenced)
        {
            RangeRemoved?.Invoke(this, new RangeRemovedEventArgs<T>(Items));
        }
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
        if (!IsSilenced)
        {
            Refill?.Invoke(this, new ResetEventArgs<T>(collection));
        }
        ClearSilent();
        AddRange(collection);
    }
    #endregion

    #region Remove*
    /// <summary>
    /// Removes an element from this <see cref="ObservableCollection{T}"/>.
    /// </summary>
    /// <param name="item">The item to remove from this <see cref="ObservableCollection{T}"/>.</param>
    public new void Remove(T item)
    {
        if (!IsSilenced)
        {
            Removed?.Invoke(this, new RemovedEventArgs<T>(item));
        }
        RemoveSilent(item);
        RaiseCollectionChanged();
    }
    /// <summary>
    /// Removes all elements from this <see cref="ObservableCollection{T}"/> as dictated by a <paramref name="selector"/> <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that determines whether an element should be removed.</param>
    public void Remove(Func<T, bool> selector)
    {
        if (!IsSilenced)
        {
            RangeRemoved?.Invoke(this, new RangeRemovedEventArgs<T>(Items.Where(selector)));
        }
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

        if (!IsSilenced)
        {
            RangeRemoved?.Invoke(this, new RangeRemovedEventArgs<T>(collection));
        }
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
    #endregion

    #region Events
    /// <summary>
    /// Raises a <see cref="NotifyCollectionChangedAction.Reset"/> event. Changes made to the <see cref="ObservableCollection{T}"/> by any methods with a "Silent" suffix will not be propagated to observers until this method is called.
    /// <paramref name="e"/>The <see cref="NotifyCollectionChangedEventArgs"/> to pass to observers. If <see langword="null"/>, a <see cref="NotifyCollectionChangedEventArgs"/> with <see cref="NotifyCollectionChangedAction.Reset"/> will be passed.
    /// </summary>
    public void RaiseCollectionChanged(NotifyCollectionChangedEventArgs? e = null) => OnCollectionChanged(e!);
    /// <inheritdoc/>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        e ??= new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

        if (KeepOrdered)
        {
            SortSilent();
        }

        if (!IsSilenced)
        {
            PreCollectionChanged?.Invoke(this, EventArgs.Empty);
            base.OnCollectionChanged(e);
            PostCollectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Occurs before a <see cref="NotifyCollectionChangedAction"/> event is raised. It may modify the collection.
    /// </summary>
    public event EventHandler PreCollectionChanged;
    /// <summary>
    /// Occurs after a <see cref="NotifyCollectionChangedAction"/> event is raised. It should not modify the collection as changes
    /// </summary>
    public event EventHandler PostCollectionChanged;
    /// <summary>
    /// Occurs before a <see cref="NotifyCollectionChangedAction.Add"/> event is raised for this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    public event EventHandler<AddedEventArgs<T>> Added;
    /// <summary>
    /// Occurs before a <see cref="NotifyCollectionChangedAction.Remove"/> event is raised for this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    public event EventHandler<RemovedEventArgs<T>> Removed;
    /// <summary>
    /// Occurs before multiple items are added to this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    public event EventHandler<RangeAddedEventArgs<T>> RangeAdded;
    /// <summary>
    /// Occurs before multiple items are removed from this <see cref="ObservableCollectionFast{T}"/>. It may modify the collection.
    /// </summary>
    public event EventHandler<RangeRemovedEventArgs<T>> RangeRemoved;
    /// <summary>
    /// Occurs when an item located at an individual index is accessed. It may modify the collection.
    /// </summary>
    public event EventHandler<IndexGetEventArgs> IndexGet;
    /// <summary>
    /// The event that is raised when an item located at an individual index is set. It may modify the collection.
    /// </summary>
    public event EventHandler<IndexSetEventArgs> IndexSet;
    /// <summary>
    /// Occurs when multiple items in a specific range are accessed. It may modify the collection.
    /// </summary>
    public event EventHandler<RangeGetEventArgs> RangeGet;
    /// <summary>
    /// Occurs when multiple items in a specific range are set. It may modify the collection.
    /// </summary>
    public event EventHandler<RangeSetEventArgs> RangeSet;
    /// <summary>
    /// Occurs when the collection is reset by clearing it and refilling it. It may modify the collection.
    /// </summary>
    public event EventHandler<ResetEventArgs<T>> Refill;
    /// <summary>
    /// Occurs when the collection is reset by clearing it. It may modify the collection.
    /// </summary>
    public event EventHandler Empty;
    #endregion
}

#region EventArgs classes
/// <summary>
/// Represents the event arguments for the Added event.
/// </summary>
/// <typeparam name="T">The type of the item being added.</typeparam>
public class AddedEventArgs<T>
{
    /// <summary>
    /// Gets the item that was added.
    /// </summary>
    public T Item { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AddedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="item">The item that was added.</param>
    public AddedEventArgs(T item)
    {
        Item = item;
    }
}

/// <summary>
/// Represents the event arguments for the Removed event.
/// </summary>
/// <typeparam name="T">The type of the item being removed.</typeparam>
public class RemovedEventArgs<T>
{
    /// <summary>
    /// Gets the item that was removed.
    /// </summary>
    public T Item { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemovedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="item">The item that was removed.</param>
    public RemovedEventArgs(T item)
    {
        Item = item;
    }
}

/// <summary>
/// Represents the event arguments for the RangeAdded event.
/// </summary>
/// <typeparam name="T">The type of the items being added.</typeparam>
public class RangeAddedEventArgs<T>
{
    /// <summary>
    /// Gets the items that were added.
    /// </summary>
    public IEnumerable<T> Items { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeAddedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="items">The items that were added.</param>
    public RangeAddedEventArgs(IEnumerable<T> items)
    {
        Items = items;
    }
}

/// <summary>
/// Represents the event arguments for the RangeRemoved event.
/// </summary>
/// <typeparam name="T">The type of the items being removed.</typeparam>
public class RangeRemovedEventArgs<T>
{
    /// <summary>
    /// Gets the items that were removed.
    /// </summary>
    public IEnumerable<T> Items { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeRemovedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="items">The items that were removed.</param>
    public RangeRemovedEventArgs(IEnumerable<T> items)
    {
        Items = items;
    }
}

/// <summary>
/// Represents the event arguments for the IndexGet event.
/// </summary>
public class IndexGetEventArgs
{
    /// <summary>
    /// Gets the index being accessed.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexGetEventArgs"/> class.
    /// </summary>
    /// <param name="index">The index being accessed.</param>
    public IndexGetEventArgs(int index)
    {
        Index = index;
    }
}

/// <summary>
/// Represents the event arguments for the IndexSet event.
/// </summary>
public class IndexSetEventArgs
{
    /// <summary>
    /// Gets the index being accessed.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexSetEventArgs"/> class.
    /// </summary>
    /// <param name="index">The index being accessed.</param>
    public IndexSetEventArgs(int index)
    {
        Index = index;
    }
}

/// <summary>
/// Represents the event arguments for the RangeGet event.
/// </summary>
public class RangeGetEventArgs
{
    /// <summary>
    /// Gets the starting index of the range being accessed.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the number of items in the range being accessed.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeGetEventArgs"/> class.
    /// </summary>
    /// <param name="index">The starting index of the range being accessed.</param>
    /// <param name="count">The number of items in the range being accessed.</param>
    public RangeGetEventArgs(int index, int count)
    {
        Index = index;
        Count = count;
    }
}

/// <summary>
/// Represents the event arguments for the RangeSet event.
/// </summary>
public class RangeSetEventArgs
{
    /// <summary>
    /// Gets the starting index of the range being accessed.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the number of items in the range being accessed.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeSetEventArgs"/> class.
    /// </summary>
    /// <param name="index">The starting index of the range being accessed.</param>
    /// <param name="count">The number of items in the range being accessed.</param>
    public RangeSetEventArgs(int index, int count)
    {
        Index = index;
        Count = count;
    }
}

/// <summary>
/// Represents the event arguments for the Reset event.
/// </summary>
/// <typeparam name="T">The type of the new contents.</typeparam>
public class ResetEventArgs<T>
{
    /// <summary>
    /// Gets the new contents.
    /// </summary>
    public IEnumerable<T> NewContents { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetEventArgs{T}"/> class.
    /// </summary>
    /// <param name="newContents">The new contents.</param>
    public ResetEventArgs(IEnumerable<T> newContents)
    {
        NewContents = newContents;
    }
}
#endregion

/// <summary>
/// Provides a builder for <see cref="ObservableCollectionFast{T}"/>s.
/// </summary>
public static class ObservableCollectionFastBuilder
{
    /// <summary>
    /// Builds a <see cref="ObservableCollectionFast{T}"/> from the passed <paramref name="span"/>.
    /// Used to allow <see cref="ObservableCollectionFast{T}"/> to be created from collection literals.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the span.</typeparam>
    /// <param name="span">The span to copy the new <see cref="ObservableCollectionFast{T}"/>'s items from.</param>
    /// <returns>A new <see cref="ObservableCollectionFast{T}"/> with the items from <paramref name="span"/>.</returns>
    public static ObservableCollectionFast<T> Create<T>(ReadOnlySpan<T> span) => new ObservableCollectionFast<T>(span);
}
