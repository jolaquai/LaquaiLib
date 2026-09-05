namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(in ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, int})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Max(Func<TSource, int> selector)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value > max)
                    max = value;
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, long})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Max(Func<TSource, long> selector)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value > max)
                    max = value;
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, float})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max(Func<TSource, float> selector)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value > max)
                    max = value;
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, double})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Max(Func<TSource, double> selector)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value > max)
                    max = value;
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, decimal})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Max(Func<TSource, decimal> selector)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value > max)
                    max = value;
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, int?})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? Max(Func<TSource, int?> selector)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                    max = value;
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, long?})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long? Max(Func<TSource, long?> selector)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                    max = value;
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, float?})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? Max(Func<TSource, float?> selector)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                    max = value;
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, double?})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Max(Func<TSource, double?> selector)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                    max = value;
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? Max(Func<TSource, decimal?> selector)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                    max = value;
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Max<TResult>(Func<TSource, TResult> selector, IComparer<TResult> comparer = null)
        {
            if (source.Length == 0)
                throw new InvalidOperationException("Span is empty.");
            var max = selector(source[0]);
            comparer ??= Comparer<TResult>.Default;
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (comparer.Compare(value, max) > 0)
                    max = value;
            }
            return max;
        }
    }
}