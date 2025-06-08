using LaquaiLib.Interfaces;
using LaquaiLib.Util.Misc;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> Type.
/// </summary>
public static partial class IEnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        /// <summary>
        /// Attempts to retrieve a <see cref="ReadOnlySpan{T}"/> over the specified source collection.
        /// </summary>
        /// <param name="span">An <see langword="out"/> variable that receives the <see cref="ReadOnlySpan{T}"/> if the operation is successful.</param>
        /// <returns><see langword="true"/> if a <see cref="ReadOnlySpan{T}"/> could be created, otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetReadOnlySpan(out ReadOnlySpan<T> span)
        {
            // Since any Span<T> can also be cast to ReadOnlySpan<T>, we can use the same method to retrieve a ReadOnlySpan<T>
            // In here go only any cases where we can ONLY get a ReadOnlySpan<T> directly, but not a Span<T>
            var result = TryGetSpan(source, out var ros);
            if (result)
            {
                span = ros;
                return true;
            }
            span = default;
            return false;
        }
        /// <summary>
        /// Attempts to retrieve a <see cref="ReadOnlySpan{T}"/> over the specified source collection.
        /// </summary>
        /// <param name="span">An <see langword="out"/> variable that receives the <see cref="ReadOnlySpan{T}"/> if the operation is successful.</param>
        /// <returns><see langword="true"/> if a <see cref="ReadOnlySpan{T}"/> could be created, otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetSpan(out Span<T> span)
        {
            var result = true;
            switch (source)
            {
                case ISpanProvider<T> spanProvider:
                    span = spanProvider.Span;
                    break;
                case T[]:
                    span = Unsafe.As<T[]>(source);
                    break;
                case List<T>:
                    span = CollectionsMarshal.AsSpan(Unsafe.As<List<T>>(source));
                    break;
                default:
                    span = default;
                    result = false;
                    break;
            }
            return result;
        }

        /// <summary>
        /// Splits a sequence of values into two sequences based on a predicate.
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines which sequence the element should be yielded to.</param>
        /// <returns>A <see cref="ValueTuple{T1, T2}"/> containing the two sequences. The first collection contains all elements that satisfy the predicate, the second collection contains all remaining elements.</returns>
        public (List<T> True, List<T> False) Split(Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            var prealloc = source.TryGetNonEnumeratedCount(out var count) ? count : 2;

            var trueList = new List<T>(prealloc);
            var falseList = new List<T>(prealloc);
            trueList.AddRange(source.Where(predicate));
            falseList.AddRange(source.Where(i => !predicate(i)));
            return (trueList, falseList);
        }
        /// <summary>
        /// Halves the input sequence.
        /// </summary>
        /// <returns>A <see cref="ValueTuple{T1, T2}"/> that contains the two halves of the input sequence.</returns>
        public (T[] First, T[] Second) Halve()
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
        /// Selects a random element from the input sequence using <see cref="Random.Shared"/>.
        /// </summary>
        /// <returns>A random element from source.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Random() => source.Random(System.Random.Shared);
        /// <summary>
        /// Selects a random element from the input sequence using the specified <paramref name="random"/> instance.
        /// </summary>
        /// <param name="random">The <see cref="System.Random"/> instance to use for random number generation.</param>
        /// <returns>The random element from source.</returns>
        public T Random(Random random = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            random ??= System.Random.Shared;

            // Try to optimize for collections that expose Count/Length
            if (source is IReadOnlyCollection<T> collection)
            {
                var count = collection.Count;
                if (count == 0)
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }

                var index = random.Next(count);

                // Further optimize for indexed access
                if (source is IReadOnlyList<T> list)
                {
                    return list[index];
                }

                // Fall back to enumeration for collections without indexed access
                using var enumerator = source.GetEnumerator();
                for (var i = 0; i <= index; i++)
                {
                    _ = enumerator.MoveNext();
                }
                return enumerator.Current;
            }
            else
            {

                // Reservoir sampling for unknown-length sequences
                using var e = source.GetEnumerator();
                if (!e.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }

                var result = e.Current;
                var count = 1;

                while (e.MoveNext())
                {
                    count++;
                    if (random.Next(count) == 0) // 1/count probability
                    {
                        result = e.Current;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Executes the specified <paramref name="action"/> on each element of the source collection.
        /// </summary>
        /// <param name="action">The action to perform on each element of the source collection. It is passed each element in the source collection.</param>
        public void ForEach(Action<T> action)
        {
            if (source.TryGetNonEnumeratedCount(out var count) && count == 0)
            {
                return;
            }
            if (source.TryGetReadOnlySpan(out var span) && span.Length > 0)
            {
                for (var i = 0; i < span.Length; i++)
                {
                    action(span[i]);
                }
                return;
            }
            switch (source)
            {
                case IReadOnlyList<T> list:
                {
                    var length = list.Count;
                    for (var i = 0; i < length; i++)
                    {
                        action(list[i]);
                    }
                    return;
                }
                default:
                {
                    foreach (var element in source)
                    {
                        action(element);
                    }
                    return;
                }
            }
        }
        /// <summary>
        /// Executes the specified <paramref name="action"/> on each element of the source collection, incorporating each element's index in the <see cref="Action{T1, T2}"/>.
        /// </summary>
        /// <param name="action">The action to perform on each element of the source collection. It is passed each element and its index in the source collection.</param>
        public void ForEach(Action<T, int> action)
        {
            if (source.TryGetNonEnumeratedCount(out var count) && count == 0)
            {
                return;
            }
            if (source.TryGetReadOnlySpan(out var span) && span.Length > 0)
            {
                for (var i = 0; i < span.Length; i++)
                {
                    action(span[i], i);
                }
                return;
            }
            switch (source)
            {
                case IReadOnlyList<T> list:
                {
                    var length = list.Count;
                    for (var i = 0; i < length; i++)
                    {
                        action(list[i], i);
                    }
                    return;
                }
                default:
                {
                    var i = 0;
                    foreach (var element in source)
                    {
                        action(element, i++);
                    }
                    return;
                }
            }
        }
        /// <summary>
        /// Asynchronously executes the specified <paramref name="func"/> on each element of the source collection.
        /// </summary>
        /// <param name="func">The action to perform on each element of the source collection. It is passed each element in the source collection.</param>
        public async Task ForEachAsync(Func<T, Task> func)
        {
            switch (source)
            {
                case T[] array:
                {
                    for (var i = 0; i < array.Length; i++)
                    {
                        await func(array[i]).ConfigureAwait(false);
                    }
                    return;
                }
                case List<T> list:
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        await func(list[i]).ConfigureAwait(false);
                    }
                    return;
                }
                case IReadOnlyList<T> list:
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        await func(list[i]).ConfigureAwait(false);
                    }
                    return;
                }
                default:
                {
                    foreach (var element in source)
                    {
                        await func(element).ConfigureAwait(false);
                    }
                    return;
                }
            }
        }
        /// <summary>
        /// Asynchronously executes the specified <paramref name="func"/> on each element of the source collection, incorporating each element's index in the <see cref="Func{T1, T2, T3}"/>.
        /// </summary>
        /// <param name="func">The action to perform on each element of the source collection. It is passed each element and its index in the source collection.</param>
        public async Task ForEachAsync(Func<T, int, Task> func)
        {
            switch (source)
            {
                case T[] array:
                {
                    for (var i = 0; i < array.Length; i++)
                    {
                        await func(array[i], i).ConfigureAwait(false);
                    }
                    return;
                }
                case List<T> list:
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        await func(list[i], i).ConfigureAwait(false);
                    }
                    return;
                }
                case IReadOnlyList<T> list:
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        await func(list[i], i).ConfigureAwait(false);
                    }
                    return;
                }
                default:
                {
                    var i = 0;
                    foreach (var element in source)
                    {
                        await func(element, i++).ConfigureAwait(false);
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Enumerates over the elements in the input sequence in the specified <paramref name="range"/>.
        /// If sourc eis not indexable, the entire sequence is enumerated.
        /// </summary>
        /// <param name="range">A <see cref="Range"/> instance that indicates where the items to be extracted are located in the source.</param>
        public IEnumerable<T> GetRange(Range range)
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
        /// <returns><see langword="true"/> if all objects in the passed sourc ecollection are equal, otherwise <see langword="false"/>.</returns>
        public bool AllEqual()
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
        /// Determines the mode of a sequence of values (that is, the value that appears most frequently). If multiple items share the highest frequency, the first one encountered is returned.
        /// </summary>
        /// <param name="equalityComparer">An <see cref="IEqualityComparer{T}"/> to use when comparing values, or null to use the default <see cref="EqualityComparer{T}.Default"/> for the type of the values.</param>
        /// <returns>The mode of source.</returns>
        /// <exception cref="ArgumentNullException">Thrown if sourc eis empty.</exception>
        public T Mode(IEqualityComparer<T> equalityComparer = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            equalityComparer ??= EqualityComparer<T>.Default;

            if (!source.Any())
            {
                throw new ArgumentException("Sequence contains no elements.", nameof(source));
            }

            var counts = source.Counts(equalityComparer);
            return counts.MaxBy(t => t.Value).Key;
        }

        /// <summary>
        /// Samples a specified number of elements from the input sequence.
        /// </summary>
        /// <param name="itemCount">The number of elements to sample from the input sequence. If not specified, 1% of the input sequence's length is used.</param>
        /// <returns>The sampled elements.</returns>
        public T[] Sample(int itemCount = -1)
        {
            var enumerated = source.Shuffle().ToArray();
            return enumerated[..(itemCount > 0 ? itemCount : enumerated.Length / 100)];
        }
        /// <summary>
        /// Samples a specified number of elements from the input sequence, ensuring that the sampled elements remain in the same order as they were in the input sequence.
        /// </summary>
        /// <param name="itemCount">The number of elements to sample from the input sequence. If not specified, 1% of the input sequence's length is used.</param>
        /// <returns>The sampled elements.</returns>
        public IEnumerable<T> OrderedSample(int itemCount = -1)
        {
            var random = new Random();
            var sourceCount = source.Count();
            itemCount = itemCount > 0 ? itemCount : sourceCount / 100;
            var chunkSize = sourceCount / itemCount;

            return source.Chunk(chunkSize)
                         .Select(chunk => chunk[random.Next(0, chunk.Length)]);
        }

        /// <summary>
        /// Conditionally projects elements from a sequence into a new form, transforming only items that satisfy a specified <paramref name="predicate"/> and returning all other items unchanged.
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines whether the element should be transformed.</param>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence, if it passes the condition encapsulated by <paramref name="predicate"/>, and produces a new value. Its type must be the same as the input sequence's.</param>
        /// <returns>The transformed elements.</returns>
        public IEnumerable<T> SelectWhere(Func<T, bool> predicate, Func<T, T> selector)
            => source.Select(item => predicate(item) ? selector(item) : item);
        /// <summary>
        /// Conditionally projects elements from a sequence into a new form, transforming only items that satisfy a specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and its index in the sourc ecollection and determines whether the element should be transformed.</param>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence and its index in the sourc ecollection, if it passes the condition encapsulated by <paramref name="predicate"/>, and produces a new value. Its type must be the same as the input sequence's.</param>
        /// <returns>The transformed elements.</returns>
        public IEnumerable<T> SelectWhere(Func<T, int, bool> predicate, Func<T, int, T> selector)
            => source.Select((item, index) => predicate(item, index) ? selector(item, index) : item);

        /// <summary>
        /// Filters a sequence of values based on a predicate if <paramref name="expr"/> is <see langword="true"/>, otherwise returns exactly the input sequence.
        /// </summary>
        /// <param name="expr">A value that determines whether the input sequence should be filtered.</param>
        /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines whether the element should be yielded.</param>
        /// <returns>The filtered input sequence if <paramref name="expr"/> is <see langword="true"/>, otherwise the input sequence as is.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> IfWhere(bool expr, Func<T, bool> predicate) => expr ? source.Where(predicate) : source;
        /// <summary>
        /// Filters a sequence of values based on a predicate. The predicate's result is inverted.
        /// </summary>
        /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines whether the element should be yielded.</param>
        /// <returns>The elements in the input sequence that do not satisfy the predicate.</returns>
        /// <remarks>
        /// This has essentially no purpose but to avoid the need to create a lambda that inverts the result of the predicate.
        /// </remarks>
        public IEnumerable<T> WhereNot(Func<T, bool> predicate) => source.Where(i => !predicate(i));

        /// <summary>
        /// Indexes the elements in the input sequence; that is, each element is paired with its number of occurrences in the sequence.
        /// </summary>
        /// <returns>A sequence of key-value pairs where the key is an element from the input sequence and the value is the number of occurrences of that element in the input sequence.</returns>
        public IEnumerable<KeyValuePair<T, int>> Indexed() => source.CountBy(static i => i);

        /// <summary>
        /// Determines if a sequence is empty.
        /// </summary>
        /// <returns><see langword="true"/> if the input sequence is empty, otherwise <see langword="false"/>.</returns>
        public bool None() => !source.Any();
        /// <summary>
        /// Determines if a sequence contains no elements that satisfy a condition.
        /// </summary>
        /// <param name="predicate">The condition to check for.</param>
        /// <returns><see langword="true"/> if the input sequence contains no elements that satisfy the condition, otherwise <see langword="false"/>.</returns>
        public bool None(Func<T, bool> predicate) => !source.Any(predicate);

        // Imma be honest, I stole these right out of System.Linq
        /// <summary>
        /// Determines whether a sequence contains exactly one element and returns that element if so, otherwise returns the specified <paramref name="defaultValue"/>.
        /// This behaves exactly like <see cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, TSource)"/> without throwing exceptions.
        /// </summary>
        /// <param name="defaultValue">The value to return if the input sequence contains no elements or more than one element.</param>
        /// <returns>The single element in the input sequence, or <paramref name="defaultValue"/> if the sequence contains no or more than one element.</returns>
        public T OnlyOrDefault(T defaultValue = default)
        {
            if (source.TryGetNonEnumeratedCount(out var count) && count > 1)
            {
                return defaultValue;
            }

            if (source is IReadOnlyList<T> list)
            {
                switch (list.Count)
                {
                    case 0:
                        return defaultValue;
                    case 1:
                        return list[0];
                }
            }
            else
            {
                using var enumerator = source.GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return defaultValue;
                }
                var current = enumerator.Current;
                if (!enumerator.MoveNext())
                {
                    return current;
                }
            }
            return defaultValue;
        }
        /// <summary>
        /// Determines whether a sequence contains exactly one element that satisfies a <paramref name="predicate"/> and returns that element if so, otherwise returns the specified <paramref name="defaultValue"/>.
        /// This behaves exactly like <see cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool}, TSource)"/> without throwing exceptions.
        /// </summary>
        /// <param name="predicate">The condition to check for.</param>
        /// <param name="defaultValue">The value to return if the input sequence contains no elements or more than one element.</param>
        /// <returns>The single element in the input sequence that satisfies the <paramref name="predicate"/>, or <paramref name="defaultValue"/> if the sequence contains no or more than one element that satisfies the <paramref name="predicate"/>.</returns>
        public T OnlyOrDefault(Func<T, bool> predicate, T defaultValue = default)
        {
            if (source.TryGetReadOnlySpan(out var span))
            {
                for (var i = 0; i < span.Length; i++)
                {
                    var val = span[i];
                    if (!predicate(val))
                    {
                        continue;
                    }
                    for (i++; (uint)i < (uint)span.Length; i++)
                    {
                        if (predicate(span[i]))
                        {
                            return defaultValue;
                        }
                    }
                    return val;
                }
            }
            else
            {
                using var enumerator = source.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (!predicate(current))
                    {
                        continue;
                    }
                    while (enumerator.MoveNext())
                    {
                        if (predicate(enumerator.Current))
                        {
                            return defaultValue;
                        }
                    }
                    return current;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Attempts to retrieve the element at the specified index from the input sequence if that index is valid for the sequence, otherwise a default value is returned.
        /// </summary>
        /// <param name="i">The index of the element to retrieve.</param>
        /// <param name="defaultValue">The value to return if the index is invalid. Defaults to the <see langword="default"/> value of <typeparamref name="T"/>.</param>
        /// <returns>The element at the specified index if it is valid, otherwise the specified default value.</returns>
        public T ElementAtOrDefault(int i, T defaultValue = default)
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
        /// Finds the first index of the specified <paramref name="item"/> in the input sequence.
        /// </summary>
        /// <param name="item">The item to find the index of.</param>
        /// <param name="equalityComparer">An <see cref="IEqualityComparer{T}"/> to use when comparing values, or <see langword="null"/> to use the default <see cref="EqualityComparer{T}.Default"/> for the type of the values.</param>
        /// <returns>The index of the first occurrence of the specified <paramref name="item"/> in the input sequence, or -1 if the item is not found.</returns>
        public int IndexOf(T item, IEqualityComparer<T> equalityComparer = null)
        {
            equalityComparer ??= EqualityComparer<T>.Default;
            if (source.TryGetReadOnlySpan(out var span))
            {
#if NET9_0
                for (var i = 0; i < span.Length; i++)
                {
                    if (equalityComparer.Equals(span[i], item))
                    {
                        return i;
                    }
                }
#elif NET10_0_OR_GREATER
            return span.IndexOf(item, equalityComparer);
#endif
            }
            else if (source is IReadOnlyList<T> list)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (equalityComparer.Equals(list[i], item))
                    {
                        return i;
                    }
                }
            }
            else
            {
                var i = 0;
                foreach (var element in source)
                {
                    if (equalityComparer.Equals(element, item))
                    {
                        return i;
                    }
                    i++;
                }
            }
            return -1;
        }
        /// <summary>
        /// Finds the first starting index of the specified <paramref name="sequence"/> in the input sequence.
        /// </summary>
        /// <param name="sequence">The sequence to find the index of.</param>
        /// <param name="equalityComparer">An <see cref="IEqualityComparer{T}"/> to use when comparing values, or <see langword="null"/> to use the default <see cref="EqualityComparer{T}.Default"/> for the type of the values.</param>
        /// <returns>The index of the first occurrence of the specified <paramref name="sequence"/> in the input sequence, or -1 if the sequence is not found.</returns>
        public int IndexOf(IEnumerable<T> sequence, IEqualityComparer<T> equalityComparer = null)
        {
            if (sequence.TryGetNonEnumeratedCount(out var count))
            {
                switch (count)
                {
                    case 0:
                        return 0;
                    case 1:
                    {
                        if (sequence.TryGetReadOnlySpan(out var asSpan))
                        {
                            return source.IndexOf(asSpan[0], equalityComparer);
                        }
                        else if (source is IReadOnlyList<T> list)
                        {
                            return list.IndexOf(list[0], equalityComparer);
                        }
                        return source.IndexOf(sequence.First(), equalityComparer);
                    }
                }
            }

            equalityComparer ??= EqualityComparer<T>.Default;
            var enumerated = sequence as T[] ?? sequence.ToArray();
            if (enumerated.Length == 0)
            {
                return 0;
            }

            if (source.TryGetReadOnlySpan(out var span))
            {
#if NET9_0
                for (var i = 0; i < span.Length - enumerated.Length + 1; i++)
                {
                    if (span.Slice(i, enumerated.Length).SequenceEqual(enumerated, equalityComparer))
                    {
                        return i;
                    }
                }
#elif NET10_0_OR_GREATER
            return span.IndexOf(enumerated, equalityComparer);
#endif
            }
            else if (source is IReadOnlyList<T> list)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (list.Count - i < enumerated.Length)
                    {
                        return -1;
                    }
                    var found = true;
                    for (var j = 0; j < enumerated.Length; j++)
                    {
                        if (!equalityComparer.Equals(list[i + j], enumerated[j]))
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                    {
                        return i;
                    }
                }
            }
            else
            {
                // Handle general IEnumerable<T> case with enumerators
                if (enumerated.Length == 0)
                {
                    return 0;
                }

                var position = 0;
                using var sourceEnumerator = source.GetEnumerator();

                // Continue until we run out of elements
                while (sourceEnumerator.MoveNext())
                {
                    // For each position, check if the sequence matches
                    var currentPosition = position;
                    var matchPossible = true;

                    // Check first element
                    if (!equalityComparer.Equals(sourceEnumerator.Current, enumerated[0]))
                    {
                        position++;
                        continue;
                    }

                    // If sequence is just one element, we found a match
                    if (enumerated.Length == 1)
                    {
                        return currentPosition;
                    }

                    // Check remaining elements in the sequence
                    var sequenceIndex = 1;
                    var tempEnumerator = source.Skip(currentPosition + 1).GetEnumerator();

                    while (sequenceIndex < enumerated.Length && tempEnumerator.MoveNext())
                    {
                        if (!equalityComparer.Equals(tempEnumerator.Current, enumerated[sequenceIndex]))
                        {
                            matchPossible = false;
                            break;
                        }
                        sequenceIndex++;
                    }

                    // Check if we ran out of elements before completing the sequence check
                    if (sequenceIndex < enumerated.Length)
                    {
                        matchPossible = false;
                    }

                    if (matchPossible)
                    {
                        return currentPosition;
                    }

                    position++;
                }
            }
            return -1;
        }

        /// <summary>
        /// Determines whether the majority of a sequence's elements satisfy a condition.
        /// </summary>
        /// <param name="predicate">The condition to check for.</param>
        /// <returns><see langword="true"/> if the majority of the input sequence's elements satisfy the condition, otherwise <see langword="false"/>.</returns>
        public bool Majority(Func<T, bool> predicate)
        {
            if (!source.TryGetNonEnumeratedCount(out var total))
            {
                total = source.Count();
            }
            var count = source.Count(predicate);
            return count > total / 2;
        }

        /// <summary>
        /// Builds a <see cref="Dictionary{TKey, TValue}"/> where both type arguments are <typeparamref name="T"/> from the input sequence. It must contain a positive and even number of elements. The first half of the elements are used as keys, the second half as values.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> built from the input sequence.</returns>
        /// <exception cref="ArgumentException">Thrown if the input sequence does not contain a positive and even number of elements.</exception>
        public Dictionary<T, T> BuildDictionaryLinear()
        {
            var enumerated = source as IReadOnlyList<T> ?? [.. source];
            if (enumerated.Count == 0 || enumerated.Count % 2 != 0)
            {
                throw new ArgumentException("The input sequence must contain an even number of elements.", nameof(source));
            }
            var result = new Dictionary<T, T>();
            var halfI = enumerated.Count / 2;
            for (var i = 0; i < halfI; i++)
            {
                result[enumerated[i]] = enumerated[i + halfI];
            }
            return result;
        }
        /// <summary>
        /// Builds a <see cref="Dictionary{TKey, TValue}"/> where both type arguments are <typeparamref name="T"/> from the input sequence. It must contain a positive and even number of elements. The elements are considered to be repeating key-value pairs.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> built from the input sequence.</returns>
        /// <exception cref="ArgumentException">Thrown if the input sequence does not contain a positive and even number of elements.</exception>
        public Dictionary<T, T> BuildDictionaryZipped()
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
        /// Returns an <see cref="IAsyncEnumerable{T}"/> wrapper around the specified <see cref="IEnumerable{T}"/>. This is fundamentally different from <see cref="AsyncEnumerable.ToAsyncEnumerable{TSource}(IEnumerable{TSource})"/> in that every <c>MoveNext</c> call is wrapped in a new <see cref="Task"/> and <see langword="await"/>ed whereas the former still consumes each element synchronously.
        /// <para/><b>Warning!</b> Do NOT use this method right before an aggregating operation (such as <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/> or similar). Instead, call <see cref="AsyncEnumerable.ToAsyncEnumerable{TSource}(IEnumerable{TSource})"/>, then use a method such as <see cref="AsyncEnumerable.ToListAsync{TSource}(IAsyncEnumerable{TSource}, CancellationToken)"/>. This method is intended for use when <c>MoveNext</c> calls on an <see cref="IEnumerator{T}"/> are expected to be computationally expensive or time-consuming. To reduce overhead, usage of the asynchronous methods in <see cref="AsyncEnumerable"/> is recommended (which are optimized for their purpose).
        /// </summary>
        /// <returns>The sourc eas an <see cref="IAsyncEnumerable{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncEnumerable<T> AsAsynchronous() => new AsyncEnumerableWrapper<T>(source);

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> that enumerates the elements of the input sequences is turn; that is, the first element of the first sequence, the first element of the second sequence, the second element of the first sequence, the second element of the second sequence, and so on.
        /// If the sequences are of unequal length, the remaining elements of the longer sequence will end up at the end of the resulting sequence.
        /// </summary>
        /// <param name="second">The second sequence.</param>
        /// <returns>A single sequence that contains the elements of both input sequences, interlaced.</returns>
        public IEnumerable<T> Interlace(IEnumerable<T> second)
        {
            using var enumerator1 = source.GetEnumerator();
            using var enumerator2 = second.GetEnumerator();

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

        /// <summary>
        /// Determines whether two sequences are equivalent, that is, whether they contain the same elements in any order.
        /// </summary>
        /// <param name="second">The second sequence to compare.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> implementation to use when comparing values, or null to use the default <see cref="EqualityComparer{T}.Default"/> for the type of the values.</param>
        /// <returns><see langword="true"/> if the two source sequences are of equal length and are equivalent, otherwise <see langword="false"/>. If one of the sequences is <see langword="null"/>, both sequences must be <see langword="null"/> to be considered equivalent.</returns>
        public bool SequenceEquivalent(IEnumerable<T> second, IEqualityComparer<T> comparer = null)
        {
            if (source == null || second == null)
            {
                return source == second;
            }
            if (ReferenceEquals(source, second))
            {
                return true;
            }
            if (source.TryGetNonEnumeratedCount(out var firstCount) && second.TryGetNonEnumeratedCount(out var secondCount) && firstCount != secondCount)
            {
                return false;
            }

            comparer ??= EqualityComparer<T>.Default;

            // The issue with this is that counts matter, meaning that HashSets created from the sequences may be equivalent, but that does not prove that the source sequences are

            var counts = new Dictionary<T, int>(comparer);

            foreach (var item in source)
            {
                CollectionsMarshal.GetValueRefOrAddDefault(counts, item, out _)++;
            }

            foreach (var item in second)
            {
                ref var count = ref CollectionsMarshal.GetValueRefOrNullRef(counts, item);
                if (Unsafe.IsNullRef(ref count) || --count < 0)
                {
                    // Item in second but not in first
                    return false;
                }
            }

            // If counts is empty, sequences are equivalent
            return true;
        }

        /// <summary>
        /// Determines the mode of a sequence of values from a given key extracted from each value (that is, the value that appears most frequently).
        /// If multiple items share the highest frequency, the first one encountered is returned.
        /// </summary>
        /// <typeparam name="TSelect">The Type of the elements <paramref name="selector"/> produces.</typeparam>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of sourc eand produces a value that is used to determine the mode of source.</param>
        /// <param name="equalityComparer">An <see cref="IEqualityComparer{T}"/> of <typeparamref name="TSelect"/> to use when comparing values, or null to use the default <see cref="EqualityComparer{T}.Default"/> for the type of the values.</param>
        /// <returns>The mode of source.</returns>
        /// <exception cref="ArgumentNullException">Thrown if sourc eis empty.</exception>
        public T ModeBy<TSelect>(Func<T, TSelect> selector, IEqualityComparer<TSelect> equalityComparer = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            equalityComparer ??= EqualityComparer<TSelect>.Default;

            if (selector is null)
            {
                return source.Mode();
            }
            if (!source.Any())
            {
                throw new ArgumentException("Sequence contains no elements.", nameof(source));
            }

            var counts = source.CountsBy(selector, equalityComparer);
            return counts.MaxBy(t => t.Value).Key;
        }

        /// <summary>
        /// Conditionally projects elements from a sequence into a new form, transforming only items that satisfy a specified <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="TResult">The Type of the elements the <paramref name="selector"/> produces.</typeparam>
        /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and determines whether the element should be transformed.</param>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence, if it passes the condition encapsulated by <paramref name="predicate"/>, and produces a new value.</param>
        /// <returns>The transformed elements.</returns>
        public IEnumerable<TResult> SelectOnlyWhere<TResult>(Func<T, bool> predicate, Func<T, TResult> selector)
            => source.Where(predicate).Select(selector);
        /// <summary>
        /// Conditionally projects elements from a sequence into a new form, transforming only items that satisfy a specified <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="TResult">The Type of the elements the <paramref name="selector"/> produces.</typeparam>
        /// <param name="predicate">The <see cref="Predicate{T}"/> that is passed each element of the input sequence and its index in the sourc ecollection and determines whether the element should be transformed.</param>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence and its index in the sourc ecollection, if it passes the condition encapsulated by <paramref name="predicate"/>, and produces a new value.</param>
        /// <returns>The transformed elements.</returns>
        public IEnumerable<TResult> SelectOnlyWhere<TResult>(Func<T, int, bool> predicate, Func<T, int, TResult> selector)
            => source.Where(predicate).Select(selector);

        /// <summary>
        /// Filters a sequence of values by their type, omitting all objects of type <typeparamref name="TDerived"/>.
        /// </summary>
        /// <typeparam name="TDerived">The Type of the elements to exclude from the output sequence. Must be, derive from or implement <typeparamref name="T"/>. If <typeparamref name="TDerived"/> is not assignable to <typeparamref name="T"/>, the input sequence is returned as is.</typeparam>
        /// <returns>A sequence of all objects from sourc ethat are not of type <typeparamref name="TDerived"/>.</returns>
        /// <remarks>
        /// <typeparamref name="TDerived"/> is not constrained with regards to <typeparamref name="T"/>, so that consuming code needn't check for type relationships before calling this method.
        /// </remarks>
        public IEnumerable<T> NotOfType<TDerived>()
            => typeof(TDerived).IsAssignableTo(typeof(T)) ? source.Where(static i => i is not TDerived) : source;

        /// <summary>
        /// Builds a <see cref="Dictionary{TKey, TValue}"/> from two separate input sequences representing the keys and values respectively.
        /// </summary>
        /// <typeparam name="TValue">The Type of the values in the input sequence.</typeparam>
        /// <param name="keys">The sequence of keys.</param>
        /// <param name="values">The sequence of values.</param>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> built from the input sequences.</returns>
        /// <exception cref="ArgumentException">Thrown if the input sequences do not have the same length.</exception>
        public Dictionary<T, TValue> BuildDictionary<TValue>(IEnumerable<TValue> values)
        {
            var keysEnumerated = source as IReadOnlyList<T> ?? [.. source];
            var valuesEnumerated = values as IReadOnlyList<TValue> ?? [.. values];
            if (keysEnumerated.Count != valuesEnumerated.Count)
            {
                throw new ArgumentException("The input sequences must have the same length.", nameof(source));
            }
            var result = new Dictionary<T, TValue>();
            for (var i = 0; i < keysEnumerated.Count; i++)
            {
                result[keysEnumerated[i]] = valuesEnumerated[i];
            }
            return result;
        }
        /// <summary>
        /// Builds a <see cref="Dictionary{TKey, TValue}"/> from the input sequence, using the specified <paramref name="valueFactory"/> to generate values for each key.
        /// </summary>
        /// <typeparam name="TValue">The Type of the values in the output dictionary.</typeparam>
        /// <param name="keys">The input sequence of keys.</param>
        /// <param name="valueFactory">The <see cref="Func{T, TResult}"/> that is passed each key from the input sequence and produces a value for the output dictionary.</param>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> built from the input sequence.</returns>
        public Dictionary<T, TValue> MapTo<TValue>(Func<T, TValue> valueFactory) => source.ToDictionary(static key => key, valueFactory);

        /// <summary>
        /// Maps every element in the input sequence to a single value in the specified <paramref name="second"/> sequence. A <paramref name="predicate"/> decides
        /// </summary>
        /// <typeparam name="TOther">The Type of the elements in the <paramref name="second"/> sequence.</typeparam>
        /// <param name="second">The second sequence.</param>
        /// <param name="predicate">A <see cref="Func{T1, T2, TResult}"/> that, in turn, is passed an element from the sourc esequence and an element from the <paramref name="second"/> sequence and determines whether they should be paired. It must return <see langword="true"/> for exactly one combination.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of tuples where each tuple contains an element from the sourc esequence and an element from the <paramref name="second"/> sequence, the combination of which satisfied the <paramref name="predicate"/>.</returns>
        public IEnumerable<(T, TOther)> Correlate<TOther>(IEnumerable<TOther> second, Func<T, TOther, bool> predicate)
        {
            var mat = second as IReadOnlyCollection<TOther> ?? second.ToArray();
            return source.Select(f => (f, mat.Single(s => predicate(f, s))));
        }
    }

    extension<T>(IEnumerable<T> source) where T : unmanaged
    {
        /// <summary>
        /// Blits the elements in the input sequence into a sequence of bytes.
        /// <typeparamref name="T"/> must be an unmanaged Type.
        /// </summary>
        /// <returns>All elements in the input sequence, blitted into a sequence of bytes and concatenated.</returns>
        public IEnumerable<byte> Blitted()
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
                for (var i = 0; i < bytes.Length; i++)
                {
                    var b = bytes[i];
                    yield return b;
                }
            }
        }
    }

    extension<T>(IEnumerable<IEnumerable<T>> source)
    {
        /// <summary>
        /// Flattens a sequence of nested sequences of the same type <typeparamref name="T"/> into a single sequence without transformation.
        /// </summary>
        /// <returns>A sequence that contains all the elements of the nested sequences.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> SelectMany() => source.SelectMany(static item => item);
    }
}