namespace LaquaiLib.Extensions.Memory.Linq;

public static partial class LinqMemoryExtensions
{
    extension<TSource>(ReadOnlySpan<TSource> source)
    {
        /// <inheritdoc cref="Enumerable.Cast{TResult}(IEnumerable)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Cast<TResult>(Span<TResult> destination)
        {
            if (destination.Length < source.Length)
            {
                throw new ArgumentException("Destination span is too short.", nameof(destination));
            }

            if (!typeof(TSource).IsAssignableTo(typeof(TResult)))
            {
                throw new InvalidCastException($"Cannot cast {typeof(TSource)} to {typeof(TResult)}.");
            }

            for (var i = 0; i < source.Length; i++)
            {
                destination[i] = (TResult)(object)source[i];
            }
            return source.Length;
        }
    }
}