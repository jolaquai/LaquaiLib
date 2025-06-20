namespace LaquaiLib.Extensions.MemoryExtensions.Linq;

public static partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Average(Func<TSource, int> selector)
        {
            if (source.Length == 0)
            {
                return 0;
            }
            double sum = Sum(source, selector);
            return sum / source.Length;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Average(Func<TSource, long> selector)
        {
            if (source.Length == 0)
            {
                return 0;
            }
            double sum = Sum(source, selector);
            return sum / source.Length;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Average(Func<TSource, float> selector)
        {
            if (source.Length == 0)
            {
                return 0;
            }
            var sum = Sum(source, selector);
            return sum / source.Length;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Average(Func<TSource, double> selector)
        {
            if (source.Length == 0)
            {
                return 0;
            }
            var sum = Sum(source, selector);
            return sum / source.Length;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Average(Func<TSource, decimal> selector)
        {
            if (source.Length == 0)
            {
                return 0;
            }
            var sum = Sum(source, selector);
            return sum / source.Length;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Average(Func<TSource, int?> selector)
        {
            double? sum = Sum(source, selector);
            return sum.HasValue ? sum.Value / source.Length : null;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Average(Func<TSource, long?> selector)
        {
            double? sum = Sum(source, selector);
            return sum.HasValue ? sum.Value / source.Length : null;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? Average(Func<TSource, float?> selector)
        {
            var sum = Sum(source, selector);
            return sum.HasValue ? sum.Value / source.Length : null;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Average(Func<TSource, double?> selector)
        {
            var sum = Sum(source, selector);
            return sum.HasValue ? sum.Value / source.Length : null;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? Average(Func<TSource, decimal?> selector)
        {
            var sum = Sum(source, selector);
            return sum.HasValue ? sum.Value / source.Length : null;
        }
    }
}