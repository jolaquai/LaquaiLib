namespace LaquaiLib.Extensions.MemoryExtensions.Linq;

public static partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Sum(Func<TSource, int> selector)
        {
            var buf = 0;
            for (var i = 0; i < source.Length; i++)
            {
                buf += selector(source[i]);
            }
            return buf;
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Sum(Func<TSource, long> selector)
        {
            var buf = 0L;
            for (var i = 0; i < source.Length; i++)
            {
                buf += selector(source[i]);
            }
            return buf;
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sum(Func<TSource, float> selector)
        {
            var buf = 0f;
            for (var i = 0; i < source.Length; i++)
            {
                buf += selector(source[i]);
            }
            return buf;
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Sum(Func<TSource, double> selector)
        {
            var buf = 0d;
            for (var i = 0; i < source.Length; i++)
            {
                buf += selector(source[i]);
            }
            return buf;
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Sum(Func<TSource, decimal> selector)
        {
            var buf = 0m;
            for (var i = 0; i < source.Length; i++)
            {
                buf += selector(source[i]);
            }
            return buf;
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? Sum(Func<TSource, int?> selector)
        {
            int? buf = 0;
            var allNull = true;
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue)
                {
                    allNull = false;
                    buf += value.Value;
                }
            }
            return allNull ? null : buf;
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long? Sum(Func<TSource, long?> selector)
        {
            long? buf = 0L;
            var allNull = true;
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue)
                {
                    allNull = false;
                    buf += value.Value;
                }
            }
            return allNull ? null : buf;
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? Sum(Func<TSource, float?> selector)
        {
            float? buf = 0f;
            var allNull = true;
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue)
                {
                    allNull = false;
                    buf += value.Value;
                }
            }
            return allNull ? null : buf;
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Sum(Func<TSource, double?> selector)
        {
            double? buf = 0d;
            var allNull = true;
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue)
                {
                    allNull = false;
                    buf += value.Value;
                }
            }
            return allNull ? null : buf;
        }

        /// <inheritdoc cref="Enumerable.Sum{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? Sum(Func<TSource, decimal?> selector)
        {
            decimal? buf = 0m;
            var allNull = true;
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue)
                {
                    allNull = false;
                    buf += value.Value;
                }
            }
            return allNull ? null : buf;
        }
    }
}