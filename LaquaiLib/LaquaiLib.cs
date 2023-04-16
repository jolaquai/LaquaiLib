namespace LaquaiLib;

public class LaquaiLib
{
    /// <summary>
    /// Compiles an <see cref="IEnumerable{T}"/> of <see cref="double"/>s from <paramref name="start"/> to <paramref name="stop"/> with a step width of <paramref name="step"/>.
    /// </summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="stop">The end of the range.</param>
    /// <param name="step">The step width of the range.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="double"/> as described.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> is 0.</exception>
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
    /// Determines if all passed <paramref name="objects"/> are equal to each other.
    /// </summary>
    /// <param name="objects">The objects to compare.</param>
    /// <returns>A value indicating whether all passed <paramref name="objects"/> are equal to each other.</returns>
    public static bool Equal(params object[] objects) => objects.Length >= 2 && objects.Skip(1).All(obj => obj.Equals(objects.First()));
}
