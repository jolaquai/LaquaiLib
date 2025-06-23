namespace LaquaiLib.Extensions.Memory.Linq;

public static partial class LinqMemoryExtensions
{
    extension(ReadOnlySpan<ulong> source)
    {
        /// <summary>
        /// Calculates the average of a <see cref="ReadOnlySpan{T}"/> of <see langword="ulong"/>s.
        /// </summary>
        /// <returns>The average of the <see cref="ReadOnlySpan{T}"/> of <see langword="ulong"/>s.</returns>
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
        /// Finds the maximum value in a <see cref="ReadOnlySpan{T}"/> of <see langword="ulong"/>s.
        /// </summary>
        /// <returns>The maximum value in the <see cref="ReadOnlySpan{T}"/> of <see langword="ulong"/>s.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="ReadOnlySpan{T}"/> is empty.</exception>
        public ulong Max()
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
        /// Finds the minimum value in a <see cref="ReadOnlySpan{T}"/> of <see langword="ulong"/>s.
        /// </summary>
        /// <returns>The minimum value in the <see cref="ReadOnlySpan{T}"/> of <see langword="ulong"/>s.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="ReadOnlySpan{T}"/> is empty.</exception>
        public ulong Min()
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
        /// Calculates the sum of a <see cref="ReadOnlySpan{T}"/> of <see langword="ulong"/>s.
        /// </summary>
        /// <returns>The sum of the <see cref="ReadOnlySpan{T}"/> of <see langword="ulong"/>s.</returns>
        /// <exception cref="OverflowException">Thrown when the calculation would overflow.</exception>
        public ulong Sum()
        {
            if (source.Length == 0)
            {
                return 0;
            }
            ulong sum = default;
            for (var i = 0; i < source.Length; i++)
            {
                sum = checked(sum + source[i]);
            }
            return sum;
        }
    }
}