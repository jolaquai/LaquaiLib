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
    /// Selects all elements of nested sequences of the same type <typeparamref name="T"/> in <paramref name="source"/> and flattens them into a single sequence without transformation.
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
}
