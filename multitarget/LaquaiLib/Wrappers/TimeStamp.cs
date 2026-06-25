using System.Diagnostics;
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
    public readonly long Start = Stopwatch.GetTimestamp();

    /// <summary>
    /// Gets the elapsed time since this timestamp was captured.
    /// </summary>
    /// <returns>A <see cref="TimeSpan"/> representing the elapsed time since this timestamp.</returns>
    public readonly TimeSpan Elapsed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Stopwatch.GetElapsedTime(Start);
    }
    /// <summary>
    /// Gets the elapsed time in ticks since this timestamp was captured.
    /// </summary>
    /// <returns>The number of ticks elapsed since this timestamp.</returns>
    public readonly long ElapsedTicks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Stopwatch.GetTimestamp() - Start;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override int GetHashCode() => Start.GetHashCode();
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Equals(object obj) => obj is TimeStamp other && Equals(other);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Equals(TimeStamp other) => Start == other.Start;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(TimeStamp left, TimeStamp right) => left.Equals(right);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(TimeStamp left, TimeStamp right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)] public int CompareTo(TimeStamp other) => Start.CompareTo(other.Start);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator <(TimeStamp left, TimeStamp right) => left.Start < right.Start;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator >(TimeStamp left, TimeStamp right) => left.Start > right.Start;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator <=(TimeStamp left, TimeStamp right) => left.Start <= right.Start;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator >=(TimeStamp left, TimeStamp right) => left.Start >= right.Start;

    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override string ToString() => Start.ToString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static TimeSpan operator -(TimeStamp left, TimeStamp right) => Stopwatch.GetElapsedTime(right.Start, left.Start);
}
