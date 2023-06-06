namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> Type.
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    /// Selects each element in the input sequence without transformation.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains each element in the input sequence.</returns>
    public static IEnumerable<T> Select<T>(this IEnumerable<T> source) => source.Select(item => item);

    /// <summary>
    /// Flattens a sequence of nested sequences of the same type <typeparamref name="T"/> into a single sequence without transformation.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence of nested sequences to flatten.</param>
    /// <returns>A sequence that contains all the elements of the nested sequences.</returns>
    public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> source) => source.SelectMany(item => item);

    /// <summary>
    /// Shuffles the elements in the input sequence.
    /// </summary>
    /// <remarks>
    /// If the calling code already has an instance of <see cref="Random"/>, it should use the <see cref="Shuffle{T}(IEnumerable{T}, Random)"/> overload.
    /// </remarks>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>A shuffled sequence of the elements in the input sequence.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) => source.Shuffle(new Random());

    /// <summary>
    /// Shuffles the elements in the input sequence, using a specified <see cref="Random"/> instance.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="random">The <see cref="Random"/> instance to use for shuffling.</param>
    /// <returns>A shuffled sequence of the elements in the input sequence.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random random) => source.OrderBy(item => random.Next());

    /// <summary>
    /// Performs the specified <paramref name="action"/> on each element of the source collection.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the collection.</typeparam>
    /// <param name="source">The source collection to iterate over.</param>
    /// <param name="action">The action to perform on each element of the source collection. It is passed each element in the source collection.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var element in source)
        {
            action(element);
        }
    }

    /// <summary>
    /// Performs the specified <paramref name="action"/> on each element of the source collection, incorporating each element's index in the <see cref="Action{T1, T2}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the collection.</typeparam>
    /// <param name="source">The source collection to iterate over.</param>
    /// <param name="action">The action to perform on each element of the source collection. It is passed each element and its index in the source collection.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        var c = 0;
        foreach (var element in source)
        {
            action(element, c++);
        }
    }

    /// <summary>
    /// Invokes the specified <paramref name="function"/> on each element of the source collection.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in the collection.</typeparam>
    /// <typeparam name="TResult">The Type of the elements that <paramref name="function"/> produces.</typeparam>
    /// <param name="source">The source collection to iterate over.</param>
    /// <param name="function">The function to invoke on each element of the source collection. It is passed each element in the source collection.</param>
    public static IEnumerable<TResult> ForEach<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> function)
    {
        foreach (var element in source)
        {
            yield return function(element);
        }
    }

    /// <summary>
    /// Invokes the specified <paramref name="function"/> on each element of the source collection, incorporating each element's index in the <see cref="Func{T1, T2, TResult}"/>.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in the collection.</typeparam>
    /// <typeparam name="TResult">The Type of the elements that <paramref name="function"/> produces.</typeparam>
    /// <param name="source">The source collection to iterate over.</param>
    /// <param name="function">The function to invoke on each element of the source collection. It is passed each element and its index in the source collection.</param>
    public static IEnumerable<TResult> ForEach<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> function)
    {
        var c = 0;
        foreach (var element in source)
        {
            yield return function(element, c++);
        }
    }

    /// <summary>
    /// Extracts a range of elements from this collection.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the collection.</typeparam>
    /// <param name="source">The collection to extract elements from.</param>
    /// <param name="range">A <see cref="Range"/> instance that indicates where the items to be extracted are located in the <paramref name="source"/>.</param>
    public static IEnumerable<T> GetRange<T>(this IEnumerable<T> source, Range range)
    {
        var (offset, length) = range.GetOffsetAndLength(source.Count());
        for (var i = offset; i < offset + length; i++)
        {
            yield return source.ElementAt(i);
        }
    }
}
