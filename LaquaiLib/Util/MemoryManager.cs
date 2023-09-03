using System.Reflection;

namespace LaquaiLib.Util;

/// <summary>
/// Provides methods and events for managing memory and working with the Garbage Collector (<see cref="GC"/>).
/// </summary>
public static class MemoryManager
{
    /// <summary>
    /// Sets a new memory limit for the application.
    /// </summary>
    /// <param name="limit">A 64-bit unsigned integer that represents the new memory limit in bytes.</param>
    public static void SetMemoryLimit(ulong limit)
    {
        AppContext.SetData("GCHeapHardLimit", limit);
        typeof(GC).GetMethod("_RefreshMemoryLimit", BindingFlags.NonPublic | BindingFlags.Static)
                  .Invoke(null, null);
    }

    /// <summary>
    /// Gets the current memory limit for the application in bytes.
    /// </summary>
    public static ulong GetMemoryLimit() => (ulong)AppContext.GetData("GCHeapHardLimit");

    /// <inheritdoc/>
    public static void e()
    {
        GCHandle
    }
}
