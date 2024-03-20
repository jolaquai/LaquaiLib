using System.ComponentModel;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a value that can be observed for and notifies its observers about changes.
/// </summary>
public class ObservableValue<T>
    : INotifyPropertyChanged, IEquatable<T>, IComparable<T>, IEquatable<ObservableValue<T>>, IComparable<ObservableValue<T>>
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
                RaisePropertyChanged(old, value);
            }
        }
    }

    /// <summary>
    /// The <see cref="IComparer{T}"/> to use when comparing instances of this type.
    /// </summary>
    public IComparer<T> Comparer { get; } = Comparer<T>.Default;

    /// <summary>
    /// Initializes an <see cref="ObservableValue{T}"/> of type <typeparamref name="T"/> and sets the value to the default for this type. At instantiation, no <see cref="PropertyChanged"/> event is raised.
    /// </summary>
    public ObservableValue()
    {
        value = default;
    }
    /// <summary>
    /// Initializes an <see cref="ObservableValue{T}"/> of type <typeparamref name="T"/> with the given initial <paramref name="value"/>. At instantiation, no <see cref="PropertyChanged"/> event is raised.
    /// </summary>
    /// <param name="value">The initial value.</param>
    public ObservableValue(T value)
    {
        this.value = value;
    }

    /// <summary>
    /// Occurs when the value changes.
    /// </summary>
    /// <remarks>
    /// Use this if you require access to the previous and the new value immediately upon change.
    /// If you need just the notification <i>that</i> the value changed, use <see cref="PropertyChanged"/> instead.
    /// </remarks>
    public event EventHandler<ValueChangedEventArgs<T>> ValueChanged;
    /// <summary>
    /// Occurs when the value is read.
    /// </summary>
    public event EventHandler<ValueReadEventArgs<T>> PropertyRead;
    /// <summary>
    /// Occurs when the value changes.
    /// </summary>
    /// <remarks>
    /// If you require access to the previous and the new value immediately upon change, use <see cref="ValueChanged"/> instead.
    /// </remarks>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>The new value.</returns>
    public T Set(T newValue) => Value = newValue;
    /// <summary>
    /// Sets the value without raising a <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    /// <returns>The new value.</returns>
    public T SetSilent(T newValue) => value = newValue;
    /// <summary>
    /// Determines if a value has been set.
    /// </summary>
    /// <returns><see langword="true"/> if a value has been set, otherwise <see langword="false"/>.</returns>
    /// <remarks>A value has been "set" if the current value is non-<see langword="null"/>. This also counts for primitive types.</remarks>
    public bool HasValue() => Value != null;
    /// <summary>
    /// Queries the value.
    /// </summary>
    /// <returns>The current value.</returns>
    public T Query() => Value;
    /// <summary>
    /// Queries the value without raising a <see cref="PropertyRead"/> event.
    /// </summary>
    /// <returns>The current value.</returns>
    public T QuerySilent() => value;

    /// <summary>
    /// Raises the <see cref="PropertyRead"/> event.
    /// </summary>
    /// <param name="value">The value that was read.</param>
    private void RaiseValueRead(T value) => PropertyRead?.Invoke(this, new ValueReadEventArgs<T>(value));
    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> and <see cref="ValueChanged"/> events.
    /// </summary>
    /// <param name="oldValue">The value before the change.</param>
    /// <param name="newValue">The value after the change.</param>
    private void RaisePropertyChanged(T oldValue, T newValue)
    {
        ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>(oldValue, newValue));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }

    #region Interface implementations
    /// <inheritdoc/>
    public int CompareTo(ObservableValue<T>? other) => Comparer.Compare(Value, other.Value);
    /// <inheritdoc/>
    public bool Equals(ObservableValue<T>? other) => Equals(Value, other.Value);
    /// <inheritdoc/>
    public int CompareTo(T? other) => Comparer.Compare(Value, other);
    /// <inheritdoc/>
    public bool Equals(T? other) => Equals(Value, other);

    /// <summary>
    /// Creates a copy of this <see cref="ObservableValue{T}"/> with the same initial value as this one's current value.
    /// </summary>
    /// <returns>The copy instance.</returns>
    public ObservableValue<T> Copy(bool deep = false)
    {
        if (deep)
        {
            // If deep copy is requested, create a new instance of ObservableValue with a deep copy of the value.
            var copy = new ObservableValue<T>();
            if (value is ICloneable cloneable)
            {
                copy.value = (T)cloneable.Clone();
            }
            else
            {
                throw new InvalidOperationException($"Type {typeof(T)} does not implement ICloneable.");
            }
            return copy;
        }
        else
        {
            // If shallow copy is requested, create a new instance of ObservableValue with the same value.
            return new ObservableValue<T>(value);
        }
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
    public override bool Equals(object? obj)
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
    public static bool operator >(ObservableValue<T> left, ObservableValue<T> right) => left?.CompareTo(right) > 0;

    /// <inheritdoc/>
    public static bool operator >=(ObservableValue<T> left, ObservableValue<T> right) => left is null ? right is null : left.CompareTo(right) >= 0;
    #endregion
}
