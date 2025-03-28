﻿namespace LaquaiLib.Core;

/// <summary>
/// Contains configuration values used across all of <see cref="LaquaiLib"/>.
/// </summary>
public static class Configuration
{
    /// <summary>
    /// Gets or sets the maximum size of any <see langword="stackalloc"/> allocation in <see langword="byte"/>s.
    /// If not set explicitly, a safe limit is calculated for the current system. However, it is guaranteed to be at least 16 KB.
    /// </summary>
    public static int MaxStackallocSize
    {
        get => field == -1 ? field = Internal.GetMaxStackallocSize() : field;
        set => field = value;
    } = -1;
}
