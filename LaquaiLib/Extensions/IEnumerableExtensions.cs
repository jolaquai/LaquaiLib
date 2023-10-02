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

    /// <summary>
    /// Checks whether the items in a sequence are all equal to each other. If any of the passed objects are <see langword="null"/>, all others must also be <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the objects to compare.</typeparam>
    /// <param name="source">The collection that contains the items to compare. An exception is thrown if the collection is empty.</param>
    /// <returns><see langword="true"/> if all objects in the passed <paramref name="source"/> collection are equal, otherwise <see langword="false"/>.</returns>
    public static bool AllEqual<T>(this IEnumerable<T> source)
    {
        if (!source.Any())
        {
            throw new ArgumentException("The passed collection must not be empty.", nameof(source));
        }
        if (source.Count() == 1)
        {
            return true;
        }

        if (source.Any(o => o is null))
        {
            return source.All(o => o is null);
        }
        var first = source.First();
        return source.Skip(1).All(item => item.Equals(first));
    }

    /// <summary>
    /// Produces the set difference of two sequences according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <param name="source">The first sequence to compare.</param>
    /// <param name="other">The second sequence to compare.</param>
    /// <param name="keySelector">The <see cref="Func{T, TResult}"/> that is passed each element of the source sequence and returns the key to use for comparison.</param>
    /// <returns>A sequence that contains the set difference of the elements of two sequences.</returns>
    /// <remarks>Basically just another <see cref="Enumerable.ExceptBy{TSource, TKey}(IEnumerable{TSource}, IEnumerable{TKey}, Func{TSource, TKey})"/> overload that... actually makes sense.</remarks>
    public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> source, IEnumerable<TSource> other, Func<TSource, TKey> keySelector)
    {
        var keys = new HashSet<TKey>(other.Select(keySelector));
        foreach (var element in source)
        {
            if (keys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    private class ModeModel<TSource, TSelect>
    {
        public TSource OriginalValue { get; set; }
        public TSelect SelectedValue { get; set; }
        public int CountBySelect { get; set; }
    }

    /// <summary>
    /// Determines the mode of a sequence of values from a given key extracted from each value; that is, the value that appears most frequently. If multiple items share the highest frequency, the first one encountered is returned.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in <paramref name="source"/>.</typeparam>
    /// <typeparam name="TSelect">The Type of the elements <paramref name="selector"/> produces.</typeparam>
    /// <param name="source">The sequence of values to determine the mode of.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of <paramref name="source"/> and produces a value that is used to determine the mode of <paramref name="source"/>.</param>
    /// <returns>The mode of <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is empty.</exception>
    public static TSource ModeBy<TSource, TSelect>(this IEnumerable<TSource> source, Func<TSource, TSelect> selector)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        if (!source.Any())
        {
            throw new ArgumentException("Cannot determine the mode of an empty sequence.", nameof(source));
        }

        var models = new List<ModeModel<TSource, TSelect>>();

        foreach (var item in source)
        {
            var selected = selector(item);
            var model = models.FirstOrDefault(m => m.SelectedValue.Equals(selected));
            if (model is null)
            {
                models.Add(new ModeModel<TSource, TSelect>
                {
                    OriginalValue = item,
                    SelectedValue = selected,
                    CountBySelect = 1
                });
            }
            else
            {
                model.CountBySelect++;
            }
        }

        return models.OrderByDescending(m => m.CountBySelect).First().OriginalValue;
    }

    /// <summary>
    /// Determines the mode of a sequence of values; that is, the value that appears most frequently. If multiple items share the highest frequency, the first one encountered is returned.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in <paramref name="source"/>.</typeparam>
    /// <param name="source">The sequence of values to determine the mode of.</param>
    /// <returns>The mode of <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is empty.</exception>
    public static T Mode<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        if (!source.Any())
        {
            throw new ArgumentException("Cannot determine the mode of an empty sequence.", nameof(source));
        }

        return source.Select(item => new KeyValuePair<T, int>(item, source.Count(i => i.Equals(item))))
                     .OrderByDescending(kvp => kvp.Value)
                     .First()
                     .Key;
    }

    /// <summary>
    /// Samples a specified number of elements from the input sequence.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sample.</param>
    /// <param name="itemCount">The number of elements to sample from the input sequence. If not specified, 1% of the input sequence's length is used.</param>
    /// <returns>The sampled elements.</returns>
    public static IEnumerable<T> Sample<T>(this IEnumerable<T> source, int itemCount = -1)
    {
        return source.Shuffle()
                     .Take(itemCount > 0 ? itemCount : source.Count() / 100);
    }

    /// <summary>
    /// Samples a specified number of elements from the input sequence, ensuring that the sampled elements remain in the same order as they were in the input sequence.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sample.</param>
    /// <param name="itemCount">The number of elements to sample from the input sequence. If not specified, 1% of the input sequence's length is used.</param>
    /// <returns>The sampled elements.</returns>
    public static IEnumerable<T> OrderedSample<T>(this IEnumerable<T> source, int itemCount = -1)
    {
        var random = new Random();
        var sourceCount = source.Count();
        itemCount = itemCount > 0 ? itemCount : sourceCount / 100;
        var chunkSize = sourceCount / itemCount;

        return source.Chunk(chunkSize)
                     .Select(chunk => chunk[random.Next(0, chunk.Length)]);
    }
}
