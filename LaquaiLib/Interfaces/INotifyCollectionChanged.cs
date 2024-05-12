using System.Collections.Specialized;

namespace LaquaiLib.Interfaces;

/// <summary>
/// Represents a type-safe collection that provides notifications when the collection is mutated.
/// This interface is designed to seamlessly interoperate with <see cref="INotifyCollectionChanged"/> while providing full type safety by automatically proxying event handler registrations.
/// </summary>
/// <typeparam name="TSelf">The type that implements this interface.</typeparam>
/// <typeparam name="TItem">The type of the items in the collection.</typeparam>
public interface INotifyCollectionChanged<TSelf, TItem> : INotifyCollectionChanged
    where TSelf : INotifyCollectionChanged<TSelf, TItem>
{
    public event EventHandler<TSelf, CollectionChangedEventArgs<TItem>>? CollectionChanged;

    event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
    {
        add => CollectionChanged += (sender, e) => value?.Invoke(sender, (NotifyCollectionChangedEventArgs)e);
        remove => CollectionChanged -= (sender, e) => value?.Invoke(sender, (NotifyCollectionChangedEventArgs)e);
    }
}
/// <summary>
/// Provides type-safe event data for the <see cref="INotifyCollectionChanged{TItem}.CollectionChanged"/> event.
/// </summary>
/// <typeparam name="TItem"></typeparam>
public class CollectionChangedEventArgs<TItem>
{
    /// <summary>
    /// The <see cref="NotifyCollectionChangedAction"/> that caused the event.
    /// </summary>
    public NotifyCollectionChangedAction Action
    {
        get;
    }
    /// <summary>
    /// The items affected by the change.
    /// </summary>
    public IList<TItem>? NewItems
    {
        get;
    }
    /// <summary>
    /// The old items affected by the change (for Replace events).
    /// </summary>
    public IList<TItem>? OldItems
    {
        get;
    }
    /// <summary>
    /// The index where the change occurred.
    /// </summary>
    public int NewStartingIndex { get; } = -1;
    /// <summary>
    /// The old index where the change occurred (for Move events).
    /// </summary>
    public int OldStartingIndex { get; } = -1;

    public CollectionChangedEventArgs(NotifyCollectionChangedAction action)
    {
        Action = action;
    }
    public CollectionChangedEventArgs(NotifyCollectionChangedAction action, IList<TItem>? changedItems)
    {
        Action = action;
        NewItems = changedItems;
    }
    public CollectionChangedEventArgs(NotifyCollectionChangedAction action, object? changedItem)
    {
        Action = action;
        NewItems = new TItem[] { (TItem)changedItem! };
    }
    public CollectionChangedEventArgs(NotifyCollectionChangedAction action, IList<TItem> newItems, IList<TItem> oldItems)
    {
        Action = action;
        NewItems = newItems;
        OldItems = oldItems;
    }
    public CollectionChangedEventArgs(NotifyCollectionChangedAction action, IList<TItem>? changedItems, int startingIndex)
    {
        Action = action;
        NewItems = changedItems;
        NewStartingIndex = startingIndex;
    }
    public CollectionChangedEventArgs(NotifyCollectionChangedAction action, object? changedItem, int index)
    {
        Action = action;
        NewItems = new TItem[] { (TItem)changedItem! };
        NewStartingIndex = index;
    }
    public CollectionChangedEventArgs(NotifyCollectionChangedAction action, object? newItem, object? oldItem)
    {
        Action = action;
        NewItems = new TItem[] { (TItem)newItem! };
        OldItems = new TItem[] { (TItem)oldItem! };
    }
    public CollectionChangedEventArgs(NotifyCollectionChangedAction action, IList<TItem> newItems, IList<TItem> oldItems, int startingIndex)
    {
        Action = action;
        NewItems = newItems;
        OldItems = oldItems;
        NewStartingIndex = startingIndex;
    }
    public CollectionChangedEventArgs(NotifyCollectionChangedAction action, IList<TItem>? changedItems, int index, int oldIndex)
    {
        Action = action;
        NewItems = changedItems;
        NewStartingIndex = index;
        OldStartingIndex = oldIndex;
    }
    public CollectionChangedEventArgs(NotifyCollectionChangedAction action, object? changedItem, int index, int oldIndex)
    {
        Action = action;
        NewItems = new TItem[] { (TItem)changedItem! };
        NewStartingIndex = index;
        OldStartingIndex = oldIndex;
    }
    public CollectionChangedEventArgs(NotifyCollectionChangedAction action, object? newItem, object? oldItem, int index)
    {
        Action = action;
        NewItems = new TItem[] { (TItem)newItem! };
        OldItems = new TItem[] { (TItem)oldItem! };
        NewStartingIndex = index;
    }

    public static explicit operator NotifyCollectionChangedEventArgs(CollectionChangedEventArgs<TItem> args)
    {
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, args.NewItems, args.NewStartingIndex);
            case NotifyCollectionChangedAction.Remove:
                return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, args.OldItems, args.OldStartingIndex);
            case NotifyCollectionChangedAction.Replace:
                return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, args.NewItems, args.OldItems, args.NewStartingIndex);
            case NotifyCollectionChangedAction.Move:
                return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, args.NewItems, args.NewStartingIndex, args.OldStartingIndex);
            default:
                return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
        }
    }
}