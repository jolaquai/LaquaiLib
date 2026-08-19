using System.Numerics;

namespace LaquaiLib.Util;

/// <summary>
/// Contains factory methods for creating sequences of numbers.
/// </summary>
public static class Sequence
{
    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s from <c>0</c> to <paramref name="stop"/> with a step width of <paramref name="step"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items in the created sequence.</typeparam>
    /// <param name="stop">The end of the range.</param>
    /// <param name="step">The step width of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/> as described.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> evaluates to a value equivalent to <c>0</c>.</exception>
    public static IEnumerable<T> Create<T>(T stop, T step) where T : ISignedNumber<T>, IComparisonOperators<T, T, bool>
        => Create(default, stop, step);
    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s from <c>0</c> to <paramref name="stop"/> - 1 with a step width of <c>1</c>.
    /// </summary>
    /// <typeparam name="T">The type of the items in the created sequence.</typeparam>
    /// <param name="stop">The end of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/> as described.</returns>
    public static IEnumerable<T> Create<T>(T stop) where T : ISignedNumber<T>, IComparisonOperators<T, T, bool>
        => Create(default, stop, T.One);
    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s from <paramref name="start"/> to <paramref name="stop"/> with a step width of <paramref name="step"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items in the created sequence.</typeparam>
    /// <param name="start">The inclusive start of the range.</param>
    /// <param name="stop">The exclusive end of the range.</param>
    /// <param name="step">The step width of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/> as described.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> evaluates to a value equivalent to <c>0</c>.</exception>
    public static IEnumerable<T> Create<T>(T start, T stop, T step) where T : ISignedNumber<T>, IComparisonOperators<T, T, bool>
    {
        if (step == T.Zero)
            throw new ArgumentException("The step width must not be zero.", nameof(step));
        return Iterator(start, stop, step);

        static IEnumerable<T> Iterator(T start, T stop, T step)
        {
            var current = start;
            yield return current;
            // The termination condition depends on the direction of iteration: ascending steps stop once
            // the next value passes above stop, descending steps once it passes below.
            if (step > T.Zero)
                while (current + step is var next && next <= stop)
                    yield return current = next;
            else
                while (current + step is var next && next >= stop)
                    yield return current = next;
        }
    }
}
