namespace LaquaiLib.Extensions.Memory.Linq;

public static partial class LinqMemoryExtensions
{
    extension(ReadOnlySpan<sbyte> source)
    {
        /// <summary>
        /// Calculates the average of a <see cref="ReadOnlySpan{T}"/> of <see langword="sbyte"/>s.
        /// </summary>
        /// <returns>The average of the <see cref="ReadOnlySpan{T}"/> of <see langword="sbyte"/>s.</returns>
        public double Average()
        {
            if (source.Length == 0)
            {
                return 0;
            }
            double sum = Sum(source);
            return sum / source.Length;
        }
        /// <summary>
        /// Finds the maximum value in a <see cref="ReadOnlySpan{T}"/> of <see langword="sbyte"/>s.
        /// </summary>
        /// <returns>The maximum value in the <see cref="ReadOnlySpan{T}"/> of <see langword="sbyte"/>s.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="ReadOnlySpan{T}"/> is empty.</exception>
        public sbyte Max()
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = source[0];
            for (var i = 1; i < source.Length; i++)
            {
                var value = source[i];
                if (value > max)
                {
                    max = value;
                }
            }
            return max;
        }
        /// <summary>
        /// Finds the minimum value in a <see cref="ReadOnlySpan{T}"/> of <see langword="sbyte"/>s.
        /// </summary>
        /// <returns>The minimum value in the <see cref="ReadOnlySpan{T}"/> of <see langword="sbyte"/>s.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="ReadOnlySpan{T}"/> is empty.</exception>
        public sbyte Min()
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = source[0];
            for (var i = 1; i < source.Length; i++)
            {
                var value = source[i];
                if (value < min)
                {
                    min = value;
                }
            }
            return min;
        }
        /// <summary>
        /// Calculates the sum of a <see cref="ReadOnlySpan{T}"/> of <see langword="sbyte"/>s.
        /// </summary>
        /// <returns>The sum of the <see cref="ReadOnlySpan{T}"/> of <see langword="sbyte"/>s.</returns>
        /// <exception cref="OverflowException">Thrown when the calculation would overflow.</exception>
        public sbyte Sum()
        {
            if (source.Length == 0)
            {
                return 0;
            }
            int sum = default;
            for (var i = 0; i < source.Length; i++)
            {
                sum += source[i];
                if (sum is < sbyte.MinValue or > sbyte.MaxValue)
                {
                    throw new OverflowException();
                }
            }
            return (sbyte)sum;
        }
    }
}