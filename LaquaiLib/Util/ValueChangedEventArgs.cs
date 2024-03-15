namespace LaquaiLib.Util;

/// <summary>
/// Provides data for the <see cref="ObservableValue{T}.ValueChanged"/> event.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class ValueChangedEventArgs<T> : EventArgs
{
    /// <summary>
    /// The value before the change.
    /// </summary>
    public T OldValue { get; }
    /// <summary>
    /// The value after the change.
    /// </summary>
    public T NewValue { get; }

    /// <summary>
    /// Instantiates new <see cref="ValueChangedEventArgs{T}"/> with the given values.
    /// </summary>
    /// <param name="oldValue">The value before the change.</param>
    /// <param name="newValue">The value after the change.</param>
    public ValueChangedEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
