namespace LaquaiLib.Core;

internal static class Internal
{
    /// <summary>
    /// Calculates a hard limit for stack allocations for the current system.
    /// </summary>
    /// <returns>A hard limit for stack allocations in bytes.</returns>
    public static int GetMaxStackallocSize()
    {
        // Get system memory info
        var systemMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);
        var processorCount = Environment.ProcessorCount;

        // Base calculation
        var baseLimit = (int)Math.Min(512, systemMemoryMB / 64); // KB

        // Scale down based on processor count (more threads = more potential stacks)
        var adjustedLimit = baseLimit / (int)Math.Sqrt(processorCount);

        // Hard boundaries
        var minLimit = 16; // KB - minimum even on resource-constrained systems
        var maxLimit = 512; // KB - maximum even on high-end systems

        return Math.Clamp(adjustedLimit, minLimit, maxLimit) * 1024; // Return in bytes
    }
}
