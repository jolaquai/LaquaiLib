using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using LaquaiLib.Collections.Enumeration;
using LaquaiLib.Core;
using LaquaiLib.Wrappers;

namespace LaquaiLib.Extensions;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

/// <summary>
/// Provides extension methods for the <see cref="Span{T}"/>, <see cref="ReadOnlySpan{T}"/>, <see cref="Memory{T}"/> and <see cref="ReadOnlyMemory{T}"/> types.
/// </summary>
public static partial class MemoryExtensions
{
    /// <summary>
    /// Converts the elements of a <see cref="ReadOnlySpan{T}"/> using a <paramref name="selector"/> function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the input span.</typeparam>
    /// <typeparam name="TResult">The type of the elements of the output array.</typeparam>
    /// <param name="span">The input span.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input span and returns the result.</param>
    /// <returns>The array of the results.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult[] ToArray<TSource, TResult>(this ReadOnlySpan<TSource> span, Func<TSource, TResult> selector)
    {
        var ret = new TResult[span.Length];
        for (var i = 0; i < span.Length; i++)
        {
            ret[i] = selector(span[i]);
        }
        return ret;
    }
    /// <summary>
    /// Converts the elements of a <see cref="ReadOnlyMemory{T}"/> using a <paramref name="selector"/> function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the input memory.</typeparam>
    /// <typeparam name="TResult">The type of the elements of the output array.</typeparam>
    /// <param name="memory">The input memory.</param>
    /// <param name="selector">A <see cref="Func{T, TResult}"/> that is passed each element of the input memory and returns the result.</param>
    /// <returns>The array of the results.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult[] ToArray<TSource, TResult>(this ReadOnlyMemory<TSource> memory, Func<TSource, TResult> selector) => memory.Span.ToArray(selector);

    // I don't know what's going on here, but static analysis seems to be broken
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE0060 // Remove unused parameter
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
    public static void Split<T>(this ReadOnlySpan<T> span, Span<T> whereTrue, Span<T> whereFalse, Func<T, bool> predicate)
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
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore IDE0060 // Remove unused parameter
    /// <summary>
    /// Splits the specified <paramref name="memory"/> into the specified destination <see cref="Memory{T}"/>s based on the given <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="memory">The <see cref="ReadOnlyMemory{T}"/> to split.</param>
    /// <param name="whereTrue">The <see cref="Memory{T}"/> that will contain all elements that match the given <paramref name="predicate"/>.</param>
    /// <param name="whereFalse">The <see cref="Memory{T}"/> that will contain all elements that do not match the given <paramref name="predicate"/>.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> that checks each element for a condition.</param>
    /// <remarks>
    /// <paramref name="whereTrue"/> and <paramref name="whereFalse"/>'s lengths are not checked against <paramref name="memory"/>'s length.
    /// If they are too small, an <see cref="IndexOutOfRangeException"/> will be thrown by the runtime.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Split<T>(this ReadOnlyMemory<T> memory, Memory<T> whereTrue, Memory<T> whereFalse, Func<T, bool> predicate) => memory.Span.Split(whereTrue.Span, whereFalse.Span, predicate);

    /// <summary>
    /// Returns a <see cref="SpanSplitEnumerator{T}"/> that enumerates the segments of a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/>s that are separated by any of the <typeparamref name="T"/>s specified by <paramref name="splits"/>.
    /// </summary>
    /// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
    /// <param name="splits">The <see langword="t"/>s to use as delimiters.</param>
    /// <returns>The created <see cref="SpanSplitEnumerator{T}"/>.</returns>
    /// <remarks><typeparamref name="T"/> must implement <see cref="IEquatable{T}"/>.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SpanSplitEnumerator<T> EnumerateSplits<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> splits) where T : IEquatable<T>
        => new SpanSplitEnumerator<T>(source, splits);
    /// <summary>
    /// Returns a <see cref="SpanSplitEnumerator{T}"/> that enumerates the segments of a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/>s that are separated by the specified <paramref name="sequence"/>.
    /// </summary>
    /// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
    /// <param name="sequence">The sequence of <typeparamref name="T"/>s to use as a delimiter.</param>
    /// <returns>The created <see cref="SpanSplitEnumerator{T}"/>.</returns>
    /// <remarks><typeparamref name="T"/> must implement <see cref="IEquatable{T}"/>.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SpanSplitBySequenceEnumerator<T> EnumerateSplitsBySequence<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> sequence) where T : IEquatable<T>
        => new SpanSplitBySequenceEnumerator<T>(source, sequence);
    /// <summary>
    /// Returns a <see cref="SpanSplitEnumerator{T}"/> that enumerates the segments of a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/>s that are separated by any of the <typeparamref name="T"/>s specified by <paramref name="splits"/>.
    /// </summary>
    /// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
    /// <param name="splits">The <see langword="t"/>s to use as delimiters.</param>
    /// <returns>The created <see cref="SpanSplitEnumerator{T}"/>.</returns>
    /// <remarks><typeparamref name="T"/> needn't implement <see cref="IEquatable{T}"/>. This has the unfortunate side effect of being significantly slower than the generic version that requires <see cref="IEquatable{T}"/>.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GeneralSpanSplitEnumerator<T> EnumerateSplitsGeneral<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> splits)
        => new GeneralSpanSplitEnumerator<T>(source, splits);
    /// <summary>
    /// Returns a <see cref="SpanSplitEnumerator{T}"/> that enumerates the segments of a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/>s that are separated by the specified <paramref name="sequence"/>.
    /// </summary>
    /// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
    /// <param name="sequence">The sequence of <typeparamref name="T"/>s to use as a delimiter.</param>
    /// <returns>The created <see cref="SpanSplitEnumerator{T}"/>.</returns>
    /// <remarks><typeparamref name="T"/> needn't implement <see cref="IEquatable{T}"/>. This has the unfortunate side effect of being significantly slower than the generic version that requires <see cref="IEquatable{T}"/>.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GeneralSpanSplitBySequenceEnumerator<T> EnumerateSplitsBySequenceGeneral<T>(this ReadOnlySpan<T> source, ReadOnlySpan<T> sequence)
        => new GeneralSpanSplitBySequenceEnumerator<T>(source, sequence);

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
    public static Span<byte> FormatInto<T>(this Span<byte> span, T data, int index = 0)
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
    /// <summary>
    /// Formats the <see langword="byte"/>s of the specified <paramref name="data"/> instance into the <paramref name="memory"/> at the specified <paramref name="index"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="data"/> instance.</typeparam>
    /// <param name="memory">The target <see cref="Memory{T}"/> of <see langword="byte"/>.</param>
    /// <param name="data">The <paramref name="data"/> instance to format into the <paramref name="memory"/>.</param>
    /// <param name="index">The index at which to start writing the <paramref name="data"/> instance into the <paramref name="memory"/>.</param>
    /// <returns>
    /// A slice of the input memory that begins immediately after the last byte written. It may have length 0.
    /// This can be used to chain calls to this method.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if the target <paramref name="memory"/> cannot accomodate the specified <paramref name="data"/> instance.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<byte> FormatInto<T>(this Memory<byte> memory, T data, int index = 0) where T : unmanaged
    {
        _ = memory.Span.FormatInto(data, index);
        return memory[Marshal.SizeOf(data)..];
    }

    /// <summary>
    /// Reads a <c>\0</c>-terminated <see langword="string"/> from the specified <paramref name="span"/>. This terminator is stripped from the input.
    /// </summary>
    /// <param name="span">The <see cref="ReadOnlySpan{T}" /> from which to read.</param>
    /// <param name="ptr">The position at which to begin reading.</param>
    /// <param name="encoding">An <see cref="Encoding" /> instance to use to interpret the read <see langword="byte"/>s. Defaults to <see cref="Encoding.UTF8" /> (which might be undesirable for Interop scenarios...).</param>
    /// <returns>The reconstructed <see langword="string"/> or an empty <see langword="string"/> if the <see langword="byte"/> at <paramref name="ptr"/> was <c>0</c>. The length of the string is equal to the number by which <paramref name="ptr"/> was incremented.</returns>
    /// <remarks>
    /// Reading to the end of the <paramref name="span"/> without encountering a <c>\0</c> <see langword="byte"/> is considered illegal behavior and will throw an exception.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if the specified <paramref name="span"/> ends before a null terminator was encountered.</exception>
    public static string ReadString(this ReadOnlySpan<byte> span, ref int ptr, Encoding encoding = null)
    {
        const byte zero = 0;
        encoding ??= Encoding.UTF8;

        var slice = span[ptr..];
        var end = slice.IndexOf(zero);
        switch (end)
        {
            case -1:
                throw new ArgumentException($"The specified span ends before a null terminator was encountered (started reading at {ptr}).");
            case 0:
                ptr++;
                return "";
            default:
                ptr += end + 1;
                unsafe
                {
                    fixed (byte* p = &slice[0])
                    {
                        return encoding.GetString(p, end);
                    }
                }
        }
    }
    /// <summary>
    /// Reads a <c>\0</c>-terminated <see langword="string"/> from the specified <paramref name="memory"/>. This terminator is stripped from the input.
    /// </summary>
    /// <param name="memory">The <see cref="ReadOnlyMemory{T}" /> from which to read.</param>
    /// <param name="ptr">The position at which to begin reading.</param>
    /// <param name="encoding">An <see cref="Encoding" /> instance to use to interpret the read <see langword="byte"/>s. Defaults to <see cref="Encoding.UTF8" /> (which might be undesirable for Interop scenarios...).</param>
    /// <returns>The reconstructed <see langword="string"/> or an empty <see langword="string"/> if the <see langword="byte"/> at <paramref name="ptr"/> was <c>0</c>. The length of the string is equal to the number by which <paramref name="ptr"/> was incremented.</returns>
    /// <remarks>
    /// Reading to the end of the <paramref name="memory"/> without encountering a <c>\0</c> <see langword="byte"/> is considered illegal behavior and will throw an exception.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if the specified <paramref name="memory"/> ends before a null terminator was encountered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadString(this ReadOnlyMemory<byte> memory, ref int ptr, Encoding encoding = null) => memory.Span.ReadString(ref ptr, encoding);

    /// <summary>
    /// Reads a <c>\0</c>-terminated <see langword="string"/> from the specified <paramref name="span"/>. This terminator is stripped from the input.
    /// </summary>
    /// <param name="span">The <see cref="ReadOnlySpan{T}" /> from which to read.</param>
    /// <param name="ptr">The position at which to begin reading.</param>
    /// <param name="encoding">An <see cref="Encoding" /> instance to use to interpret the read <see langword="byte"/>s. Defaults to <see cref="Encoding.UTF8" /> (which might be undesirable for Interop scenarios...).</param>
    /// <returns>The reconstructed <see langword="string"/> or an empty <see langword="string"/> if the <see langword="byte"/> at <paramref name="ptr"/> was <c>0</c>. The length of the string is equal to the number by which <paramref name="ptr"/> was incremented.</returns>
    /// <remarks>
    /// Reading to the end of the <paramref name="span"/> without encountering a <c>\0</c> <see langword="byte"/> is considered illegal behavior and will throw an exception.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if the specified <paramref name="span"/> ends before a null terminator was encountered.</exception>
    public static string ReadString(this ReadOnlySpan<byte> span, int ptr, Encoding encoding = null)
    {
        const byte zero = 0;
        encoding ??= Encoding.UTF8;

        var slice = span[ptr..];
        var end = slice.IndexOf(zero);
        switch (end)
        {
            case -1:
                throw new ArgumentException($"The specified span ends before a null terminator was encountered (started reading at {ptr}).");
            case 0:
                ptr++;
                return "";
            default:
                ptr += end + 1;
                unsafe
                {
                    fixed (byte* p = &slice[0])
                    {
                        return encoding.GetString(p, end);
                    }
                }
        }
    }
    /// <summary>
    /// Reads a <c>\0</c>-terminated <see langword="string"/> from the specified <paramref name="memory"/>. This terminator is stripped from the input.
    /// </summary>
    /// <param name="memory">The <see cref="ReadOnlyMemory{T}" /> from which to read.</param>
    /// <param name="ptr">The position at which to begin reading.</param>
    /// <param name="encoding">An <see cref="Encoding" /> instance to use to interpret the read <see langword="byte"/>s. Defaults to <see cref="Encoding.UTF8" /> (which might be undesirable for Interop scenarios...).</param>
    /// <returns>The reconstructed <see langword="string"/> or an empty <see langword="string"/> if the <see langword="byte"/> at <paramref name="ptr"/> was <c>0</c>. The length of the string is equal to the number by which <paramref name="ptr"/> was incremented.</returns>
    /// <remarks>
    /// Reading to the end of the <paramref name="memory"/> without encountering a <c>\0</c> <see langword="byte"/> is considered illegal behavior and will throw an exception.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if the specified <paramref name="memory"/> ends before a null terminator was encountered.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadString(this ReadOnlyMemory<byte> memory, int ptr, Encoding encoding = null) => memory.Span.ReadString(ptr, encoding);

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
    public static T Read<T>(this ReadOnlySpan<byte> span, ref int ptr)
        where T : struct
    {
        var typeOfT = typeof(T);
        if (!typeOfT.IsValueType && typeOfT != typeof(string))
        {
            throw new ArgumentException("The requested type of the value to be read must be a struct or string.");
        }
        if (span.Length == 0)
        {
            throw new ArgumentException("The span is empty.");
        }
        if (ptr >= span.Length)
        {
            throw new ArgumentException($"The pointer is outside the bounds of the span (length {span.Length}, pointer target is offset {ptr}).");
        }
        var sizeOfT = Marshal.SizeOf<T>();
        if (ptr + sizeOfT > span.Length)
        {
            throw new ArgumentException($"The requested value of type {typeOfT.Name} is too large to be read from the span (length {span.Length}, required range is [{ptr}..{ptr + sizeOfT}]).");
        }

        if (typeOfT == typeof(string))
        {
            return (T)(object)span.ReadString(ref ptr);
        }
        else if (typeOfT == typeof(byte))
        {
            return (T)(object)span[ptr++];
        }
        else if (typeOfT == typeof(bool))
        {
            return (T)(object)(span[ptr++] != 0);
        }

        T value;
        unsafe
        {
            // Marshal.SizeOf does exactly the same thing
            fixed (void* p = &span[ptr])
            {
                // Now just cast the pointer to the type, deref and increment the pointer's value
                var tPtr = (T*)p;
                value = *tPtr;
                ptr += sizeOfT;
            }
        }

        return value;
    }
    /// <summary>
    /// Reads a value of type <typeparamref name="T"/> from the specified <paramref name="memory"/> at the specified <paramref name="ptr"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to read.</typeparam>
    /// <param name="memory">The source <see cref="ReadOnlyMemory{T}"/> of <see langword="byte"/>.</param>
    /// <param name="ptr">The pointer to the position in the <paramref name="memory"/> from which to read the value.</param>
    /// <returns>An instance of <typeparamref name="T"/> read from the <paramref name="memory"/>.</returns>
    /// <remarks>
    /// There are several special-cased types.
    /// <list type="bullet">
    /// <item/><see langword="string"/>s are read using <see cref="ReadString(ReadOnlySpan{byte}, ref int, Encoding)"/> instead of pointer casts.
    /// <item/><see langword="byte"/>s require no cast at all.
    /// <item/><see langword="bool"/>s are normalized to hold a value of exactly <c>1</c> or <c>0</c>.
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if <typeparamref name="T"/> is any reference type except <see langword="string"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="memory"/> is empty.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="ptr"/> lies outside the bounds of the <paramref name="memory"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the requested value of type <typeparamref name="T"/> is too large to be read from the <paramref name="memory"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this ReadOnlyMemory<byte> memory, ref int ptr) where T : struct => memory.Span.Read<T>(ref ptr);
    /// <summary>
    /// Reads <paramref name="count"/> consecutive values of type <typeparamref name="T"/> from the specified <paramref name="span"/> at the specified <paramref name="ptr"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values to read.</typeparam>
    /// <param name="span">The source <see cref="ReadOnlySpan{T}"/> of <see langword="byte"/>.</param>
    /// <param name="ptr">The pointer to the position in the <paramref name="span"/> from which to read the values.</param>
    /// <param name="count">The number of values to read.</param>
    /// <returns>An array of <typeparamref name="T"/> of type <paramref name="count"/> read from the <paramref name="span"/>.</returns>
    public static T[] Read<T>(this ReadOnlySpan<byte> span, ref int ptr, int count)
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
    /// Reads <paramref name="count"/> consecutive values of type <typeparamref name="T"/> from the specified <paramref name="memory"/> at the specified <paramref name="ptr"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values to read.</typeparam>
    /// <param name="memory">The source <see cref="ReadOnlyMemory{T}"/> of <see langword="byte"/>.</param>
    /// <param name="ptr">The pointer to the position in the <paramref name="memory"/> from which to read the values.</param>
    /// <param name="count">The number of values to read.</param>
    /// <returns>An array of <typeparamref name="T"/> of type <paramref name="count"/> read from the <paramref name="memory"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Read<T>(this ReadOnlyMemory<byte> memory, ref int ptr, int count) where T : struct => memory.Span.Read<T>(ref ptr, count);

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
    public static T Read<T>(this ReadOnlySpan<byte> span, int ptr)
        where T : struct
    {
        var typeOfT = typeof(T);
        if (!typeOfT.IsValueType && typeOfT != typeof(string))
        {
            throw new ArgumentException("The requested type of the value to be read must be a struct or string.");
        }
        if (span.Length == 0)
        {
            throw new ArgumentException("The span is empty.");
        }
        if (ptr >= span.Length)
        {
            throw new ArgumentException($"The pointer is outside the bounds of the span (length {span.Length}, pointer target is offset {ptr}).");
        }
        var sizeOfT = Marshal.SizeOf<T>();
        if (ptr + sizeOfT > span.Length)
        {
            throw new ArgumentException($"The requested value of type {typeOfT.Name} is too large to be read from the span (length {span.Length}, required range is [{ptr}..{ptr + sizeOfT}]).");
        }

        if (typeOfT == typeof(string))
        {
            return (T)(object)span.ReadString(ref ptr);
        }
        else if (typeOfT == typeof(byte))
        {
            return (T)(object)span[ptr++];
        }
        else if (typeOfT == typeof(bool))
        {
            return (T)(object)(span[ptr++] != 0);
        }

        T value;
        unsafe
        {
            // Marshal.SizeOf does exactly the same thing
            fixed (void* p = &span[ptr])
            {
                // Now just cast the pointer to the type, deref and increment the pointer's value
                var tPtr = (T*)p;
                value = *tPtr;
                ptr += sizeOfT;
            }
        }

        return value;
    }
    /// <summary>
    /// Reads a value of type <typeparamref name="T"/> from the specified <paramref name="memory"/> at the specified <paramref name="ptr"/>.
    /// </summary
    /// <typeparam name="T">The type of the value to read.</typeparam>
    /// <param name="memory">The source <see cref="ReadOnlyMemory{T}"/> of <see langword="byte"/>.</param>
    /// <param name="ptr">The pointer to the position in the <paramref name="memory"/> from which to read the value.</param>
    /// <returns>An instance of <typeparamref name="T"/> read from the <paramref name="memory"/>.</returns>
    /// <remarks>
    /// There are several special-cased types.
    /// <list type="bullet">
    /// <item/><see langword="string"/>s are read using <see cref="ReadString(ReadOnlySpan{byte}, int, Encoding)"/> instead of pointer casts.
    /// <item/><see langword="byte"/>s require no cast at all.
    /// <item/><see langword="bool"/>s are normalized to hold a value of exactly <c>1</c> or <c>0</c>.
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if <typeparamref name="T"/> is any reference type except <see langword="string"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="memory"/> is empty.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="ptr"/> lies outside the bounds of the <paramref name="memory"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the requested value of type <typeparamref name="T"/> is too large to be read from the <paramref name="memory"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this ReadOnlyMemory<byte> memory, int ptr) where T : struct => memory.Span.Read<T>(ptr);
    /// <summary>
    /// Reads <paramref name="count"/> consecutive values of type <typeparamref name="T"/> from the specified <paramref name="span"/> at the specified <paramref name="ptr"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values to read.</typeparam>
    /// <param name="span">The source <see cref="ReadOnlySpan{T}"/> of <see langword="byte"/>.</param>
    /// <param name="ptr">The pointer to the position in the <paramref name="span"/> from which to read the values.</param>
    /// <param name="count">The number of values to read.</param>
    /// <returns>An array of <typeparamref name="T"/> of type <paramref name="count"/> read from the <paramref name="span"/>.</returns>
    public static T[] Read<T>(this ReadOnlySpan<byte> span, int ptr, int count)
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
    /// Reads <paramref name="count"/> consecutive values of type <typeparamref name="T"/> from the specified <paramref name="memory"/> at the specified <paramref name="ptr"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values to read.</typeparam>
    /// <param name="memory">The source <see cref="ReadOnlyMemory{T}"/> of <see langword="byte"/>.</param>
    /// <param name="ptr">The pointer to the position in the <paramref name="memory"/> from which to read the values.</param>
    /// <param name="count">The number of values to read.</param>
    /// <returns>An array of <typeparamref name="T"/> of type <paramref name="count"/> read from the <paramref name="memory"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Read<T>(this ReadOnlyMemory<byte> memory, int ptr, int count) where T : struct => memory.Span.Read<T>(ptr, count);

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/> to its equivalent string representation that is encoded with uppercase hex characters.
    /// </summary>
    /// <param name="bytes">The <see cref="byte"/> span to convert.</param>
    /// <returns>The string as described.</returns>
    /// <remarks>This method uses the internal <see cref="Convert.ToHexString(byte[])"/> method for the conversion, but its output is reversed appropriately to account for endianness differences.</remarks>
    public static string ToHexString(this ReadOnlySpan<byte> bytes)
    {
        var str = StringUtility.AllocString(bytes.Length * 2);
        using (var pin = PinWrapper.Pin(str))
        {
            var span = pin.AsSpan(str.Length);
            _ = Convert.TryToHexString(bytes, span, out _);
        }
        return str;
    }
    /// <summary>
    /// Converts a <see cref="ReadOnlyMemory{T}"/> of <see cref="byte"/> to its equivalent string representation that is encoded with uppercase hex characters.
    /// </summary>
    /// <param name="bytes">The <see cref="byte"/> memory to convert.</param>
    /// <returns>The string as described.</returns>
    /// <remarks>This method uses the internal <see cref="Convert.ToHexString(byte[])"/> method for the conversion, but its output is reversed appropriately to account for endianness differences.</remarks>
    public static string ToHexString(this ReadOnlyMemory<byte> bytes) => bytes.Span.ToHexString();

    /// <summary>
    /// Gets the index of the first occurrence of the specified <paramref name="item"/> in the <paramref name="span"/>.
    /// The type specified need not implement <see cref="IEquatable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="item">The item to find.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use for comparing elements. If omitted, <see cref="EqualityComparer{T}.Default"/> is used.</param>
    /// <returns>The index of the first occurrence of the specified <paramref name="item"/> in the <paramref name="span"/> or <c>-1</c> if the item was not found.</returns>
    /// <remarks>
    /// This method is not optimized like the framework-provided <see cref="Span{T}"/> methods.
    /// </remarks>
    public static int IndexOf<T>(this ReadOnlySpan<T> span, T item, IEqualityComparer<T> comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        for (var i = 0; i < span.Length; i++)
        {
            if (comparer.Equals(span[i], item))
            {
                return i;
            }
        }
        return -1;
    }
    /// <summary>
    /// Gets the index of the first occurrence of the specified <paramref name="item"/> in the <paramref name="memory"/>.
    /// The type specified need not implement <see cref="IEquatable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the memory.</typeparam>
    /// <param name="memory">The memory to search.</param>
    /// <param name="item">The item to find.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use for comparing elements. If omitted, <see cref="Comparer{T}.Default"/> is used.</param>
    /// <returns>The index of the first occurrence of the specified <paramref name="item"/> in the <paramref name="memory"/> or <c>-1</c> if the item was not found.</returns>
    public static int IndexOf<T>(this ReadOnlyMemory<T> memory, T item, IEqualityComparer<T> comparer = null) => memory.Span.IndexOf(item, comparer);
}
