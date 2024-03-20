using LaquaiLib.Util;

/// <summary>
/// Provides data for the <see cref="ObservableValue{T}.PropertyRead"/> event.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class ValueReadEventArgs<T> : EventArgs
{
    /// <summary>
    /// The value the reader received.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes new <see cref="ValueReadEventArgs{T}"/> with the given value.
    /// </summary>
    /// <param name="value">The value the reader received.</param>
    public ValueReadEventArgs(T value)
    {
        Value = value;
    }
}
