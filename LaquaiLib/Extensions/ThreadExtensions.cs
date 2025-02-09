using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using LaquaiLib.Util.ExceptionManagement;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Thread"/> Type.
/// </summary>
public static partial class ThreadExtensions
{
    [LibraryImport("kernel32.dll")]
    private static partial nint OpenThread(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwThreadId);
    private static nint OpenThread(uint dwThreadId) => OpenThread(1, false, dwThreadId);
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TerminateThread(nint hThread, uint dwExitCode);
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(nint hObject);
    [UnsafeAccessor(UnsafeAccessorKind.Field)]
    private static extern ref nint _DONT_USE_InternalThread(this Thread thread);

    /// <summary>
    /// Attempts to kill the specified managed <see cref="Thread"/>.
    /// </summary>
    /// <param name="thread">The <see cref="Thread"/> to kill.</param>
    /// <exception cref="InvalidOperationException">Thrown when the thread could not be terminated, either because a handle to it could not be acquired or because the termination failed.</exception>
    public static void Assassinate(this Thread thread)
    {
        ArgumentNullException.ThrowIfNull(thread);

        if (thread.IsAlive)
        {
            var owner = Process.GetCurrentProcess();

            var procThreads = owner.Threads.Cast<ProcessThread>().ToArray();
            Console.WriteLine(string.Join(", ", procThreads.Select(t => t.Id)));
            var procThread = Array.Find(procThreads, t => t.Id == thread.ManagedThreadId);
            var threadHandle = OpenThread((uint)procThread.Id);
            if (threadHandle == 0)
            {
                throw new InvalidOperationException("Failed to open thread handle.");
            }

            if (!TerminateThread(thread._DONT_USE_InternalThread(), unchecked((uint)HResults.COR_E_THREADABORTED)))
            {
                throw new InvalidOperationException("Failed to terminate thread.");
            }

            _ = CloseHandle(threadHandle);
        }
    }
}
