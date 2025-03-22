using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using LaquaiLib.Extensions;
using LaquaiLib.Util;

namespace LaquaiLib.Unsafe;

#pragma warning disable CA1069 // Enums values should not be duplicated

/// <summary>
/// Allows reading and writing arbitrary memory of the current or another process.
/// </summary>
internal partial class ProcessMemoryAccessor : IDisposable
{
    /// <summary>
    /// Gets whether the internals of <see cref="ProcessMemoryAccessor"/> will map and allow access to the memory spaces of system modules.
    /// This field is constant.
    /// </summary>
    public const bool AllowSystemModules = false;
    /// <summary>
    /// Gets whether the internals of <see cref="ProcessMemoryAccessor"/> will map and allow access to the memory spaces of modules that do not belong to the target process.
    /// This field is constant.
    /// </summary>
    public const bool AllowForeignModules = false;
    /// <summary>
    /// Gets whether <see cref="Process.EnterDebugMode"/> is called when an instance of this type is initialized.
    /// This field is constant.
    /// </summary>
    public const bool Force = true;

    private nint _handle;
    private readonly int _pid;
    private readonly Process _process;

    // The idea behind this setup is, you find the index of one and you can use the same index to get back the entire module
    private ProcessModule[] _modules;
    private string[] _moduleNames;
    private nint[] _moduleAddresses;

    private static volatile int _instanceCount;

    /// <summary>
    /// Initializes a new <see cref="ProcessMemoryAccessor"/> with the specified process ID.
    /// Failure to acquire a handle to the target process will throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="pid">The process ID of the target process.</param>
    /// <remarks>
    /// <see cref="ProcessMemoryAccessor"/> can only be initialized with PIDs since names may not be unique.
    /// <para/>Manual disposal of instances of this type is paramount to prevent resource leaks.
    /// </remarks>
    public ProcessMemoryAccessor(int pid)
    {
        Process.EnterDebugMode();
        Interlocked.Increment(ref _instanceCount);

        _pid = pid;
        EnsureAllowTarget();
        AcquireHandle();
        _process = Process.GetProcessById(_pid);
        MapModules();
    }
    /// <summary>
    /// Initializes a new <see cref="ProcessMemoryAccessor"/> with the specified process ID.
    /// Failure to acquire a handle to the target process will throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="process">A <see cref="Process"/> instance that represents the target process.</param>
    public ProcessMemoryAccessor(Process process) : this(process.Id)
    {
    }

    /// <summary>
    /// Attempts to find information about the approximate amount of space required to dump the entire owned memory of the current process into a buffer or file.
    /// </summary>
    /// <param name="memory">The amount of memory required to dump the entire owned memory of the current process.</param>
    /// <remarks>
    /// This requires exactly the same processing as <see cref="Dump(Span{T})"/> or <see cref="Dump(Stream)"/>, so guarding calls to those methods with this one is extremely wasteful.
    /// </remarks>
    public void DumpInfo(out long memory)
    {
        _process.Refresh();
        memory = _process.PrivateMemorySize64;
    }

    /// <summary>
    /// Attempts to read a single value of type <typeparamref name="T"/> from the target process at the specified address and offset.
    /// </summary>
    /// <typeparam name="T">The type of the value to read. Must be a <see langword="struct"/>.</typeparam>
    /// <param name="address">The address within the target process's memory space to read from.</param>
    /// <param name="offset">The offset from the address to read from.</param>
    /// <returns><see langword="true"/> if the read operation was successful, otherwise <see langword="false"/>.</returns>
    public bool TryRead<T>(nint address, nint offset, out T value) where T : struct
    {
        address += offset;

        // Find the module address is in
        var module = _modules.FirstOrDefault(m => m.BaseAddress <= address && address < m.BaseAddress + m.ModuleMemorySize)
            ?? throw new AccessViolationException("The specified address is not within the memory space of any module in the target process.");
        var baseAddress = module.BaseAddress;
        var buffer = MemoryManager.CreateBuffer(Marshal.SizeOf<T>());
        var succeeded = Interop.ReadProcessMemory(_handle, baseAddress, buffer, out _);
        if (!succeeded)
        {
            value = default;
            return false;
        }
        value = MemoryMarshal.Read<T>(buffer);
        return true;
    }
    /// <summary>
    /// Attempts to read a single value of type <typeparamref name="T"/> at the specified offset in the <see cref="Process.MainModule"/> of the target process.
    /// </summary>
    /// <param name="offset">The offset from the base address of the <see cref="Process.MainModule"/> to read from.</param>
    /// <typeparam name="T">The type of the value to read. Must be a <see langword="struct"/>.</typeparam>
    /// <returns><see langword="true"/> if the read operation was successful, otherwise <see langword="false"/>.</returns>
    public bool TryRead<T>(nint offset, out T value) where T : struct => TryRead(_process.MainModule.BaseAddress, offset, out value);
    /// <summary>
    /// Reads a single value of type <typeparamref name="T"/> from the target process at the specified address and offset.
    /// </summary>
    /// <typeparam name="T">The type of the value to read. Must be a <see langword="struct"/>.</typeparam>
    /// <param name="address">The address within the target process's memory space to read from.</param>
    /// <param name="offset">The offset from the address to read from.</param>
    /// <returns>The value of type <typeparamref name="T"/> read from the target process.</returns>
    public T Read<T>(nint address, nint offset) where T : struct
    {
        address += offset;

        // Find the module address is in
        var module = _modules.FirstOrDefault(m => m.BaseAddress <= address && address < m.BaseAddress + m.ModuleMemorySize)
            ?? throw new AccessViolationException("The specified address is not within the memory space of any module in the target process.");
        var baseAddress = module.BaseAddress;
        var buffer = MemoryManager.CreateBuffer(Marshal.SizeOf<T>());
        var succeeded = Interop.ReadProcessMemory(_handle, baseAddress, buffer, out _);
        if (!succeeded)
        {
            ThrowForLastError("Failed to read memory from the target process.");
        }
        return MemoryMarshal.Read<T>(buffer);
    }
    /// <summary>
    /// Reads a single value of type <typeparamref name="T"/> at the specified offset in the <see cref="Process.MainModule"/> of the target process.
    /// </summary>
    /// <param name="offset">The offset from the base address of the <see cref="Process.MainModule"/> to read from.</param>
    /// <typeparam name="T">The type of the value to read. Must be a <see langword="struct"/>.</typeparam>
    public T Read<T>(nint offset) where T : struct => Read<T>(_process.MainModule.BaseAddress, offset);

    /// <summary>
    /// Attempts to read as many bytes as will fit into <paramref name="destination"/> from the target process's memory space at the specified address and offset.
    /// </summary>
    /// <param name="address">The address within the target process's memory space to read from.</param>
    /// <param name="offset">The offset from the address to read from.</param>
    /// <param name="destination">The span to write the value to.</param>
    /// <returns><see langword="true"/> if the read operation was successful, otherwise <see langword="false"/>.</returns>
    public bool TryRead(nint address, nint offset, Span<byte> destination)
    {
        address += offset;

        // Find the module address is in
        var module = _modules.FirstOrDefault(m => m.BaseAddress <= address && address < m.BaseAddress + m.ModuleMemorySize)
            ?? throw new AccessViolationException("The specified address is not within the memory space of any module in the target process.");
        var baseAddress = module.BaseAddress;
        // If you define structs that would blow the stack, you deserve what's coming to you
        return Interop.ReadProcessMemory(_handle, baseAddress, destination, out _);
    }
    /// <summary>
    /// Attempts to read as many bytes as will fit into <paramref name="destination"/> at the specified offset in the <see cref="Process.MainModule"/> of the target process.
    /// </summary>
    /// <returns><see langword="true"/> if the read operation was successful, otherwise <see langword="false"/>.</returns>
    public bool TryRead(nint offset, Span<byte> destination) => TryRead(_process.MainModule.BaseAddress, offset, destination);
    /// <summary>
    /// Reads as many bytes as will fit into <paramref name="destination"/> from the target process's memory space at the specified address and offset.
    /// </summary>
    /// <param name="address">The address within the target process's memory space to read from.</param>
    /// <param name="offset">The offset from the address to read from.</param>
    /// <param name="destination">The span to write the data into.</param>
    public void Read(nint address, nint offset, Span<byte> destination)
    {
        address += offset;

        // Find the module address is in
        var module = _modules.FirstOrDefault(m => m.BaseAddress <= address && address < m.BaseAddress + m.ModuleMemorySize)
            ?? throw new AccessViolationException("The specified address is not within the memory space of any module in the target process.");
        var baseAddress = module.BaseAddress;
        if (!Interop.ReadProcessMemory(_handle, baseAddress, destination, out _))
        {
            ThrowForLastError("Failed to read memory from the target process.");
        }
    }
    /// <summary>
    /// Reads as many bytes as will fit into <paramref name="destination"/> at the specified offset in the <see cref="Process.MainModule"/> of the target process.
    /// </summary>
    /// <param name="offset">The offset from the base address of the <see cref="Process.MainModule"/> to read from.</param>
    /// <param name="destination">The span to write the data into.</param>
    public void Read(nint offset, Span<byte> destination) => Read(_process.MainModule.BaseAddress, offset, destination);

    /// <summary>
    /// Attempts to find a sequence of bytes in the target process's memory space.
    /// The search is conducted in all supported modules of the target process.
    /// </summary>
    /// <param name="data">The sequence of bytes to search for.</param>
    /// <returns>The address of the first occurrence of the sequence of bytes in the target process's memory space, or <see cref="nint.Zero"/> if the sequence was not found.</returns>
    public nint Find(params ReadOnlySpan<byte> data)
    {
        var chunkSize = Environment.SystemPageSize;
        var overlap = data.Length - 1;

        // This will conditionally stackalloc, and fortunately, the size doesn't depend on the contents of the loop, so we can do this once and keep reusing it
        var span = MemoryManager.CreateBuffer(chunkSize + overlap);

        for (var i = 0; i < _modules.Length; i++)
        {
            var module = _modules[i];
            var baseAddress = module.BaseAddress;
            var regionSize = module.ModuleMemorySize;

            var leftover = 0;

            for (var offset = 0; offset < regionSize; offset += chunkSize)
            {
                var sizeToRead = Math.Min(chunkSize, regionSize - offset);
                var readSpan = span.Slice(leftover, sizeToRead);

                if (!Interop.ReadProcessMemory(_handle, baseAddress + offset, readSpan, out var bytesRead))
                {
                    leftover = 0;
                    continue;
                }

                // Combine leftover and newly read data
                var combinedSpan = span[..(leftover + bytesRead)];
                var index = combinedSpan.IndexOf(data);
                if (index >= 0)
                {
                    return baseAddress + offset - overlap - overlap + index;
                }

                // Keep final overlap bytes for next iteration
                leftover = Math.Min(overlap, bytesRead);
                combinedSpan.Slice(bytesRead - leftover, leftover).CopyTo(span[..leftover]);
            }
        }

        return 0;
    }
    /// <summary>
    /// Attempts to find the sequence of bytes that make up the specified instance of <typeparamref name="T"/> in the target process's memory space.
    /// The search is conducted in all supported modules of the target process.
    /// </summary>
    /// <typeparam name="T">The type of the instance to search for. Must be a <see langword="struct"/>.</typeparam>
    /// <param name="instance">The instance to search for.</param>
    /// <returns>The address of the first occurrence of the sequence of bytes in the target process's memory space, or <see cref="nint.Zero"/> if the sequence was not found.</returns>
    public nint Find<T>(T instance) where T : struct => Find(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref instance, 1)));
    /// <summary>
    /// Attempts to find the specified string in the target process's memory space.
    /// The search is conducted in all supported modules of the target process.
    /// </summary>
    /// <param name="str">The string to search for.</param>
    /// <param name="encoding">The encoding to use when converting the string to bytes. If <see langword="null"/>, the default encoding is used.</param>
    /// <returns>The address of the first occurrence of the string in the target process's memory space, or <see cref="nint.Zero"/> if the string was not found.</returns>
    /// <remarks>
    /// <see cref="Find(string, Encoding)"/> is the only <c>Find</c> overload that must allocate to work. It is recommended for callers to utilize an existing buffer and use <see cref="Encoding.GetMaxByteCount(int)"/> and <see cref="Encoding.GetBytes(ReadOnlySpan{char}, Span{byte})"/> and call <see cref="Find(ReadOnlySpan{byte})"/> to avoid unnecessary allocations.
    /// </remarks>
    public nint Find(string str, Encoding encoding = null)
    {
        encoding ??= Encoding.Default;
        var bytes = encoding.GetBytes(str);
        return Find(bytes);
    }

    /// <summary>
    /// Writes a sequence of bytes to the target process's memory space at the specified address and offset, optionally writing a reversed copy for little-endian systems instead.
    /// </summary>
    /// <param name="address">The address within the target process's memory space to write to.</param>
    /// <param name="offset">The offset from the address to write to.</param>
    /// <param name="data">The sequence of bytes to write.</param>
    /// <param name="reverseLittleEndian">Whether to create a reversed copy the data for little-endian systems. Passing large enough <paramref name="data"/> buffers may require allocation. This parameter is ignored when <see cref="BitConverter.IsLittleEndian"/> is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if the write operation was successful, otherwise <see langword="false"/>.</returns>
    public bool Write(nint address, nint offset, ReadOnlySpan<byte> data, bool reverseLittleEndian)
    {
        address += offset;

        var module = _modules.FirstOrDefault(m => m.BaseAddress <= address && address < m.BaseAddress + m.ModuleMemorySize)
            ?? throw new AccessViolationException("The specified address is not within the memory space of any module in the target process.");

        // VirtualQueryEx the location to ensure we can write to it

        // If reversal is requested, reverse the byte sequence
        if (reverseLittleEndian && BitConverter.IsLittleEndian)
        {
            var temp = MemoryManager.CreateBuffer(data.Length);
            data.CopyTo(temp);
            temp.Reverse();

            // To avoid breaking ref safety on the stackalloc (if it gets used), need to copy the code below in here (CS9080)
            var succeeded = Interop.WriteProcessMemory(_handle, address, temp, out var written);
            if (!succeeded)
            {
                ThrowForLastError("Failed to write memory to the target process.");
            }
            return succeeded;
        }

        {
            var succeeded = Interop.WriteProcessMemory(_handle, address, data, out var written);
            if (!succeeded)
            {
                ThrowForLastError("Failed to write memory to the target process.");
            }
            return succeeded;
        }
    }
    /// <summary>
    /// Writes a sequence of bytes to the target process's memory space at the specified address and offset.
    /// The byte sequence is not reversed for little-endian systems.
    /// </summary>
    /// <param name="address">The address within the target process's memory space to write to.</param>
    /// <param name="offset">The offset from the address to write to.</param>
    /// <param name="data">The sequence of bytes to write.</param>
    /// <returns><see langword="true"/> if the write operation was successful, otherwise <see langword="false"/>.</returns>
    public bool Write(nint address, nint offset, params ReadOnlySpan<byte> data) => Write(address, offset, data, false);
    /// <summary>
    /// Writes the bytes that make up a single value of type <typeparamref name="T"/> to the target process's memory space at the specified address and offset.
    /// </summary>
    /// <typeparam name="T">The type of the value to write. Must be a <see langword="struct"/>.</typeparam>
    /// <param name="address">The address within the target process's memory space to write to.</param>
    /// <param name="offset">The offset from the address to write to.</param>
    /// <param name="instance">The value to write.</param>
    /// <returns><see langword="true"/> if the write operation was successful, otherwise <see langword="false"/>.</returns>
    public bool Write<T>(nint address, nint offset, T instance) where T : struct
        => Write(address, offset, MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref instance, 1)), false);

    private static int HResultFromWin32Error(int x) => x <= 0 ? x : ((int)((x & 0x0000FFFF) | (7 << 16) | 0x80000000));
    [DoesNotReturn]
    private static void ThrowForLastError(string outerMsg)
    {
        var exit = HResultFromWin32Error(Interop.GetLastError());
        var ex = Marshal.GetExceptionForHR(exit);
        var msg = ex?.Message ?? Marshal.GetLastPInvokeErrorMessage();

        // TODO: Debug.Fail
        var message = $"{outerMsg} ('{msg}').";
        Debug.Fail(message);
        throw new InvalidOperationException(message, ex);
    }

    private void AcquireHandle()
    {
        if (!TryAcquireHandle())
        {
            var exit = Marshal.GetLastSystemError();
            var ex = Marshal.GetExceptionForHR(exit);
            var msg = Marshal.GetPInvokeErrorMessage(exit);
            throw new InvalidOperationException($"Failed to acquire a handle to the target process ('{msg}').", ex);
        }
    }
    private bool TryAcquireHandle()
    {
        _handle = Interop.OpenProcess(ProcessOpenAccess.ProcessVMOperation | ProcessOpenAccess.ProcessVMRead | ProcessOpenAccess.ProcessVMWrite | ProcessOpenAccess.ProcessQueryInformation | ProcessOpenAccess.StandardRightsRequired, false, _pid);
        return _handle != nint.Zero;
    }

    private void EnsureAllowTarget()
    {
        switch (_pid)
        {
            case 0:
                throw new AccessViolationException("The System Idle Process cannot be opened.");
            case 4:
                throw new AccessViolationException("The System Process cannot be opened.");
        }
        if (Process.GetProcessesByName("csrss").Any(p => p.Id == _pid))
        {
            throw new AccessViolationException("The Client/Server Runtime Subsystem (csrss.exe) processes cannot be opened.");
        }
    }

    // Maps the modules of the target process into our arrays
    // By default, only modules (DLLs) belonging to the process are mapped, meaning they must come from somewhere in the main module's directory
    // There should be a damn good reason for ever explicitly disabling this safety net... then again, this type is designed to read other processes' memory...
    private void MapModules()
    {
        var main = Path.GetDirectoryName(_process.MainModule.FileName);

        _modules = [.. _process.Modules.Cast<ProcessModule>()
            .IfWhere(!AllowSystemModules, m => !FileSystemHelper.IsBaseOf("C:\\Windows", m.FileName))
            .IfWhere(!AllowForeignModules, m => FileSystemHelper.IsBaseOf(main, m.FileName))];

        var count = _modules.Length;
        _moduleNames = new string[count];
        _moduleAddresses = new nint[count];
        for (var i = 0; i < count; i++)
        {
            var module = _modules[i];
            _moduleNames[i] = module.ModuleName;
            _moduleAddresses[i] = module.BaseAddress;
        }
        // Sort the arrays by the module addresses in ascending order
        ArrayHelper.Sort(_moduleAddresses, _modules, _moduleNames);
    }

    public void Dispose()
    {
        if (Interlocked.Decrement(ref _instanceCount) == 0)
        {
            Process.LeaveDebugMode();
        }

        if (_handle != nint.Zero)
        {
            Interop.CloseHandle(_handle);
            _handle = nint.Zero;
        }
    }
    public override string ToString() => $"{_process.ProcessName} ({_pid}, 0x{_handle:X16})";

    #region private static partial class Interop
    private static partial class Interop
    {
        [LibraryImport("kernel32.dll")]
        public static partial int GetLastError();
        [LibraryImport("kernel32.dll")]
        public static partial nint OpenProcess(ProcessOpenAccess access, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CloseHandle(nint handle);
        [LibraryImport("kernel32.dll")]
        public static partial nint GetCurrentProcess();

        public static void VirtualQueryEx(nint hProcess, out MEMORY_BASIC_INFORMATION lpBuffer) => VirtualQueryExImpl(hProcess, nint.Zero, out lpBuffer, Marshal.SizeOf<MEMORY_BASIC_INFORMATION>());
        [LibraryImport("kernel32.dll", EntryPoint = nameof(VirtualQueryEx))]
        public static partial nint VirtualQueryExImpl(nint hProcess, nint lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, nint sizeT);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public nint BaseAddress;
            public nint AllocationBase;
            public int AllocationProtect;
            public short PartitionId;
            public nint RegionSize;
            public int State;
            public int Protect;
            public int Type;
        }

        public static bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, Span<byte> lpBuffer, out int lpNumberOfBytesRead) => ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, lpBuffer.Length, out lpNumberOfBytesRead);
        [LibraryImport("kernel32.dll", EntryPoint = nameof(ReadProcessMemory))]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, Span<byte> lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        public static bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, ReadOnlySpan<byte> lpBuffer, out nint lpNumberOfBytesWritten) => WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, lpBuffer.Length, out lpNumberOfBytesWritten);
        [LibraryImport("kernel32.dll", EntryPoint = nameof(WriteProcessMemory))]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, ReadOnlySpan<byte> lpBuffer, int nSize, out nint lpNumberOfBytesWritten);
    }
    #endregion
}

[Flags]
internal enum ProcessOpenAccess : uint
{
    /// <summary>
    /// Required to delete the object.
    /// </summary>
    Delete = 0x10000,
    /// <summary>
    /// Required to read information in the security descriptor for the object, not including the information in the SACL.
    /// To read or write the SACL, you must request the <b>ACCESS_SYSTEM_SECURITY</b> access right.
    /// For more information, see <see href="https://learn.microsoft.com/en-us/windows/win32/secauthz/sacl-access-right">SACL Access Right</see>.
    /// </summary>
    ReadControl = 0x20000,
    /// <summary>
    /// The right to use the object for synchronization.
    /// This enables a thread to wait until the object is in the signaled state.
    /// </summary>
    Synchronize = 0x100000,
    /// <summary>
    /// Required to modify the DACL in the security descriptor for the object.
    /// </summary>
    WriteDac = 0x40000,
    /// <summary>
    /// Required to change the owner in the security descriptor for the object.
    /// </summary>
    WriteOwner = 0x80000,

    /// <summary>
    /// Combines <see cref="Delete"/>, <see cref="ReadControl"/>, <see cref="WriteDac"/>, <see cref="WriteOwner"/>, and <see cref="Synchronize"/> access.
    /// </summary>
    StandardRightsAll = Delete | ReadControl | WriteDac | WriteOwner | Synchronize,
    /// <summary>
    /// Combines <see cref="Delete"/>, <see cref="ReadControl"/>, <see cref="WriteDac"/> and <see cref="WriteOwner"/> access.
    /// </summary>
    StandardRightsRequired = Delete | ReadControl | WriteDac | WriteOwner,

    /// <summary>
    /// All possible access rights for a process object.
    /// </summary>
    ProcessAllAccess = StandardRightsRequired | Synchronize,
    /// <summary>
    /// Required to use this process as the parent process with <see href="https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-updateprocthreadattribute">PROC_THREAD_ATTRIBUTE_PARENT_PROCESS</see>.
    /// </summary>
    ProcessCreateProcess = 0x80,
    /// <summary>
    /// Required to create a thread in the process.
    /// </summary>
    ProcessCreateThread = 0x2,
    /// <summary>
    /// Required to duplicate a handle using <see href="https://learn.microsoft.com/en-us/windows/win32/api/handleapi/nf-handleapi-duplicatehandle">DuplicateHandle</see>.
    /// </summary>
    ProcessDupHandle = 0x40,
    /// <summary>
    /// Required to retrieve certain information about a process, such as its token, exit code, and priority class (see <see href="https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-openprocesstoken">OpenProcessToken</see>).
    /// A handle that has this access right is implicitly also granted the <see cref="ProcessQueryInformation"/> access right.
    /// </summary>
    ProcessQueryInformation = 0x400,
    /// <summary>
    /// Required to retrieve certain information about a process (see <see href="https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getexitcodeprocess">GetExitCodeProcess</see>, <see href="https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getpriorityclass">GetPriorityClass</see>, <see href="https://learn.microsoft.com/en-us/windows/win32/api/jobapi/nf-jobapi-isprocessinjob">IsProcessInJob</see>, <see href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-queryfullprocessimagenamea">QueryFullProcessImageName</see>).
    /// A handle that has the <see cref="ProcessQueryInformation"/> access right is implicitly also granted this access right.
    /// </summary>
    ProcessQueryLimitedInformation = 0x1000,
    /// <summary>
    /// Required to set certain information about a process, such as its priority class (see <see href="https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-setpriorityclass">SetPriorityClass</see>).
    /// </summary>
    ProcessSetInformation = 0x200,
    /// <summary>
    /// Required to set memory limits using <see href="https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-setprocessworkingsetsize">SetProcessWorkingSetSize</see>.
    /// </summary>
    ProcessSetQuota = 0x100,
    /// <summary>
    /// Required to suspend or resume a process.
    /// </summary>
    ProcessSuspendResume = 0x800,
    /// <summary>
    /// Required to terminate a process using <see href="https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-terminateprocess">TerminateProcess</see>.
    /// </summary>
    ProcessTerminate = 0x1,
    /// <summary>
    /// Required to perform an operation on the address space of a process (see <see href="https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualprotectex">VirtualProtectEx</see> and <see href="https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-writeprocessmemory">WriteProcessMemory</see>).
    /// </summary>
    ProcessVMOperation = 0x8,
    /// <summary>
    /// Required to read memory in a process using <see href="https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-readprocessmemory">ReadProcessMemory</see>.
    /// </summary>
    ProcessVMRead = 0x10,
    /// <summary>
    /// Required to write to memory in a process using <see href="https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-writeprocessmemory">WriteProcessMemory</see>.
    /// </summary>
    ProcessVMWrite = 0x20
}