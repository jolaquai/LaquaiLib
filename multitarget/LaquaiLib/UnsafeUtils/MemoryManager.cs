namespace LaquaiLib.UnsafeUtils;

/// <summary>
/// Provides methods and events for managing memory, working with the Garbage Collector (<see cref="GC"/>) and allocating unmanaged memory.
/// </summary>
public static unsafe class MemoryManager
{
    /// <summary>
    /// Sets a new memory limit for the application.
    /// </summary>
    /// <param name="limit">A 64-bit unsigned integer that represents the new memory limit in bytes.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetMemoryLimit(ulong limit)
    {
        AppContext.SetData("GCHeapHardLimit", limit);
        // Make the GC aware of the new limit
        GC.RefreshMemoryLimit();
    }
    /// <summary>
    /// Gets the current memory limit for the application in bytes or <c>0</c> if no limit is set or the value could not be retrieved.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetMemoryLimit() => AppContext.GetData("GCHeapHardLimit") is ulong limit ? limit : 0;

    /// <summary>
    /// Allocates the specified number of bytes of unmanaged memory and returns a <see langword="void"/> pointer to the first byte.
    /// </summary>
    /// <param name="bytes">The number of bytes to allocate.</param>
    /// <param name="pressure">Whether to inform the GC about the allocated memory using <see cref="GC.AddMemoryPressure(long)"/>.</param>
    /// <returns>A <see langword="void"/> pointer to the first byte of the allocated memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* UnsafeMAlloc(int bytes, bool pressure = false)
    {
        if (pressure)
            GC.AddMemoryPressure(bytes);
        return (void*)Marshal.AllocHGlobal(bytes);
    }
    /// <summary>
    /// Allocates a region of unmanaged memory large enough to accommodate <paramref name="count"/> instances of type <typeparamref name="T"/> and returns a pointer to the first byte.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type of the instances to allocate memory for.</typeparam>
    /// <param name="count">The number of instances to allocate memory for.</param>
    /// <param name="pressure">Whether to inform the GC about the allocated memory using <see cref="GC.AddMemoryPressure(long)"/>.</param>
    /// <returns>A <typeparamref name="T"/>-typed pointer to the first byte of the allocated memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* UnsafeCAlloc<T>(int count, bool pressure = false) where T : unmanaged
    {
        var bytes = count * sizeof(T);
        if (pressure)
            GC.AddMemoryPressure(bytes);
        return (T*)Marshal.AllocHGlobal(bytes);
    }
    /// <summary>
    /// Allocates a region of unmanaged memory large enough to accommodate <paramref name="count"/> instances of type <typeparamref name="T"/> and returns a <see langword="ref"/> to the first instance.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type of the instances to allocate memory for.</typeparam>
    /// <param name="count">The number of instances to allocate memory for.</param>
    /// <param name="pressure">Whether to inform the GC about the allocated memory using <see cref="GC.AddMemoryPressure(long)"/>.</param>
    /// <returns>A <typeparamref name="T"/>-typed managed pointer to the first instance in the allocated memory region.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T CAlloc<T>(int count, bool pressure = false) where T : unmanaged => ref Unsafe.AsRef<T>(UnsafeCAlloc<T>(count, pressure));
    /// <summary>
    /// Gets a <see cref="Span{T}"/> from a managed pointer to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type of the instances to create a span for.</typeparam>
    /// <param name="ptr">The managed pointer to the first instance of type <typeparamref name="T"/>.</param>
    /// <param name="count">The number of instances to include in the span.</param>
    /// <returns>The created <see cref="Span{T}"/>.</returns>
    public static Span<T> AsSpan<T>(ref this T ptr, int count) where T : unmanaged => new Span<T>(Unsafe.AsPointer(ref ptr), count);
    /// <summary>
    /// Gets a <see cref="Span{T}"/> from a pointer to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type of the instances to create a span for.</typeparam>
    /// <param name="ptr">The pointer to the first instance of type <typeparamref name="T"/>.</param>
    /// <param name="count">The number of instances to include in the span.</param>
    /// <returns>The created <see cref="Span{T}"/>.</returns>
    public static Span<T> AsSpan<T>(ref T* ptr, int count) where T : unmanaged => new Span<T>(ptr, count);
    /// <summary>
    /// Allocates a region of unmanaged memory large enough to accommodate <paramref name="count"/> instances of type <typeparamref name="T"/> and returns a <see cref="Span{T}"/> around it.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type of the instances to allocate memory for.</typeparam>
    /// <param name="count">The number of instances to allocate memory for.</param>
    /// <param name="pressure">Whether to inform the GC about the allocated memory using <see cref="GC.AddMemoryPressure(long)"/>.</param>
    /// <returns>The created <see cref="Span{T}"/>.</returns>
    public static Span<T> Allocate<T>(int count, bool pressure = false) where T : unmanaged => AsSpan(ref CAlloc<T>(count, pressure), count);

    /// <summary>
    /// Resizes a previously allocated region of memory to the specified number of bytes and returns a <see langword="void"/> pointer to the first byte.
    /// </summary>
    /// <param name="ptr">A pointer to the first byte of the previously allocated memory.</param>
    /// <param name="bytes">The new size of the memory region in bytes.</param>
    /// <param name="oldLength">The old length of the block of memory that is being resized. Depending on the new size, either <see cref="GC.AddMemoryPressure(long)"/> or <see cref="GC.RemoveMemoryPressure(long)"/> is called using this value. If omitted or <c>== 0</c>, no action is taken.</param>
    /// <returns>A <see langword="void"/> pointer to the first byte of the resized memory region.</returns>
    public static void* UnsafeReMAlloc(void* ptr, int bytes, long oldLength = 0)
    {
        if (oldLength != 0)
        {
            if (bytes > oldLength)
                GC.AddMemoryPressure(bytes - oldLength);
            else if (bytes < oldLength)
                GC.RemoveMemoryPressure(oldLength - bytes);
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
    public static T* UnsafeReCAlloc<T>(T* ptr, int count, long oldCount = 0) where T : unmanaged
    {
        var bytes = count * sizeof(T);
        if (oldCount != 0)
        {
            var oldBytes = oldCount * sizeof(T);
            if (bytes > oldBytes)
                GC.AddMemoryPressure(bytes - oldBytes);
            else if (bytes < oldBytes)
                GC.RemoveMemoryPressure(oldBytes - bytes);
        }
        return (T*)Marshal.ReAllocHGlobal((nint)ptr, bytes);
    }
    /// <summary>
    /// Resizes a previously allocated region of memory to the specified number of instances of type <typeparamref name="T"/> and returns a <see langword="ref"/> to the first instance.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type of the instances to allocate memory for.</typeparam>
    /// <param name="ptr">A pointer to the first byte of the previously allocated memory.</param>
    /// <param name="count">The number of instances to allocate memory for.</param>
    /// <param name="oldCount">The number of instances the block of memory was previously assigned for. Depending on the new size, either <see cref="GC.AddMemoryPressure(long)"/> or <see cref="GC.RemoveMemoryPressure(long)"/> is called using this value. If omitted or <c>== 0</c>, no action is taken.</param>
    /// <returns>A <typeparamref name="T"/>-typed managed pointer to the first instance in the allocated memory region.</returns>
    public static ref T ReCAlloc<T>(ref T ptr, int count, long oldCount = 0) where T : unmanaged
    {
        var bytes = count * sizeof(T);
        if (oldCount != 0)
        {
            var oldBytes = oldCount * sizeof(T);
            if (bytes > oldBytes)
                GC.AddMemoryPressure(bytes - oldBytes);
            else if (bytes < oldBytes)
                GC.RemoveMemoryPressure(oldBytes - bytes);
        }
        var pointer = (T*)Unsafe.AsPointer(ref ptr);
        return ref Unsafe.AsRef<T>((void*)Marshal.ReAllocHGlobal((nint)pointer, bytes));
    }

    /// <summary>
    /// Frees a previously allocated region of memory.
    /// </summary>
    /// <param name="ptr">A pointer to the first byte of the previously allocated memory.</param>
    /// <param name="pressure">The length of the block of memory that is being freed. If <c>&gt; 0</c>, <see cref="GC.RemoveMemoryPressure(long)"/> is called with this value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free(void* ptr, long pressure = -1)
    {
        if (pressure > 0)
            GC.RemoveMemoryPressure(pressure);
        Marshal.FreeHGlobal((nint)ptr);
    }
    /// <summary>
    /// Frees a previously allocated region of memory.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type of the instances that were allocated in the memory.</typeparam>
    /// <param name="ptr">A pointer to the first instance of type <typeparamref name="T"/> in the previously allocated memory.</param>
    /// <param name="pressure">The length of the block of memory that is being freed (that is, the number of instance of <typeparamref name="T"/>). If <c>&gt; 0</c>, <see cref="GC.RemoveMemoryPressure(long)"/> is called with this value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free<T>(T* ptr, long pressure = -1) where T : unmanaged => Free((void*)ptr, pressure * sizeof(T));
    /// <summary>
    /// Frees a previously allocated region of memory.
    /// </summary>
    /// <param name="ptr">A managed pointer to the first byte of the previously allocated memory.</param>
    /// <param name="pressure">The length of the block of memory that is being freed. If <c>&gt; 0</c>, <see cref="GC.RemoveMemoryPressure(long)"/> is called with this value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free<T>(ref T ptr, long pressure = -1) => Free(Unsafe.AsPointer(ref ptr), pressure);
    /// <summary>
    /// Frees a previously allocated region of memory.
    /// </summary>
    /// <param name="span">A <see cref="ReadOnlySpan{T}"/> around a block of memory that was previously allocated.</param>
    /// <param name="pressure">Whether to call <see cref="GC.RemoveMemoryPressure(long)"/> with the total byte count of the block of memory that is being freed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free<T>(ReadOnlySpan<T> span, bool pressure = false) where T : unmanaged => Free(ref MemoryMarshal.GetReference(span), pressure ? (long)span.Length * sizeof(T) : -1);

    /// <summary>
    /// Returns a new <see langword="void"/> pointer that is offset from the specified pointer by the specified byte <paramref name="count"/>. That value may be negative.
    /// </summary>
    /// <param name="ptr">The pointer to offset.</param>
    /// <param name="count">The number of bytes to offset the pointer by.</param>
    /// <returns>A <see langword="void"/> pointer that is offset from <paramref name="ptr"/> by the specified <paramref name="count"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* Next(void* ptr, int count = 1) => (void*)((nint)ptr + count);
    /// <summary>
    /// Returns a new <see langword="void"/> pointer that is offset from the specified pointer by the size of <typeparamref name="T"/> <paramref name="count"/> times. That value may be negative.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type to obtain the size of to calculate the offset.</typeparam>
    /// <param name="ptr">The pointer to offset.</param>
    /// <param name="count">The number of times the size of <typeparamref name="T"/> is added to the pointer.</param>
    /// <returns>The <see langword="void"/> pointer that is offset from <paramref name="ptr"/> by the size of <typeparamref name="T"/> <paramref name="count"/> times.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void* Next<T>(void* ptr, int count = 1) where T : unmanaged => (void*)((nint)ptr + (sizeof(T) * count));
    /// <summary>
    /// Returns a new pointer to <typeparamref name="T"/> that is offset from the specified pointer by the size of <typeparamref name="T"/> <paramref name="count"/> times. That value may be negative.
    /// </summary>
    /// <typeparam name="T">The <see langword="unmanaged"/> type pointed to.</typeparam>
    /// <param name="ptr">The pointer to offset.</param>
    /// <param name="count">The number of times the size of <typeparamref name="T"/> is added to the pointer.</param>
    /// <returns>A pointer to <typeparamref name="T"/> that is offset from <paramref name="ptr"/> by the size of <typeparamref name="T"/> <paramref name="count"/> times.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* Next<T>(T* ptr, int count = 1) where T : unmanaged => (T*)((nint)ptr + (sizeof(T) * count));
}
