using LaquaiLib.Extensions;

namespace LaquaiLib;

public static partial class Math
{
    public static double Sum(double x, double n, Func<double, double> fn) => LaquaiLib.Range(x, n, 1).Select(fn).Sum();
    public static double Product(double x, double n, Func<double, double> fn) => LaquaiLib.Range(x, n, 1).Select(fn).Aggregate(1d, (seed, res) => seed *= res);

    public static int GCD(params int[] nums)
    {
        if (nums.Length == 1)
        {
            return nums[0];
        }
        List<int> numbers = nums.Select(System.Math.Abs).ToList();
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
    public static double RoundToMultiple(double number, double multiple = 1) => System.Math.Round(number / multiple) * multiple;

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
    public static Func<double, double> SmoothFunctions(Func<double, double> f, Func<double, double> g, double xStart = 0, double xEnd = 1) => SmoothFunctions(f, g,x => x > xEnd ? 1 : (x < xStart ? 0 : System.Math.Pow(x - xStart, 2) / (System.Math.Pow(x - xStart, 2) + System.Math.Pow(xEnd - x, 2))), xStart, xEnd);

    /// <summary>
    /// Computes the factorial of any number using the <see cref="Gamma"/> function.
    /// </summary>
    /// <param name="x">The number to calculate the factorial of. May be non-integral.</param>
    /// <returns></returns>
    public static double Factorial(double x)
    {
        if ((int)x == x)
        {
            return (int)System.Math.Exp(Gamma.Log(x + 1));
        }

        return System.Math.Exp(Gamma.Log(x + 1));
    }

    public static class Trigonometry
    {
        public static (Func<double, double> Sin, Func<double, double> Cos) EllipseAround(double x, double y, double rSin, double rCos, double resolution) => (new Func<double, double>(d => rSin * System.Math.Sin((d / resolution) * (2 * System.Math.PI)) + x), new Func<double, double>(d => rCos * -System.Math.Cos((d / resolution) * (2 * System.Math.PI)) + y));

        public static (Func<double, double> Sin, Func<double, double> Cos) EllipseAround(double x, double y, double rSin, double resolution) => EllipseAround(x, y, rSin, rSin, resolution);

        public static (double X, double Y) PointInCircle(double x, double y, double r)
        {
            Random ran = new();
            double tX = x - r;
            double tY = y - r;

            double pX = 0;
            double pY = 0;
            double d = r + 1;

            while (System.Math.Abs(d) > r)
            {
                pX = tX + ran.Next(0, (int)(2 * r));
                pY = tY + ran.Next(0, (int)(2 * r));
                d = System.Math.Sqrt(System.Math.Pow(x - pX, 2) + System.Math.Pow(y - pY, 2));
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

            for (int i = 3; i <= System.Math.Sqrt(n); i += 2)
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
            IEnumerable<int> primes = PrimeFactors(n);

            var outcp = LaquaiLib.Range(1, n, 1).Select(x => (int)x).ToList();
            foreach (int pk in primes)
            {
                foreach (int searchx in LaquaiLib.Range(1, n - 1, 1).Select(x => (int)x))
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

        public static bool Equal(params object[] objects)
        {
            if (!objects.Any())
            {
                return false;
            }

            var first = objects.First();
            foreach (object obj in objects.Skip(1))
            {
                if (first != obj || !first.Equals(obj))
                {
                    return false;
                }
            }
            return true;
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
            double y = x;
            double t = (x + 5.5);
            t -= (x + 0.5) * System.Math.Log(t);
            double sum = 1.000000000190015;
            for (int i = 0; i < 6; i++)
            {
                sum += Coefficients[i] / ++y;
            }
            return -t + System.Math.Log(2.5066282746310005 * sum / x);
        }
    }
}