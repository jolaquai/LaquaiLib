using System.Collections.Immutable;

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
    public static IEnumerable<T> Select<T>(this IEnumerable<T> source) => source.ToArray();

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

    #region Mode
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
        ArgumentNullException.ThrowIfNull(source);

        if (selector is null)
        {
            return source.Mode();
        }

        var enumerated = source as TSource[] ?? source.ToArray();

        if (enumerated.Length != 0)
        {
            throw new ArgumentException("Cannot determine the mode of an empty sequence.", nameof(source));
        }

        var models = new List<ModeModel<TSource, TSelect>>();

        foreach (var item in source)
        {
            var selected = selector(item);
            var model = models.Find(m => m.SelectedValue.Equals(selected));
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

        var enumerated = source as T[] ?? source.ToArray();
        if (enumerated.Length == 0)
        {
            throw new ArgumentException("Cannot determine the mode of an empty sequence.", nameof(source));
        }

        return enumerated.Select(item => (Item: item,
                                            Count: source.Count(i => i.Equals(item))))
                         .OrderByDescending(kvp => kvp.Count)
                         .First()
                         .Item;
    }
    #endregion

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

    /// <summary>
    /// Determines if two sequences are equivalent, meaning they contain the same elements, regardless of order.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to reference.</param>
    /// <param name="other">The sequence to compare to.</param>
    /// <param name="comparer">An instance of an <see cref="IEqualityComparer{T}"/>-implementing Type that is used to compare the elements in the sequences. If not specified, the default comparer for <typeparamref name="T"/> is used.</param>
    /// <returns><see langword="true"/> if the sequences are equivalent, otherwise <see langword="false"/>.</returns>
    public static bool SequenceEquivalent<T>(this IEnumerable<T> source, IEnumerable<T> other, IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;

        if (other is null && source is null)
        {
            return true;
        }
        if (other is null || source is null)
        {
            return false;
        }

        var sourceEnumerated = source as T[] ?? source.ToArray();
        var otherEnumerated = other as T[] ?? other.ToArray();
        if (sourceEnumerated.Length != otherEnumerated.Length)
        {
            return false;
        }

        return Array.TrueForAll(sourceEnumerated, item => otherEnumerated.Contains(item, comparer));
    }

    /// <summary>
    /// Conditionally projects elements from a sequence into a new form, transforming only items that satisfy a specified <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in the input sequence.</typeparam>
    /// <typeparam name="TResult">The Type of the elements the <paramref name="selector"/> produces.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines whether the element should be transformed.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence and produces a new value.</param>
    /// <returns>The transformed elements.</returns>
    public static IEnumerable<TResult> ConditionalSelect<TSource, TResult>(this IEnumerable<TSource> source, Predicate<TSource> predicate, Func<TSource, TResult> selector)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                yield return selector(item);
            }
        }
    }

    /// <summary>
    /// Blits the elements in the input sequence into a sequence of bytes.
    /// <typeparamref name="T"/> must be an unmanaged Type.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>All elements in the input sequence, blitted into a sequence of bytes and concatenated.</returns>
    public static IEnumerable<byte> Blitted<T>(this IEnumerable<T> source)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(source);

        return Blitted2();

        IEnumerable<byte> Blitted2()
        {
            foreach (var item in source)
            {
                var bytes = item switch
                {
                    bool v => BitConverter.GetBytes(v),
                    char v => BitConverter.GetBytes(v),
                    short v => BitConverter.GetBytes(v),
                    int v => BitConverter.GetBytes(v),
                    long v => BitConverter.GetBytes(v),
                    ushort v => BitConverter.GetBytes(v),
                    uint v => BitConverter.GetBytes(v),
                    ulong v => BitConverter.GetBytes(v),
                    Half v => BitConverter.GetBytes(v),
                    float v => BitConverter.GetBytes(v),
                    double v => BitConverter.GetBytes(v),
                    byte v => [v],
                    byte[] v => v,
                    _ => [],
                };
                foreach (var b in bytes)
                {
                    yield return b;
                }
            }
        }
    }

    /// <summary>
    /// Filters a sequence of values based on a predicate. The predicate's result is inverted.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines whether the element should be yielded.</param>
    /// <returns>The elements in the input sequence that do not satisfy the predicate.</returns>
    /// <remarks>
    /// This has essentially no purpose but to avoid the need to create a lambda that inverts the result of the predicate.
    /// </remarks>
    public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        return WhereNot2();

        IEnumerable<T> WhereNot2()
        {
            foreach (var item in source)
            {
                if (!predicate(item))
                {
                    yield return item;
                }
            }
        }
    }
    /// <summary>
    /// Splits a sequence of values into two sequences based on a predicate.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines which sequence the element should be yielded to.</param>
    /// <returns>A <see cref="Tuple{T1, T2}"/> containing the two sequences.</returns>
    public static (IEnumerable<T> True, IEnumerable<T> False) Split<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var trueList = new List<T>();
        var falseList = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
            {
                trueList.Add(item);
            }
            else
            {
                falseList.Add(item);
            }
        }

        return (trueList, falseList);
    }
}
