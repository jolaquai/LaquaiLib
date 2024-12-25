using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> Type.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <summary>
    /// Flattens a sequence of nested sequences of the same type <typeparamref name="T"/> into a single sequence without transformation.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The sequence of nested sequences to flatten.</param>
    /// <returns>A sequence that contains all the elements of the nested sequences.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> source) => source.SelectMany(static item => item);

    /// <summary>
    /// Splits a sequence of values into two sequences based on a predicate.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines which sequence the element should be yielded to.</param>
    /// <returns>A <see cref="ValueTuple{T1, T2}"/> containing the two sequences. The first collection contains all elements that satisfy the predicate, the second collection contains all remaining elements.</returns>
    public static (IEnumerable<T> True, IEnumerable<T> False) Split<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        return (source.Where(predicate), source.WhereNot(predicate));
    }
    /// <summary>
    /// Halves the input sequence.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>A <see cref="ValueTuple{T1, T2}"/> that contains the two halves of the input sequence.</returns>
    public static (T[] First, T[] Second) Halve<T>(this IEnumerable<T> source)
    {
        var enumerated = source.ToArray();
        if (enumerated.Length is < 2)
        {
            throw new ArgumentException("The input sequence must contain at least two elements.", nameof(source));
        }
        var half = enumerated.Length / 2;
        return (enumerated[..half], enumerated[half..]);
    }

    /// <summary>
    /// Shuffles the elements in the input sequence using <see cref="Random.Shared"/>.
    /// </summary>
    /// <remarks>
    /// If the calling code already has an instance of <see cref="Random"/>, it should use the <see cref="Shuffle{T}(IEnumerable{T}, Random)"/> overload.
    /// </remarks>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>A shuffled sequence of the elements in the input sequence.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) => source.Shuffle(Random.Shared);
    /// <summary>
    /// Shuffles the elements in the input sequence, using a specified <see cref="Random"/> instance.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="random">The <see cref="Random"/> instance to use for shuffling.</param>
    /// <returns>A shuffled sequence of the elements in the input sequence.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random random) => source.OrderBy(_ => random.Next());
    /// <summary>
    /// Interlaces the items of the specified sequences.
    /// If the sequences are of unequal length, the remaining elements of the longer sequence will end up at the end of the resulting sequence.
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
    /// Executes the specified <paramref name="action"/> on each element of the source collection.
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
    /// Executes the specified <paramref name="action"/> on each element of the source collection, incorporating each element's index in the <see cref="Action{T1, T2}"/>.
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
    /// Asynchronously executes the specified <paramref name="func"/> on each element of the source collection.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the collection.</typeparam>
    /// <param name="source">The source collection to iterate over.</param>
    /// <param name="func">The action to perform on each element of the source collection. It is passed each element in the source collection.</param>
    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> func)
    {
        foreach (var element in source)
        {
            await func(element);
        }
    }
    /// <summary>
    /// Asynchronously executes the specified <paramref name="func"/> on each element of the source collection, incorporating each element's index in the <see cref="Func{T1, T2, T3}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the collection.</typeparam>
    /// <param name="source">The source collection to iterate over.</param>
    /// <param name="func">The action to perform on each element of the source collection. It is passed each element and its index in the source collection.</param>
    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, int, Task> func)
    {
        var c = 0;
        foreach (var element in source)
        {
            await func(element, c++);
        }
    }

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
        var rng = range.GetRange(source.Count()).ToArray();
        return source.Index().Where(t => rng.Contains(t.Index)).Select(t => t.Item);
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
    /// <summary>
    /// Determines the mode of a sequence of values from a given key extracted from each value (that is, the value that appears most frequently). If multiple items share the highest frequency, the first one encountered is returned.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in <paramref name="source"/>.</typeparam>
    /// <typeparam name="TSelect">The Type of the elements <paramref name="selector"/> produces.</typeparam>
    /// <param name="source">The sequence of values to determine the mode of.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of <paramref name="source"/> and produces a value that is used to determine the mode of <paramref name="source"/>.</param>
    /// <param name="equalityComparer">An <see cref="IEqualityComparer{T}"/> of <typeparamref name="TSelect"/> to use when comparing values, or null to use the default <see cref="EqualityComparer{T}.Default"/> for the type of the values.</param>
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

        var enumerated = source as TSource[] ?? [.. source];
        if (enumerated.Length != 0)
        {
            return default;
        }
        var converted = enumerated.Select(selector).ToArray();

        var i = 0;
        return source.MaxBy(_ => converted.Count(item2 => equalityComparer.Equals(item2, converted[i++])));
    }

    /// <summary>
    /// Determines the mode of a sequence of values (that is, the value that appears most frequently). If multiple items share the highest frequency, the first one encountered is returned.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in <paramref name="source"/>.</typeparam>
    /// <param name="source">The sequence of values to determine the mode of.</param>
    /// <param name="equalityComparer">An <see cref="IEqualityComparer{T}"/> to use when comparing values, or null to use the default <see cref="EqualityComparer{T}.Default"/> for the type of the values.</param>
    /// <returns>The mode of <paramref name="source"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is empty.</exception>
    public static T Mode<T>(this IEnumerable<T> source, IEqualityComparer<T> equalityComparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        equalityComparer ??= EqualityComparer<T>.Default;

        var enumerated = source as T[] ?? [.. source];
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
        // 
        if (first == null || second == null)
        {
            return first == second;
        }
        if (ReferenceEquals(first, second))
        {
            return true;
        }
        if (first.TryGetNonEnumeratedCount(out var firstCount) && second.TryGetNonEnumeratedCount(out var secondCount) && firstCount != secondCount)
        {
            return false;
        }

        comparer ??= EqualityComparer<T>.Default;

        // The issue with this is that counts matter, meaning that HashSets created from the sequences may be equivalent, but that does not prove that the source sequences are

        var counts = new Dictionary<T, int>(comparer);

        foreach (var item in first)
        {
            CollectionsMarshal.GetValueRefOrAddDefault(counts, item, out _)++;
        }

        foreach (var item in second)
        {
            ref var count = ref CollectionsMarshal.GetValueRefOrNullRef(counts, item);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref count) || --count < 0)
            {
                // Item in second but not in first
                return false;
            }
        }

        // If counts is empty, sequences are equivalent
        return true;
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
        => source.Select(item => predicate(item) ? selector(item) : item);
    /// <summary>
    /// Conditionally projects elements from a sequence into a new form, transforming only items that satisfy a specified <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and its index in the <paramref name="source"/> collection and determines whether the element should be transformed.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence and its index in the <paramref name="source"/> collection, if it passes the condition encapsulated by <paramref name="predicate"/>, and produces a new value. Its type must be the same as the input sequence's.</param>
    /// <returns>The transformed elements.</returns>
    public static IEnumerable<T> SelectWhere<T>(this IEnumerable<T> source, Func<T, int, bool> predicate, Func<T, int, T> selector)
        => source.Select((item, index) => predicate(item, index) ? selector(item, index) : item);
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
        => source.Where(predicate).Select(selector);
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
        => source.Where(predicate).Select(selector);

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
    /// Filters a sequence of values by their type, omitting all objects of type <typeparamref name="TDerived"/>.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in the input sequence.</typeparam>
    /// <typeparam name="TDerived">The Type of the elements to exclude from the output sequence. Must be, derive from or implement <typeparamref name="TSource"/>. If <typeparamref name="TDerived"/> is not assignable to <typeparamref name="TSource"/>, the input sequence is returned as is.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>A sequence of all objects from <paramref name="source"/> that are not of type <typeparamref name="TDerived"/>.</returns>
    /// <remarks>
    /// <typeparamref name="TDerived"/> is not constrained with regards to <typeparamref name="TSource"/>, so that consuming code needn't check for type relationships before calling this method.
    /// </remarks>
    public static IEnumerable<TSource> NotOfType<TSource, TDerived>(this IEnumerable<TSource> source) => typeof(TDerived).IsAssignableTo(typeof(TSource)) ? source.Where(static i => i is not TDerived) : source;

    /// <summary>
    /// For each element in the <paramref name="source"/> sequence, combines it with each value from an<paramref name="other"/> sequence using a specified <paramref name="selector"/> function.
    /// This is essentially a cross join and may yield a large number of non-sensical results if the input sequences are large.
    /// </summary>
    /// <typeparam name="TSource">The Type of the elements in the input sequence.</typeparam>
    /// <typeparam name="TOther">The Type of the elements in the other sequence.</typeparam>
    /// <typeparam name="TResult">The Type of the elements the <paramref name="selector"/> produces.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <param name="other">The other sequence.</param>
    /// <param name="selector">The <see cref="Func{T1, T2, TResult}"/> that is passed each element of the input sequence and each element of the other sequence and produces a new value.</param>
    /// <returns>A sequence that contains the new values produced by the selector function.</returns>
    public static IEnumerable<TResult> Join<TSource, TOther, TResult>(this IEnumerable<TSource> source, IEnumerable<TOther> other, Func<TSource, TOther, TResult> selector)
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
    /// <returns>The single element in the input sequence, or <paramref name="defaultValue"/> if the sequence contains no or more than one element.</returns>
    public static T OnlyOrDefault<T>(this IEnumerable<T> source, T defaultValue = default)
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
    /// <returns>The single element in the input sequence that satisfies the <paramref name="predicate"/>, or <paramref name="defaultValue"/> if the sequence contains no or more than one element that satisfies the <paramref name="predicate"/>.</returns>
    public static T OnlyOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, T defaultValue = default)
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
    public static T ElementAtOrDefault<T>(this IEnumerable<T> source, int i, T defaultValue = default)
    {
        switch (source)
        {
            case null:
                throw new ArgumentNullException(nameof(source));
            case IReadOnlyList<T> list:
                return i >= 0 && i < list.Count ? list[i] : defaultValue;
            default:
                try
                {
                    return source.ElementAt(i);
                }
                catch
                {
                    return defaultValue;
                }
        }
    }

    /// <summary>
    /// Builds a <see cref="Dictionary{TKey, TValue}"/> where both type arguments are <typeparamref name="T"/> from the input sequence. It must contain a positive and even number of elements. The first half of the elements are used as keys, the second half as values.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>A <see cref="Dictionary{TKey, TValue}"/> built from the input sequence.</returns>
    /// <exception cref="ArgumentException">Thrown if the input sequence does not contain a positive and even number of elements.</exception>
    public static Dictionary<T, T> BuildDictionaryLinear<T>(this IEnumerable<T> source)
    {
        var enumerated = source as IReadOnlyList<T> ?? [.. source];
        if (enumerated.Count == 0 || enumerated.Count % 2 != 0)
        {
            throw new ArgumentException("The input sequence must contain an even number of elements.", nameof(source));
        }
        var result = new Dictionary<T, T>();
        var halfI = enumerated.Count / 2;
        for (var i = 0; i < enumerated.Count; i += 2)
        {
            result[enumerated[i]] = enumerated[i + halfI];
        }
        return result;
    }
    /// <summary>
    /// Builds a <see cref="Dictionary{TKey, TValue}"/> where both type arguments are <typeparamref name="T"/> from the input sequence. It must contain a positive and even number of elements. The elements are considered to be repeating key-value pairs.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The input sequence.</param>
    /// <returns>A <see cref="Dictionary{TKey, TValue}"/> built from the input sequence.</returns>
    /// <exception cref="ArgumentException">Thrown if the input sequence does not contain a positive and even number of elements.</exception>
    public static Dictionary<T, T> BuildDictionaryZipped<T>(this IEnumerable<T> source)
    {
        var enumerated = source as IReadOnlyList<T> ?? [.. source];
        if (enumerated.Count == 0 || enumerated.Count % 2 != 0)
        {
            throw new ArgumentException("The input sequence must contain a positive and even number of elements.", nameof(source));
        }
        var result = new Dictionary<T, T>();
        for (var i = 0; i < enumerated.Count; i += 2)
        {
            result[enumerated[i]] = enumerated[i + 1];
        }
        return result;
    }
    /// <summary>
    /// Builds a <see cref="Dictionary{TKey, TValue}"/> from two separate input sequences representing the keys and values respectively.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys in the input sequence.</typeparam>
    /// <typeparam name="TValue">The Type of the values in the input sequence.</typeparam>
    /// <param name="keys">The sequence of keys.</param>
    /// <param name="values">The sequence of values.</param>
    /// <returns>A <see cref="Dictionary{TKey, TValue}"/> built from the input sequences.</returns
    /// <exception cref="ArgumentException">Thrown if the input sequences do not have the same length.</exception>
    public static Dictionary<TKey, TValue> BuildDictionary<TKey, TValue>(this IEnumerable<TKey> keys, IEnumerable<TValue> values)
    {
        var keysEnumerated = keys as IReadOnlyList<TKey> ?? [.. keys];
        var valuesEnumerated = values as IReadOnlyList<TValue> ?? [.. values];
        if (keysEnumerated.Count != valuesEnumerated.Count)
        {
            throw new ArgumentException("The input sequences must have the same length.", nameof(keys));
        }
        var result = new Dictionary<TKey, TValue>();
        for (var i = 0; i < keysEnumerated.Count; i++)
        {
            result[keysEnumerated[i]] = valuesEnumerated[i];
        }
        return result;
    }
}