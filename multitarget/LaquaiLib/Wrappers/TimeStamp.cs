using System.Numerics;

namespace LaquaiLib.Wrappers;

/// <summary>
/// Represents an instant in time captured on the current machine via a high-resolution timer.
/// The value is not wall-clock time and is only meaningful within the same process lifetime.
/// Uses <see cref="Stopwatch"/> internally.
/// </summary>
public readonly struct TimeStamp() : IEquatable<TimeStamp>, IComparable<TimeStamp>,
    ISubtractionOperators<TimeStamp, TimeStamp, TimeSpan>
{
    /// <summary>
    /// Gets the value of the current timestamp in ticks.
    /// </summary>
    public readonly long Value = Stopwatch.GetTimestamp();

    /// <summary>
    /// Gets the elapsed time since this timestamp was captured.
    /// </summary>
    /// <returns>A <see cref="TimeSpan"/> representing the elapsed time since this timestamp.</returns>
    public readonly TimeSpan Elapsed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Stopwatch.GetElapsedTime(Value);
    }
    /// <summary>
    /// Gets the elapsed time in ticks since this timestamp was captured.
    /// </summary>
    /// <returns>The number of ticks elapsed since this timestamp.</returns>
    public readonly long ElapsedTicks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Stopwatch.GetTimestamp() - Value;
    }
    /// <summary>
    /// Gets the elapsed time in milliseconds since this timestamp was captured.
    /// </summary>
    /// <returns>The number of milliseconds elapsed since this timestamp.</returns>
    public readonly long ElapsedMilliseconds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ElapsedTicks * 1000 / Stopwatch.Frequency;
    }

    /// <summary>
    /// Gets a hash code for the current instance.
    /// </summary>
    /// <returns>The hash code for the current instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override int GetHashCode() => Value.GetHashCode();
    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the other object is a <see cref="TimeStamp"/> and has the same value; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Equals(object obj) => obj is TimeStamp other && Equals(other);
    /// <summary>
    /// Determines whether the specified <see cref="TimeStamp"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="TimeStamp"/> to compare with the current instance.</param>
    /// <returns>true if the other <see cref="TimeStamp"/> has the same value; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Equals(TimeStamp other) => Value == other.Value;
    /// <summary>
    /// Determines whether two <see cref="TimeStamp"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="TimeStamp"/> to compare.</param>
    /// <param name="right">The second <see cref="TimeStamp"/> to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="TimeStamp"/> instances have the same value; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(TimeStamp left, TimeStamp right) => left.Equals(right);
    /// <summary>
    /// Determines whether two <see cref="TimeStamp"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="TimeStamp"/> to compare.</param>
    /// <param name="right">The second <see cref="TimeStamp"/> to compare.</param>
    /// <returns><see langword="true"/> if the two <see cref="TimeStamp"/> instances do not have the same value; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(TimeStamp left, TimeStamp right) => !left.Equals(right);

    /// <summary>
    /// Computes an <see langword="int"/> that describes whether the current instance compares less than, equal to, or greater than the specified <see cref="TimeStamp"/>.
    /// </summary>
    /// <param name="other">The <see cref="TimeStamp"/> to compare with the current instance.</param>
    /// <returns>A negative value if the current instance is less than <paramref name="other"/>, zero if they are equal, and a positive value if the current instance is greater than <paramref name="other"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public int CompareTo(TimeStamp other) => Value.CompareTo(other.Value);
    /// <summary>
    /// Determines whether one <see cref="TimeStamp"/> is less than another.
    /// </summary>
    /// <param name="left">The first <see cref="TimeStamp"/> to compare.</param>
    /// <param name="right">The second <see cref="TimeStamp"/> to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator <(TimeStamp left, TimeStamp right) => left.Value < right.Value;
    /// <summary>
    /// Determines whether one <see cref="TimeStamp"/> is greater than another.
    /// </summary>
    /// <param name="left">The first <see cref="TimeStamp"/> to compare.</param>
    /// <param name="right">The second <see cref="TimeStamp"/> to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator >(TimeStamp left, TimeStamp right) => left.Value > right.Value;
    /// <summary>
    /// Determines whether one <see cref="TimeStamp"/> is less than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="TimeStamp"/> to compare.</param>
    /// <param name="right">The second <see cref="TimeStamp"/> to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator <=(TimeStamp left, TimeStamp right) => left.Value <= right.Value;
    /// <summary>
    /// Determines whether one <see cref="TimeStamp"/> is greater than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="TimeStamp"/> to compare.</param>
    /// <param name="right">The second <see cref="TimeStamp"/> to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator >=(TimeStamp left, TimeStamp right) => left.Value >= right.Value;

    /// <summary>
    /// Subtracts one <see cref="TimeStamp"/> from another, returning the elapsed time as a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="left">The first <see cref="TimeStamp"/>.</param>
    /// <param name="right">The second <see cref="TimeStamp"/> to subtract from the first.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the elapsed time between the two <see cref="TimeStamp"/> instances.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static TimeSpan operator -(TimeStamp left, TimeStamp right) => Stopwatch.GetElapsedTime(right.Value, left.Value);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override string ToString() => string.Concat("TimeStamp {", Value, '}');
}
