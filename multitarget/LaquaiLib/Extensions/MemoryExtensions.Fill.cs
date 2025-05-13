using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.Extensions;

public static partial class MemoryExtensions
{
    extension<T>(Span<T> span)
    {
        /// <summary>
        /// Fills the specified <paramref name="span"/> with the <see langword="default"/> value for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the span.</typeparam>
        /// <param name="span">The <see cref="Span{T}"/> of <typeparamref name="T"/> to fill.</param>
        public void Fill()
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = default;
            }
        }
        /// <summary>
        /// Fills the specified <paramref name="span"/> using the specified <paramref name="factory"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the span.</typeparam>
        /// <param name="span">The <see cref="Span{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        public void Fill(Func<T> factory)
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = factory();
            }
        }
        /// <summary>
        /// Fills the specified <paramref name="span"/> using the specified <paramref name="factory"/>. It is passed the previous iteration's value.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the span.</typeparam>
        /// <param name="span">The <see cref="Span{T}"/> of <typeparamref name="T"/> to fill.</param>
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
        /// Fills the specified <paramref name="span"/> using the specified <paramref name="factory"/>. It is passed the index in the span that is being assigned.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the span.</typeparam>
        /// <param name="span">The <see cref="Span{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        public void Fill(Func<int, T> factory)
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = factory(i);
            }
        }
        /// <summary>
        /// Fills the specified <paramref name="span"/> using the specified <paramref name="factory"/>. It is passed the index in the span that is being assigned and the previous iteration's value.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the span.</typeparam>
        /// <param name="span">The <see cref="Span{T}"/> of <typeparamref name="T"/> to fill.</param>
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
        /// Fills the specified <paramref name="multiDimArrayEnumerable"/> with the <see langword="default"/> value for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/>.</typeparam>
        /// <param name="multiDimArrayEnumerable">The <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/> to fill.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill() => multiDimArrayEnumerable.Span.Fill();
        /// <summary>
        /// Fills the specified <paramref name="multiDimArrayEnumerable"/> using the specified <paramref name="factory"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/>.</typeparam>
        /// <param name="multiDimArrayEnumerable">The <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<T> factory) => multiDimArrayEnumerable.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <paramref name="multiDimArrayEnumerable"/> using the specified <paramref name="factory"/>. It is passed the previous iteration's value.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/>.</typeparam>
        /// <param name="multiDimArrayEnumerable">The <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<T, T> factory) => multiDimArrayEnumerable.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <paramref name="multiDimArrayEnumerable"/> using the specified <paramref name="factory"/>. It is passed the index in the <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/> that is being assigned.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/>.</typeparam>
        /// <param name="multiDimArrayEnumerable">The <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<int, T> factory) => multiDimArrayEnumerable.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <paramref name="multiDimArrayEnumerable"/> using the specified <paramref name="factory"/>. It is passed the index in the <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/> that is being assigned and the previous iteration's value.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/>.</typeparam>
        /// <param name="multiDimArrayEnumerable">The <see cref="MultiDimArrayEnumerable{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<int, T, T> factory) => multiDimArrayEnumerable.Span.Fill(factory);

        /// <summary>
        /// Asynchronously fills the specified <paramref name="multiDimArrayEnumerable"/> using the specified <paramref name="factory"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="multiDimArrayEnumerable">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task FillAsync(Func<ValueTask<T>> factory) => multiDimArrayEnumerable.Memory.FillAsync(factory);
        /// <summary>
        /// Asynchronously fills the specified <paramref name="multiDimArrayEnumerable"/> using the specified <paramref name="factory"/>. It is passed the previous iteration's value.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="multiDimArrayEnumerable">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task FillAsync(Func<T, ValueTask<T>> factory) => multiDimArrayEnumerable.Memory.FillAsync(factory);
        /// <summary>
        /// Asynchronously fills the specified <paramref name="multiDimArrayEnumerable"/> using the specified <paramref name="factory"/>. It is passed the index in the memory that is being assigned.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="multiDimArrayEnumerable">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task FillAsync(Func<int, ValueTask<T>> factory) => multiDimArrayEnumerable.Memory.FillAsync(factory);
        /// <summary>
        /// Asynchronously fills the specified <paramref name="multiDimArrayEnumerable"/> using the specified <paramref name="factory"/>. It is passed the index in the memory that is being assigned and the previous iteration's value.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="multiDimArrayEnumerable">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task FillAsync(Func<int, T, ValueTask<T>> factory) => multiDimArrayEnumerable.Memory.FillAsync(factory);
    }

    extension<T>(Memory<T> memory)
    {
        /// <summary>
        /// Fills the specified <paramref name="memory"/> with the <see langword="default"/> value for type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill() => memory.Span.Fill();
        /// <summary>
        /// Fills the specified <paramref name="memory"/> using the specified <paramref name="factory"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<T> factory) => memory.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <paramref name="memory"/> using the specified <paramref name="factory"/>. It is passed the previous iteration's value.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<T, T> factory) => memory.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <paramref name="memory"/> using the specified <paramref name="factory"/>. It is passed the index in the memory that is being assigned.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<int, T> factory) => memory.Span.Fill(factory);
        /// <summary>
        /// Fills the specified <paramref name="memory"/> using the specified <paramref name="factory"/>. It is passed the index in the memory that is being assigned and the previous iteration's value.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
        /// <param name="factory">The factory method that produces the values to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(Func<int, T, T> factory) => memory.Span.Fill(factory);

        /// <summary>
        /// Asynchronously fills the specified <paramref name="memory"/> using the specified <paramref name="factory"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
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
        /// Asynchronously fills the specified <paramref name="memory"/> using the specified <paramref name="factory"/>. It is passed the previous iteration's value.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
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
        /// Asynchronously fills the specified <paramref name="memory"/> using the specified <paramref name="factory"/>. It is passed the index in the memory that is being assigned.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
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
        /// Asynchronously fills the specified <paramref name="memory"/> using the specified <paramref name="factory"/>. It is passed the index in the memory that is being assigned and the previous iteration's value.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the memory.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
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
