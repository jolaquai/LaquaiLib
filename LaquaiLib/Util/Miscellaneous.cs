using System.Collections;
using System.Diagnostics;
using System.Numerics;

namespace LaquaiLib.Util;

/// <summary>
/// Contains miscellaneous functionality.
/// </summary>
public static class Miscellaneous
{
    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s from <c>0</c> to <paramref name="stop"/> with a step width of <paramref name="step"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items in the created sequence.</typeparam>
    /// <param name="stop">The end of the range.</param>
    /// <param name="step">The step width of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/> as described.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> evaluates to a value equivalent to <c>0</c>.</exception>
    public static IEnumerable<T> Range<T>(T stop, T step) where T : ISignedNumber<T>, IComparisonOperators<T, T, bool>
        => Range(default, stop, step);
    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s from <c>0</c> to <paramref name="stop"/> - 1 with a step width of <c>1</c>.
    /// </summary>
    /// <typeparam name="T">The type of the items in the created sequence.</typeparam>
    /// <param name="stop">The end of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/> as described.</returns>
    public static IEnumerable<T> Range<T>(T stop) where T : ISignedNumber<T>, IComparisonOperators<T, T, bool>
        => Range(default, stop, (T)Convert.ChangeType(1, typeof(T)));
    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s from <paramref name="start"/> to <paramref name="stop"/> with a step width of <paramref name="step"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items in the created sequence.</typeparam>
    /// <param name="start">The inclusive start of the range.</param>
    /// <param name="stop">The exclusive end of the range.</param>
    /// <param name="step">The step width of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/> as described.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> evaluates to a value equivalent to <c>0</c>.</exception>
    public static IEnumerable<T> Range<T>(T start, T stop, T step) where T : ISignedNumber<T>, IComparisonOperators<T, T, bool>
         => new RangeIterable<T>(start, stop, step);
}

[method: StackTraceHidden]
internal class RangeIterable<T>(T start, T stop, T step) : IEnumerable<T> where T : ISignedNumber<T>, IComparisonOperators<T, T, bool>
{
    private readonly T _start = start;
    private readonly T _stop = stop;
    private readonly T _step = step;

    [StackTraceHidden]
    public RangeIterable(T stop, T step) : this(default, stop, step) { }
    [StackTraceHidden]
    public RangeIterable(T stop) : this(default, stop, T.One) { }

    public IEnumerator<T> GetEnumerator()
    {
        var current = _start;
        yield return current;
        while (current + _step is var next && next <= _stop)
        {
            yield return current = next;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
