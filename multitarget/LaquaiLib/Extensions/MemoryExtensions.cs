using LaquaiLib.Collections.Enumeration;

namespace LaquaiLib.Extensions;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
/// Provides extension methods for the <see cref="Span{T}"/>, <see cref="ReadOnlySpan{T}"/>, <see cref="Memory{T}"/> and <see cref="ReadOnlyMemory{T}"/> types.
/// </summary>
public static partial class MemoryExtensions
{
    extension<T>(ReadOnlySpan<T> span)
    {
        /// <summary>
        /// Invokes the specified <paramref name="action"/> for each element in the <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action{T}"/> to invoke for each element.</param>
        /// <returns>The original <see cref="ReadOnlySpan{T}"/>.</returns>
        public ReadOnlySpan<T> ForEach(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            for (var i = 0; i < span.Length; i++)
            {
                action(span[i]);
            }
            return span;
        }
        /// <summary>
        /// Invokes the specified <paramref name="action"/> for each element in the <see cref="ReadOnlySpan{T}"/>, passing the element and its index.
        /// </summary>
        /// <param name="action">The <see cref="Action{T1, T2}"/> to invoke for each element.</param>
        /// <returns>The original <see cref="ReadOnlySpan{T}"/>.</returns>
        public ReadOnlySpan<T> ForEach(Action<T, int> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            for (var i = 0; i < span.Length; i++)
            {
                action(span[i], i);
            }
            return span;
        }

        /// <summary>
        /// Converts the elements of a <see cref="ReadOnlySpan{T}"/> using a <paramref name="selector"/> function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input span.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the output array.</typeparam>
        /// <param name="span">The input span.</param>
        /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input span and returns the result.</param>
        /// <returns>The array of the results.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult[] ToArray<TResult>(Func<T, TResult> selector)
        {
            var ret = new TResult[span.Length];
            for (var i = 0; i < span.Length; i++)
            {
                ret[i] = selector(span[i]);
            }
            return ret;
        }

#if !NET10_0_OR_GREATER
        /// <summary>
        /// Finds the index of the first occurrence of a specified value in the <see cref="ReadOnlySpan{T}"/> using an <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="equalityComparer">An <see cref="IEqualityComparer{T}"/> implementation to use for comparison. If <see langword="null"/>, the default equality comparer for <typeparamref name="T"/> is used.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="value"/> within the <see cref="ReadOnlySpan{T}"/>, if found; otherwise, -1.</returns>
        public int IndexOf(T value, IEqualityComparer<T> equalityComparer = null)
        {
            if (value is IEquatable<T> equatable)
            {
                for (var i = 0; i < span.Length; i++)
                {
                    if (equatable.Equals(span[i]))
                    {
                        return i;
                    }
                }
                return -1;
            }

            equalityComparer ??= EqualityComparer<T>.Default;
            for (var i = 0; i < span.Length; i++)
            {
                if (equalityComparer.Equals(span[i], value))
                {
                    return i;
                }
            }
            return -1;
        }
#endif

        /// <summary>
        /// Splits the specified <paramref name="span"/> into the specified destination <see cref="Span{T}"/>s based on the given <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">The Type of the items in the array.</typeparam>
        /// <param name="span">The <see cref="ReadOnlySpan{T}"/> to split.</param>
        /// <param name="whereTrue">The <see cref="Span{T}"/> that will contain all elements that match the given <paramref name="predicate"/>.</param>
        /// <param name="whereFalse">The <see cref="Span{T}"/> that will contain all elements that do not match the given <paramref name="predicate"/>.</param>
        /// <param name="predicate">The <see cref="Predicate{T}"/> that checks each element for a condition.</param>
        /// <remarks>
        /// <paramref name="whereTrue"/> and <paramref name="whereFalse"/>'s lengths are not checked against <paramref name="span"/>'s length.
        /// If they are too small, an <see cref="IndexOutOfRangeException"/> will be thrown by the runtime.
        /// </remarks>
        public void Split(Span<T> whereTrue, Span<T> whereFalse, Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            var trueIndex = 0;
            var falseIndex = 0;
            for (var i = 0; i < span.Length; i++)
            {
                if (predicate(span[i]))
                {
                    whereTrue[trueIndex] = span[i];
                    trueIndex++;
                }
                else
                {
                    whereFalse[falseIndex] = span[i];
                    falseIndex++;
                }
            }
        }
    }
    extension(ReadOnlySpan<byte> span)
    {
        /// <summary>
        /// Reads a <c>\0</c> or equivalently terminated (based on the specified <paramref name="encoding"/>) <see langword="string"/> from the specified <paramref name="span"/>. This terminator is stripped from the input.
        /// </summary>
        /// <param name="span">The <see cref="ReadOnlySpan{T}" /> from which to read.</param>
        /// <param name="ptr">The position at which to begin reading.</param>
        /// <param name="encoding">An <see cref="Encoding" /> instance to use to interpret the read <see langword="byte"/>s. Defaults to <see cref="Encoding.UTF8" /> (which might be undesirable for Interop scenarios...).</param>
        /// <returns>The reconstructed <see langword="string"/> or an empty <see langword="string"/> if the <see langword="byte"/> at <paramref name="ptr"/> was <c>0</c>. The length of the string is equal to the number by which <paramref name="ptr"/> was incremented.</returns>
        /// <remarks>
        /// Reading to the end of the <paramref name="span"/> without encountering a <c>\0</c> <see langword="byte"/> is considered illegal behavior and will throw an exception.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if the specified <paramref name="span"/> ends before a null terminator was encountered.</exception>
        public string ReadString(ref int ptr, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;

            var slice = span[ptr..];
            var end = 0;
            var terminatorLength = 0;

            // Search for null terminator based on encoding
            if (encoding == Encoding.UTF8 || encoding == Encoding.ASCII)
            {
                end = slice.IndexOf((byte)0);
                terminatorLength = 1;
                if (end == -1)
                {
                    throw new ArgumentException($"No null terminator found (started at position {ptr}).");
                }
            }
            else if (encoding == Encoding.Unicode) // UTF16-LE
            {
                // Look for two consecutive zero bytes (UTF-16 null char)
                terminatorLength = 2;
                for (var i = 0; i < slice.Length - 1; i += 2)
                {
                    if (slice[i] == 0 && slice[i + 1] == 0)
                    {
                        end = i;
                        break;
                    }
                }
                if (end == 0 && !(slice.Length >= 2 && slice.IsZero(2)))
                {
                    throw new ArgumentException($"No UTF-16 null terminator found (started at position {ptr}).");
                }
            }
            else if (encoding == Encoding.BigEndianUnicode) // UTF16-BE
            {
                terminatorLength = 2;
                for (var i = 0; i < slice.Length - 1; i += 2)
                {
                    if (slice[i] == 0 && slice[i + 1] == 0)
                    {
                        end = i;
                        break;
                    }
                }
                if (end == 0 && !(slice.Length >= 2 && slice.IsZero(2)))
                {
                    throw new ArgumentException($"No UTF-16 BE null terminator found (started at position {ptr}).");
                }
            }
            else if (encoding == Encoding.UTF32)
            {
                terminatorLength = 4;
                for (var i = 0; i < slice.Length - 3; i += 4)
                {
                    if (slice[i] == 0 && slice[i + 1] == 0 && slice[i + 2] == 0 && slice[i + 3] == 0)
                    {
                        end = i;
                        break;
                    }
                }
                if (end == 0 && !(slice.Length >= 4 && slice.IsZero(4)))
                {
                    throw new ArgumentException($"No UTF-32 null terminator found (started at position {ptr}).");
                }
            }
            else
            {
                throw new ArgumentException($"Unsupported encoding: {encoding.EncodingName}.");
            }

            // Handle empty string case
            if (end == 0)
            {
                ptr += terminatorLength; // Skip the terminator
                return "";
            }

            // Read the string and update pointer
            string result;
            unsafe
            {
                fixed (byte* p = &slice[0])
                {
                    result = encoding.GetString(p, end);
                }
            }

            ptr += end + terminatorLength; // Move past the string and terminator
            return result;
        }
        /// <summary>
        /// Reads a <c>\0</c> or equivalently terminated (based on the specified <paramref name="encoding"/>) <see langword="string"/> from the specified <paramref name="span"/>. This terminator is stripped from the input.
        /// </summary>
        /// <param name="span">The <see cref="ReadOnlySpan{T}" /> from which to read.</param>
        /// <param name="ptr">The position at which to begin reading.</param>
        /// <param name="encoding">An <see cref="Encoding" /> instance to use to interpret the read <see langword="byte"/>s. Defaults to <see cref="Encoding.UTF8" /> (which might be undesirable for Interop scenarios...).</param>
        /// <returns>The reconstructed <see langword="string"/> or an empty <see langword="string"/> if the <see langword="byte"/> at <paramref name="ptr"/> was <c>0</c>. The length of the string is equal to the number by which <paramref name="ptr"/> was incremented.</returns>
        /// <remarks>
        /// Reading to the end of the <paramref name="span"/> without encountering a <c>\0</c> <see langword="byte"/> is considered illegal behavior and will throw an exception.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if the specified <paramref name="span"/> ends before a null terminator was encountered.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString(int ptr, Encoding encoding = null) => span.ReadString(ref ptr, encoding);

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the specified <paramref name="span"/> at the specified <paramref name="ptr"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.</typeparam>
        /// <param name="span">The source <see cref="ReadOnlySpan{T}"/> of <see langword="byte"/>.</param>
        /// <param name="ptr">The pointer to the position in the <paramref name="span"/> from which to read the value.</param>
        /// <returns>An instance of <typeparamref name="T"/> read from the <paramref name="span"/>.</returns>
        /// <remarks>
        /// There are several special-cased types.
        /// <list type="bullet">
        /// <item/><see langword="string"/>s are read using <see cref="ReadString(ReadOnlySpan{byte}, ref int, Encoding)"/> instead of pointer casts.
        /// <item/><see langword="byte"/>s require no cast at all.
        /// <item/><see langword="bool"/>s are normalized to hold a value of exactly <c>1</c> or <c>0</c>.
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if <typeparamref name="T"/> is any reference type except <see langword="string"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="span"/> is empty.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="ptr"/> lies outside the bounds of the <paramref name="span"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the requested value of type <typeparamref name="T"/> is too large to be read from the <paramref name="span"/>.</exception>
        public T Read<T>(ref int ptr) where T : struct
        {
            var size = Unsafe.SizeOf<T>();
            if (size > span.Length - ptr)
            {
                throw new ArgumentOutOfRangeException($"The source span is too short to read a value of type {typeof(T).Name} (size: {size}, remaining: {span.Length - ptr}).");
            }

            var value = MemoryMarshal.Read<T>(span[ptr..(ptr + size)]);
            ptr += size;
            return value;
        }
        /// <summary>
        /// Reads <paramref name="count"/> consecutive values of type <typeparamref name="T"/> from the specified <paramref name="span"/> at the specified <paramref name="ptr"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values to read.</typeparam>
        /// <param name="span">The source <see cref="ReadOnlySpan{T}"/> of <see langword="byte"/>.</param>
        /// <param name="ptr">The pointer to the position in the <paramref name="span"/> from which to read the values.</param>
        /// <param name="count">The number of values to read.</param>
        /// <returns>An array of <typeparamref name="T"/> of type <paramref name="count"/> read from the <paramref name="span"/>.</returns>
        public T[] Read<T>(ref int ptr, int count)
            where T : struct
        {
            var ret = new T[count];
            for (var i = 0; i < count; i++)
            {
                ret[i] = span.Read<T>(ref ptr);
            }
            return ret;
        }
        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the specified <paramref name="span"/> at the specified <paramref name="ptr"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to read.</typeparam>
        /// <param name="span">The source <see cref="ReadOnlySpan{T}"/> of <see langword="byte"/>.</param>
        /// <param name="ptr">The pointer to the position in the <paramref name="span"/> from which to read the value.</param>
        /// <returns>An instance of <typeparamref name="T"/> read from the <paramref name="span"/>.</returns>
        /// <remarks>
        /// There are several special-cased types.
        /// <list type="bullet">
        /// <item/><see langword="string"/>s are read using <see cref="ReadString(ReadOnlySpan{byte}, ref int, Encoding)"/> instead of pointer casts.
        /// <item/><see langword="byte"/>s require no cast at all.
        /// <item/><see langword="bool"/>s are normalized to hold a value of exactly <c>1</c> or <c>0</c>.
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if <typeparamref name="T"/> is any reference type except <see langword="string"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="span"/> is empty.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="ptr"/> lies outside the bounds of the <paramref name="span"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the requested value of type <typeparamref name="T"/> is too large to be read from the <paramref name="span"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(int ptr) where T : struct => span.Read<T>(ref ptr);
        /// <summary>
        /// Reads <paramref name="count"/> consecutive values of type <typeparamref name="T"/> from the specified <paramref name="span"/> at the specified <paramref name="ptr"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values to read.</typeparam>
        /// <param name="span">The source <see cref="ReadOnlySpan{T}"/> of <see langword="byte"/>.</param>
        /// <param name="ptr">The pointer to the position in the <paramref name="span"/> from which to read the values.</param>
        /// <param name="count">The number of values to read.</param>
        /// <returns>An array of <typeparamref name="T"/> of type <paramref name="count"/> read from the <paramref name="span"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] Read<T>(int ptr, int count) where T : struct => span.Read<T>(ref ptr, count);
        private bool IsZero(int length)
        {
            if (span.Length < length)
            {
                return false;
            }

            for (var i = 0; i < length; i++)
            {
                if (span[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
    extension<T>(ReadOnlySpan<T> span) where T : IEquatable<T>
    {
        /// <summary>
        /// Returns a <see cref="SpanSplitEnumerable{T}"/> that enumerates the segments of a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/>s that are separated by any of the <typeparamref name="T"/>s specified by <paramref name="splits"/>.
        /// </summary>
        /// <param name="span">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
        /// <param name="splits">The <see langword="t"/>s to use as delimiters.</param>
        /// <returns>The created <see cref="SpanSplitEnumerable{T}"/>.</returns>
        /// <remarks><typeparamref name="T"/> must implement <see cref="IEquatable{T}"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanSplitEnumerable<T> EnumerateSplits(ReadOnlySpan<T> splits) => new SpanSplitEnumerable<T>(span, splits);
        /// <summary>
        /// Returns a <see cref="SpanSplitEnumerable{T}"/> that enumerates the segments of a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/>s that are separated by the specified <paramref name="sequence"/>.
        /// </summary>
        /// <param name="span">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
        /// <param name="sequence">The sequence of <typeparamref name="T"/>s to use as a delimiter.</param>
        /// <returns>The created <see cref="SpanSplitEnumerable{T}"/>.</returns>
        /// <remarks><typeparamref name="T"/> must implement <see cref="IEquatable{T}"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanSplitBySequenceEnumerable<T> EnumerateSplitsBySequence(ReadOnlySpan<T> sequence) => new SpanSplitBySequenceEnumerable<T>(span, sequence);
    }
    extension<T>(Span<T> span)
    {
#if !NET10_0_OR_GREATER
        /// <summary>
        /// Finds the index of the first occurrence of a specified value in the <see cref="ReadOnlySpan{T}"/> using an <see cref="IEqualityComparer{T}"/>.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="ReadOnlySpan{T}"/>.</param>
        /// <param name="equalityComparer">An <see cref="IEqualityComparer{T}"/> implementation to use for comparison. If <see langword="null"/>, the default equality comparer for <typeparamref name="T"/> is used.</param>
        /// <returns>The zero-based index of the first occurrence of <paramref name="value"/> within the <see cref="ReadOnlySpan{T}"/>, if found; otherwise, -1.</returns>
        public int IndexOf(T value, IEqualityComparer<T> equalityComparer = null)
        {
            if (value is IEquatable<T> equatable)
            {
                for (var i = 0; i < span.Length; i++)
                {
                    if (equatable.Equals(span[i]))
                    {
                        return i;
                    }
                }
                return -1;
            }

            equalityComparer ??= EqualityComparer<T>.Default;
            for (var i = 0; i < span.Length; i++)
            {
                if (equalityComparer.Equals(span[i], value))
                {
                    return i;
                }
            }
            return -1;
        }
#endif

        /// <summary>
        /// Invokes the specified <paramref name="action"/> for each element in the <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action{T}"/> to invoke for each element.</param>
        /// <returns>The original <see cref="Span{T}"/>.</returns>
        public Span<T> ForEach(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            for (var i = 0; i < span.Length; i++)
            {
                action(span[i]);
            }
            return span;
        }
        /// <summary>
        /// Invokes the specified <paramref name="action"/> for each element in the <see cref="Span{T}"/>, passing the element and its index.
        /// </summary>
        /// <param name="action">The <see cref="Action{T1, T2}"/> to invoke for each element.</param>
        /// <returns>The original <see cref="Span{T}"/>.</returns>
        public Span<T> ForEach(Action<T, int> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            for (var i = 0; i < span.Length; i++)
            {
                action(span[i], i);
            }
            return span;
        }
    }
    extension(Span<byte> span)
    {
        /// <summary>
        /// Formats the <see langword="byte"/>s of the specified <paramref name="data"/> instance into the <paramref name="span"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <typeparam name="T">The type of the <paramref name="data"/> instance.</typeparam>
        /// <param name="span">The target <see cref="Span{T}"/> of <see langword="byte"/>.</param>
        /// <param name="data">The <paramref name="data"/> instance to format into the <paramref name="span"/>.</param>
        /// <param name="index">The index at which to start writing the <paramref name="data"/> instance into the <paramref name="span"/>.</param>
        /// <returns>
        /// A slice of the input span that begins immediately after the last byte written. It may have length 0.
        /// This can be used to chain calls to this method.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the target <paramref name="span"/> cannot accomodate the specified <paramref name="data"/> instance.</exception>
        public Span<byte> Write<T>(T data, int index = 0)
            where T : unmanaged
        {
            if (index > 0)
            {
                span = span[index..];
            }

            var size = Marshal.SizeOf(data);
            if (span.Length < size)
            {
                throw new ArgumentException($"The target {nameof(span)} cannot accomodate the specified {nameof(data)} instance (need {size} bytes, have {span.Length}).");
            }

            // Cool thing is, Span pointer magic does all of what we need to do here
            unsafe
            {
                var bytes = MemoryMarshal.AsBytes(new ReadOnlySpan<T>(&data, 1));
                bytes.CopyTo(span);
            }

            return span[size..];
        }
    }
}
