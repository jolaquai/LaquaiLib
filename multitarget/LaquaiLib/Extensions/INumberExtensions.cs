using System.Numerics;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="INumber{TSelf}"/>-implementing Types.
/// </summary>
public static class NumberExtensions
{
    extension<T>(T number) where T : IEqualityOperators<T, T, bool>, IBitwiseOperators<T, T, T>
    {
        /// <summary>
        /// Determines whether the specified number has the specified flag(s) set, that is, whether the bitwise AND of the number and the other number is equal to the other number.
        /// This is insanely slow compared to doing the bitwise operation yourself since this has to use interface binding.
        /// </summary>
        /// <typeparam name="T">The type of the number.</typeparam>
        /// <param name="number">The number.</param>
        /// <param name="other">The other number.</param>
        /// <returns>Whether the specified number has the specified flag(s) set.</returns>
        public bool HasFlag(T other) => (number & other) == other;
    }

    extension<T>(T number) where T : INumber<T>
    {
        /// <summary>
        /// Converts the specified number to its binary representation.
        /// </summary>
        /// <typeparam name="T">The type of the number.</typeparam>
        /// <param name="number">The number.</param>
        /// <returns>The binary representation of the specified number.</returns>
        public string AsBinary() => number.ToString("B", null);
        /// <summary>
        /// Converts the specified number to its hexadecimal representation.
        /// </summary>
        /// <typeparam name="T">The type of the number.</typeparam>
        /// <param name="number">The number.</param>
        /// <returns>The hexadecimal representation of the specified number.</returns>
        public string AsHex() => number.ToString("X", null);
    }
}
