#warning TODO: Documentation is currently largely inheritdoc'd from System.Linq.Enumerable - Rewrite that

using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.Extensions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public partial class MemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        #region Aggregate
        /// <inheritdoc cref="Enumerable.Aggregate{TSource}(IEnumerable{TSource}, Func{TSource, TSource, TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Aggregate(Func<TSource, TSource, TSource> func)
        {
            var result = source[0];
            for (var i = 1; i < source.Length; i++)
            {
                result = func(result, source[i]);
            }
            return result;
        }

        /// <inheritdoc cref="Enumerable.Aggregate{TSource, TAccumulate}(IEnumerable{TSource}, TAccumulate, Func{TAccumulate, TSource, TAccumulate})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TAccumulate Aggregate<TAccumulate>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            for (var i = 0; i < source.Length; i++)
            {
                seed = func(seed, source[i]);
            }
            return seed;
        }

        /// <inheritdoc cref="Enumerable.Aggregate{TSource, TAccumulate, TResult}(IEnumerable{TSource}, TAccumulate, Func{TAccumulate, TSource, TAccumulate}, Func{TAccumulate, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Aggregate<TAccumulate, TResult>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            for (var i = 0; i < source.Length; i++)
            {
                seed = func(seed, source[i]);
            }
            return resultSelector(seed);
        }
        #endregion

        #region Any
        /// <inheritdoc cref="Enumerable.Any{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any() => source.Length > 0;

        /// <inheritdoc cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region All
        /// <inheritdoc cref="Enumerable.All{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool All(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Average
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
        #endregion

        #region Cast

        /// <inheritdoc cref="Enumerable.Cast{TResult}(IEnumerable)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Cast<TResult>(Span<TResult> destination)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }

            if (!typeof(TSource).IsAssignableTo(typeof(TResult)))
            {
                throw new InvalidCastException($"Cannot cast {typeof(TSource)} to {typeof(TResult)}.");
            }

            for (var i = 0; i < source.Length; i++)
            {
                destination[i] = (TResult)(object)source[i];
            }
            return source.Length;
        }
        #endregion

        #region Chunk
        /// <inheritdoc cref="Enumerable.Chunk{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanChunkEnumerable<TSource> Chunk(int size) => new SpanChunkEnumerable<TSource>(source, size);
        #endregion

        #region Contains
#if !NET10_0_OR_GREATER
        /// <inheritdoc cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TSource value, IEqualityComparer<TSource> comparer) => source.IndexOf(value, comparer) > -1;
#endif
        #endregion

        #region AggregateBy
        /// <inheritdoc cref="Enumerable.AggregateBy{TSource, TKey, TAccumulate}(IEnumerable{TSource}, Func{TSource, TKey}, TAccumulate, Func{TAccumulate, TSource, TAccumulate}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(Func<TSource, TKey> keySelector, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey> keyComparer)
            => GroupBy(source, keySelector, keyComparer).Select(g => KeyValuePair.Create(g.Key, g.Aggregate(seed, func)));

        /// <inheritdoc cref="Enumerable.AggregateBy{TSource, TKey, TAccumulate}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TKey, TAccumulate}, Func{TAccumulate, TSource, TAccumulate}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, TAccumulate>> AggregateBy<TKey, TAccumulate>(Func<TSource, TKey> keySelector, Func<TKey, TAccumulate> seedSelector, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey> keyComparer)
            => GroupBy(source, keySelector, keyComparer).Select(g => KeyValuePair.Create(g.Key, g.Aggregate(seedSelector(g.Key), func)));
        #endregion

        #region CountBy
        /// <inheritdoc cref="Enumerable.CountBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<TKey, int>> CountBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            var dict = new Dictionary<TKey, int>(keyComparer);
            for (var i = 0; i < source.Length; i++)
            {
                var key = keySelector(source[i]);
                dict.AddOrUpdate(key, 1, (k, v) => v + 1);
            }
            return dict;
        }
        #endregion

        #region Count
        /// <inheritdoc cref="Enumerable.Count{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count() => source.Length;

        /// <inheritdoc cref="Enumerable.Count{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count(Func<TSource, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    count++;
                }
            }
            return count;
        }
        #endregion

        #region TryGetNonEnumeratedCount
        /// <inheritdoc cref="Enumerable.TryGetNonEnumeratedCount{TSource}(IEnumerable{TSource}, out int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNonEnumeratedCount(out int count) => (count = source.Length) > -1;
        #endregion

        #region LongCount
        /// <inheritdoc cref="Enumerable.LongCount{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long LongCount() => source.Length;

        /// <inheritdoc cref="Enumerable.LongCount{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long LongCount(Func<TSource, bool> predicate) => Count(source, predicate);
        #endregion

        #region DefaultIfEmpty
        /// <summary>
        /// Leaves the specified <paramref name="destination"/> <see cref="Span{T}"/> unchanged if the source <see cref="ReadOnlySpan{T}"/> is not empty; otherwise, the first element of the destination span is set to the <see langword="default"/> value of <typeparamref name="TSource"/>.
        /// </summary>
        /// <param name="destination">The destination span to potentially receive the <see langword="default"/> value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the destination span is too short to hold the <see langword="default"/> value.</exception>
        /// <returns>The number of elements written to in <paramref name="destination"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DefaultIfEmpty(Span<TSource> destination)
        {
            if (source.Length == 0)
            {
                if (destination.Length == 0)
                {
                    throw new ArgumentException("Destination span is too short.", nameof(destination));
                }
                destination[0] = default;
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Assigns the <see langword="default"/> value of <typeparamref name="TSource"/> to the <paramref name="defaultValue"/> parameter if the source <see cref="ReadOnlySpan{T}"/> is empty; otherwise, leaves <paramref name="defaultValue"/> unchanged.
        /// </summary>
        /// <param name="defaultValue">The variable to potentially receive the <see langword="default"/> value.</param>
        /// <returns><see langword="true"/> if the source <see cref="ReadOnlySpan{T}"/> is not empty (that is, <paramref name="defaultValue"/> was not modified by the call to this method); otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DefaultIfEmpty(ref TSource defaultValue)
        {
            if (source.Length == 0)
            {
                defaultValue = default;
                return false;
            }
            return true;
        }
        #endregion

        #region Distinct
        // Unfortunately, this is basically what Enumerable.Distinct does, except that THAT method remains lazy, BUT since we 
        /// <inheritdoc cref="Enumerable.Distinct{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Distinct() => Distinct(source, EqualityComparer<TSource>.Default);

        /// <inheritdoc cref="Enumerable.Distinct{TSource}(IEnumerable{TSource}, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TSource> Distinct(IEqualityComparer<TSource> comparer) => ToHashSet(source, comparer);

        /// <summary>
        /// Filters the source <see cref="ReadOnlySpan{T}"/> for distinct elements and stores them in the specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="destination">The destination <see cref="Span{T}"/> to store the distinct elements.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> implementation to use for comparing elements.</param>
        /// <returns>The number of distinct elements written to the <paramref name="destination"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="destination"/> is shorter than the source <see cref="ReadOnlySpan{T}"/> (this is enforced because all elements could already be distinct).</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Distinct(Span<TSource> destination, IEqualityComparer<TSource> comparer = null)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }

            comparer ??= EqualityComparer<TSource>.Default;
            var destIndex = 0;
            // Around 10^7 elements is the point where the performance of using a HashSet becomes better than using a simple loop to check for duplicates
            // This and this method alone is unfortunately the only place where we can avoid HashSet
            if (source.Length < 10_000_000)
            {
                for (var i = 0; i < source.Length; i++)
                {
                    if (destIndex == 0 || destination[..destIndex].IndexOf(source[i], comparer) < 0)
                    {
                        destination[destIndex++] = source[i];
                    }
                }
                return destIndex;
            }

            var hashSet = new HashSet<TSource>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                if (hashSet.Add(source[i]))
                {
                    destination[destIndex++] = source[i];
                }
            }
            return destIndex;
        }
        #endregion

        #region DistinctBy
        /// <inheritdoc cref="Enumerable.DistinctBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DistinctBy<TKey>(Func<TSource, TKey> keySelector, Span<TSource> destination) => DistinctBy(source, keySelector, destination, EqualityComparer<TKey>.Default);

        /// <inheritdoc cref="Enumerable.DistinctBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DistinctBy<TKey>(Func<TSource, TKey> keySelector, Span<TSource> destination, IEqualityComparer<TKey> comparer)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }

            var destIndex = 0;
            comparer ??= EqualityComparer<TKey>.Default;
            var hashSet = new HashSet<TKey>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                if (hashSet.Add(keySelector(source[i])))
                {
                    destination[destIndex++] = source[i];
                }
            }
            return destIndex;
        }
        #endregion

        #region ElementAt
        /// <inheritdoc cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAt(int index) => source[index];

        /// <inheritdoc cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Index)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAt(Index index) => source[index];
        #endregion

        #region ElementAtOrDefault
        /// <inheritdoc cref="Enumerable.ElementAtOrDefault{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAtOrDefault(int index) => index >= 0 && index < source.Length ? source[index] : default;

        /// <inheritdoc cref="Enumerable.ElementAtOrDefault{TSource}(IEnumerable{TSource}, Index)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource ElementAtOrDefault(Index index)
        {
            var offset = index.GetOffset(source.Length);
            return offset < 0 || offset >= source.Length ? default : source[offset];
        }
        #endregion

        #region First
        /// <inheritdoc cref="Enumerable.First{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource First() => source[0];

        /// <inheritdoc cref="Enumerable.First{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource First(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            throw new InvalidOperationException("Span does not contain any elements that match the predicate.");
        }
        #endregion

        #region FirstOrDefault
        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault() => source.Length > 0 ? source[0] : default;

        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault(TSource defaultValue) => source.Length > 0 ? source[0] : defaultValue;

        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault(Func<TSource, bool> predicate)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return default;
        }

        /// <inheritdoc cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource FirstOrDefault(Func<TSource, bool> predicate, TSource defaultValue)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return defaultValue;
        }
        #endregion

        #region GroupBy
        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TSource>> GroupBy<TKey>(Func<TSource, TKey> keySelector) => ToLookup(source, keySelector);

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TSource>> GroupBy<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) => ToLookup(source, keySelector, comparer);

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) => GroupBy(source, keySelector, elementSelector);

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) => GroupBy(source, keySelector, elementSelector, comparer);

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TKey, IEnumerable{TSource}, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TResult>(Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
            => ToLookup(source, keySelector).Select(group => resultSelector(group.Key, group));

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TKey, IEnumerable{TSource}, TResult}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TResult>(Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
            => ToLookup(source, keySelector, comparer).Select(group => resultSelector(group.Key, group));

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, Func{TKey, IEnumerable{TElement}, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TElement, TResult>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
            => ToLookup(source, keySelector, elementSelector).Select(group => resultSelector(group.Key, group));

        /// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey, TElement, TResult}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, Func{TKey, IEnumerable{TElement}, TResult}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> GroupBy<TKey, TElement, TResult>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
            => ToLookup(source, keySelector, elementSelector, comparer).Select(group => resultSelector(group.Key, group));
        #endregion

        #region Last
        /// <inheritdoc cref="Enumerable.Last{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Last() => source[^1];

        /// <inheritdoc cref="Enumerable.Last{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Last(Func<TSource, bool> predicate)
        {
            for (var i = source.Length - 1; i >= 0; i--)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            throw new InvalidOperationException("Span does not contain any elements that match the predicate.");
        }
        #endregion

        #region LastOrDefault
        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault() => source.Length > 0 ? source[^1] : default;

        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault(TSource defaultValue) => source.Length > 0 ? source[^1] : defaultValue;

        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault(Func<TSource, bool> predicate)
        {
            for (var i = source.Length - 1; i >= 0; i--)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return default;
        }

        /// <inheritdoc cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource LastOrDefault(Func<TSource, bool> predicate, TSource defaultValue)
        {
            for (var i = source.Length - 1; i >= 0; i--)
            {
                if (predicate(source[i]))
                {
                    return source[i];
                }
            }
            return defaultValue;
        }
        #endregion

        #region Max

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, int})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Max(Func<TSource, int> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value > max)
                {
                    max = value;
                }
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, long})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Max(Func<TSource, long> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value > max)
                {
                    max = value;
                }
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, float})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max(Func<TSource, float> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value > max)
                {
                    max = value;
                }
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, double})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Max(Func<TSource, double> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value > max)
                {
                    max = value;
                }
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, decimal})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal Max(Func<TSource, decimal> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value > max)
                {
                    max = value;
                }
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, int?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int? Max(Func<TSource, int?> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            for (var i = 0; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                {
                    max = value;
                }
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, long?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long? Max(Func<TSource, long?> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                {
                    max = value;
                }
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, float?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float? Max(Func<TSource, float?> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                {
                    max = value;
                }
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, double?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double? Max(Func<TSource, double?> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                {
                    max = value;
                }
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, Func{TSource, decimal?})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal? Max(Func<TSource, decimal?> selector)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (value.HasValue && (!max.HasValue || value.Value > max.Value))
                {
                    max = value;
                }
            }
            return max;
        }

        /// <inheritdoc cref="Enumerable.Max{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Max<TResult>(Func<TSource, TResult> selector, IComparer<TResult> comparer = null)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var max = selector(source[0]);
            comparer ??= Comparer<TResult>.Default;
            for (var i = 1; i < source.Length; i++)
            {
                var value = selector(source[i]);
                if (comparer.Compare(value, max) > 0)
                {
                    max = value;
                }
            }
            return max;
        }
        #endregion

        #region MaxBy
        /// <inheritdoc cref="Enumerable.MaxBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MaxBy<TKey>(Func<TSource, TKey> keySelector) => MaxBy(source, keySelector, Comparer<TKey>.Default);

        /// <inheritdoc cref="Enumerable.MaxBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MaxBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var maxItem = source[0];
            var maxKey = keySelector(maxItem);
            for (var i = 1; i < source.Length; i++)
            {
                var item = source[i];
                var key = keySelector(item);
                if (comparer.Compare(key, maxKey) > 0)
                {
                    maxItem = item;
                    maxKey = key;
                }
            }
            return maxItem;
        }
        #endregion

        #region Min
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
        #endregion

        #region MinBy
        /// <inheritdoc cref="Enumerable.MinBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MinBy<TKey>(Func<TSource, TKey> keySelector) => MinBy(source, keySelector, Comparer<TKey>.Default);

        /// <inheritdoc cref="Enumerable.MinBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource MinBy<TKey>(Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source.Length == 0)
            {
                throw new InvalidOperationException("Span is empty.");
            }
            var minItem = source[0];
            var minKey = keySelector(minItem);
            for (var i = 1; i < source.Length; i++)
            {
                var item = source[i];
                var key = keySelector(item);
                if (comparer.Compare(key, minKey) < 0)
                {
                    minItem = item;
                    minKey = key;
                }
            }
            return minItem;
        }
        #endregion

        #region Select
        // These are safe to invoke when source and destination point to the same location (if the types are compatible)
        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into a new form and stores the results in a specified destination <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a transformed element.</param>
        /// <param name="destination">A <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the projected elements.</exception>
        /// <returns>The number of elements written to in <paramref name="destination"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Select<TResult>(Func<TSource, TResult> selector, Span<TResult> destination)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }
            for (var i = 0; i < source.Length; i++)
            {
                destination[i] = selector(source[i]);
            }
            return source.Length;
        }

        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into a new form while incorporating the element's index and stores the results in a specified destination <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and its index in the source <see cref="ReadOnlySpan{T}"/> and returns a transformed element.</param>
        /// <param name="destination">A <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the projected elements.</exception>
        /// <returns>The number of elements written to in <paramref name="destination"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Select<TResult>(Func<TSource, int, TResult> selector, Span<TResult> destination)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }
            for (var i = 0; i < source.Length; i++)
            {
                destination[i] = selector(source[i], i);
            }
            return source.Length;
        }
        #endregion

        #region SelectMany
        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into an <see cref="IEnumerable{T}"/> of <typeparamref name="TResult"/> and stores those elements in the specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns an <see cref="IEnumerable{T}"/> of projected elements.</param>
        /// <param name="destination">The <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <returns>The number of elements written to <paramref name="destination"/>.</returns>
        /// <remarks>
        /// This and the other overloads of this method group should only be used with spans owned and controlled by the caller to ensure no unexpected results occur.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SelectMany<TResult>(Func<TSource, IEnumerable<TResult>> selector, Span<TResult> destination)
        {
            var destIndex = 0;
            for (var i = 0; i < source.Length; i++)
            {
                foreach (var item in selector(source[i]))
                {
                    if (destIndex >= destination.Length)
                    {
                        return destIndex; // Last assigned index
                    }

                    destination[destIndex++] = item;
                }
            }
            return destIndex;
        }

        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into an <see cref="IEnumerable{T}"/> of <typeparamref name="TResult"/> and stores those elements in the specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="selector">A <see cref="Func{T1, T2, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and its index in the source <see cref="ReadOnlySpan{T}"/> and returns an <see cref="IEnumerable{T}"/> of projected elements.</param>
        /// <param name="destination">The <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <returns>The number of elements written to <paramref name="destination"/>.</returns>
        /// <remarks>
        /// This and the other overloads of this method group should only be used with spans owned and controlled by the caller to ensure no unexpected results occur.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SelectMany<TResult>(Func<TSource, int, IEnumerable<TResult>> selector, Span<TResult> destination)
        {
            var destIndex = 0;
            for (var i = 0; i < source.Length; i++)
            {
                foreach (var item in selector(source[i], i))
                {
                    if (destIndex >= destination.Length)
                    {
                        return destIndex; // Last assigned index
                    }

                    destination[destIndex++] = item;
                }
            }
            return destIndex;
        }

        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into an <see cref="IEnumerable{T}"/> of <typeparamref name="TCollection"/>, which is then projected into an <see cref="IEnumerable{T}"/> of <typeparamref name="TResult"/>, and stores those elements in the specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The type of the elements in the collection returned by <paramref name="collectionSelector"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="collectionSelector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns an <see cref="IEnumerable{T}"/> of <typeparamref name="TCollection"/> of projected elements.</param>
        /// <param name="resultSelector">A <see cref="Func{T1, T2, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and, in turn, each corresponding element from the <see cref="IEnumerable{T}"/> returned by <paramref name="collectionSelector"/>, and returns the projected elements of type <typeparamref name="TResult"/>.</param>
        /// <param name="destination">The <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <returns>The number of elements written to <paramref name="destination"/>.</returns>
        /// <remarks>
        /// This and the other overloads of this method group should only be used with spans owned and controlled by the caller to ensure no unexpected results occur.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SelectMany<TCollection, TResult>(Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector, Span<TResult> destination)
        {
            var destIndex = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var collection = collectionSelector(source[i]);
                foreach (var item in collection)
                {
                    if (destination.Length <= i)
                    {
                        return destIndex;
                    }
                    destination[i] = resultSelector(source[i], item);
                }
            }
            return destIndex;
        }


        /// <summary>
        /// Projects each element of the source <see cref="ReadOnlySpan{T}"/> into an <see cref="IEnumerable{T}"/> of <typeparamref name="TCollection"/>, which is then projected into an <see cref="IEnumerable{T}"/> of <typeparamref name="TResult"/>, and stores those elements in the specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TCollection">The type of the elements in the collection returned by <paramref name="collectionSelector"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
        /// <param name="collectionSelector">A <see cref="Func{T1, T2, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and its index in the source <see cref="ReadOnlySpan{T}"/> and returns an <see cref="IEnumerable{T}"/> of <typeparamref name="TCollection"/> of projected elements.</param>
        /// <param name="resultSelector">A <see cref="Func{T1, T2, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and, in turn, each corresponding element from the <see cref="IEnumerable{T}"/> returned by <paramref name="collectionSelector"/>, and returns the projected elements of type <typeparamref name="TResult"/>.</param>
        /// <param name="destination">The <see cref="Span{T}"/> to store the results of the projection.</param>
        /// <returns>The number of elements written to <paramref name="destination"/>.</returns>
        /// <remarks>
        /// This and the other overloads of this method group should only be used with spans owned and controlled by the caller to ensure no unexpected results occur.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SelectMany<TCollection, TResult>(Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector, Span<TResult> destination)
        {
            var destIndex = 0;
            for (var i = 0; i < source.Length; i++)
            {
                var collection = collectionSelector(source[i], i);
                foreach (var item in collection)
                {
                    if (destination.Length <= i)
                    {
                        return destIndex;
                    }
                    destination[i] = resultSelector(source[i], item);
                }
            }
            return destIndex;
        }
        #endregion

        #region Single
        /// <inheritdoc cref="Enumerable.Single{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Single() => source.Length != 1 ? throw new InvalidOperationException("Span does not contain exactly one element.") : source[0];

        /// <inheritdoc cref="Enumerable.Single{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource Single(Func<TSource, bool> predicate)
        {
            TSource result = default;
            var found = false;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    if (found)
                    {
                        throw new InvalidOperationException("Span contains more than one element that matches the predicate.");
                    }
                    result = source[i];
                    found = true;
                }
            }
            return !found ? throw new InvalidOperationException("Span does not contain any elements that match the predicate.") : result;
        }
        #endregion

        #region SingleOrDefault
        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault() => SingleOrDefault(source, default(TSource));

        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault(TSource defaultValue) => source.Length switch
        {
            0 => defaultValue,
            > 1 => throw new InvalidOperationException("Span contains more than one element."),
            _ => source[0]
        };

        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault(Func<TSource, bool> predicate) => SingleOrDefault(source, predicate, default);

        /// <inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource SingleOrDefault(Func<TSource, bool> predicate, TSource defaultValue)
        {
            var result = defaultValue;
            var found = false;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    if (found)
                    {
                        throw new InvalidOperationException("Span contains more than one element that matches the predicate.");
                    }
                    result = source[i];
                    found = true;
                }
            }
            return found ? result : defaultValue;
        }
        #endregion

        #region Skip
        /// <inheritdoc cref="Enumerable.Skip{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> Skip(int count) => source[count..];
        #endregion

        #region SkipWhile
        /// <inheritdoc cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> SkipWhile(Func<TSource, bool> predicate)
        {
            var newStart = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                {
                    newStart = i;
                    break;
                }
            }
            return source[newStart..];
        }

        /// <inheritdoc cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> SkipWhile(Func<TSource, int, bool> predicate)
        {
            var newStart = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i], i))
                {
                    newStart = i;
                    break;
                }
            }
            return source[newStart..];
        }
        #endregion

        #region SkipLast
        /// <inheritdoc cref="Enumerable.SkipLast{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> SkipLast(int count) => source[..^count];
        #endregion

        #region Sum
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
        #endregion

        #region Take
        /// <inheritdoc cref="Enumerable.Take{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> Take(int count) => source.Length < count ? source : source[..count];

        /// <inheritdoc cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Range)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> Take(Range range) => source[range];
        #endregion

        #region TakeWhile
        /// <inheritdoc cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> TakeWhile(Func<TSource, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                {
                    break;
                }
                count++;
            }
            return source[..count];
        }

        /// <inheritdoc cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> TakeWhile(Func<TSource, int, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i], i))
                {
                    break;
                }
                count++;
            }
            return source[..count];
        }
        #endregion

        #region TakeLast
        /// <inheritdoc cref="Enumerable.TakeLast{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSource> TakeLast(int count) => source.Length < count ? source : source[^count..];
        #endregion

        #region Where
        /// <summary>
        /// Filters the elements of the <see cref="ReadOnlySpan{T}"/> becased on a <paramref name="predicate"/> function and stores all matching elements in a specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="predicate">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a <see langword="bool"/> indicating whether the element should be included in the result.</param>
        /// <param name="destination">A <see cref="Span{T}"/> to store the results of the filtering.</param>
        /// <returns>The number of elements written to in <paramref name="destination"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the filtered elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Where(Func<TSource, bool> predicate, Span<TSource> destination)
        {
            var requiredSpace = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    requiredSpace++;
                }
            }
            if (destination.Length < requiredSpace)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }
            var index = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    destination[index++] = source[i];
                }
            }
            return requiredSpace;
        }

        /// <summary>
        /// Filters the elements of the <see cref="ReadOnlySpan{T}"/> becased on a <paramref name="predicate"/> function and stores all matching elements in a specified <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="predicate">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and its index in the source <see cref="ReadOnlySpan{T}"/> and returns a <see langword="bool"/> indicating whether the element should be included in the result.</param>
        /// <param name="destination">A <see cref="Span{T}"/> to store the results of the filtering.</param>
        /// <returns>The number of elements written to in <paramref name="destination"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the filtered elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Where(Func<TSource, int, bool> predicate, Span<TSource> destination)
        {
            var requiredSpace = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i], i))
                {
                    requiredSpace++;
                }
            }
            if (destination.Length < requiredSpace)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }
            var index = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i], i))
                {
                    destination[index++] = source[i];
                }
            }
            return requiredSpace;
        }
        #endregion

        #region Zip
        /// <summary>
        /// Merges two <see cref="ReadOnlySpan{T}"/>s into another <see cref="Span{T}"/> by applying a result selector function to each pair of elements.
        /// </summary>
        /// <typeparam name="TSecond">The type of the elements in the second <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements in the result <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <param name="second">The second <see cref="ReadOnlySpan{T}"/> to merge with the source <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="resultSelector">A <see cref="Func{T1, T2, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and the corresponding element of the second <see cref="ReadOnlySpan{T}"/>, and returns a transformed element.</param>
        /// <param name="destination">The destination <see cref="Span{T}"/> to store the results of the merge.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the results.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Zip<TSecond, TResult>(ReadOnlySpan<TSecond> second, Func<TSource, TSecond, TResult> resultSelector, Span<TResult> destination)
        {
            var minLen = Math.Min(source.Length, second.Length);
            if (minLen > destination.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }
            var i = 0;
            for (; i < source.Length && i < second.Length; i++)
            {
                destination[i] = resultSelector(source[i], second[i]);
            }
            return minLen;
        }

        /// <summary>
        /// Merges two <see cref="ReadOnlySpan{T}"/>s into another <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TSecond">The type of the elements in the second <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <param name="second">The second <see cref="ReadOnlySpan{T}"/> to merge with the source <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="destination">The destination <see cref="Span{T}"/> to store the results of the merge.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the results.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Zip<TSecond>(ReadOnlySpan<TSecond> second, Span<(TSource, TSecond)> destination)
        {
            var minLen = Math.Min(source.Length, second.Length);
            if (minLen > destination.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }

            var i = 0;
            for (; i < source.Length && i < second.Length; i++)
            {
                destination[i] = (source[i], second[i]);
            }
            return minLen;
        }

        /// <summary>
        /// Merges three <see cref="ReadOnlySpan{T}"/>s into another <see cref="Span{T}"/>.
        /// </summary>
        /// <typeparam name="TSecond">The type of the elements in the second <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <typeparam name="TThird">The type of the elements in the third <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <param name="second">The second <see cref="ReadOnlySpan{T}"/> to merge with the source <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="third">The third <see cref="ReadOnlySpan{T}"/> to merge with the source <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="destination">The destination <see cref="Span{T}"/> to store the results of the merge.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is not large enough to hold the results.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Zip<TSecond, TThird>(ReadOnlySpan<TSecond> second, ReadOnlySpan<TThird> third, Span<(TSource, TSecond, TThird)> destination)
        {
            var minLen = Math.Min(source.Length, Math.Min(second.Length, third.Length));
            if (minLen > destination.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }

            var i = 0;
            for (; i < source.Length && i < second.Length && i < third.Length; i++)
            {
                destination[i] = (source[i], second[i], third[i]);
            }
            return minLen;
        }
        #endregion

        #region ToLookup
        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TSource> ToLookup<TKey>(Func<TSource, TKey> keySelector)
        {
            var spanLookup = new SpanLookup<TKey, TSource>(source.Length, null);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);

                var list = spanLookup._lookup[key] ??= [];
                list.Add(value);
            }
            return spanLookup;
        }

        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TSource> ToLookup<TKey>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var spanLookup = new SpanLookup<TKey, TSource>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                var list = spanLookup._lookup[key] ??= [];
                list.Add(value);
            }
            return spanLookup;
        }

        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TElement> ToLookup<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var spanLookup = new SpanLookup<TKey, TElement>(source.Length, null);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                var element = elementSelector(value);
                var list = spanLookup._lookup[key] ??= [];
                list.Add(element);
            }
            return spanLookup;
        }

        /// <inheritdoc cref="Enumerable.ToLookup{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILookup<TKey, TElement> ToLookup<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            var spanLookup = new SpanLookup<TKey, TElement>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                var element = elementSelector(value);
                var list = spanLookup._lookup[key] ??= [];
                list.Add(element);
            }
            return spanLookup;
        }
        #endregion

        #region ToList
        /// <inheritdoc cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<TSource> ToList()
        {
            var list = new List<TSource>();
            list.SetCount(source.Length);
            var span = list.AsSpan();
            source.CopyTo(span);
            return list;
        }
        #endregion

        #region ToDictionary
        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TSource> ToDictionary<TKey, TValue>(Func<TSource, TKey> keySelector)
        {
            var dict = new Dictionary<TKey, TSource>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                if (!dict.TryAdd(key, value))
                {
                    throw new ArgumentException($"Duplicate key found: {key}");
                }
            }
            return dict;
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TSource> ToDictionary<TKey, TValue>(Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var dict = new Dictionary<TKey, TSource>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                if (!dict.TryAdd(key, value))
                {
                    throw new ArgumentException($"Duplicate key found: {key}");
                }
            }
            return dict;
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var dict = new Dictionary<TKey, TElement>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                var element = elementSelector(value);
                if (!dict.TryAdd(key, element))
                {
                    throw new ArgumentException($"Duplicate key found: {key}");
                }
            }
            return dict;
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            var dict = new Dictionary<TKey, TElement>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var value = source[i];
                var key = keySelector(value);
                var element = elementSelector(value);
                if (!dict.TryAdd(key, element))
                {
                    throw new ArgumentException($"Duplicate key found: {key}");
                }
            }
            return dict;
        }
        #endregion

        #region ToHashSet
        /// <inheritdoc cref="Enumerable.ToHashSet{TSource}(IEnumerable{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSet<TSource> ToHashSet()
        {
            var set = new HashSet<TSource>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                _ = set.Add(source[i]);
            }
            return set;
        }

        /// <inheritdoc cref="Enumerable.ToHashSet{TSource}(IEnumerable{TSource}, IEqualityComparer{TSource})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashSet<TSource> ToHashSet(IEqualityComparer<TSource> comparer)
        {
            var set = new HashSet<TSource>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                _ = set.Add(source[i]);
            }
            return set;
        }
        #endregion

        #region CUSTOM
        /// <summary>
        /// Copies the contents of the source <see cref="ReadOnlySpan{T}"/> into the specified <paramref name="destination"/> <see cref="Span{T}"/> if the source is not empty.
        /// </summary>
        /// <param name="destination">The destination span to copy the elements into.</param>
        /// <exception cref="ArgumentException">Thrown when the destination span is too short to hold the elements from the source.</exception>
        public int CopyToIfNotEmpty(Span<TSource> destination)
        {
            if (source.Length != 0)
            {
                return 0;
            }

            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }

            source.CopyTo(destination);
            return source.Length;
        }

        /// <summary>
        /// Combines <see cref="Select{TSource, TResult}(ReadOnlySpan{TSource}, Func{TSource, TResult}, Span{TResult})"/> and <see cref="ReadOnlySpan{T}.ToArray"/>
        /// </summary>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a transformed element.</param>
        /// <returns>An array of <typeparamref name="TResult"/> containing the elements produced by the <paramref name="selector"/>.</returns>
        /// <remarks>
        /// This is provided as a replacement for something like <c>AsEnumerable</c>, since yielding is not possible with <see cref="ReadOnlySpan{T}"/>.
        /// </remarks>
        public TResult[] ToArray<TResult>(Func<TSource, TResult> selector)
        {
            var arr = GC.AllocateUninitializedArray<TResult>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                arr[i] = selector(source[i]);
            }
            return arr;
        }
        /// <summary>
        /// Combines <see cref="Select{TSource, TResult}(ReadOnlySpan{TSource}, Func{TSource, int, TResult}, Span{TResult})"/> and <see cref="ReadOnlySpan{T}.ToArray"/>
        /// </summary>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a transformed element.</param>
        /// <returns>An array of <typeparamref name="TResult"/> containing the elements produced by the <paramref name="selector"/>.</returns>
        /// <remarks>
        /// This is provided as a replacement for something like <c>AsEnumerable</c>, since yielding is not possible with <see cref="ReadOnlySpan{T}"/>.
        /// </remarks>
        public TResult[] ToArray<TResult>(Func<TSource, int, TResult> selector)
        {
            var arr = GC.AllocateUninitializedArray<TResult>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                arr[i] = selector(source[i], i);
            }
            return arr;
        }

        /// <summary>
        /// Combines <see cref="Where{TSource}(ReadOnlySpan{TSource}, Func{TSource, bool}, Span{TSource})"/> and <see cref="Select{TSource, TResult}(ReadOnlySpan{TSource}, Func{TSource, TResult}, Span{TResult})"/> and returns an array of <typeparamref name="TResult"/> containing the results.
        /// Neither parameter may be <see langword="null"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the resulting array.</typeparam>
        /// <param name="where">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a <see langword="bool"/> indicating whether the element should be included in the result.</param>
        /// <param name="select">A <see cref="Func{T, TResult}"/> that is passed each element of the source <see cref="ReadOnlySpan{T}"/> and returns a transformed element.</param>
        /// <returns>The created array of <typeparamref name="TResult"/>.</returns>
        public TResult[] WhereSelectToArray<TResult>(Func<TSource, bool> where, Func<TSource, TResult> select)
        {
            ArgumentNullException.ThrowIfNull(where);
            ArgumentNullException.ThrowIfNull(select);

            if (where.IsStatic && select.IsStatic)
            {
                return UWSToArray(source, where, select);
            }
            else
            {
                var ret = GC.AllocateUninitializedArray<TResult>(source.Length);
                var k = 0;
                for (var i = 0; i < source.Length; i++)
                {
                    if (where(source[i]))
                    {
                        ret[k++] = select(source[i]);
                    }
                }
                Array.Resize(ref ret, k);
                return ret;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe TResult[] UWSToArray<TResult>(Func<TSource, bool> where, Func<TSource, TResult> select)
        {
            // Optimize for static methods by using function pointers
            var wherePtr = (delegate*<TSource, bool>)where.Method.MethodHandle.GetFunctionPointer();
            var selectPtr = (delegate*<TSource, TResult>)select.Method.MethodHandle.GetFunctionPointer();

            var ret = GC.AllocateUninitializedArray<TResult>(source.Length);
            var k = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (wherePtr(source[i]))
                {
                    ret[k++] = selectPtr(source[i]);
                }
            }
            Array.Resize(ref ret, k);
            return ret;
        }

        #region OnlyOrDefault
        // Imma be honest, I stole these right out of System.Linq
        /// <summary>
        /// Determines whether a <see cref="ReadOnlySpan{T}"/> contains exactly one element and returns that element if so, otherwise returns the specified <paramref name="defaultValue"/>.
        /// This behaves exactly like <see cref="SingleOrDefault{TSource}(ReadOnlySpan{TSource}, TSource)"/> without throwing exceptions.
        /// </summary>
        /// <param name="defaultValue">The value to return if the source <see cref="ReadOnlySpan{T}"/> contains no elements or more than one element.</param>
        /// <returns>The single element in the source <see cref="ReadOnlySpan{T}"/>, or <paramref name="defaultValue"/> if the sequence contains no or more than one element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource OnlyOrDefault(TSource defaultValue = default) => source.Length != 1 ? defaultValue : source[0];
        /// <summary>
        /// Determines whether a <see cref="ReadOnlySpan{T}"/> contains exactly one element that satisfies a <paramref name="predicate"/> and returns that element if so, otherwise returns the specified <paramref name="defaultValue"/>.
        /// This behaves exactly like <see cref="SingleOrDefault{TSource}(ReadOnlySpan{TSource}, Func{TSource, bool}, TSource)"/> without throwing exceptions.
        /// </summary>
        /// <param name="predicate">The condition to check for.</param>
        /// <param name="defaultValue">The value to return if the source <see cref="ReadOnlySpan{T}"/> contains no elements or more than one element.</param>
        /// <returns>The single element in the source <see cref="ReadOnlySpan{T}"/> that satisfies the <paramref name="predicate"/>, or <paramref name="defaultValue"/> if the sequence contains no or more than one element that satisfies the <paramref name="predicate"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TSource OnlyOrDefault(Func<TSource, bool> predicate, TSource defaultValue = default)
        {
            var result = defaultValue;
            var found = false;
            for (var i = 0; i < source.Length; i++)
            {
                if (predicate(source[i]))
                {
                    if (found)
                    {
                        return defaultValue;
                    }
                    result = source[i];
                    found = true;
                }
            }
            return found ? result : defaultValue;
        }
        #endregion
        #endregion
    }

    // In here goes anything that NEEDS to modify the source OR requires the return type to be the same as the source type (i.e. anything that returns a Span<T>, namely Skip* and Take*)
    // All of these should return either a reference to the original span or a slice from it so calls can be chained
    extension<TSource>(Span<TSource> source)
    {
        // Order* are already provided by System.MemoryExtensions through the Sort* methods

        #region Skip
        /// <inheritdoc cref="Enumerable.Skip{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> Skip(int count) => source[count..];
        #endregion

        #region SkipWhile
        /// <inheritdoc cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> SkipWhile(Func<TSource, bool> predicate)
        {
            var newStart = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                {
                    newStart = i;
                    break;
                }
            }
            return source[newStart..];
        }

        /// <inheritdoc cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> SkipWhile(Func<TSource, int, bool> predicate)
        {
            var newStart = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i], i))
                {
                    newStart = i;
                    break;
                }
            }
            return source[newStart..];
        }
        #endregion

        #region SkipLast
        /// <inheritdoc cref="Enumerable.SkipLast{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> SkipLast(int count) => source[..^count];
        #endregion

        #region Take
        /// <inheritdoc cref="Enumerable.Take{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> Take(int count) => source.Length < count ? source : source[..count];

        /// <inheritdoc cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Range)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> Take(Range range) => source[range];
        #endregion

        #region TakeWhile
        /// <inheritdoc cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> TakeWhile(Func<TSource, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i]))
                {
                    break;
                }
                count++;
            }
            return source[..count];
        }

        /// <inheritdoc cref="Enumerable.TakeWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> TakeWhile(Func<TSource, int, bool> predicate)
        {
            var count = 0;
            for (var i = 0; i < source.Length; i++)
            {
                if (!predicate(source[i], i))
                {
                    break;
                }
                count++;
            }
            return source[..count];
        }
        #endregion

        #region TakeLast
        /// <inheritdoc cref="Enumerable.TakeLast{TSource}(IEnumerable{TSource}, int)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TSource> TakeLast(int count) => source.Length < count ? source : source[^count..];
        #endregion
    }

    #region Specific types

    #region extension<TKey, TValue>(ReadOnlySpan<ValueTuple<TKey, TValue>> source)
    extension<TKey, TValue>(ReadOnlySpan<(TKey, TValue)> source)
    {
        /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{ValueTuple{TKey, TValue}})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dictionary = new Dictionary<TKey, TValue>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var pair = source[i];
                dictionary.Add(pair.Item1, pair.Item2);
            }
            return dictionary;
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{ValueTuple{TKey, TValue}}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary(IEqualityComparer<TKey> comparer)
        {
            var dictionary = new Dictionary<TKey, TValue>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var pair = source[i];
                dictionary.Add(pair.Item1, pair.Item2);
            }
            return dictionary;
        }
    }
    #endregion

    #region extension<TKey, TValue>(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
    extension<TKey, TValue>(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
    {
        /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dictionary = new Dictionary<TKey, TValue>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var pair = source[i];
                dictionary.Add(pair.Key, pair.Value);
            }
            return dictionary;
        }

        /// <inheritdoc cref="Enumerable.ToDictionary{TKey, TValue}(IEnumerable{KeyValuePair{TKey, TValue}}, IEqualityComparer{TKey})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ToDictionary(IEqualityComparer<TKey> comparer)
        {
            var dictionary = new Dictionary<TKey, TValue>(source.Length, comparer);
            for (var i = 0; i < source.Length; i++)
            {
                var pair = source[i];
                dictionary.Add(pair.Key, pair.Value);
            }
            return dictionary;
        }
    }
    #endregion

    /*
    [Average methods]

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource})" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSource Max()
    {
        for (var i = 0; i < source.Length; i++)
        {
        }
    }

    /// <inheritdoc cref="Enumerable.Max{TSource}(IEnumerable{TSource}, IComparer{TSource})" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSource Max(IComparer<TSource> comparer)
    {
        for (var i = 0; i < source.Length; i++)
        {
        }
    }


    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource})" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSource Min()
    {
        for (var i = 0; i < source.Length; i++)
        {
        }
    }

    /// <inheritdoc cref="Enumerable.Min{TSource}(IEnumerable{TSource}, IComparer{TSource})" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TSource Min(IComparer<TSource> comparer)
    {
        for (var i = 0; i < source.Length; i++)
        {
        }
    }
    */

    extension(ReadOnlySpan<bool> source)
    {
        /// <summary>
        /// Determines whether all values in the source <see cref="ReadOnlySpan{T}"/> are <see langword="true"/>.
        /// </summary>
        /// <returns><see langword="true"/> if all values in the source <see cref="ReadOnlySpan{T}"/> are <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        public bool All() => source.Length > 0 && source.All(static x => x);
    }

    extension(ReadOnlySpan<int> source) { }
    extension(ReadOnlySpan<uint> source) { }
    extension(ReadOnlySpan<long> source) { }
    extension(ReadOnlySpan<ulong> source) { }
    extension(ReadOnlySpan<double> source) { }
    extension(ReadOnlySpan<float> source) { }
    extension(ReadOnlySpan<decimal> source) { }
    extension(ReadOnlySpan<sbyte> source) { }
    extension(ReadOnlySpan<byte> source) { }
    extension(ReadOnlySpan<int?> source) { }
    extension(ReadOnlySpan<uint?> source) { }
    extension(ReadOnlySpan<long?> source) { }
    extension(ReadOnlySpan<ulong?> source) { }
    extension(ReadOnlySpan<double?> source) { }
    extension(ReadOnlySpan<float?> source) { }
    extension(ReadOnlySpan<decimal?> source) { }
    extension(ReadOnlySpan<sbyte?> source) { }
    extension(ReadOnlySpan<byte?> source) { }

    #endregion
}

internal class SpanLookup<TKey, TElement>(int capacity, IEqualityComparer<TKey> equalityComparer) : ILookup<TKey, TElement>
{
    internal readonly Dictionary<TKey, List<TElement>> _lookup = new Dictionary<TKey, List<TElement>>(capacity, equalityComparer);
    public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
    {
        foreach (var kvp in _lookup)
        {
            yield return new Grouping<TKey, TElement>(kvp.Key, kvp.Value);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _lookup.Count;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(TKey key) => _lookup.ContainsKey(key);
    public IEnumerable<TElement> this[TKey key]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _lookup.TryGetValue(key, out var list) ? list : [];
    }
}

// I'd have loved to make this useless thing a ref struct, but since we're only going to be handed around as IGrouping<TKey, TElement>, I can't
// And not even a normal struct makes sense since the interface cast will box us anyway
internal class Grouping<TKey, TElement>(TKey key, List<TElement> elements) : IGrouping<TKey, TElement>
{
    public TKey Key
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => key;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<TElement> GetEnumerator() => elements.GetEnumerator();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}