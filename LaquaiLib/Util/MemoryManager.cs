namespace LaquaiLib.Util;

/// <summary>
/// Provides methods and events for managing memory, working with the Garbage Collector (<see cref="GC"/>) and allocating unmanaged memory.
/// </summary>
public static unsafe class MemoryManager
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

    /// <summary>
    /// Allocates the specified number of bytes of unmanaged memory and returns a <see langword="void"/> pointer to the first byte.
    /// </summary>
    /// <param name="bytes">The number of bytes to allocate.</param>
    /// <param name="pressure">Whether to inform the GC about the allocated memory using <see cref="GC.AddMemoryPressure(long)"/>.</param>
    /// <returns>A <see langword="void"/> pointer to the first byte of the allocated memory.</returns>
    public static void* MAlloc(int bytes, bool pressure = false)
    {
        if (pressure)
        {
            GC.AddMemoryPressure(bytes);
        }
        return (void*)Marshal.AllocHGlobal(bytes);
    }
    /// <summary>
    /// Allocates a region of memory large enough to accommodate <paramref name="count"/> instances of type <typeparamref name="T"/> and returns a pointer to the first byte.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type of the instances to allocate memory for.</typeparam>
    /// <param name="count">The number of instances to allocate memory for.</param>
    /// <param name="pressure">Whether to inform the GC about the allocated memory using <see cref="GC.AddMemoryPressure(long)"/>.</param>
    /// <returns>A <typeparamref name="T"/>-typed pointer to the first byte of the allocated memory.</returns>
    public static T* CAlloc<T>(int count, bool pressure = false)
        where T : unmanaged
    {
        var bytes = count * sizeof(T);
        if (pressure)
        {
            GC.AddMemoryPressure(bytes);
        }
        return (T*)Marshal.AllocHGlobal(bytes);
    }

    /// <summary>
    /// Resizes a previously allocated region of memory to the specified number of bytes and returns a <see langword="void"/> pointer to the first byte.
    /// </summary>
    /// <param name="ptr">A pointer to the first byte of the previously allocated memory.</param>
    /// <param name="bytes">The new size of the memory region in bytes.</param>
    /// <param name="oldLength">The old length of the block of memory that is being resized. Depending on the new size, either <see cref="GC.AddMemoryPressure(long)"/> or <see cref="GC.RemoveMemoryPressure(long)"/> is called using this value. If omitted or <c>== 0</c>, no action is taken.</param>
    /// <returns>A <see langword="void"/> pointer to the first byte of the resized memory region.</returns>
    public static void* ReMAlloc(void* ptr, int bytes, long oldLength = 0)
    {
        if (oldLength != 0)
        {
            if (bytes > oldLength)
            {
                GC.AddMemoryPressure(bytes - oldLength);
            }
            else if (bytes < oldLength)
            {
                GC.RemoveMemoryPressure(oldLength - bytes);
            }
        }
        return (void*)Marshal.ReAllocHGlobal((nint)ptr, bytes);
    }
    /// <summary>
    /// Resizes a previously allocated region of memory to the specified number of instances of type <typeparamref name="T"/> and returns a pointer to the first byte.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type of the instances to allocate memory for.</typeparam>
    /// <param name="ptr">A pointer to the first byte of the previously allocated memory.</param>
    /// <param name="count">The number of instances to allocate memory for.</param>
    /// <param name="oldCount">The number of instances the block of memory was previously assigned for. Depending on the new size, either <see cref="GC.AddMemoryPressure(long)"/> or <see cref="GC.RemoveMemoryPressure(long)"/> is called using this value. If omitted or <c>== 0</c>, no action is taken.</param>
    /// <returns>A <typeparamref name="T"/>-typed pointer to the first byte of the resized memory region.</returns>
    public static T* ReCAlloc<T>(T* ptr, int count, long oldCount = 0)
        where T : unmanaged
    {
        var bytes = count * sizeof(T);
        if (oldCount != 0)
        {
            var oldBytes = oldCount * sizeof(T);
            if (bytes > oldBytes)
            {
                GC.AddMemoryPressure(bytes - oldBytes);
            }
            else if (bytes < oldBytes)
            {
                GC.RemoveMemoryPressure(oldBytes - bytes);
            }
        }
        return (T*)Marshal.ReAllocHGlobal((nint)ptr, bytes);
    }

    /// <summary>
    /// Frees a previously allocated region of memory.
    /// </summary>
    /// <param name="ptr">A pointer to the first byte of the previously allocated memory.</param>
    /// <param name="pressure">The length of the block of memory that is being freed. If <c>&gt; 0</c>, <see cref="GC.RemoveMemoryPressure(long)"/> is called with this value.</param>
    public static void Free(void* ptr, long pressure = -1)
    {
        if (pressure > 0)
        {
            GC.RemoveMemoryPressure(pressure);
        }
        Marshal.FreeHGlobal((nint)ptr);
    }

    /// <summary>
    /// Returns a new <see langword="void"/> pointer that is offset from the specified pointer by the specified byte <paramref name="count"/>. That value may be negative.
    /// </summary>
    /// <param name="ptr">The pointer to offset.</param>
    /// <param name="count">The number of bytes to offset the pointer by.</param>
    /// <returns>A <see langword="void"/> pointer that is offset from <paramref name="ptr"/> by the specified <paramref name="count"/></returns>
    public static void* Next(void* ptr, int count = 1) => (void*)((nint)ptr + count);
    /// <summary>
    /// Returns a new <see langword="void"/> pointer that is offset from the specified pointer by the size of <typeparamref name="T"/> <paramref name="count"/> times. That value may be negative.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type to obtain the size of to calculate the offset.</typeparam>
    /// <param name="ptr">The pointer to offset.</param>
    /// <param name="count">The number of times the size of <typeparamref name="T"/> is added to the pointer.</param>
    /// <returns>The <see langword="void"/> pointer that is offset from <paramref name="ptr"/> by the size of <typeparamref name="T"/> <paramref name="count"/> times.</returns>
    public static void* Next<T>(void* ptr, int count = 1)
        where T : unmanaged
    {
        return (void*)((nint)ptr + (sizeof(T) * count));
    }
}
