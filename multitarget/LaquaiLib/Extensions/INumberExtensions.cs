using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for <see cref="INumber{TSelf}"/>-implementing Types.
/// </summary>
public static class NumberExtensions
{
    extension<T>(T number) where T : IEqualityOperators<T, T, bool>, IBitwiseOperators<T, T, T>
    {
        /// <summary>
        /// Determines whether the specified number has the specified flag(s) set, that is, whether the bitwise AND of the number and the other number is equal to the other number.
        /// This is insanely slow compared to doing the bitwise operation yourself since this has to use interface binding.
        /// </summary>
        /// <param name="other">The other number.</param>
        /// <returns>Whether the specified number has the specified flag(s) set.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasFlag(T other) => (number & other) == other;
    }

    extension<T>(T number) where T : INumber<T>
    {
        /// <summary>
        /// Converts the specified number to its binary representation.
        /// </summary>
        /// <returns>The binary representation of the specified number.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string AsBinary() => number.ToString("B", null);
        /// <summary>
        /// Converts the specified number to its hexadecimal representation.
        /// </summary>
        /// <returns>The hexadecimal representation of the specified number.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string AsHex() => number.ToString("X", null);
    }

    extension<T>(T number) where T : IBinaryInteger<T>
    {
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AlignUp(T alignment)
        {
            if (!BitOperations.IsPow2(long.CreateSaturating(alignment)))
                ThrowAlignmentNotPow2();

            var a = alignment - T.One;
            return (number + a) & ~a;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] private static void ThrowAlignmentNotPow2() => throw new ArgumentException("Alignment must be a power of two.");
}
