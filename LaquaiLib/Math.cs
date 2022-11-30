using System.Collections;

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

    public static double RoundToMultiple(double n, double m) => System.Math.Round(n / m) * m;

    public static Func<double, double> SmoothFunctions(Func<double, double> f, Func<double, double> g, double xStart, double xEnd)
    {
        double smoothf(double x)
        {
            if (x > xEnd)
            {
                return 1;
            }
            else if (x < xStart)
            {
                return 0;
            }
            else
            {
                return System.Math.Pow(x - xStart, 2) / (System.Math.Pow(x - xStart, 2) + System.Math.Pow(xEnd - x, 2));
            }
        }

        return new Func<double, double>(p => (smoothf(p) * f(p)) + (smoothf(xEnd - p) + g(p)));
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

    public static class VectorGeometry
    {
        public class Vector : IEnumerable, IEnumerator
        {
            public int Dimension { get; }

            private List<double> Coordinates { get; set; } = new();
            public double this[int i]
            {
                get => Coordinates[i];
                set => Coordinates[i] = value;
            }

            private int _current = -1;
            public object Current => Coordinates[_current];

            public Vector(params double[] args)
            {
                if (args.Length < 2)
                {
                    throw new ArgumentException($"Cannot construct {typeof(Vector)} from {args.Length} parameters, expected 2 or more.");
                }

                foreach (double co in args)
                {
                    Coordinates.Add(co);
                }
            }

            /// <summary>
            /// Scalar product
            /// </summary>
            public static double operator *(Vector a, Vector b)
            {
                int hDim = new List<int>() { a.Dimension, b.Dimension }.Max();

                return Sum(
                    1,
                    hDim,
                    i => (a.Dimension < (int)i ? a[(int)i] : 0) * (b.Dimension < (int)i ? b[(int)i] : 0)
                );
            }

            public static Vector operator *(Vector a, double n) => new(a.Coordinates.Select(co => co * n).ToArray());

            /// <summary>
            /// Vector product
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Vector operator ^(Vector a, Vector b)
            {
                if (a.Dimension == 1)
                {
                    a = new(a[0], 0, 0);
                }
                else if (a.Dimension == 2)
                {
                    a = new(a[0], a[1], 0);
                }
                else if (a.Dimension > 3)
                {
                    throw new ArgumentException($"Invalid {typeof(Vector)} {nameof(a)}. Vector products are only defined in three-dimensional space.", nameof(a));
                }

                if (a.Dimension == 1)
                {
                    b = new(b[0], 0, 0);
                }
                else if (a.Dimension == 2)
                {
                    b = new(b[0], b[1], 0);
                }
                else if (a.Dimension > 3)
                {
                    throw new ArgumentException($"Invalid {typeof(Vector)} {nameof(b)}. Vector products are only defined in three-dimensional space.", nameof(b));
                }

                return new(
                    (a[2] * b[1]) - (a[3] * b[2]),
                    (a[3] * b[1]) - (a[1] * b[3]),
                    (a[1] * b[2]) - (a[2] * b[1])
                );
            }

            public static Vector operator +(Vector a, Vector b)
            {
                List<double> newcoords = new();
                for (int i = 1; i <= new List<Vector>() { a, b }.Select(v => v.Dimension).Max(); i++)
                {
                    newcoords.Add((a.Dimension < i ? a[i] : 0) + (b.Dimension < i ? b[i] : 0));
                }
                return new(newcoords.ToArray());
            }
            
            public static Vector operator -(Vector a)
            {
                List<double> newcoords = new();
                for (int i = 0; i < a.Dimension; i++)
                {
                    newcoords.Add(-a[i]);
                }
                return new(newcoords.ToArray());
            }

            public static Vector operator -(Vector a, Vector b) => a + (-b);

            public static bool operator ==(Vector a, Vector b)
            {
                if (a.Dimension != b.Dimension)
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < a.Dimension; i++)
                    {
                        if (a[i] != b[i])
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            
            public static bool operator !=(Vector a, Vector b) => !(a == b);

            public double Abs() => Sum(1, Dimension, n => System.Math.Pow(this[(int)n], 2));

            public Vector Simplify() => new(Coordinates.Select(co => co / GCD(Coordinates.Select(x => (int)x).ToArray())).ToArray());

            public IEnumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                if (_current >= Dimension)
                {
                    _current++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset() => _current = -1;

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (obj is null)
                {
                    return false;
                }

                return this == (Vector)obj;
            }

            public override int GetHashCode() => HashCode.Combine(Dimension, Coordinates, _current, Current);
        }

        public static Vector NullVector(int d) => new(0d.Repeat(d).Select(obj => (double)obj).ToArray());

        public static double IntersectAngle(Vector dv1, Vector dv2)
        {
            if (dv1.Dimension != 3)
            {
                throw new ArgumentException($"Intersect angle of non-three-dimensional {typeof(Vector)}s.", nameof(dv1));
            }
            if (dv2.Dimension != 3)
            {
                throw new ArgumentException($"Intersect angle of non-three-dimensional {typeof(Vector)}s.", nameof(dv2));
            }

            dv1 = dv1.Simplify();
            dv2 = dv2.Simplify();
            return System.Math.Acos(System.Math.Abs(dv1 * dv2) / (dv1.Abs() * dv2.Abs()));
        }

        public static double IntersectAngleLinePlane(Vector dv, Vector pv1, Vector pv2) => IntersectAngleLinePlane(dv, pv1 ^ pv2);
        public static double IntersectAngleLinePlane(Vector dv, Vector nv)
        {
            if (dv.Dimension != 3)
            {
                throw new ArgumentException($"Intersect angle of non-three-dimensional {typeof(Vector)}s.", nameof(dv));
            }

            dv = dv.Simplify();
            nv = nv.Simplify();
            return (System.Math.PI / 2) - System.Math.Asin(System.Math.Abs(dv * nv) / (dv.Abs() * nv.Abs()));
        }

        public static Dictionary<string, Tuple<double, double, double>> PlaneIntersections(Vector pv, Vector dv)
        {
            if (pv.Dimension != 3)
            {
                throw new ArgumentException($"Intersect angle of non-three-dimensional {typeof(Vector)}s.", nameof(pv));
            }
            if (dv.Dimension != 3)
            {
                throw new ArgumentException($"Intersect angle of non-three-dimensional {typeof(Vector)}s.", nameof(dv));
            }

            Dictionary<string, Tuple<double, double, double>> intersects = new();

            // If a coordinate in the direction Vector is `0`, the line defined by the vectors cannot intersect with the plane that is missing that coordinate as the two run parallel to each other, i.e.
            // - `dv.v1 = 0` -> `line ∉ x2x3`
            // - `dv.v2 = 0` -> `line ∉ x1x3`
            // - `dv.v3 = 0` -> `line ∉ x1x2`
            List<int> possibleIntersections = new() { 1, 2, 3 };
            for (int i = 0; i < dv.Dimension; i++)
            {
                if (dv[i] == 0)
                {
                    possibleIntersections.Remove(i);
                }
            }

            // The entire idea of using a loop like this was absolutely disgusting to come up with, understand and then actually write
            // The actual solution, as in, the code, is    B E A U T I F U L, but that's about it
            // AAAAND it works so I reeeeaaaally don't care :3
            foreach (int searchx in possibleIntersections)
            {
                List<int> baseN = new() { 1, 2, 3 };
                baseN.Remove(searchx);

                double lambda = 0;
                double v = pv[searchx];
                if (v != 0)
                {
                    lambda += (v > 0 ? -v : v);
                }

                try
                {
                    lambda /= dv[searchx];
                }
                catch
                {
                    intersects[$"x{baseN[0]}x{baseN[1]}"] = null;
                }

                List<double> outlist = new() { 0d, 0d, 0d };
                outlist[baseN[0]] = pv[baseN[0]] + (dv[baseN[0]] * lambda);
                outlist[baseN[1]] = pv[baseN[1]] + (dv[baseN[1]] * lambda);

                intersects[$"x{baseN[0]}x{baseN[1]}"] = new(outlist[0], outlist[1], outlist[2]);
            }
            return intersects;
        }

        public static bool LinearDependence(params Vector[] vectors)
        {
            int hdim = vectors.Select(v => v.Dimension).Max();
            for (int i = 0; i < vectors.Length; i++)
            {
                if (vectors[i].Dimension < hdim)
                {
                    List<double> coords = new();
                    for (int j = 0; i < hdim; j++)
                    {
                        try
                        {
                            coords.Add(vectors[i][j]);
                        }
                        catch
                        {
                            coords.Add(0);
                        }
                    }
                    vectors[i] = new(coords.ToArray());
                }
            }

            foreach (Vector v in vectors)
            {
                if (v == NullVector(hdim))
                {
                    return true;
                }
            }

            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i].Simplify();
            }

            switch (vectors.Length)
            {
                case 2:
                    bool r = vectors[0] == vectors[1];
                    if (r)
                    {
                        return true;
                    }
                    else
                    {
                        return Miscellaneous.Equal(vectors[0][0] / vectors[1][0],
                                                   vectors[0][1] / vectors[1][1],
                                                   vectors[0][2] / vectors[1][2]);
                    }
                case 3:
                    // Triple product == 0 => linear dependence
                    return (vectors[0] * (vectors[1] ^ vectors[2])) == 0;
                    // Can also be determined using the determinant of a matrix created from the three vectors:
                    // return
                case 4:
                    return true;
                default:
                    throw new ArgumentException($"Cannot determine linear dependence of {vectors.Length} vectors.", nameof(vectors));
            }
        }
    }
}