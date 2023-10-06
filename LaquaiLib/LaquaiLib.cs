using System.Numerics;

namespace LaquaiLib;

/// <summary>
/// Entry point for the library.
/// </summary>
public class LaquaiLib
{
    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s from <paramref name="start"/> to <paramref name="stop"/> with a step width of <paramref name="step"/>.
    /// To use this method:
    /// <list type="bullet">
    /// <item/><typeparamref name="T"/> must be a <see langword="struct"/>.
    /// <item/><typeparamref name="T"/> must implement: <c>IAdditionOperators&lt;T, T, T&gt;, IUnaryNegationOperators&lt;T, T&gt;, ISubtractionOperators&lt;T, T, T&gt;, IComparisonOperators&lt;T, T, bool&gt;</c>.
    /// <item/>The <see langword="default"/> of <typeparamref name="T"/> should represent a value equivalent to <c>0</c>.
    /// </list>
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="stop">The end of the range.</param>
    /// <param name="step">The step width of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/> as described.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> evaluates to a value equivalent to <c>0</c>.</exception>
    public static IEnumerable<T> Range<T>(T start, T stop, T step)
        where T : struct, 
                  IAdditionOperators<T, T, T>,
                  IUnaryNegationOperators<T, T>,
                  ISubtractionOperators<T, T, T>,
                  IComparisonOperators<T, T, bool>
    {
        var zero = default(T);
        if (step == zero)
        {
            throw new ArgumentException($"{nameof(step)} cannot be 0.", nameof(step));
        }

        if (start == stop)
        {
            yield return start;
            yield break;
        }
        if ((start > stop && step > zero) || (start < stop && step < zero))
        {
            step = -step;
        }

        var current = start - step;
        while (step > zero ? current + step <= stop : current + step >= stop)
        {
            yield return current += step;
        }
    }

    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <see cref="int"/>s from <paramref name="start"/> to <paramref name="stop"/> with a step width of <paramref name="step"/>.
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="stop">The end of the range.</param>
    /// <param name="step">The step width of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="int"/> as described.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> is <c>0</c>.</exception>
    public static IEnumerable<int> Range(int start, int stop, int step = 1)
    {
        if (step == 0)
        {
            throw new ArgumentException($"{nameof(step)} cannot be 0.", nameof(step));
        }

        if (start == stop)
        {
            yield return start;
            yield break;
        }
        if ((start > stop && step > 0) || (start < stop && step < 0))
        {
            step = -step;
        }

        var current = start - step;
        while (step > 0 ? current + step <= stop : current + step >= stop)
        {
            yield return current += step;
        }
    }

    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <see cref="double"/>s from <paramref name="start"/> to <paramref name="stop"/> with a step width of <paramref name="step"/>.
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="stop">The end of the range.</param>
    /// <param name="step">The step width of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="double"/> as described.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> is <c>0</c>.</exception>
    public static IEnumerable<double> Range(double start, double stop, double step = 1d)
    {
        if (step == 0)
        {
            throw new ArgumentException($"{nameof(step)} cannot be 0.", nameof(step));
        }

        if (start == stop)
        {
            yield return start;
            yield break;
        }
        if ((start > stop && step > 0) || (start < stop && step < 0))
        {
            step = -step;
        }

        var current = start - step;
        while (step > 0 ? current + step <= stop : current + step >= stop)
        {
            yield return current += step;
        }
    }

    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <see cref="decimal"/>s from <paramref name="start"/> to <paramref name="stop"/> with a step width of <paramref name="step"/>.
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="stop">The end of the range.</param>
    /// <param name="step">The step width of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="decimal"/> as described.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> is <c>0</c>.</exception>
    public static IEnumerable<decimal> Range(decimal start, decimal stop, decimal step = 1M)
    {
        if (step == 0)
        {
            throw new ArgumentException($"{nameof(step)} cannot be 0.", nameof(step));
        }

        if (start == stop)
        {
            yield return start;
            yield break;
        }
        if ((start > stop && step > 0) || (start < stop && step < 0))
        {
            step = -step;
        }

        var current = start - step;
        while (step > 0 ? current + step <= stop : current + step >= stop)
        {
            yield return current += step;
        }
    }
}
