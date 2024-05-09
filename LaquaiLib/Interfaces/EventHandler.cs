namespace LaquaiLib.Interfaces;

/// <summary>
/// Represents the method that will handle an event by a sender of type <typeparamref name="TSender"/> when the event does not provide data.
/// </summary>
/// <typeparam name="TSender">The type of the sender.</typeparam>
/// <param name="sender">The source of the event.</param>
public delegate void EventHandler<TSender>(TSender sender);
/// <summary>
/// Represents the method that will handle an event by a sender of type <typeparamref name="TSender"/> when the event provides data of type <typeparamref name="TEventArgs"/>.
/// </summary>
/// <typeparam name="TSender">The type of the sender.</typeparam>
/// <typeparam name="TEventArgs">The type of the event data.</typeparam>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
public delegate void EventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);
