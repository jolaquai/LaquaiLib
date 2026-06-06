using LaquaiLib.UnsafeUtils;

namespace LaquaiLib.Text;

/// <summary>
/// Contains utility methods for the <see cref="string"/> type.
/// </summary>
internal static class StringHelpers
{
    /// <summary>
    /// Allocates a <see langword="string"/> that is safe to be mutated by the caller.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string AllocString(int length) => new string('\0', length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref char GetCharRef(string str) => ref Unsafe.AsRef(in str.GetPinnableReference());
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Span<char> GetSpan(string str) => MemoryMarshal.CreateSpan(ref GetCharRef(str), str.Length);
}
