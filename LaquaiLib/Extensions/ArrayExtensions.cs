using LaquaiLib.Collections.Enumeration;
using LaquaiLib.Interfaces;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for <see cref="Array"/> Types.
/// </summary>
public static partial class ArrayExtensions
{
    /// <summary>
    /// Reinterprets the reference to <paramref name="source"/> as <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>.
    /// This allows using Linq methods on multi-dimensional <see cref="Array"/>s.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="source">The <see cref="Array"/> to transform.</param>
    /// <returns>The reinterpreted reference to <paramref name="source"/>.</returns>
    public static IEnumerable<T> AsEnumerable<T>(this Array source) => new MultiDimArrayEnumerable<T>(source);
    /// <summary>
    /// Gets an <see cref="ISpanProvider{T}"/> for the specified <paramref name="source"/>.
    /// It must be disposed after use to release the array handle, otherwise this will result in a memory leak.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="source">The <see cref="Array"/> to get the <see cref="ISpanProvider{T}"/> for.</param>
    /// <returns>An <see cref="ISpanProvider{T}"/> implementation that provides a <see cref="Span{T}"/> over the array.</returns>
    public static ISpanProvider<T> GetSpanProvider<T>(this Array source) => new MultiDimArrayEnumerable<T>(source);
    /// <summary>
    /// Gets an <see cref="ISpanProvider{T}"/> for the specified <paramref name="source"/>.
    /// It must be disposed after use to release the array handle, otherwise this will result in a memory leak.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="source">The <see cref="Array"/> to get the <see cref="ISpanProvider{T}"/> for.</param>
    /// <returns>An <see cref="ISpanProvider{T}"/> implementation that provides a <see cref="Span{T}"/> over the array.</returns>
    public static IReadOnlySpanProvider<T> GetReadOnlySpanProvider<T>(this Array source) => new MultiDimArrayEnumerable<T>(source);
    /// <summary>
    /// Attempts to retrieve a <see cref="Span{T}"/> from the specified <paramref name="array"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to retrieve the span from.</param>
    /// <param name="spanProvider">An <see langword="out"/> variable that receives the <see cref="ISpanProvider{T}"/> for the array if it is not a one-dimensional array.</param>
    /// <param name="span">The <see cref="Span{T}"/> over the array.</param>
    /// <returns><see langword="true"/> if the span could be retrieved, otherwise <see langword="false"/>.</returns>
    public static bool TryGetSpan<T>(this Array array, out ISpanProvider<T> spanProvider, out Span<T> span)
    {
        if (array is T[] linear)
        {
            span = linear;
            spanProvider = null;
            return true;
        }
        try
        {
            spanProvider = array.GetSpanProvider<T>();
            span = spanProvider.Span;
            return true;
        }
        catch
        {
            span = default;
            spanProvider = null;
            return false;
        }
    }
    /// <summary>
    /// Attempts to retrieve a <see cref="ReadOnlySpan{T}"/> from the specified <paramref name="array"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to retrieve the span from.</param>
    /// <param name="rosProvider">An <see langword="out"/> variable that receives the <see cref="ISpanProvider{T}"/> for the array if it is not a one-dimensional array.</param>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> over the array.</param>
    /// <returns><see langword="true"/> if the span could be retrieved, otherwise <see langword="false"/>.</returns>
    public static bool TryGetReadOnlySpan<T>(this Array array, out IReadOnlySpanProvider<T> rosProvider, out ReadOnlySpan<T> span)
    {
        if (array is T[] linear)
        {
            span = linear;
            rosProvider = null;
            return true;
        }
        try
        {
            rosProvider = array.GetReadOnlySpanProvider<T>();
            span = rosProvider.ReadOnlySpan;
            return true;
        }
        catch
        {
            span = default;
            rosProvider = null;
            return false;
        }
    }

    /// <summary>
    /// Copies the elements of the specified <paramref name="source"/> array to the specified <paramref name="destination"/> array, starting at position <c>0</c> in all dimensions of both arrays.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the arrays.</typeparam>
    /// <param name="source">The array to copy elements from.</param>
    /// <param name="destination">The array to copy elements to.</param>
    public static void MultiDimCopyTo<T>(this Array source, Array destination) => source.MultiDimCopyTo<T>(0, destination, 0, source.Length);
    /// <summary>
    /// Copies a range of elements of the specified <paramref name="source"/> array to the specified <paramref name="destination"/> array, starting at position <c>0</c> in all dimensions of both arrays.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the arrays.</typeparam>
    /// <param name="source">The array to copy elements from.</param>
    /// <param name="destination">The array to copy elements to.</param>
    /// <param name="length">The total number of elements to copy.</param>
    public static void MultiDimCopyTo<T>(this Array source, Array destination, int length) => source.MultiDimCopyTo<T>(0, destination, 0, length);
    /// <summary>
    /// Copies a range of elements of the specified <paramref name="source"/> array to the specified <paramref name="destination"/> array.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the arrays.</typeparam>
    /// <param name="source">The array to copy elements from.</param>
    /// <param name="sourceIndex">The index in the source array at which copying begins.</param>
    /// <param name="destination">The array to copy elements to.</param>
    /// <param name="destinationIndex">The index in the destination array at which storing begins.</param>
    /// <param name="length">The total number of elements to copy.</param>
    public static void MultiDimCopyTo<T>(this Array source, long sourceIndex, Array destination, long destinationIndex, long length)
        => source.MultiDimCopyTo<T>((int)sourceIndex, destination, (int)destinationIndex, (int)length);
    /// <summary>
    /// Copies a range of elements of the specified <paramref name="source"/> array to the specified <paramref name="destination"/> array.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the arrays.</typeparam>
    /// <param name="source">The array to copy elements from.</param>
    /// <param name="sourceIndex">The index in the source array at which copying begins.</param>
    /// <param name="destination">The array to copy elements to.</param>
    /// <param name="destinationIndex">The index in the destination array at which storing begins.</param>
    /// <param name="length">The total number of elements to copy.</param>
    public static void MultiDimCopyTo<T>(this Array source, int sourceIndex, Array destination, int destinationIndex, int length)
    {
        if (source.GetType() != destination.GetType())
        {
            throw new ArgumentException("The specified source and destination arrays are not of the same type.", nameof(destination));
        }
        if (typeof(T) != source.GetType().GetElementType())
        {
            throw new ArgumentException("The specified type does not match the type of the source array.", nameof(T));
        }
        if (typeof(T) != destination.GetType().GetElementType())
        {
            throw new ArgumentException("The specified type does not match the type of the destination array.", nameof(T));
        }
        // No Array.Rank comparison since a 2x3 and a 3x2 array are compatible for copying as long as the total number of elements is the same, and since length is specified, this is guaranteed
        // As such, this is able to transpose and even linearize arrays into a T[]

        IReadOnlySpanProvider<T> sourceSpanProv = null;
        ISpanProvider<T> destSpanProv = null;
        try
        {
            TryGetReadOnlySpan(source, out sourceSpanProv, out var srcSpan);
            TryGetSpan(destination, out destSpanProv, out var destSpan);

            if (sourceIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), sourceIndex, "The specified source index cannot be less than zero.");
            }
            if (destinationIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex), destinationIndex, "The specified destination index cannot be less than zero.");
            }
            if (sourceIndex + length > srcSpan.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "The specified index and length does not specify a valid range in the source array.");
            }
            if (destinationIndex + length > destSpan.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "The specified index and length does not specify a valid range in the destination array.");
            }
            if (length > srcSpan.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "The specified length exceeds the length of the source array.");
            }
            if (length > destSpan.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "The specified length exceeds the length of the destination array.");
            }

            srcSpan[sourceIndex..length].CopyTo(destSpan[destinationIndex..length]);
        }
        finally
        {
            sourceSpanProv?.Dispose();
            destSpanProv?.Dispose();
        }
    }

    /// <summary>
    /// Copies the elements of the specified <paramref name="source"/> array to the specified <paramref name="destination"/> array, starting at position <c>0</c> in all dimensions of both arrays.
    /// If the copy attempt fails for any reason, the original data is restored to the <paramref name="destination"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the arrays.</typeparam>
    /// <param name="source">The array to copy elements from.</param>
    /// <param name="destination">The array to copy elements to.</param>
    public static void MultiDimConstrainedCopyTo<T>(this Array source, Array destination) => source.MultiDimConstrainedCopyTo<T>(0, destination, 0, source.Length);
    /// <summary>
    /// Copies a range of elements of the specified <paramref name="source"/> array to the specified <paramref name="destination"/> array, starting at position <c>0</c> in all dimensions of both arrays.
    /// If the copy attempt fails for any reason, the original data is restored to the <paramref name="destination"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the arrays.</typeparam>
    /// <param name="source">The array to copy elements from.</param>
    /// <param name="destination">The array to copy elements to.</param>
    /// <param name="length">The total number of elements to copy.</param>
    public static void MultiDimConstrainedCopyTo<T>(this Array source, Array destination, int length) => source.MultiDimConstrainedCopyTo<T>(0, destination, 0, length);
    /// <summary>
    /// Copies a range of elements of the specified <paramref name="source"/> array to the specified <paramref name="destination"/> array.
    /// If the copy attempt fails for any reason, the original data is restored to the <paramref name="destination"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the arrays.</typeparam>
    /// <param name="source">The array to copy elements from.</param>
    /// <param name="sourceIndex">The index in the source array at which copying begins.</param>
    /// <param name="destination">The array to copy elements to.</param>
    /// <param name="destinationIndex">The index in the destination array at which storing begins.</param>
    /// <param name="length">The total number of elements to copy.</param>
    public static void MultiDimConstrainedCopyTo<T>(this Array source, long sourceIndex, Array destination, long destinationIndex, long length)
        => source.MultiDimConstrainedCopyTo<T>((int)sourceIndex, destination, (int)destinationIndex, (int)length);
    /// <summary>
    /// Copies a range of elements of the specified <paramref name="source"/> array to the specified <paramref name="destination"/> array.
    /// If the copy attempt fails for any reason, the original data is restored to the <paramref name="destination"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the arrays.</typeparam>
    /// <param name="source">The array to copy elements from.</param>
    /// <param name="sourceIndex">The index in the source array at which copying begins.</param>
    /// <param name="destination">The array to copy elements to.</param>
    /// <param name="destinationIndex">The index in the destination array at which storing begins.</param>
    /// <param name="length">The total number of elements to copy.</param>
    public static void MultiDimConstrainedCopyTo<T>(this Array source, int sourceIndex, Array destination, int destinationIndex, int length)
    {
        if (source.GetType() != destination.GetType())
        {
            throw new ArgumentException("The specified source and destination arrays are not of the same type.", nameof(destination));
        }
        if (typeof(T) != source.GetType().GetElementType())
        {
            throw new ArgumentException("The specified type does not match the type of the source array.", nameof(T));
        }
        if (typeof(T) != destination.GetType().GetElementType())
        {
            throw new ArgumentException("The specified type does not match the type of the destination array.", nameof(T));
        }
        // No Array.Rank comparison since a 2x3 and a 3x2 array are compatible for copying as long as the total number of elements is the same, and since length is specified, this is guaranteed
        // As such, this is able to transpose and even linearize arrays into a T[]

        var destCopy = System.Runtime.CompilerServices.Unsafe.As<Array>(destination.Clone());

        ISpanProvider<T> sourceSpanProv = null;
        ISpanProvider<T> destSpanProv = null;
        try
        {
            ReadOnlySpan<T> srcSpan = default;
            Span<T> destSpan = default;

            // Avoid allocating the span provider (which will invariable get boxed because of the interface cast) if possible
            if (source is T[] linearSource)
            {
                srcSpan = linearSource.AsSpan();
            }
            else
            {
                sourceSpanProv = source.GetSpanProvider<T>();
                srcSpan = sourceSpanProv.Span;
            }
            if (destination is T[] linearDest)
            {
                destSpan = linearDest;
            }
            else
            {
                destSpanProv = destination.GetSpanProvider<T>();
                destSpan = destSpanProv.Span;
            }

            if (sourceIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), sourceIndex, "The specified source index cannot be less than zero.");
            }
            if (destinationIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex), destinationIndex, "The specified destination index cannot be less than zero.");
            }
            if (sourceIndex + length > srcSpan.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "The specified index and length does not specify a valid range in the source array.");
            }
            if (destinationIndex + length > destSpan.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "The specified index and length does not specify a valid range in the destination array.");
            }
            if (length > srcSpan.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "The specified length exceeds the length of the source array.");
            }
            if (length > destSpan.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "The specified length exceeds the length of the destination array.");
            }

            srcSpan[sourceIndex..length].CopyTo(destSpan[destinationIndex..length]);
        }
        catch
        {
            destCopy.MultiDimCopyTo<T>(0, destination, 0, destCopy.Length);
            throw;
        }
        finally
        {
            sourceSpanProv?.Dispose();
            destSpanProv?.Dispose();
        }
    }

    /// <summary>
    /// Attempts to retrieve the element at the specified index from the array if that index is valid for the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The array to retrieve the element from.</param>
    /// <param name="i">The index of the element to retrieve.</param>
    /// <param name="value">An <see langword="out"/> variable that receives the element at the specified index if it is valid.</param>
    /// <returns><see langword="true"/> if the index was valid and the element could be retrieved, otherwise <see langword="false"/>.</returns>
    public static bool TryGetAt<T>(this T[] array, int i, out T value)
    {
        if (i < array.Length && i >= 0)
        {
            value = array[i];
            return true;
        }
        value = default;
        return false;
    }
    /// <summary>
    /// Attempts to retrieve the element at specified index from the array if that index is valid for the array, otherwise returns the specified default value.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The array to retrieve the element from.</param>
    /// <param name="i">The index of the element to retrieve.</param>
    /// <param name="defaultValue">The default value to return if the index is invalid.</param>
    /// <returns>The element at the specified index if it is valid, otherwise the specified default value.</returns>
    public static T GetAtOrDefault<T>(this T[] array, int i, T defaultValue = default)
    {
        if (array.GetLowerBound(0) <= i && i <= array.GetUpperBound(0))
        {
            return array[i];
        }
        return defaultValue;
    }

    /// <summary>
    /// Determines whether the two arrays are equal in length and contain the same elements in the same order.
    /// The dimensions of the arrays need not match for this method to return <see langword="true"/>.
    /// </summary>
    /// <param name="first">The first array to compare.</param>
    /// <param name="other">The second array to compare.</param>
    /// <param name="equalityComparer">An optional <see cref="IEqualityComparer{T}"/> to use for comparing elements in the arrays.</param>
    /// <returns><see langword="true"/> if the arrays are equal in length and contain the same elements in the same order, otherwise <see langword="false"/>.</returns>
    public static bool SequenceEqual<T>(this Array first, Array other, IEqualityComparer<T> equalityComparer = null)
    {
        equalityComparer ??= EqualityComparer<T>.Default;

        if (first.GetType() != other.GetType())
        {
            return false;
        }
        if (first.Length != other.Length)
        {
            return false;
        }

        IReadOnlySpanProvider<T> firstSpanProv = null;
        IReadOnlySpanProvider<T> otherSpanProv = null;
        try
        {
            first.TryGetReadOnlySpan(out firstSpanProv, out var firstSpan);
            other.TryGetReadOnlySpan(out otherSpanProv, out var otherSpan);

            return firstSpan.SequenceEqual(otherSpan, equalityComparer);
        }
        finally
        {
            firstSpanProv?.Dispose();
            otherSpanProv?.Dispose();
        }
    }
}
