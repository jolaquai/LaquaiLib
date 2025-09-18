namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Min(Func<TSource, int> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value < min)
                {
                    min = value;
                }
            }
            return min;
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Min(Func<TSource, long> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value < min)
                {
                    min = value;
                }
            }
            return min;
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Min(Func<TSource, float> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value < min)
                {
                    min = value;
                }
            }
            return min;
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Min(Func<TSource, double> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value < min)
                {
                    min = value;
                }
            }
            return min;
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Min(Func<TSource, decimal> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value < min)
                {
                    min = value;
                }
            }
            return min;
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? Min(Func<TSource, int?> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!min.HasValue || value.Value < min.Value))
                {
                    min = value;
                }
            }
            return min;
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long? Min(Func<TSource, long?> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!min.HasValue || value.Value < min.Value))
                {
                    min = value;
                }
            }
            return min;
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? Min(Func<TSource, float?> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!min.HasValue || value.Value < min.Value))
                {
                    min = value;
                }
            }
            return min;
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Min(Func<TSource, double?> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!min.HasValue || value.Value < min.Value))
                {
                    min = value;
                }
            }
            return min;
        }

        /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? Min(Func<TSource, decimal?> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!min.HasValue || value.Value < min.Value))
                {
                    min = value;
                }
            }
            return min;
        }

        /// <inheritdoc cref="Enumerable.Min{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Min<TResult>(Func<TSource, TResult> selector, IComparer<TResult> comparer = null)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var min = selector(source[0]);
            comparer ??= Comparer<TResult>.Default;
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (comparer.Compare(value, min) < 0)
                {
                    min = value;
                }
            }
            return min;
        }
    }
}