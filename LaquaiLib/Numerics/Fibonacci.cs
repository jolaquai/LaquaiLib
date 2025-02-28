using System.Diagnostics;
using System.Numerics;

namespace LaquaiLib.Numerics;

/// <summary>
/// A comprehensive Fibonacci calculation library that efficiently handles both one-off calculations and sequences of consecutive Fibonacci numbers.
/// </summary>
public static class Fibonacci
{
    /// <summary>
    /// Calculates the <paramref name="n"/>th Fibonacci number.
    /// </summary>
    /// <param name="n">The position in the Fibonacci sequence (zero-based).</param>
    /// <returns>The <paramref name="n"/>th Fibonacci number.</returns>
    public static BigInteger GetNth(ulong n)
    {
        if (n <= 1)
        {
            return n;
        }

        return MatrixPower(n - 1)[0, 0];
    }

    /// <summary>
    /// Generates a sequence of consecutive Fibonacci numbers from start to end (inclusive).
    /// Optimized for calculating multiple sequential values with O(end-start) time complexity.
    /// </summary>
    /// <param name="start">The starting position (zero-based, inclusive)</param>
    /// <param name="end">The ending position (zero-based, inclusive)</param>
    /// <returns>An enumerable of consecutive Fibonacci numbers</returns>
    /// <exception cref="ArgumentException">Thrown when end is less than start</exception>
    public static IEnumerable<BigInteger> GenerateSequence(ulong start, ulong end)
    {
        if (end < start)
        {
            throw new ArgumentException("End position must be greater than or equal to start position", nameof(end));
        }

        // For very short sequences or sequences starting from 0/1, 
        // just use the iterative approach from the beginning
        if (end - start < 5 || start <= 1)
        {
            foreach (var fib in IterativeSequence(start, end))
            {
                yield return fib;
            }
            yield break;
        }

        // For longer sequences starting at higher positions,
        // calculate the first two values using the fast matrix method,
        // then generate the rest iteratively
        var a = GetNth(start);
        var b = GetNth(start + 1);

        yield return a;

        if (start == end)
        {
            yield break;
        }

        yield return b;

        for (var i = start + 2; i <= end; i++)
        {
            var c = a + b;
            yield return c;
            a = b;
            b = c;
        }
    }

    /// <summary>
    /// Helper method that generates Fibonacci sequence using the simple iterative approach.
    /// </summary>
    private static IEnumerable<BigInteger> IterativeSequence(ulong start, ulong end)
    {
        // Calculate first two Fibonacci numbers
        BigInteger a = 0;
        BigInteger b = 1;

        // Skip values before the start position
        for (var i = 0ul; i < start; i++)
        {
            var c = a + b;
            a = b;
            b = c;
        }

        // Generate all values from start to end
        for (var i = start; i <= end; i++)
        {
            if (i == 0)
            {
                yield return 0;
                continue;
            }

            if (i == 1)
            {
                yield return 1;
                continue;
            }

            var c = a + b;
            yield return c;
            a = b;
            b = c;
        }
    }

    /// <summary>
    /// Uses fast matrix exponentiation to compute Fibonacci numbers.
    /// Based on the matrix identity:
    /// [1 1]^n = [F(n+1) F(n)  ]
    /// [1 0]     [F(n)   F(n-1)]
    /// </summary>
    private static BigInteger[,] MatrixPower(ulong power)
    {
        BigInteger[,] result = { { 1, 0 }, { 0, 1 } }; // Identity matrix
        BigInteger[,] matrix = { { 1, 1 }, { 1, 0 } }; // Fibonacci matrix

        while (power > 0)
        {
            // If the current bit is 1, multiply the result by the current power of the matrix
            if ((power & 1) == 1)
            {
                result = MultiplyMatrix(result, matrix);
            }

            // Square the matrix for the next bit
            matrix = MultiplyMatrix(matrix, matrix);

            // Move to the next bit
            power >>= 1;
        }

        return result;
    }

    /// <summary>
    /// Multiplies two 2x2 matrices efficiently.
    /// </summary>
    private static BigInteger[,] MultiplyMatrix(BigInteger[,] a, BigInteger[,] b)
    {
        Debug.Assert(a.GetLength(0) == 2 && a.GetLength(1) == 2);
        Debug.Assert(b.GetLength(0) == 2 && b.GetLength(1) == 2);

        return new BigInteger[,]
        {
            {
                (a[0, 0] * b[0, 0]) + (a[0, 1] * b[1, 0]),
                (a[0, 0] * b[0, 1]) + (a[0, 1] * b[1, 1])
            },
            {
                (a[1, 0] * b[0, 0]) + (a[1, 1] * b[1, 0]),
                (a[1, 0] * b[0, 1]) + (a[1, 1] * b[1, 1])
            }
        };
    }
}
