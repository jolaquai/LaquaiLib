using DocumentFormat.OpenXml.Bibliography;

using LaquaiLib.Extensions;

namespace LaquaiLib;

public static partial class RandomMath
{
    /// <summary>
    /// Calculates the sum of a series of output values of a function.
    /// </summary>
    /// <param name="x">The </param>
    /// <param name="n"></param>
    /// <param name="fn"></param>
    /// <returns></returns>
    public static double Sum(double x, double n, Func<double, double> fn) => LaquaiLib.Range(x, n, 1).Select(fn).Sum();
    public static double Product(double x, double n, Func<double, double> fn) => LaquaiLib.Range(x, n, 1).Select(fn).Aggregate(1d, (seed, res) => seed *= res);

    public static int GCD(params int[] nums)
    {
        if (nums.Length == 1)
        {
            return nums[0];
        }
        var numbers = nums.Select(Math.Abs).ToList();
        if (numbers.Any(n => n == 1))
        {
            return 1;
        }

        foreach (int g in LaquaiLib.Range(numbers.Max(), 2))
        {
            if (numbers.Select(n => n % g == 0).All())
            {
                return g;
            }
        }
        return 1;
    }

    /// <summary>
    /// Rounds a <paramref name="number"/> to the nearest multiple of a given number <paramref name="multiple"/>.
    /// </summary>
    /// <param name="number">The number to round.</param>
    /// <param name="multiple">The number a multiple of which <paramref name="number"/> is to be rounded to.</param>
    /// <returns><paramref name="number"/> rounded to a multiple of <paramref name="multiple"/>.</returns>
    public static double RoundToMultiple(double number, double multiple = 1) => Math.Round(number / multiple) * multiple;

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
    public static Func<double, double> SmoothFunctions(Func<double, double> f, Func<double, double> g, Func<double, double> smoothFunc, double xStart = 0, double xEnd = 1)
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
    public static Func<double, double> SmoothFunctions(Func<double, double> f, Func<double, double> g, double xStart = 0, double xEnd = 1) => SmoothFunctions(f, g,x => x > xEnd ? 1 : (x < xStart ? 0 : Math.Pow(x - xStart, 2) / (Math.Pow(x - xStart, 2) + Math.Pow(xEnd - x, 2))), xStart, xEnd);

    /// <summary>
    /// Computes the factorial of any number using the <see cref="Gamma"/> function.
    /// </summary>
    /// <param name="x">The number to calculate the factorial of. May be non-integral.</param>
    /// <returns></returns>
    public static double Factorial(double x)
    {
        if ((int)x == x)
        {
            return (int)Math.Exp(Gamma.Log(x + 1));
        }

        return Math.Exp(Gamma.Log(x + 1));
    }

    public static class Trigonometry
    {
        /// <summary>
        /// Returns the <c>sin</c> and <c>cos</c> functions that, together, trace an ellipse with specified sine and cosine radii and a given smoothness around a point.
        /// </summary>
        /// <param name="x">The <c>x</c>-coordinate of the point to trace the ellipse around.</param>
        /// <param name="y">The <c>y</c>-coordinate of the point to trace the ellipse around.</param>
        /// <param name="rSin">The "horizontal" radius of the ellipse. If equal to <paramref name="rCos"/>, the ellipse is a circle.</param>
        /// <param name="rCos">The "vertical" radius of the ellipse. If equal to <paramref name="rSin"/>, the ellipse is a circle.</param>
        /// <param name="resolution">How many degrees / points constitute a full rotation around the circle.</param>
        /// <returns>A <see cref="Tuple{T1, T2}"/> with the <c>Sin</c> and <c>Cos</c> functions that, together, trace an ellipse with the specified radii <paramref name="rSin"/> and <paramref name="rCos"/> and <paramref name="resolution"/> around the point <c>(<paramref name="x"/>, <paramref name="y"/>)</c>.</returns>
        public static (Func<double, double> Sin, Func<double, double> Cos) EllipseAround(double x, double y, double rSin, double rCos, double resolution = 360) => (new Func<double, double>(d => rSin * Math.Sin((d / resolution) * (2 * Math.PI)) + x), new Func<double, double>(d => rCos * -Math.Cos((d / resolution) * (2 * Math.PI)) + y));

        public static (Func<double, double> Sin, Func<double, double> Cos) EllipseAround(double x, double y, double radius, double resolution) => EllipseAround(x, y, radius, radius, resolution = 360);

        public static (double X, double Y) PointInCircle(double x, double y, double r)
        {
            Random ran = new();
            var tX = x - r;
            var tY = y - r;

            double pX = 0;
            double pY = 0;
            var d = r + 1;

            while (Math.Abs(d) > r)
            {
                pX = tX + ran.Next(0, (int)(2 * r));
                pY = tY + ran.Next(0, (int)(2 * r));
                d = Math.Sqrt(Math.Pow(x - pX, 2) + Math.Pow(y - pY, 2));
            }
            return (pX, pY);
        }
    }

    public static class Miscellaneous
    {
        public static IEnumerable<int> PrimeFactors(int n)
        {
            while (n % 2 == 0)
            {
                yield return 2;
                n /= 2;
            }

            for (var i = 3; i <= Math.Sqrt(n); i += 2)
            {
                while (n % i == 0)
                {
                    yield return i;
                    n /= i;
                }
            }

            if (n > 2)
            {
                yield return n;
            }
        }

        public static IEnumerable<int> Coprimes(int n)
        {
            var primes = PrimeFactors(n);

            var outcp = LaquaiLib.Range(1, n, 1).Select(x => (int)x).ToList();
            foreach (var pk in primes)
            {
                foreach (var searchx in LaquaiLib.Range(1, n - 1, 1).Select(x => (int)x))
                {
                    if (pk * searchx > n)
                    {
                        break;
                    }
                    outcp.RemoveAll(x => x == pk * searchx);
                }
            }
            return outcp.Select();
        }
    }

    public static class Geometry
    {

    }

    private static class Gamma
    {
        private static readonly double[] Coefficients = new double[]
        {
            76.18009172947146,
            -86.50532032941677,
            24.01409824083091,
            -1.231739572450155,
            0.1208650973866179e-2,
            -0.5395239384953e-5
        };

        public static double Log(double x)
        {
            if (x <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "x must be greater than 0.");
            }
            var y = x;
            var t = (x + 5.5);
            t -= (x + 0.5) * Math.Log(t);
            var sum = 1.000000000190015;
            for (var i = 0; i < 6; i++)
            {
                sum += Coefficients[i] / ++y;
            }
            return -t + Math.Log(2.5066282746310005 * sum / x);
        }
    }
}
