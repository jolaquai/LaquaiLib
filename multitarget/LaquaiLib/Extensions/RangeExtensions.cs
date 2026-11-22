using System.Runtime.Intrinsics;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for the <see cref="Range"/> type.
/// </summary>
public static class RangeExtensions
{
    extension(in Range range)
    {
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="int"/>s that are within the given <paramref name="range"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="int"/>s that are within the given <paramref name="range"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> GetRange() => range.Start.IsFromEnd || range.End.IsFromEnd
                ? throw new ArgumentException("Range indices cannot be from the end since there is no end to reference from.", nameof(range))
                : Enumerable.Range(range.Start.Value, range.End.Value - range.Start.Value);
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="int"/>s that are within the given <paramref name="range"/>, calculating the required indices from the given <paramref name="length"/>.
        /// </summary>
        /// <param name="length">The length of the range to reference.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="int"/>s that are within the given <paramref name="range"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> GetRange(int length)
        {
            var (offs, len) = range.GetOffsetAndLength(length);
            return Enumerable.Range(offs, len);
        }
        /// <summary>
        /// Returns an array of <see cref="int"/>s that are within the given <paramref name="range"/>.
        /// </summary>
        /// <returns>The array of <see cref="int"/>s that are within the given <paramref name="range"/>.</returns>
        public int[] ToArray()
        {
            if (range.Start.IsFromEnd || range.End.IsFromEnd)
                throw new ArgumentException("Range indices cannot be from the end since there is no end to reference from.", nameof(range));

            var len = range.End.Value < range.Start.Value ? range.Start.Value - range.End.Value : range.End.Value - range.Start.Value;
            var arr = GC.AllocateUninitializedArray<int>(len);
            var span = arr.AsSpan();
            var step = range.End.Value > range.Start.Value ? 1 : -1;

            var start = range.Start.Value;
            var i = 0;
            if (Vector256.IsHardwareAccelerated)
            {
                while (arr.Length - i > Vector256<int>.Count)
                {
                    Vector256.CreateSequence(start, step).StoreUnsafe(ref span.RefAt(i));
                    start += Vector256<int>.Count * step;
                    i += Vector256<int>.Count;
                }
            }
            if (Vector128.IsHardwareAccelerated)
            {
                while (arr.Length - i > Vector128<int>.Count)
                {
                    Vector128.CreateSequence(start, step).StoreUnsafe(ref span.RefAt(i));
                    start += Vector128<int>.Count * step;
                    i += Vector128<int>.Count;
                }
            }

            if (step > 0)
                for (; start < range.End.Value; start += step, i += step)
                    arr[i] = start;
            else
                for (; start > range.End.Value; start += step, i += -step)
                    arr[i] = start;
            return arr;
        }
        /// <summary>
        /// Returns an array of <see cref="int"/>s that are within the given <paramref name="range"/>, calculating the required indices from the given <paramref name="length"/>.
        /// </summary>
        /// <param name="length">The length of the range to reference.</param>
        /// <returns>The array of <see cref="int"/>s that are within the given <paramref name="range"/>.</returns>
        public int[] ToArray(int length)
        {
            var effectiveStart = range.Start.IsFromEnd ? length - range.Start.Value : range.Start.Value;
            var effectiveEnd = range.End.IsFromEnd ? length - range.End.Value : range.End.Value;

            var len = effectiveEnd < effectiveStart ? effectiveStart - effectiveEnd : effectiveEnd - effectiveStart;

            var arr = GC.AllocateUninitializedArray<int>(len);
            var span = arr.AsSpan();
            var step = effectiveEnd > effectiveStart ? 1 : -1;

            var start = effectiveStart;
            var i = 0;
            if (Vector256.IsHardwareAccelerated)
            {
                while (arr.Length - i > Vector256<int>.Count)
                {
                    Vector256.CreateSequence(start, step).StoreUnsafe(ref span.RefAt(i));
                    start += Vector256<int>.Count * step;
                    i += Vector256<int>.Count;
                }
            }
            if (Vector128.IsHardwareAccelerated)
            {
                while (arr.Length - i > Vector128<int>.Count)
                {
                    Vector128.CreateSequence(start, step).StoreUnsafe(ref span.RefAt(i));
                    start += Vector128<int>.Count * step;
                    i += Vector128<int>.Count;
                }
            }

            if (step > 0)
                for (var v = start; v < effectiveEnd; v += step, i += step)
                    arr[i] = v;
            else
                for (var v = start; v > effectiveEnd; v += step, i += -step)
                    arr[i] = v;
            return arr;
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> of <see cref="int"/>s that may be used to iterate through the numbers within the given <paramref name="range"/>.
        /// </summary>
        /// <returns>The <see cref="IEnumerator{T}"/> as described.</returns>
        /// <remarks>
        /// This wouldn't typically be called directly, but rather through a <see langword="foreach"/> loop.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RangeEnumerator GetEnumerator() => new RangeEnumerator(range);

        /// <inheritdoc/>
        public IEnumerable<TResult> Select<TResult>(Func<int, TResult> selector) =>
        range.GetRange().Select(selector);

        /// <inheritdoc/>
        public IEnumerable<int> Where(Func<int, bool> predicate) =>
            range.GetRange().Where(predicate);

        /// <inheritdoc/>
        public IEnumerable<TResult> SelectMany<TResult>(Func<int, IEnumerable<TResult>> selector) =>
            range.GetRange().SelectMany(selector);

        /// <inheritdoc/>
        public IEnumerable<TResult> SelectMany<TCollection, TResult>(
            Func<int, IEnumerable<TCollection>> collectionSelector,
            Func<int, TCollection, TResult> resultSelector) =>
            range.GetRange().SelectMany(collectionSelector, resultSelector);

        /// <inheritdoc/>
        public IEnumerable<TResult> Join<TInner, TKey, TResult>(
            IEnumerable<TInner> inner,
            Func<int, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<int, TInner, TResult> resultSelector) =>
            range.GetRange().Join(inner, outerKeySelector, innerKeySelector, resultSelector);

        /// <inheritdoc/>
        public IEnumerable<TResult> GroupJoin<TInner, TKey, TResult>(
            IEnumerable<TInner> inner,
            Func<int, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<int, IEnumerable<TInner>, TResult> resultSelector) =>
            range.GetRange().GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector);

        /// <inheritdoc/>
        public IOrderedEnumerable<int> OrderBy<TKey>(Func<int, TKey> keySelector) =>
            range.GetRange().OrderBy(keySelector);

        /// <inheritdoc/>
        public IOrderedEnumerable<int> OrderByDescending<TKey>(Func<int, TKey> keySelector) =>
            range.GetRange().OrderByDescending(keySelector);

        /// <inheritdoc/>
        public IEnumerable<IGrouping<TKey, int>> GroupBy<TKey>(Func<int, TKey> keySelector) =>
            range.GetRange().GroupBy(keySelector);

        /// <inheritdoc/>
        public IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(
            Func<int, TKey> keySelector,
            Func<int, TElement> elementSelector) =>
            range.GetRange().GroupBy(keySelector, elementSelector);
    }
}

/// <summary>
/// Enumerates the indices of a <see cref="Range"/> whose <see cref="Index"/> values may not be from the end of the range, since there is no end to reference from.
/// </summary>
/// <param name="range">The <see cref="Range"/> whose indices are to be enumerated.</param>
public struct RangeEnumerator(Range range) : IEnumerator<int>
{
    private readonly int _start = range.Start.IsFromEnd ? throw new ArgumentException("Range indices cannot be from the end since there is no end to reference from.", nameof(range)) : range.Start.Value;
    private readonly int _end = range.End.IsFromEnd ? throw new ArgumentException("Range indices cannot be from the end since there is no end to reference from.", nameof(range)) : range.End.Value;
    private bool _started;
    /// <summary>
    /// Gets the current index in the enumeration of the <see cref="Range"/>.
    /// </summary>
    public int Current { get; private set; }
    readonly object IEnumerator.Current => Current;
    /// <summary>
    /// Disposes the enumerator. This is a no-op.
    /// </summary>
    public readonly void Dispose() { }
    /// <summary>
    /// Advances the enumerator to the next index in the enumeration of the <see cref="Range"/>.
    /// </summary>
    /// <returns><see langword="true"/> if advancing the enumerator was successful; otherwise, <see langword="false"/>.</returns>
    public bool MoveNext()
    {
        if (!_started)
        {
            Current = _start - 1; // Start at one less than the start value so that the first MoveNext() call sets it to the start value.
            _started = true;
        }
        else
            Current++;
        return Current < _end;
    }
    /// <summary>
    /// Resets the enumerator to its initial state; that is, before the first index in the enumeration of the <see cref="Range"/>.
    /// </summary>
    public void Reset()
    {
        _started = false;
        Current = default;
    }
}