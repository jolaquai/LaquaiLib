using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

using Timer = System.Threading.Timer;

namespace LaquaiLib.Util;

/// <summary>
/// Provides methods and events for working with windows.
/// </summary>
public static partial class Windows
{
    private static readonly Timer _timer = new Timer(ConditionalRaiseEvents, null, Timeout.Infinite, 10);
    private static List<string> previousWindowList = [];
    private static string? previousActiveWindowTitle = GetActiveWindowTitle();
    private static nint? previousActiveWindowHandle = GetActiveWindowHandle();
    private static DateTime lastActiveWindowChange = DateTime.MinValue;

    /// <summary>
    /// The <see cref="object"/> that is locked on when modifying collections in any of the "GetAll..." methods in <see cref="Windows"/>. <b>Callers should lock on this when accessing these collections as well, otherwise, exceptions may be thrown during enumeration.</b>
    /// </summary>
    private static object SyncRoot { get; } = new object();

    static Windows()
    {
        GetAllWindowTitles(previousWindowList);
    }

    [LibraryImport("user32.dll")]
    [return: MaybeNull]
    private static partial nint GetForegroundWindow();
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MaybeNull]
    private static extern int GetWindowText(nint hWnd, StringBuilder text, int count);
    [LibraryImport("user32.dll")]
    [return: MaybeNull]
    private static partial int GetWindowThreadProcessId(nint hWnd, out int lpdwProcessId);
    [LibraryImport("user32.dll")]
    [return: MaybeNull]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumWindows(EnumWindowsProc enumProc, nint lParam);
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    [return: MaybeNull]
    private static partial nint FindWindowA(string lpClassName, string lpWindowName);

    /// <summary>
    /// Encapsulates a method that is called for each top-level window that is enumerated using <see cref="EnumWindows"/>.
    /// </summary>
    /// <param name="hWnd">The handle of the window.</param>
    /// <param name="lParam">An application-defined value given in <see cref="EnumWindows"/>.</param>
    /// <returns></returns>
    public delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [return: MaybeNull]
    private static string? GetWindowText(nint hWnd)
    {
        const int nChars = 256;
        var buff = new StringBuilder(nChars);
        return GetWindowText(hWnd, buff, nChars) > 0 ? buff.ToString() : null;
    }

    /// <summary>
    /// Retrieves the handle of the currently active window.
    /// </summary>
    /// <returns>The handle of the currently active window or <see langword="null"/> if no window is active or the retrieval failed.</returns>
    [return: MaybeNull]
    public static nint? GetActiveWindowHandle()
    {
        return GetForegroundWindow() is nint handle ? handle : null;
    }
    /// <summary>
    /// Retrieves the title of the currently active window.
    /// </summary>
    /// <returns>The title of the currently active window or <see langword="null"/> if no window is active or the retrieval failed.</returns>
    [return: MaybeNull]
    public static string? GetActiveWindowTitle()
    {
        return GetForegroundWindow() is nint handle ? GetWindowText(handle) : null;
    }
    /// <summary>
    /// Retrieves the PID of the process that owns the currently active window.
    /// </summary>
    /// <returns>The PID of the process that owns the currently active window or <see langword="null"/> if no window is active or the retrieval failed.</returns>
    [return: MaybeNull]
    public static int? GetActiveWindowPid()
    {
        return GetForegroundWindow() is nint handle ? GetWindowThreadProcessId(handle, out var pid) > 0 ? pid : null : (int?)null;
    }
    /// <summary>
    /// Retrieves the handle of the first window that matches the specified <paramref name="title"/>.
    /// </summary>
    /// <param name="title">The title of the window to find.</param>
    /// <returns>The HWND of the first window that matches the specified <paramref name="title"/> or <see langword="null"/> if no window matches the specified <paramref name="title"/>.</returns>
    [return: MaybeNull]
    public static nint? GetWindowHandle(string title)
    {
        return FindWindowA(null, title);
    }

    /// <summary>
    /// Replaces the contents of the given <paramref name="existing"/> <see cref="ICollection{T}"/> of <see cref="string"/> with the titles of all top-level windows.
    /// </summary>
    /// <param name="existing">The <see cref="ICollection{T}"/> of <see cref="string"/> to place the window titles into.</param>
    /// <remarks>For the entire duration of this method, a lock on <see cref="SyncRoot"/> is held.</remarks>
    public static void GetAllWindowTitles(ICollection<string> existing)
    {
        lock (SyncRoot)
        {
            var windows = new ArrayList();

            using (var handle = new GCHandle<ArrayList>(windows))
            {
                EnumWindows(EnumWindowsCallback, GCHandle.ToIntPtr(handle));
            }

            existing.Clear();
            foreach (var handle in windows.Cast<nint>())
            {
                if (handle != nint.Zero
                    && GetWindowText(handle) is string title)
                {
                    // title must be non-null, otherwise ignore the window
                    existing.Add(title);
                }
            }
        }
    }
    /// <summary>
    /// Replaces the contents of the given <paramref name="existing"/> <see cref="ICollection{T}"/> of <see cref="nint"/> with the handles of all top-level windows.
    /// </summary>
    /// <param name="existing">The <see cref="ICollection{T}"/> of <see cref="nint"/> to place the window handles into.</param>
    /// <remarks>For the entire duration of this method, a lock on <see cref="SyncRoot"/> is held.</remarks>
    public static void GetAllWindowHandles(ICollection<nint> existing)
    {
        lock (SyncRoot)
        {
            var windows = new ArrayList();
            using (var handle = new GCHandle<ArrayList>(windows))
            {
                EnumWindows(EnumWindowsCallback, GCHandle.ToIntPtr(handle));
            }

            existing.Clear();
            foreach (var handle in windows.Cast<nint>())
            {
                if (handle != nint.Zero)
                {
                    existing.Add(handle);
                }
            }
        }
    }
    /// <summary>
    /// Replaces the contents of the given <paramref name="existing"/> <see cref="IDictionary{TKey, TValue}"/> of <see cref="nint"/> and <see cref="string"/> with the handles and titles of all top-level windows.
    /// </summary>
    /// <param name="existing">The <see cref="IDictionary{TKey, TValue}"/> of <see cref="nint"/> and <see cref="string"/> to place the window handle-title pairs into.</param>
    /// <remarks>For the entire duration of this method, a lock on <see cref="SyncRoot"/> is held.</remarks>
    public static void GetAllWindows(IDictionary<nint, string> existing)
    {
        lock (SyncRoot)
        {
            var windows = new ArrayList();
            using (var handle = new GCHandle<ArrayList>(windows))
            {
                EnumWindows(EnumWindowsCallback, GCHandle.ToIntPtr(handle));
            }

            existing.Clear();
            foreach (var handle in windows.Cast<nint>())
            {
                if (handle != nint.Zero
                    && GetWindowText(handle) is string title)
                {
                    existing.Add(handle, title);
                }
            }
        }
    }

    // Important about GCHandle instances: Freeing one frees all of them, which causes Exceptions when trying to obtain the target of a GCHandle, EVEN IF a new GCHandle is created with the same target
    // That's why this method specifically creates GCHandles without disposing them, so future calls to this method can still use the target
    // The caller is therefore responsible for disposing the GCHandle after ALL calls to this method
    private static bool EnumWindowsCallback(nint hWnd, nint lParam)
    {
        if (hWnd == nint.Zero)
        {
            // Skip this window, its handle is invalid
            return true;
        }

        // We assume this just works, if it doesn't, something went horribly wrong anyway

        var handle = new GCHandle<ArrayList>(lParam);
        var handles = handle.Target;
        lock (handles.SyncRoot)
        {
            handles.Add(hWnd);
        }

        return true;
    }

    private static event WindowEvent? activeWindowChanged;
    private static event WindowEvent? windowCreated;
    private static event WindowEvent? windowDestroyed;

    /// <summary>
    /// Occurs when the active window changes.
    /// </summary>
    /// <remarks>Before a delegate is added to this event's invocation list, the current active window is stored internally to prevent immediately having the event fire.</remarks>
    public static event WindowEvent? ActiveWindowChanged {
        add {
            lock (SyncRoot)
            {
                if (GetActiveWindowTitle() is string currentTitle
                    && GetActiveWindowHandle() is nint currentHandle)
                {
                    previousActiveWindowTitle = currentTitle;
                    previousActiveWindowHandle = currentHandle;
                }
                activeWindowChanged += value;
            }
        }
        remove {
            lock (SyncRoot)
            {
                activeWindowChanged -= value;
            }
        }
    }
    /// <summary>
    /// Occurs when a new window is created / opened.
    /// </summary>
    /// <remarks>Before a delegate is added to this event's invocation list, the list of currently existent windows is stored internally to prevent immediately having the event fire.</remarks>
    public static event WindowEvent? WindowCreated {
        add {
            lock (SyncRoot)
            {
                GetAllWindowTitles(previousWindowList);
                windowCreated += value;
            }
        }
        remove {
            lock (SyncRoot)
            {
                windowCreated -= value;
            }
        }
    }
    /// <summary>
    /// Occurs when a window is destroyed / closed.
    /// </summary>
    /// <remarks>Before a delegate is added to this event's invocation list, the list of currently existent windows is stored internally to prevent immediately having the event fire.</remarks>
    public static event WindowEvent? WindowDestroyed {
        add {
            lock (SyncRoot)
            {
                GetAllWindowTitles(previousWindowList);
                windowDestroyed += value;
            }
        }
        remove {
            lock (SyncRoot)
            {
                windowDestroyed -= value;
            }
        }
    }

    /// <summary>
    /// Encapsulates a method that is called when a window event defined in <see cref="Windows"/> occurs.
    /// </summary>
    /// <param name="handle">The handle of the window. If <see langword="null"/>, the handle could not be obtained.</param>
    /// <param name="title">The title of the window. If <see langword="null"/>, the title could not be obtained.</param>
    public delegate void WindowEvent(nint? handle, string? title);

    /// <summary>
    /// Removes all entries in the invocation lists of the events defined in <see cref="Windows"/>.
    /// </summary>
    public static void Clear()
    {
        activeWindowChanged = null;
        windowCreated = null;
        windowDestroyed = null;
    }
    /// <summary>
    /// Starts raising the events defined in <see cref="Windows"/>.
    /// </summary>
    public static void Start()
    {
        _timer.Change(0, 10);
    }
    /// <summary>
    /// Stops raising the events defined in <see cref="Windows"/>.
    /// </summary>
    public static void Stop()
    {
        _timer.Change(Timeout.Infinite, 10);
    }

    /// <summary>
    /// Raises the events defined in <see cref="Windows"/> if their conditions are met.
    /// </summary>
    /// <param name="state">Unused / ignored unconditionally.</param>
    private static void ConditionalRaiseEvents(object? state)
    {
        // The title AND handle must be different to avoid raising the event for the same window infinitely often if just the title changes
        var now = DateTime.Now;
        if (GetActiveWindowTitle() is string currentTitle
            && GetActiveWindowHandle() is nint currentHandle
            && currentTitle != previousActiveWindowTitle
            && currentHandle != previousActiveWindowHandle
            && now - lastActiveWindowChange > TimeSpan.FromMilliseconds(25))
        {
            activeWindowChanged?.Invoke(GetWindowHandle(currentTitle), currentTitle);
            previousActiveWindowTitle = currentTitle;
            previousActiveWindowHandle = currentHandle;
            lastActiveWindowChange = now;
        }

        if (windowCreated is not null || windowDestroyed is not null)
        {
            var currentProcessList = new List<string>();
            GetAllWindowTitles(currentProcessList);
            var newWindows = currentProcessList.Except(previousWindowList);
            var closedWindows = previousWindowList.Except(currentProcessList);

            foreach (var window in newWindows)
            {
                windowCreated?.Invoke(GetWindowHandle(window), window);
            }

            foreach (var window in closedWindows)
            {
                windowDestroyed?.Invoke(GetWindowHandle(window), window);
            }

            previousWindowList = currentProcessList;
        }
    }
}
