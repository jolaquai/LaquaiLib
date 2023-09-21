using System.Collections.Specialized;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a value that can be observed for and notifies its observers about changes.
/// </summary>
public class ObservableValue<T>
    : IEquatable<T>, IComparable<T>, IEquatable<ObservableValue<T>>, IComparable<ObservableValue<T>>, ICloneable
{
    private T value;
    /// <summary>
    /// The current value.
    /// </summary>
    public T Value
    {
        get
        {
            RaiseValueRead(value);
            return value;
        }
        set
        {
            if (!Equals(this.value, value))
            {
                var old = this.value;
                this.value = value;
                RaiseValueChanged(old, value);
            }
        }
    }

    /// <summary>
    /// The <see cref="IComparer{T}"/> to use when comparing instances of this type.
    /// </summary>
    public IComparer<T> Comparer { get; } = Comparer<T>.Default;

    /// <summary>
    /// Instantiates an <see cref="ObservableValue{T}"/> of type <typeparamref name="T"/> and sets the value to the default for this type. At instantiation, no <see cref="ValueChanged"/> event is raised.
    /// </summary>
    public ObservableValue()
    {
        this.value = default;
    }
    /// <summary>
    /// Instantiates an <see cref="ObservableValue{T}"/> of type <typeparamref name="T"/> with the given initial <paramref name="value"/>. At instantiation, no <see cref="ValueChanged"/> event is raised.
    /// </summary>
    /// <param name="value">The initial value.</param>
    public ObservableValue(T value)
    {
        this.value = value;
    }

    /// <summary>
    /// Represents a method that is called when the value changes.
    /// </summary>
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs<T> e);
    /// <summary>
    /// Occurs when the value changes.
    /// </summary>
    public event ValueChangedEventHandler ValueChanged;
    /// <summary>
    /// Represents a method that is called when the value is read.
    /// </summary>
    public delegate void ValueReadEventHandler(object sender, ValueReadEventArgs<T> e);
    /// <summary>
    /// Occurs when the value is read.
    /// </summary>
    public event ValueReadEventHandler ValueRead;

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>The new value.</returns>
    public T Set(T newValue)
    {
        return Value = newValue;
    }
    /// <summary>
    /// Sets the value without raising a <see cref="ValueChanged"/> event.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>The new value.</returns>
    public T SetSilent(T newValue)
    {
        return value = newValue;
    }
    /// <summary>
    /// Determines if a value has been set.
    /// </summary>
    /// <returns><see langword="true"/> if a value has been set, otherwise <see langword="false"/>.</returns>
    /// <remarks>A value has been "set" if the current value is non-<see langword="null"/>. This also counts for primitive types.</remarks>
    public bool HasValue() => value != null;
    /// <summary>
    /// Queries the value.
    /// </summary>
    /// <returns>The current value.</returns>
    public T Query()
    {
        return Value;
    }
    /// <summary>
    /// Queries the value without raising a <see cref="ValueRead"/> event.
    /// </summary>
    /// <returns>The current value.</returns>
    public T QuerySilent()
    {
        return value;
    }

    private void RaiseValueRead(T value)
    {
        ValueRead?.Invoke(this, new ValueReadEventArgs<T>(value));
    }
    private void RaiseValueChanged(T oldValue, T newValue)
    {
        ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>(oldValue, newValue));
    }

    #region Interface implementations
    /// <inheritdoc/>
    public int CompareTo(ObservableValue<T>? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Comparer<T>.Default.Compare(Value, other.Value);
    }
    /// <inheritdoc/>
    public bool Equals(ObservableValue<T>? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Equals(Value, other.Value);
    }
    /// <inheritdoc/>
    public int CompareTo(T? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Comparer<T>.Default.Compare(Value, other);
    }
    /// <inheritdoc/>
    public bool Equals(T? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Equals(Value, other);
    }

    /// <summary>
    /// Creates a copy of this <see cref="ObservableValue{T}"/> with the same initial value as this one's current value.
    /// </summary>
    /// <returns>The copy instance.</returns>
    public object Clone()
    {
        return new ObservableValue<T>(value);
    }

    #endregion

    /// <summary>
    /// Converts the given <see cref="ObservableValue{T}"/> to its underlying value.
    /// </summary>
    /// <param name="value">The <see cref="ObservableValue{T}"/> to convert.</param>
    public static implicit operator T(ObservableValue<T> value) => value.Value;
    /// <summary>
    /// Makes the given value observable.
    /// </summary>
    /// <param name="value">The value to instantiate an <see cref="ObservableValue{T}"/> from.</param>
    public static explicit operator ObservableValue<T>(T value) => new ObservableValue<T>(value);

    #region Equality / comparison overrides
    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj is null)
        {
            return false;
        }
        return obj is ObservableValue<T> observableValue && Equals(observableValue);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc/>
    public static bool operator ==(ObservableValue<T> left, ObservableValue<T> right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(ObservableValue<T> left, ObservableValue<T> right) => !(left == right);

    /// <inheritdoc/>
    public static bool operator <(ObservableValue<T> left, ObservableValue<T> right) => left is null ? right is not null : left.CompareTo(right) < 0;

    /// <inheritdoc/>
    public static bool operator <=(ObservableValue<T> left, ObservableValue<T> right) => left is null || left.CompareTo(right) <= 0;

    /// <inheritdoc/>
    public static bool operator >(ObservableValue<T> left, ObservableValue<T> right) => left is not null && left.CompareTo(right) > 0;

    /// <inheritdoc/>
    public static bool operator >=(ObservableValue<T> left, ObservableValue<T> right) => left is null ? right is null : left.CompareTo(right) >= 0;
    #endregion
}

/// <summary>
/// Provides data for the <see cref="ObservableValue{T}.ValueRead"/> event.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class ValueReadEventArgs<T> : EventArgs
{
    /// <summary>
    /// The value the reader received.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Instantiates new <see cref="ValueReadEventArgs{T}"/> with the given value.
    /// </summary>
    /// <param name="value">The value the reader received.</param>
    public ValueReadEventArgs(T value)
    {
        Value = value;
    }
}

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
