using System.Numerics;
using System.Runtime.Intrinsics;

namespace LaquaiLib.Util;

/// <summary>
/// Provides extensions for filling <see cref="Span{T}"/>s with sequential numeric values.
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

    // Vector add wraps identically to unchecked scalar add, so each width can run over the whole span with no remainder masking;
    // widest-first cascade means every tier below the top processes at most one block (its Count is half the tier above it)
    private static void FillWrapping<T>(Span<T> destination, T start)
        where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        var written = 0;

        if (Vector512.IsHardwareAccelerated && Vector512<T>.IsSupported)
            written += FillBlocks512(destination[written..], ref start);

        if (Vector256.IsHardwareAccelerated && Vector256<T>.IsSupported)
            written += FillBlocks256(destination[written..], ref start);

        if (Vector128.IsHardwareAccelerated && Vector128<T>.IsSupported)
            written += FillBlocks128(destination[written..], ref start);

        for (; written < destination.Length; written++, start++)
            destination[written] = start;
    }

    private static int FillBlocks512<T>(Span<T> destination, ref T start)
        where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        var count = Vector512<T>.Count;
        var bound = destination.Length - count;
        if (bound < 0)
            return 0;

        var current = Vector512.CreateSequence(start, T.One);
        var blockStep = Vector512.Create(T.CreateTruncating(count));
        var written = 0;
        for (; written <= bound; written += count)
        {
            current.CopyTo(destination.Slice(written, count));
            current += blockStep;
        }
        start = current.GetElement(0);
        return written;
    }

    private static int FillBlocks256<T>(Span<T> destination, ref T start)
        where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        var count = Vector256<T>.Count;
        var bound = destination.Length - count;
        if (bound < 0)
            return 0;

        var current = Vector256.CreateSequence(start, T.One);
        var blockStep = Vector256.Create(T.CreateTruncating(count));
        var written = 0;
        for (; written <= bound; written += count)
        {
            current.CopyTo(destination.Slice(written, count));
            current += blockStep;
        }
        start = current.GetElement(0);
        return written;
    }

    private static int FillBlocks128<T>(Span<T> destination, ref T start)
        where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        var count = Vector128<T>.Count;
        var bound = destination.Length - count;
        if (bound < 0)
            return 0;

        var current = Vector128.CreateSequence(start, T.One);
        var blockStep = Vector128.Create(T.CreateTruncating(count));
        var written = 0;
        for (; written <= bound; written += count)
        {
            current.CopyTo(destination.Slice(written, count));
            current += blockStep;
        }
        start = current.GetElement(0);
        return written;
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
