using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib;

/// <summary>
/// Contains all P/Invoke <see cref="LibraryImportAttribute"/> declarations for the <see cref="LaquaiLib"/> library.
/// </summary>
internal static partial class Interop
{
    #region public static partial class User32
    /// <summary>
    /// Contains P/Invoke <see cref="LibraryImportAttribute"/> declarations for <c>user32.dll</c>.
    /// </summary>
    public static partial class User32
    {
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool EnumDisplayMonitors(nint hdc, nint lpRect, MonitorEnumProc callback, int dwData);
        [LibraryImport("user32.dll", EntryPoint = "MessageBoxW")]
        public static partial uint PInvokeMessageBox(nint hWnd, [MarshalAs(UnmanagedType.LPWStr)] string lpText, [MarshalAs(UnmanagedType.LPWStr)] string lpCaption, uint uType);
        [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW")]
        public static partial nint CreateWindowExW(uint dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName, [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam);
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool DestroyWindow(nint hwnd);
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetWindowRect(nint hWnd, out RECT lpRect);

        [LibraryImport("user32.dll")]
        public static partial nint GetDC(nint hWnd);
        [LibraryImport("user32.dll")]
        public static partial int ReleaseDC(nint hWnd, nint hDC);

        [LibraryImport("user32.dll", EntryPoint = "GetMessageW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetMessage(out TOOLTIPMSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [LibraryImport("user32.dll")]
        public static partial nint DispatchMessage(ref TOOLTIPMSG lpmsg);
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool TranslateMessage(ref TOOLTIPMSG lpMsg);

        [LibraryImport("user32.dll")]
        [return: MaybeNull]
        public static partial nint GetForegroundWindow();
        [LibraryImport("user32.dll")]
        public static partial nint MonitorFromPoint(POINT pt, uint dwFlags);

        [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

        [LibraryImport("user32.dll", EntryPoint = "CreateWindowExA", StringMarshalling = StringMarshalling.Utf8)]
        public static partial nint CreateWindowEx(int exstyle, string classname, string windowname, uint style, int x, int y, int width, int height, nint hwndParent, nint hMenu, nint hInstance, nint lpParam);
        [LibraryImport("user32.dll", EntryPoint = "SendMessageA")]
        public static partial nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetCursorPos(out POINT lpPoint);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetKeyboardState(Span<byte> lpKeyState);
        [LibraryImport("user32.dll")]
        public static partial int ToUnicodeEx(uint wVirtKey, uint wScanCode, ReadOnlySpan<byte> lpKeyState, [MarshalAs(UnmanagedType.LPWStr)] string pwszBuff, int cchBuff, uint wFlags, [Optional] int dwhkl);
        [LibraryImport("user32.dll")]
        public static partial short GetKeyState(uint wVirtKey);
    }
    #endregion

    #region public static partial class Kernel32
    /// <summary>
    /// Contains P/Invoke <see cref="LibraryImportAttribute"/> declarations for <c>kernel32.dll</c>.
    /// </summary>
    public static partial class Kernel32
    {
        [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CopyFileEx(string lpExistingFileName, string lpNewFileName, nint lpProgressRoutine, nint lpData, [MarshalAs(UnmanagedType.Bool)] ref bool pbCancel, uint dwCopyFlags);
        [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetDiskFreeSpace(string lpRootPathName, out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters, out uint lpTotalNumberOfClusters);
        [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetFileAttributesEx(string lpFileName, int fInfoLevelId, out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial nint CreateFile([MarshalAs(UnmanagedType.LPStr)] string lpFileName, uint dwDesiredAccess, uint dwShareMode,
        nint lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, nint hTemplateFile);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool BackupRead(nint hFile, Span<byte> lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, [MarshalAs(UnmanagedType.Bool)] bool bAbort, [MarshalAs(UnmanagedType.Bool)] bool bProcessSecurity, ref nint lpContext);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CloseHandle(nint hObject);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetProcessAffinityMask(nint hProcess, nint dwProcessAffinityMask);
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetProcessAffinityMask(nint hProcess, out nint lpProcessAffinityMask, out nint lpSystemAffinityMask);

        [LibraryImport("kernel32.dll")]
        public static partial int GetLastError();
        [LibraryImport("kernel32.dll")]
        public static partial nint OpenProcess(ProcessOpenAccess access, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);
        [LibraryImport("kernel32.dll")]
        public static partial nint GetCurrentProcess();
        [LibraryImport("kernel32.dll")]
        public static partial nint VirtualQueryEx(nint hProcess, nint lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, nint sizeT);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, Span<byte> lpBuffer, int dwSize, out int lpNumberOfBytesRead);
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, ReadOnlySpan<byte> lpBuffer, int nSize, out nint lpNumberOfBytesWritten);
    }
    #endregion

    #region public static partial class Gdi32
    /// <summary>
    /// Contains P/Invoke <see cref="LibraryImportAttribute"/> declarations for <c>gdi32.dll</c>.
    /// </summary>
    public static partial class Gdi32
    {
        [LibraryImport("gdi32.dll")]
        public static partial int GetDeviceCaps(nint hdc, int nIndex);
    }
    #endregion

    #region public static partial class Shcore
    /// <summary>
    /// Contains P/Invoke <see cref="LibraryImportAttribute"/> declarations for <c>shcore.dll</c>.
    /// </summary>
    public static partial class Shcore
    {
        [LibraryImport("shcore.dll")]
        public static partial nint GetScaleFactorForMonitor(nint hmonitor, out nint deviceScaleFactor);
    }
    #endregion

    #region struct declarations
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
    [StructLayout(LayoutKind.Sequential)]
    public struct TOOLINFO
    {
        public int cbSize;
        public int uFlags;
        public nint hwnd;
        public nint hinst;
        public nint lpszText;
        public RECT rect;
        public nint lParam;
        public nint lpReserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X, Y;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
        public readonly bool Contains(POINT point) => point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WIN32_STREAM_ID
    {
        public uint dwStreamId;
        public uint dwStreamAttributes;
        public long Size;
        public uint dwStreamNameSize;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct TOOLTIPMSG
    {
        public nint hwnd;
        public uint message;
        public nint wParam;
        public nint lParam;
        public uint time;
        public POINT pt;
        public uint lpublic;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct WIN32_FILE_ATTRIBUTE_DATA
    {
        public uint dwFileAttributes;
        public long ftCreationTime, ftLastAccessTime, ftLastWriteTime;
        public uint nFileSizeLow, nFileSizeHigh;
    }
    #endregion
    #region enum declarations
    [Flags]
    public enum ProcessOpenAccess : uint
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
    #endregion
    #region delegate declarations
    public delegate bool MonitorEnumProc(nint hDesktop, nint hdc, ref RECT pRect, int dwData);
    #endregion
}