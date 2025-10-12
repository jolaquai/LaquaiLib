using LaquaiLib.UnsafeUtils;

namespace LaquaiLib.Text;

/// <summary>
/// Contains utility methods for the <see cref="string"/> type.
/// </summary>
internal static class StringHelpers
{
    internal static unsafe string AllocString(int length)
    {
        var ptr = MemoryManager.UnsafeCAlloc<char>(length + 1);
        ptr[length] = '\0';
        return new string(ptr, 0, length);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref char GetCharRef(string str) => ref MemoryMarshal.GetReference(str.AsSpan());
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Span<char> GetSpan(string str) => MemoryMarshal.CreateSpan(ref GetCharRef(str), str.Length);
}
