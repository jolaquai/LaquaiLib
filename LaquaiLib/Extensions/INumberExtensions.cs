using System.Numerics;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="INumber{TSelf}"/>-implementing Types.
/// </summary>
public static class NumberExtensions
{
    /// <summary>
    /// Determines whether the specified number has the specified flag(s) set, that is, whether the bitwise AND of the number and the other number is equal to the other number.
    /// </summary>
    /// <typeparam name="T">The type of the number.</typeparam>
    /// <param name="number">The number.</param>
    /// <param name="other">The other number.</param>
    /// <returns>Whether the specified number has the specified flag(s) set.</returns>
    public static bool HasFlag<T>(this T number, T other)
        where T : INumber<T>, IBitwiseOperators<T, T, T>
    {
        return (number & other) == other;
    }
}
