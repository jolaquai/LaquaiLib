using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.Extensions.Memory;

public static partial class MemoryExtensions
{
    extension<T>(Span<T> span)
    {
        /// <summary>
        /// Fills the specified <see cref="Span{T}"/> with the <see langword="default"/> value for type <typeparamref name="T"/>.
        /// </summary>
        public void Fill()
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = default;
            }
        }
        /// <summary>
        /// Fills the specified <see cref="Span{T}"/> using the specified <paramref name="factory"/>.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        public void Fill(Func<T> factory)
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = factory();
            }
        }
        /// <summary>
        /// Fills the specified <see cref="Span{T}"/> using the specified <paramref name="factory"/>. It is passed the previous iteration's value.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        public void Fill(Func<T, T> factory)
        {
            T last = default;
            for (var i = 0; i < span.Length; i++)
            {
                last = span[i] = factory(last);
            }
        }
        /// <summary>
        /// Fills the specified <see cref="Span{T}"/> using the specified <paramref name="factory"/>. It is passed the index in the span that is being assigned.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        public void Fill(Func<int, T> factory)
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = factory(i);
            }
        }
        /// <summary>
        /// Fills the specified <see cref="Span{T}"/> using the specified <paramref name="factory"/>. It is passed the index in the span that is being assigned and the previous iteration's value.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        public void Fill(Func<int, T, T> factory)
        {
            T last = default;
            for (var i = 0; i < span.Length; i++)
            {
                last = span[i] = factory(i, last);
            }
        }
    }

    extension<T>(MultiDimArrayEnumerable<T> multiDimArrayEnumerable)
    {
        /// <summary>
        /// Fills the specified <see cref="MultiDimArrayEnumerable{T}"/> with the <see langword="default"/> value for type <typeparamref name="T"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill() => multiDimArrayEnumerable.Span.Fill();
        /// <summary>
        /// Fills the specified <see cref="MultiDimArrayEnumerable{T}"/> using the specified <paramref name="factory"/>.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<T> factory) => multiDimArrayEnumerable.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <see cref="MultiDimArrayEnumerable{T}"/> using the specified <paramref name="factory"/>. It is passed the previous iteration's value.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<T, T> factory) => multiDimArrayEnumerable.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <see cref="MultiDimArrayEnumerable{T}"/> using the specified <paramref name="factory"/>. It is passed the index in the <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/> that is being assigned.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<int, T> factory) => multiDimArrayEnumerable.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <see cref="MultiDimArrayEnumerable{T}"/> using the specified <paramref name="factory"/>. It is passed the index in the <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/> that is being assigned and the previous iteration's value.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<int, T, T> factory) => multiDimArrayEnumerable.Span.Fill(factory);
    }

    extension<T>(Memory<T> memory)
    {
        /// <summary>
        /// Fills the specified <see cref="Memory{T}"/> with the <see langword="default"/> value for type <typeparamref name="T"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill() => memory.Span.Fill();
        /// <summary>
        /// Fills the specified <see cref="Memory{T}"/> using the specified <paramref name="factory"/>.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<T> factory) => memory.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <see cref="Memory{T}"/> using the specified <paramref name="factory"/>. It is passed the previous iteration's value.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<T, T> factory) => memory.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <see cref="Memory{T}"/> using the specified <paramref name="factory"/>. It is passed the index in the memory that is being assigned.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<int, T> factory) => memory.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <see cref="Memory{T}"/> using the specified <paramref name="factory"/>. It is passed the index in the memory that is being assigned and the previous iteration's value.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<int, T, T> factory) => memory.Span.Fill(factory);

        /// <summary>
        /// Asynchronously fills the specified <see cref="Memory{T}"/> using the specified <paramref name="factory"/>.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FillAsync(Func<ValueTask<T>> factory)
        {
            for (var i = 0; i < memory.Length; i++)
            {
                var t = await factory().ConfigureAwait(false);
                memory.Span[i] = t;
            }
        }
        /// <summary>
        /// Asynchronously fills the specified <see cref="Memory{T}"/> using the specified <paramref name="factory"/>. It is passed the previous iteration's value.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FillAsync(Func<T, ValueTask<T>> factory)
        {
            T last = default;
            for (var i = 0; i < memory.Length; i++)
            {
                var t = await factory(last).ConfigureAwait(false);
                last = memory.Span[i] = t;
            }
        }
        /// <summary>
        /// Asynchronously fills the specified <see cref="Memory{T}"/> using the specified <paramref name="factory"/>. It is passed the index in the memory that is being assigned.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FillAsync(Func<int, ValueTask<T>> factory)
        {
            for (var i = 0; i < memory.Length; i++)
            {
                var t = await factory(i).ConfigureAwait(false);
                memory.Span[i] = t;
            }
        }
        /// <summary>
        /// Asynchronously fills the specified <see cref="Memory{T}"/> using the specified <paramref name="factory"/>. It is passed the index in the memory that is being assigned and the previous iteration's value.
        /// </summary>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FillAsync(Func<int, T, ValueTask<T>> factory)
        {
            T last = default;
            for (var i = 0; i < memory.Length; i++)
            {
                var t = await factory(i, last).ConfigureAwait(false);
                last = memory.Span[i] = t;
            }
        }
    }
}
