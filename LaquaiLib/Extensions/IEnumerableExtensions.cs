using System.Diagnostics.Tracing;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> Type.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <summary>
    /// Selects each element in the input sequence without transformation.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains each element in the input sequence.</returns>
    public static IEnumerable<T> Select<T>(this IEnumerable<T> source) => source;
    /// <summary>
    /// Flattens a sequence of nested sequences of the same type <typeparamref name="T"/> into a single sequence without transformation.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence of nested sequences to flatten.</param>
    /// <returns>A sequence that contains all the elements of the nested sequences.</returns>
    public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> source) => source.SelectMany(item => item);

    /// <summary>
    /// Halves the input sequence.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>A <see cref="ValueTuple{T1, T2}"/> that contains the two halves of the input sequence.</returns>
    public static (T[] First, T[] Second) Halve<T>(this IEnumerable<T> source)
    {
        var enumerated = source.ToArray();
        if (enumerated.Length is <= 1)
        {
            throw new ArgumentException("The input sequence must contain at least two elements.", nameof(source));
        }
        var half = enumerated.Length / 2;
        return (enumerated[..half], enumerated[half..]);
    }

    /// <summary>
    /// Shuffles the elements in the input sequence.
    /// </summary>
    /// <remarks>
    /// If the calling code already has an instance of <see cref="Random"/>, it should use the <see cref="Shuffle{T}(IEnumerable{T}, Random)"/> overload.
    /// </remarks>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>A shuffled sequence of the elements in the input sequence.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) => source.Shuffle(Random.Shared);
    /// <summary>
    /// Shuffles the elements in the input sequence, using a specified <see cref="Random"/> instance.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="random">The <see cref="Random"/> instance to use for shuffling.</param>
    /// <returns>A shuffled sequence of the elements in the input sequence.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random random) => source.OrderBy(_ => random.Next());
    /// <summary>
    /// Performs a Ruffle operation on the input sequence; that is, the sequence is halved, then the individual items are interlaced.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>The sequence after the operation has been performed.</returns>
    public static IEnumerable<T> Ruffle<T>(this IEnumerable<T> source)
    {
        var (first, second) = source.Halve();
        var ruffled = Interlace(first, second);
        return ruffled;
    }

    /// <summary>
    /// Interlaces the items of the specified sequences.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequences.</typeparam>
    /// <param name="first">The first sequence to interlace.</param>
    /// <param name="second">The second sequence to interlace.</param>
    /// <returns>A single sequence that contains the elements of both input sequences, interlaced.</returns>
    public static IEnumerable<T> Interlace<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        using (var enumerator1 = first.GetEnumerator())
        using (var enumerator2 = second.GetEnumerator())
        {
            var hasNext1 = enumerator1.MoveNext();
            var hasNext2 = enumerator2.MoveNext();

            while (hasNext1 || hasNext2)
            {
                if (hasNext1)
                {
                    yield return enumerator1.Current;
                    hasNext1 = enumerator1.MoveNext();
                }

                if (hasNext2)
                {
                    yield return enumerator2.Current;
                    hasNext2 = enumerator2.MoveNext();
                }
            }
        }
    }

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
    /// Represents the state of a single iteration in a loop construct. This has no functionality and is used solely to support the iterator pattern.
    /// </summary>

    /// <summary>
    /// Enumerates over the elements in the input sequence in the specified <paramref name="range"/>.
    /// If <paramref name="source"/> is not indexable, the entire sequence is enumerated.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the collection.</typeparam>
    /// <param name="source">The collection to extract elements from.</param>
    /// <param name="range">A <see cref="Range"/> instance that indicates where the items to be extracted are located in the <paramref name="source"/>.</param>
    public static IEnumerable<T> GetRange<T>(this IEnumerable<T> source, Range range)
    {
        if (source is IReadOnlyList<T> rol)
        {
            var (start, length) = range.GetOffsetAndLength(rol.Count);
            IEnumerable<T> GetRangeImpl()
            {
                for (var i = start; i < start + length; i++)
                {
                    yield return rol[i];
                }
            }
            return GetRangeImpl();
        }
        return source.ToArray()[range];
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
    private struct ModeModel<TSource, TSelect>
    {
        public readonly TSource OriginalValue;
        public readonly TSelect SelectedValue;
        public int CountBySelect;
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
    public static TSource ModeBy<TSource, TSelect>(this IEnumerable<TSource> source, Func<TSource, TSelect> selector, IEqualityComparer<TSelect> equalityComparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        equalityComparer ??= EqualityComparer<TSelect>.Default;

        if (selector is null)
        {
            return source.Mode();
        }

        var enumerated = source as TSource[] ?? source.ToArray();
        if (enumerated.Length != 0)
        {
            return default;
        }
        var converted = enumerated.Select(selector).ToArray();

        var i = 0;
        return source.MaxBy(_ => converted.Count(item2 => equalityComparer.Equals(item2, converted[i++])));
    }

    /// <summary>
    /// Determines the mode of a sequence of values; that is, the value that appears most frequently. If multiple items share the highest frequency, the first one encountered is returned.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in <paramref name="source"/>.</typeparam>
    /// <param name="source">The sequence of values to determine the mode of.</param>
    /// <returns>The mode of <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is empty.</exception>
    public static T Mode<T>(this IEnumerable<T> source, IEqualityComparer<T> equalityComparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        equalityComparer ??= EqualityComparer<T>.Default;

        var enumerated = source as T[] ?? source.ToArray();
        return enumerated.Length == 0 ? default : enumerated.MaxBy(item => source.Count(item2 => equalityComparer.Equals(item, item2)));
    }
    #endregion

    /// <summary>
    /// Samples a specified number of elements from the input sequence.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sample.</param>
    /// <param name="itemCount">The number of elements to sample from the input sequence. If not specified, 1% of the input sequence's length is used.</param>
    /// <returns>The sampled elements.</returns>
    public static T[] Sample<T>(this IEnumerable<T> source, int itemCount = -1)
    {
        var enumerated = source.Shuffle().ToArray();
        return enumerated[..(itemCount > 0 ? itemCount : enumerated.Length / 100)];
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
    /// Determines whether two sequences are equivalent, that is, whether they contain the same elements in any order.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the input sequences.</typeparam>
    /// <param name="first">The first sequence to compare.</param>
    /// <param name="second">The second sequence to compare.</param>
    /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> implementation to use when comparing values, or null to use the default <see cref="EqualityComparer{T}.Default"/> for the type of the values.</param>
    /// <returns><see langword="true"/> if the two source sequences are of equal length and are equivalent, otherwise <see langword="false"/>. If one of the sequences is <see langword="null"/>, both sequences must be <see langword="null"/> to be considered equivalent.</returns>
    public static bool SequenceEquivalent<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer = null)
    {
        if (first is null || second is null)
        {
            return first == second;
        }

        comparer ??= EqualityComparer<T>.Default;
        try
        {
            var firstEnumerated = first.ToHashSet(comparer);
            var secondEnumerated = second.ToHashSet(comparer);
            if (firstEnumerated.Count != secondEnumerated.Count)
            {
                return false;
            }
            return firstEnumerated.SetEquals(secondEnumerated);
        }
        catch
        {
            var firstEnumerated = first.ToArray();
            var secondEnumerated = second.ToArray();
            if (firstEnumerated.Length != secondEnumerated.Length)
            {
                return false;
            }
            return Array.TrueForAll(firstEnumerated, f => Array.Exists(secondEnumerated, s => comparer.Equals(f, s)));
        }
    }

    /// <summary>
    /// Conditionally projects elements from a sequence into a new form, transforming only items that satisfy a specified <paramref name="predicate"/> and returning all other items unchanged.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines whether the element should be transformed.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence, if it passes the condition encapsulated by <paramref name="predicate"/>, and produces a new value. Its type must be the same as the input sequence's.</param>
    /// <returns>The transformed elements.</returns>
    public static IEnumerable<T> SelectWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate, Func<T, T> selector)
    {
        foreach (var item in source)
        {
            yield return predicate(item) ? selector(item) : item;
        }
    }
    /// <summary>
    /// Conditionally projects elements from a sequence into a new form, transforming only items that satisfy a specified <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and its index in the <paramref name="source"/> collection and determines whether the element should be transformed.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence and its index in the <paramref name="source"/> collection, if it passes the condition encapsulated by <paramref name="predicate"/>, and produces a new value. Its type must be the same as the input sequence's.</param>
    /// <returns>The transformed elements.</returns>
    public static IEnumerable<T> SelectWhere<T>(this IEnumerable<T> source, Func<T, int, bool> predicate, Func<T, int, T> selector)
    {
        var c = 0;
        foreach (var item in source)
        {
            yield return predicate(item, c) ? selector(item, c) : item;
            c++;
        }
    }

    /// <summary>
    /// Conditionally projects elements from a sequence into a new form, transforming only items that satisfy a specified <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in the input sequence.</typeparam>
    /// <typeparam name="TResult">The Type of the elements the <paramref name="selector"/> produces.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines whether the element should be transformed.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence, if it passes the condition encapsulated by <paramref name="predicate"/>, and produces a new value.</param>
    /// <returns>The transformed elements.</returns>
    public static IEnumerable<TResult> SelectOnlyWhere<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
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
    /// Conditionally projects elements from a sequence into a new form, transforming only items that satisfy a specified <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in the input sequence.</typeparam>
    /// <typeparam name="TResult">The Type of the elements the <paramref name="selector"/> produces.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and its index in the <paramref name="source"/> collection and determines whether the element should be transformed.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence and its index in the <paramref name="source"/> collection, if it passes the condition encapsulated by <paramref name="predicate"/>, and produces a new value.</param>
    /// <returns>The transformed elements.</returns>
    public static IEnumerable<TResult> SelectOnlyWhere<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate, Func<TSource, int, TResult> selector)
    {
        var c = 0;
        foreach (var item in source)
        {
            if (predicate(item, c))
            {
                yield return selector(item, c);
            }
            c++;
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
        foreach (var item in source)
        {
            if (!predicate(item))
            {
                yield return item;
            }
        }
    }
    /// <summary>
    /// Splits a sequence of values into two sequences based on a predicate.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines which sequence the element should be yielded to.</param>
    /// <returns>A <see cref="Tuple{T1, T2}"/> containing the two sequences. The first collection contains all elements that satisfy the predicate, the second collection contains all remaining elements.</returns>
    public static (IEnumerable<T> True, IEnumerable<T> False) Split<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        return (source.Where(predicate), source.WhereNot(predicate));
    }
    /// <summary>
    /// Filters a sequence of values by their type, omitting all objects of type <typeparamref name="TDerived"/>.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in the input sequence.</typeparam>
    /// <typeparam name="TDerived">The Type of the elements to exclude from the output sequence. Must be, derive from or implement <typeparamref name="TSource"/>.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>A sequence of all objects from <paramref name="source"/> that are not of type <typeparamref name="TDerived"/>.</returns>
    public static IEnumerable<TSource> NotOfType<TSource, TDerived>(this IEnumerable<TSource> source)
        where TDerived : TSource
        => source.Where(i => i is not TDerived);

    /// <summary>
    /// For each element in the input sequence, selects each value from another sequence and produces a new value using a specified selector function.
    /// This is essentially a cross join and may yield a large number of non-sensical results if the input sequences are large.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in the input sequence.</typeparam>
    /// <typeparam name="TOther">The Type of the elements in the other sequence.</typeparam>
    /// <typeparam name="TResult">The Type of the elements the <paramref name="selector"/> produces.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="other">The other sequence.</param>
    /// <param name="selector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and each element of the other sequence and produces a new value.</param>
    /// <returns>A sequence that contains the new values produced by the selector function.</returns>
    public static IEnumerable<TResult> CrossSelect<TSource, TOther, TResult>(this IEnumerable<TSource> source, IEnumerable<TOther> other, Func<TSource, TOther, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(other);
        ArgumentNullException.ThrowIfNull(selector);

        foreach (var item in source)
        {
            foreach (var otherItem in other)
            {
                yield return selector(item, otherItem);
            }
        }
    }

    /// <summary>
    /// Determines if a sequence is empty.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns><see langword="true"/> if the input sequence is empty, otherwise <see langword="false"/>.</returns>
    public static bool None<T>(this IEnumerable<T> source) => !source.Any();
    /// <summary>
    /// Determines if a sequence contains no elements that satisfy a condition.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The condition to check for.</param>
    /// <returns><see langword="true"/> if the input sequence contains no elements that satisfy the condition, otherwise <see langword="false"/>.</returns>
    public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate) => !source.Any(predicate);

    /// <summary>
    /// Determines whether a sequence contains exactly one element and returns that element if so, otherwise returns the specified <paramref name="defaultValue"/>.
    /// This behaves exactly like <see cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, TSource)"/> without throwing exceptions.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="defaultValue">The value to return if the input sequence contains no elements or more than one element.</param>
    /// <returns>The single element in the input sequence, or <paramref name="defaultValue"/> if the sequence contains no elements or more than one element.</returns>
    public static T? OnlyOrDefault<T>(this IEnumerable<T> source, T defaultValue = default)
    {
        try
        {
            return source.Single();
        }
        catch
        {
            return defaultValue;
        }
    }
    /// <summary>
    /// Determines whether a sequence contains exactly one element that satisfies a <paramref name="predicate"/> and returns that element if so, otherwise returns the specified <paramref name="defaultValue"/>.
    /// This behaves exactly like <see cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)"/> without throwing exceptions.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The condition to check for.</param>
    /// <param name="defaultValue">The value to return if the input sequence contains no elements or more than one element.</param>
    /// <returns>The single element in the input sequence that satisfies the <paramref name="predicate"/>, or <paramref name="defaultValue"/> if the sequence contains no elements or more than one element.</returns>
    public static T? OnlyOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, T defaultValue = default)
    {
        try
        {
            return source.Single(predicate);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Attempts to retrieve the element at the specified index from the input sequence if that index is valid for the sequence, otherwise a default value is returned.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="i">The index of the element to retrieve.</param>
    /// <param name="defaultValue">The value to return if the index is invalid. Defaults to the <see langword="default"/> value of <typeparamref name="T"/>.</param>
    /// <returns>The element at the specified index if it is valid, otherwise the specified default value.</returns>
    public static T? ElementAtOrDefault<T>(this IEnumerable<T> source, int i, T defaultValue = default)
    {
        try
        {
            return source.ElementAt(i);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Sorts the elements of a sequence in ascending order according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the original sequence and produces a key to use for sorting.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, Func<T, int, T> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.OrderBy(e => keySelector(e, i++));
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the original sequence and produces a key to use for sorting.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, Func<T, int, T> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.ThenBy(e => keySelector(e, i++));
    }
    /// <summary>
    /// Sorts the elements of a sequence in descending order according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the original sequence and produces a key to use for sorting.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, Func<T, int, T> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.OrderByDescending(e => keySelector(e, i++));
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence according to a key extracted from each element.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keySelector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and its index in the original sequence and produces a key to use for sorting.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> that iterates over the sorted input sequence.</returns>
    public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, Func<T, int, T> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var i = 0;
        return source.ThenByDescending(e => keySelector(e, i++));
    }

    /// <summary>
    /// Sorts the elements of a sequence in ascending order according to another sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IEnumerable<T> keys)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<T> ?? keys.ToArray();
        return source.OrderBy((_, i) => enumeratedKeys[i]);
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence using the specified sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IEnumerable<T> keys)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<T> ?? keys.ToArray();
        return source.ThenBy((_, i) => enumeratedKeys[i]);
    }
    /// <summary>
    /// Sorts the elements of a sequence in descending order according to another sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, IEnumerable<T> keys)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<T> ?? keys.ToArray();
        return source.OrderByDescending((_, i) => enumeratedKeys[i]);
    }
    /// <summary>
    /// Augments the sort order of a previously sorted sequence using the specified sequence that specifies the keys to use for sorting.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the sequences.</typeparam>
    /// <param name="source">The input sequence to sort.</param>
    /// <param name="keys">The sequence that specifies the keys to use for sorting.</param>
    /// <returns>The sorted input sequence.</returns>
    public static IOrderedEnumerable<T> ThenByDescending<T>(this IOrderedEnumerable<T> source, IEnumerable<T> keys)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keys);

        var enumeratedKeys = keys as IList<T> ?? keys.ToArray();
        return source.ThenByDescending((_, i) => enumeratedKeys[i]);
    }
}
