namespace LaquaiLib.Integral;

/// <summary>
/// Represents an integer that dynamically resizes to fit any integer value.
/// </summary>
public struct DynamicInt
{
    private static readonly int _baseSize = nint.Size;
    /// <summary>
    /// Gets the base size of <see cref="DynamicInt"/>. This is precisely equal to <see cref="nint.Size"/>.
    /// </summary>
    public static int BaseSize => _baseSize;
    /// <summary>
    /// Gets the base size of <see cref="DynamicInt"/> in bits.
    /// </summary>
    public static int BaseSizeBits => _baseSize * 8;
    private static readonly Type _baseType = nint.Size == 4 ? typeof(int) : typeof(long);
    /// <summary>
    /// Gets the built-in equivalent type of <see cref="DynamicInt"/>. This is <see cref="int"/> if <see cref="BaseSize"/> is 4, otherwise <see cref="long"/>.
    /// </summary>
    public static Type BaseEquivalent => _baseType;

    
}
