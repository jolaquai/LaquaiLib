namespace LaquaiLib.Extensions;

partial class MemoryExtensions
{
    /// <summary>
    /// Fills the specified <paramref name="span"/> with the <see langword="default"/> value for type <typeparamref name="T"/>.
    /// The assignment is shallow; reference types will be assigned with the same reference.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="span">The <see cref="Span{T}"/> of <typeparamref name="T"/> to fill.</param>
    public static void Fill<T>(this Span<T> span)
    {
        for (var i = 0; i < span.Length; i++)
        {
            span[i] = default;
        }
    }
    /// <summary>
    /// Fills the specified <paramref name="span"/> using the given <paramref name="factory"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="span">The <see cref="Span{T}"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    public static void Fill<T>(this Span<T> span, Func<T> factory)
    {
        for (var i = 0; i < span.Length; i++)
        {
            span[i] = factory();
        }
    }
    /// <summary>
    /// Fills the specified <paramref name="span"/> using the given <paramref name="factory"/>. It is passed the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="span">The <see cref="Span{T}"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    public static void Fill<T>(this Span<T> span, Func<T, T> factory)
    {
        T last = default;
        for (var i = 0; i < span.Length; i++)
        {
            last = span[i] = factory(last);
        }
    }
    /// <summary>
    /// Fills the specified <paramref name="span"/> using the given <paramref name="factory"/>. It is passed the index in the array that is being assigned.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="span">The <see cref="Span{T}"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    public static void Fill<T>(this Span<T> span, Func<int, T> factory)
    {
        for (var i = 0; i < span.Length; i++)
        {
            span[i] = factory(i);
        }
    }
    /// <summary>
    /// Fills the specified <paramref name="span"/> using the given <paramref name="factory"/>. It is passed the index in the array that is being assigned and the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="span">The <see cref="Span{T}"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    public static void Fill<T>(this Span<T> span, Func<int, T, T> factory)
    {
        T last = default;
        for (var i = 0; i < span.Length; i++)
        {
            last = span[i] = factory(i, last);
        }
    }
    /// <summary>
    /// Fills the specified <paramref name="memory"/> with the <see langword="default"/> value for type <typeparamref name="T"/>.
    /// The assignment is shallow; reference types will be assigned with the same reference.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
    public static void Fill<T>(this Memory<T> memory) => memory.Span.Fill();
    /// <summary>
    /// Fills the specified <paramref name="memory"/> using the given <paramref name="factory"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    public static void Fill<T>(this Memory<T> memory, Func<T> factory) => memory.Span.Fill(factory);
    /// <summary>
    /// Fills the specified <paramref name="memory"/> using the given <paramref name="factory"/>. It is passed the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    public static void Fill<T>(this Memory<T> memory, Func<T, T> factory) => memory.Span.Fill(factory);
    /// <summary>
    /// Fills the specified <paramref name="memory"/> using the given <paramref name="factory"/>. It is passed the index in the array that is being assigned.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    public static void Fill<T>(this Memory<T> memory, Func<int, T> factory) => memory.Span.Fill(factory);
    /// <summary>
    /// Fills the specified <paramref name="memory"/> using the given <paramref name="factory"/>. It is passed the index in the array that is being assigned and the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="memory">The <see cref="Memory{T}"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    public static void Fill<T>(this Memory<T> memory, Func<int, T, T> factory) => memory.Span.Fill(factory);

    /// <summary>
    /// Asynchronously fills the specified <paramref name="memory"/> using the given <paramref name="factory"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="memory">The <see cref="Array"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task FillAsync<T>(this Memory<T> memory, Func<Task<T>> factory)
    {
        for (var i = 0; i < memory.Length; i++)
        {
            var val = await factory();
            memory.Span[i] = val;
        }
    }
    /// <summary>
    /// Asynchronously fills the specified <paramref name="memory"/> using the given <paramref name="factory"/>. It is passed the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="memory">The <see cref="Array"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task FillAsync<T>(this Memory<T> memory, Func<T, Task<T>> factory)
    {
        T last = default;
        for (var i = 0; i < memory.Length; i++)
        {
            var val = await factory(last);
            last = memory.Span[i] = val;
        }
    }
    /// <summary>
    /// Asynchronously fills the specified <paramref name="memory"/> using the given <paramref name="factory"/>. It is passed the index in the array that is being assigned.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="memory">The <see cref="Array"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task FillAsync<T>(this Memory<T> memory, Func<int, Task<T>> factory)
    {
        for (var i = 0; i < memory.Length; i++)
        {
            var val = await factory(i);
            memory.Span[i] = val;
        }
    }
    /// <summary>
    /// Asynchronously fills the specified <paramref name="memory"/> using the given <paramref name="factory"/>. It is passed the index in the array that is being assigned and the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="memory">The <see cref="Array"/> of <typeparamref name="T"/> to fill.</param>
    /// <param name="factory">The factory method that produces the values to fill the span with.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task FillAsync<T>(this Memory<T> memory, Func<int, T, Task<T>> factory)
    {
        T last = default;
        for (var i = 0; i < memory.Length; i++)
        {
            var val = await factory(i, last);
            last = memory.Span[i] = val;
        }
    }
}
