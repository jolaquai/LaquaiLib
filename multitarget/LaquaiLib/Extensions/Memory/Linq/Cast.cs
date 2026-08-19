namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(Span<TSource> source) where TSource : struct
    {
        /// <inheritdoc cref="Convert{TSource, TResult}(ReadOnlySpan{TSource}, Span{TResult})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Convert<TResult>(Span<TResult> destination) where TResult : struct
            => ((ReadOnlySpan<TSource>)source).Convert(destination);
        /// <inheritdoc cref="BitCast{TSource, TResult}(ReadOnlySpan{TSource}, Span{TResult})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BitCast<TResult>(Span<TResult> destination) where TResult : struct
            => ((ReadOnlySpan<TSource>)source).BitCast(destination);
    }
    extension<TSource>(Span<TSource> source) where TSource : class
    {
        /// <inheritdoc cref="Cast{TSource, TResult}(ReadOnlySpan{TSource}, Span{TResult})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Cast<TResult>(Span<TResult> destination) where TResult : class, TSource
            => ((ReadOnlySpan<TSource>)source).Cast(destination);
        /// <inheritdoc cref="ReinterpretCast{TSource, TResult}(ReadOnlySpan{TSource}, Span{TResult})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReinterpretCast<TResult>(Span<TResult> destination) where TResult : class
            => ((ReadOnlySpan<TSource>)source).ReinterpretCast(destination);
    }
    extension<TSource>(ReadOnlySpan<TSource> source) where TSource : struct
    {
        /// <summary>
        /// Converts each element of the source span to <typeparamref name="TResult"/> and stores the results in <paramref name="destination"/>.
        /// Uses memory copy optimization when types are identical.
        /// </summary>
        /// <typeparam name="TResult">The type to cast to. Must be a value type.</typeparam>
        /// <param name="destination">The span to store the cast results.</param>
        /// <returns>The number of elements written to <paramref name="destination"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="destination"/> is shorter than the source span.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Convert<TResult>(Span<TResult> destination) where TResult : struct
        {
            if (destination.Length < source.Length)
                throw new ArgumentException("Destination span is too short.", nameof(destination));

            if (typeof(TSource) == typeof(TResult))
                MemoryMarshal.Cast<TSource, TResult>(source).CopyTo(destination);
            else
                for (var i = 0; i < source.Length; i++)
                    destination[i] = Unsafe.As<TSource, TResult>(ref Unsafe.AsRef(in source[i]));
            return source.Length;
        }
        /// <summary>
        /// Performs bit-level reinterpretation of each element from <typeparamref name="TSource"/> to <typeparamref name="TResult"/>.
        /// No type compatibility checks are performed - data is copied at the bit level.
        /// </summary>
        /// <typeparam name="TResult">The type to reinterpret the data of the source span as. Must be a value type.</typeparam>
        /// <param name="destination">The span to store the results.</param>
        /// <returns>The number of elements written to <paramref name="destination"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="destination"/> is shorter than the source span.</exception>
        /// <remarks>
        /// This method assumes types have compatible memory layouts. Behavior of this method and of code using the results of calls where this condition is not met is considered undefined.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BitCast<TResult>(Span<TResult> destination) where TResult : struct
        {
            if (destination.Length < source.Length)
                throw new ArgumentException("Destination span is too short.", nameof(destination));

            MemoryMarshal.Cast<TSource, TResult>(source).CopyTo(destination);
            return source.Length;
        }
    }
    extension<TSource>(ReadOnlySpan<TSource> source) where TSource : class
    {
        /// <summary>
        /// Casts each element of the span to <typeparamref name="TResult"/> and stores the results in <paramref name="destination"/>.
        /// </summary>
        /// <typeparam name="TResult">The type to cast the elements to. Must be a reference type and compatible with <typeparamref name="TSource"/>.</typeparam>
        /// <param name="destination">The destination span to store the results.</param>
        /// <returns>The number of elements written to the <paramref name="destination"/> span.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="destination"/> span is shorter than the source span.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Cast<TResult>(Span<TResult> destination) where TResult : class, TSource
        {
            if (destination.Length < source.Length)
                throw new ArgumentException("Destination span is too short.", nameof(destination));

            for (var i = 0; i < source.Length; i++)
                destination[i] = (TResult)source[i];
            return source.Length;
        }
        /// <summary>
        /// Reinterprets the references of <typeparamref name="TSource"/> as references of <typeparamref name="TResult"/> and stores the results in <paramref name="destination"/>.
        /// </summary>
        /// <typeparam name="TResult">The type to cast the elements to. Must be a reference type.</typeparam>
        /// <param name="destination">The destination span to store the results.</param>
        /// <returns>The number of elements written to the destination span.</returns>
        /// <exception cref="ArgumentException">Thrown when the destination span is shorter than the source span.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReinterpretCast<TResult>(Span<TResult> destination) where TResult : class
        {
            if (destination.Length < source.Length)
                throw new ArgumentException("Destination span is too short.", nameof(destination));

            for (var i = 0; i < source.Length; i++)
                destination[i] = Unsafe.As<TResult>(source[i]);
            return source.Length;
        }
    }
}