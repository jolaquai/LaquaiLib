namespace LaquaiLib;

/// <summary>
/// Contains configuration values used across all of <see cref="LaquaiLib"/>.
/// </summary>
internal static class Config
{
    /// <summary>
    /// The maximum boundary for a single <see langword="stackalloc"/> in <see langword="byte"/>s.
    /// Set rather conservatively since there may be multiple threads or multiple <see langword="stackalloc"/>s per call chain.
    /// </summary>
    public const int MaxStackallocSize = 1024;
}
