namespace LaquaiLib;

public class LaquaiLib
{
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

        double current = start - step;
        while (step > 0 ? current + step <= stop : current + step >= stop)
        {
            yield return current += step;
        }
    }
}
