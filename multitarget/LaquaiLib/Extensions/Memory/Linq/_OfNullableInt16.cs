namespace LaquaiLib.Extensions.Memory.Linq;

public static partial class LinqMemoryExtensions
{
    extension(ReadOnlySpan<short?> source)
    {
        /// <summary>
        /// Calculates the average of a <see cref="ReadOnlySpan{T}"/> of nullable <see langword="short"/>s.
        /// </summary>
        /// <returns>The average of the <see cref="ReadOnlySpan{T}"/> of nullable <see langword="short"/>s.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Average()
        {
            double? sum = Sum(source);
            return sum.HasValue ? sum.Value / source.Length : null;
        }
        /// <summary>
        /// Finds the maximum value in a <see cref="ReadOnlySpan{T}"/> of nullable <see langword="short"/>s.
        /// </summary>
        /// <returns>The maximum value in the <see cref="ReadOnlySpan{T}"/> of nullable <see langword="short"/>s.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="ReadOnlySpan{T}"/> is empty.</exception>
        public short? Max()
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = source[0];
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                {
                    max = value;
                }
            }
            return max;
        }
        /// <summary>
        /// Finds the minimum value in a <see cref="ReadOnlySpan{T}"/> of nullable <see langword="short"/>s.
        /// </summary>
        /// <returns>The minimum value in the <see cref="ReadOnlySpan{T}"/> of nullable <see langword="short"/>s.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="ReadOnlySpan{T}"/> is empty.</exception>
        public short? Min()
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = source[0];
            for (var i = 1; i < source.Length; i++)
            {
                var value = source[i];
                if (value.HasValue && (!min.HasValue || value.Value < min.Value))
                {
                    min = value;
                }
            }
            return min;
        }
        /// <summary>
        /// Calculates the sum of a <see cref="ReadOnlySpan{T}"/> of nullable <see langword="short"/>s.
        /// </summary>
        /// <returns>The sum of the <see cref="ReadOnlySpan{T}"/> of nullable <see langword="short"/>s.</returns>
        /// <exception cref="OverflowException">Thrown when the calculation would overflow.</exception>
        public short? Sum()
        {
            int? buf = 0;
            var allNull = true;
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                if (value.HasValue)
                {
                    allNull = false;
                    buf += value.Value;
                    if (buf is < short.MinValue or > short.MaxValue)
                    {
                        throw new OverflowException();
                    }
                }
            }
            return (short?)(allNull ? null : buf);
        }
    }
}