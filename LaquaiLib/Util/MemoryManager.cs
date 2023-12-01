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
        // Make the GC aware of the new limit
        GC.RefreshMemoryLimit();
    }

    /// <summary>
    /// Gets the current memory limit for the application in bytes or <c>0</c> if no limit is set or the value could not be retrieved.
    /// </summary>
    public static ulong GetMemoryLimit() => AppContext.GetData("GCHeapHardLimit") is ulong limit ? limit : 0;
}
