using System.Numerics;

namespace LaquaiLib.Util;

/// <summary>
/// Provides extension methods for filling <see cref="Span{T}"/>s with sequential numeric values.
/// </summary>
public static class SpanFiller
{
    /// <summary>
    /// Fills <paramref name="destination"/> with sequential values of <typeparamref name="T"/>, beginning at <paramref name="start"/> and incrementing by <c>1</c> for each subsequent element.
    /// </summary>
    /// <typeparam name="T">The numeric type to fill <paramref name="destination"/> with.</typeparam>
    /// <param name="destination">The <see cref="Span{T}"/> to fill.</param>
    /// <param name="start">The value to assign to the first element. Defaults to <c>0</c>.</param>
    /// <param name="wrap">Whether the sequence wraps around to <typeparamref name="T"/>'s minimum value (equivalent to <c>0</c> for unsigned types) instead of throwing once it would exceed <typeparamref name="T"/>'s maximum value. Defaults to <see langword="true"/>.</param>
    /// <exception cref="OverflowException">Thrown if <paramref name="wrap"/> is <see langword="false"/> and the sequence exceeds the range of <typeparamref name="T"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FillSequential<T>(this Span<T> destination, T start = default, bool wrap = true)
        where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        if (destination.IsEmpty)
            return;

        if (wrap)
            FillWrapping(destination, start);
        else
            FillChecked(destination, start);
    }

    private static void FillWrapping<T>(Span<T> destination, T start)
        where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        var i = 0;
        if (Vector.IsHardwareAccelerated && Vector<T>.IsSupported && destination.Length >= Vector<T>.Count)
        {
            var current = Vector.CreateSequence(start, T.One);
            var blockStep = new Vector<T>(T.CreateTruncating(Vector<T>.Count));
            var vectorBound = destination.Length - Vector<T>.Count;
            for (; i <= vectorBound; i += Vector<T>.Count)
            {
                current.CopyTo(destination.Slice(i, Vector<T>.Count));
                current += blockStep;
            }
            // current already holds the values for the next (unwritten) block; vector add wraps identically to unchecked scalar add
            start = current[0];
        }

        for (; i < destination.Length; i++, start++)
            destination[i] = start;
    }

    private static void FillChecked<T>(Span<T> destination, T start)
        where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        for (var i = 0; i < destination.Length - 1; i++)
        {
            destination[i] = start;
            start = checked(start + T.One);
        }
        destination[^1] = start;
    }
}
