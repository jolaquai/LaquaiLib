using LaquaiLib.Extensions;

namespace LaquaiLib;

/// <summary>
/// Contains methods for various mathematical operations.
/// </summary>
public static class RandomMath
{
    /// <summary>
    /// Calculates the sum of a series of output values of a function.
    /// </summary>
    /// <param name="x">The first input value to the function.</param>
    /// <param name="n">The last input value to the function.</param>
    /// <param name="fn">The function that calculates the output values given the input values.</param>
    /// <returns>The sum of the values returned by <paramref name="fn"/> for each input value between <paramref name="x"/> and <paramref name="n"/>.</returns>
    public static double Sum(double x, double n, Func<double, double> fn) => LaquaiLib.Range(x, n, 1).Select(fn).Sum();
    /// <summary>
    /// Calculates the product of a series of output values of a function.
    /// </summary>
    /// <param name="x">The first input value to the function.</param>
    /// <param name="n">The last input value to the function.</param>
    /// <param name="fn">The function that calculates the output values given the input values.</param>
    /// <returns>The product of the values returned by <paramref name="fn"/> for each input value between <paramref name="x"/> and <paramref name="n"/>.</returns>
    public static double Product(double x, double n, Func<double, double> fn) => LaquaiLib.Range(x, n, 1).Select(fn).Aggregate(1d, (seed, res) => seed *= res);

    /// <summary>
    /// Determines the greatest common divisor of a series of numbers.
    /// </summary>
    /// <param name="numbers">The numbers to determine the GCD of.</param>
    /// <returns>The GCD of the given <paramref name="numbers"/>.</returns>
    public static int GCD(params int[] numbers)
    {
        if (numbers.Length == 1)
        {
            return numbers[0];
        }
        var _numbers = numbers.Select(Math.Abs).ToList();
        if (_numbers.Any(n => n == 1))
        {
            return 1;
        }

        foreach (var g in LaquaiLib
            .Range(_numbers.Max(), 2)
            .Where(g => _numbers.Select(n => n % g == 0).All()))
        {
            return g;
        }

        return 1;
    }

    /// <summary>
    /// Rounds a <paramref name="number"/> to the nearest <paramref name="multiple"/> of a given number.
    /// </summary>
    /// <param name="number">The number to round.</param>
    /// <param name="multiple">The number a multiple of which <paramref name="number"/> is to be rounded to.</param>
    /// <returns><paramref name="number"/> rounded to a multiple of <paramref name="multiple"/>.</returns>
    public static double RoundToMultiple(double number, double multiple = 1) => Math.Round(number * multiple) / multiple;

    /// <summary>
    /// Smooths two functions over a given interval using a custom smoothing function.
    /// </summary>
    /// <param name="f">The first function to use when constructing the output function.</param>
    /// <param name="g">The second function to use when constructing the output function.</param>
    /// <param name="smoothFunc">The custom smoothing function to use.</param>
    /// <param name="xStart">The start of the interval over which to smooth <paramref name="f"/> into <paramref name="g"/>.</param>
    /// <param name="xEnd">The end of the interval over which to smooth <paramref name="f"/> into <paramref name="g"/>.</param>
    /// <returns>A function that returns the result of <paramref name="f"/> when the input parameter is less than <paramref name="xStart"/>, the result of <paramref name="g"/> when the input parameter is greater than <paramref name="xEnd"/> and the result of <paramref name="smoothFunc"/> that combines the results of <paramref name="f"/> and <paramref name="g"/> otherwise.</returns>
    /// <exception cref="ArgumentException"><paramref name="xStart"/> was greater than <paramref name="xEnd"/>.</exception>
    public static Func<double, double> InterpolateLinear(Func<double, double> f, Func<double, double> g, Func<double, double> smoothFunc, double xStart = 0, double xEnd = 1)
    {
        if (xEnd < xStart)
        {
            throw new ArgumentException("Smoothing end value must be greater than start value.", nameof(xEnd));
        }

        return p => (smoothFunc(p) * f(p)) + (smoothFunc(xEnd - p) + g(p));
    }

    /// <summary>
    /// Smooths two functions over a given interval.
    /// </summary>
    /// <param name="f">The first function to use when constructing the output function.</param>
    /// <param name="g">The second function to use when constructing the output function.</param>
    /// <param name="xStart">The start of the interval over which to smooth <paramref name="f"/> into <paramref name="g"/>.</param>
    /// <param name="xEnd">The end of the interval over which to smooth <paramref name="f"/> into <paramref name="g"/>.</param>
    /// <returns>A function that returns the result of <paramref name="f"/> when the input parameter is less than <paramref name="xStart"/>, the result of <paramref name="g"/> when the input parameter is greater than <paramref name="xEnd"/> and the result of a smoothing function that combines the results of <paramref name="f"/> and <paramref name="g"/> otherwise.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Func<double, double> InterpolateLinear(Func<double, double> f, Func<double, double> g, double xStart = 0, double xEnd = 1) => InterpolateLinear(f, g, x => x > xEnd ? 1 : (x < xStart ? 0 : Math.Pow(x - xStart, 2) / (Math.Pow(x - xStart, 2) + Math.Pow(xEnd - x, 2))), xStart, xEnd);

    /// <summary>
    /// Contains methods that use trigonometric functions.
    /// </summary>
    public static class Trigonometry
    {
        /// <summary>
        /// Returns the <c>sin</c> and <c>cos</c> functions that, together, trace an ellipse with specified sine and cosine radii and a given smoothness around a point.
        /// </summary>
        /// <param name="x">The <c>x</c>-coordinate of the point to trace the ellipse around.</param>
        /// <param name="y">The <c>y</c>-coordinate of the point to trace the ellipse around.</param>
        /// <param name="rSin">The "horizontal" radius of the ellipse. If equal to <paramref name="rCos"/>, the ellipse is a circle. In that case, use <see cref="EllipseAround(double, double, double, double)"/> instead.</param>
        /// <param name="rCos">The "vertical" radius of the ellipse. If equal to <paramref name="rSin"/>, the ellipse is a circle. In that case, use <see cref="EllipseAround(double, double, double, double)"/> instead.</param>
        /// <param name="resolution">How many degrees / points constitute a full rotation around the circle.</param>
        /// <returns>A <see cref="Tuple{T1, T2}"/> with the <c>Sin</c> and <c>Cos</c> functions that, together, trace an ellipse with the specified radii <paramref name="rSin"/> and <paramref name="rCos"/> and <paramref name="resolution"/> around the point <c>(<paramref name="x"/>, <paramref name="y"/>)</c>.</returns>
        public static (Func<double, double> Sin, Func<double, double> Cos) EllipseAround(double x, double y, double rSin, double rCos, double resolution = 360) => (new Func<double, double>(d => rSin * Math.Sin(d / resolution * (2 * Math.PI)) + x), new Func<double, double>(d => rCos * -Math.Cos(d / resolution * (2 * Math.PI)) + y));

        /// <summary>
        /// Returns the <c>sin</c> and <c>cos</c> functions that, together, trace a circle with specified radius and a given smoothness around a point.
        /// </summary>
        /// <param name="x">The <c>x</c>-coordinate of the point to trace the circle around.</param>
        /// <param name="y">The <c>y</c>-coordinate of the point to trace the circle around.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="resolution">How many degrees / points constitute a full rotation around the circle.</param>
        /// <returns>A <see cref="Tuple{T1, T2}"/> with the <c>Sin</c> and <c>Cos</c> functions that, together, trace an circle with the specified <paramref name="radius"/> and <paramref name="resolution"/> around the point <c>(<paramref name="x"/>, <paramref name="y"/>)</c>.</returns>
        public static (Func<double, double> Sin, Func<double, double> Cos) EllipseAround(double x, double y, double radius, double resolution = 360) => EllipseAround(x, y, radius, radius, resolution);
    }
}
