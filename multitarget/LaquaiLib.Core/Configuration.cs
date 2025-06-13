namespace LaquaiLib.Core;

/// <summary>
/// Contains configuration values used across all of <see cref="LaquaiLib"/>.
/// </summary>
public static class Configuration
{
    /// <summary>
    /// Gets or sets the maximum size of any <see langword="stackalloc"/> allocation in <see langword="byte"/>s.
    /// Note that if more than one thread simultaneously <see langword="stackalloc"/>s memory, this limit is per-thread, not global. Threads do not share stack space.
    /// If not set explicitly, a safe limit is calculated for the current system. However, it is guaranteed to be at least 16 KB.
    /// </summary>
    public static int MaxStackallocSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => field == -1 ? field = Internal.GetMaxStackallocSize() : field;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => field = value;
    } = -1;
}
