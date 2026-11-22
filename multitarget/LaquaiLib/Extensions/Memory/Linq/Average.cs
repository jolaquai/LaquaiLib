namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, int})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Average(Func<TSource, int> selector)
        {
            if (source.Length == 0)
                return 0;
            double sum = Sum(source, selector);
            return sum / source.Length;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, long})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Average(Func<TSource, long> selector)
        {
            if (source.Length == 0)
                return 0;
            double sum = Sum(source, selector);
            return sum / source.Length;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, float})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Average(Func<TSource, float> selector)
        {
            if (source.Length == 0)
                return 0;
            var sum = Sum(source, selector);
            return sum / source.Length;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, double})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Average(Func<TSource, double> selector)
        {
            if (source.Length == 0)
                return 0;
            var sum = Sum(source, selector);
            return sum / source.Length;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, decimal})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Average(Func<TSource, decimal> selector)
        {
            if (source.Length == 0)
                return 0;
            var sum = Sum(source, selector);
            return sum / source.Length;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, int?})"/>
        // Matches Enumerable.Average: null entries are ignored and the divisor is the count of
        // non-null elements (returning null when there are none), not source.Length.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Average(Func<TSource, int?> selector)
        {
            long sum = 0;
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue)
                {
                    sum += value.Value;
                    count++;
                }
            }
            return count == 0 ? null : (double)sum / count;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, long?})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Average(Func<TSource, long?> selector)
        {
            long sum = 0;
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue)
                {
                    sum += value.Value;
                    count++;
                }
            }
            return count == 0 ? null : (double)sum / count;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, float?})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? Average(Func<TSource, float?> selector)
        {
            double sum = 0;
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue)
                {
                    sum += value.Value;
                    count++;
                }
            }
            return count == 0 ? null : (float)(sum / count);
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, double?})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Average(Func<TSource, double?> selector)
        {
            double sum = 0;
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue)
                {
                    sum += value.Value;
                    count++;
                }
            }
            return count == 0 ? null : sum / count;
        }

        /// <inheritdoc cref="Enumerable.Average{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? Average(Func<TSource, decimal?> selector)
        {
            decimal sum = 0;
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue)
                {
                    sum += value.Value;
                    count++;
                }
            }
            return count == 0 ? null : sum / count;
        }
    }
}