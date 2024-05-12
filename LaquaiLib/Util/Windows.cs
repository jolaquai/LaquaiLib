using System.Collections;
using System.Diagnostics.CodeAnalysis;
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
    private static readonly object _syncRoot = new object();

    static Windows()
    {
        GetAllWindowTitles(previousWindowList);
    }

    #region P/Invoke
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
    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf8)]
    [return: MaybeNull]
    private static partial nint FindWindow(string lpClassName, string lpWindowName);
    #endregion

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
        return FindWindow(null, title);
    }

    /// <summary>
    /// Replaces the contents of the given <paramref name="existing"/> <see cref="ICollection{T}"/> of <see cref="string"/> with the titles of all top-level windows.
    /// </summary>
    /// <param name="existing">The <see cref="ICollection{T}"/> of <see cref="string"/> to place the window titles into.</param>
    /// <remarks>For the entire duration of this method, a lock on <see cref="_syncRoot"/> is held.</remarks>
    public static void GetAllWindowTitles(ICollection<string> existing)
    {
        lock (_syncRoot)
        {
            var windows = new ArrayList();

            using (var handle = new Wrappers.GCHandle<ArrayList>(windows))
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
    /// <remarks>For the entire duration of this method, a lock on <see cref="_syncRoot"/> is held.</remarks>
    public static void GetAllWindowHandles(ICollection<nint> existing)
    {
        lock (_syncRoot)
        {
            var windows = new ArrayList();
            using (var handle = new Wrappers.GCHandle<ArrayList>(windows))
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
    /// <remarks>For the entire duration of this method, a lock on <see cref="_syncRoot"/> is held.</remarks>
    public static void GetAllWindows(IDictionary<nint, string> existing)
    {
        lock (_syncRoot)
        {
            var windows = new ArrayList();
            using (var handle = new Wrappers.GCHandle<ArrayList>(windows))
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

        var handle = new Wrappers.GCHandle<ArrayList>(lParam);
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
    public static event WindowEvent? ActiveWindowChanged
    {
        add
        {
            lock (_syncRoot)
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
        remove
        {
            lock (_syncRoot)
            {
                activeWindowChanged -= value;
            }
        }
    }
    /// <summary>
    /// Occurs when a new window is created / opened.
    /// </summary>
    /// <remarks>Before a delegate is added to this event's invocation list, the list of currently existent windows is stored internally to prevent immediately having the event fire.</remarks>
    public static event WindowEvent? WindowCreated
    {
        add
        {
            lock (_syncRoot)
            {
                GetAllWindowTitles(previousWindowList);
                windowCreated += value;
            }
        }
        remove
        {
            lock (_syncRoot)
            {
                windowCreated -= value;
            }
        }
    }
    /// <summary>
    /// Occurs when a window is destroyed / closed.
    /// </summary>
    /// <remarks>Before a delegate is added to this event's invocation list, the list of currently existent windows is stored internally to prevent immediately having the event fire.</remarks>
    public static event WindowEvent? WindowDestroyed
    {
        add
        {
            lock (_syncRoot)
            {
                GetAllWindowTitles(previousWindowList);
                windowDestroyed += value;
            }
        }
        remove
        {
            lock (_syncRoot)
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

    /// <summary>
    /// Provides methods and constants for manipulating the style of a window.
    /// </summary>
    public static partial class Style
    {
        #region Style enums
        /// <summary>
        /// Identifies styles applicable to windows.
        /// </summary>
        [Flags]
        public enum WindowStyles : long
        {
            /// <summary>
            /// The window has a thin-line border
            /// </summary>
            WS_BORDER = 0x00800000L,
            /// <summary>
            /// The window has a title bar (includes the WS_BORDER style).
            /// </summary>
            WS_CAPTION = 0x00C00000L,
            /// <summary>
            /// The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.
            /// </summary>
            WS_CHILD = 0x40000000L,
            /// <summary>
            /// Same as the WS_CHILD style.
            /// </summary>
            WS_CHILDWINDOW = 0x40000000L,
            /// <summary>
            /// Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.
            /// </summary>
            WS_CLIPCHILDREN = 0x02000000L,
            /// <summary>
            /// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated. If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
            /// </summary>
            WS_CLIPSIBLINGS = 0x04000000L,
            /// <summary>
            /// The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.
            /// </summary>
            WS_DISABLED = 0x08000000L,
            /// <summary>
            /// The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.
            /// </summary>
            WS_DLGFRAME = 0x00400000L,
            /// <summary>
            /// The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style. The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys. You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
            /// </summary>
            WS_GROUP = 0x00020000L,
            /// <summary>
            /// The window has a horizontal scroll bar.
            /// </summary>
            WS_HSCROLL = 0x00100000L,
            /// <summary>
            /// The window is initially minimized. Same as the WS_MINIMIZE style.
            /// </summary>
            WS_ICONIC = 0x20000000L,
            /// <summary>
            /// The window is initially maximized.
            /// </summary>
            WS_MAXIMIZE = 0x01000000L,
            /// <summary>
            /// The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.
            /// </summary>
            WS_MAXIMIZEBOX = 0x00010000L,
            /// <summary>
            /// The window is initially minimized. Same as the WS_ICONIC style.
            /// </summary>
            WS_MINIMIZE = 0x20000000L,
            /// <summary>
            /// The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.
            /// </summary>
            WS_MINIMIZEBOX = 0x00020000L,
            /// <summary>
            /// The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_TILED style.
            /// </summary>
            WS_OVERLAPPED = 0x00000000L,
            /// <summary>
            /// The window is an overlapped window. Same as the WS_TILEDWINDOW style.
            /// </summary>
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            /// <summary>
            /// The window is a pop-up window. This style cannot be used with the WS_CHILD style.
            /// </summary>
            WS_POPUP = 0x80000000L,
            /// <summary>
            /// The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.
            /// </summary>
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            /// <summary>
            /// The window has a sizing border. Same as the WS_THICKFRAME style.
            /// </summary>
            WS_SIZEBOX = 0x00040000L,
            /// <summary>
            /// The window has a window menu on its title bar. The WS_CAPTION style must also be specified.
            /// </summary>
            WS_SYSMENU = 0x00080000L,
            /// <summary>
            /// The window is a control that can receive the keyboard focus when the user presses the TAB key. Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style. You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function. For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
            /// </summary>
            WS_TABSTOP = 0x00010000L,
            /// <summary>
            /// The window has a sizing border. Same as the WS_SIZEBOX style.
            /// </summary>
            WS_THICKFRAME = 0x00040000L,
            /// <summary>
            /// The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_OVERLAPPED style.
            /// </summary>
            WS_TILED = 0x00000000L,
            /// <summary>
            /// The window is an overlapped window. Same as the WS_OVERLAPPEDWINDOW style.
            /// </summary>
            WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            /// <summary>
            /// The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.
            /// </summary>
            WS_VISIBLE = 0x10000000L,
            /// <summary>
            /// The window has a vertical scroll bar.
            /// </summary>
            WS_VSCROLL = 0x00200000L,
        }
        /// <summary>
        /// Identifies extended styles applicable to windows.
        /// </summary>
        [Flags]
        public enum ExtendedWindowStyles : long
        {
            /// <summary>
            /// The window accepts drag-drop files.
            /// </summary>
            WS_EX_ACCEPTFILES = 0x00000010L,
            /// <summary>
            /// Forces a top-level window onto the taskbar when the window is visible.
            /// </summary>
            WS_EX_APPWINDOW = 0x00040000L,
            /// <summary>
            /// The window has a border with a sunken edge.
            /// </summary>
            WS_EX_CLIENTEDGE = 0x00000200L,
            /// <summary>
            /// Paints all descendants of a window in bottom-to-top painting order using double-buffering.
            /// Bottom-to-top painting order allows a descendent window to have translucency (alpha) and transparency (color-key) effects, but only if the descendent window also has the WS_EX_TRANSPARENT bit set.
            /// Double-buffering allows the window and its descendents to be painted without flicker.
            /// This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
            /// </summary>
            WS_EX_COMPOSITED = 0x02000000L,
            /// <summary>
            /// The title bar of the window includes a question mark.
            /// When the user clicks the question mark, the cursor changes to a question mark with a pointer.
            /// If the user then clicks a child window, the child receives a WM_HELP message.
            /// The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command.
            /// The Help application displays a pop-up window that typically contains help for the child window.
            /// </summary>
            WS_EX_CONTEXTHELP = 0x00000400L,
            /// <summary>
            /// The window itself contains child windows that should take part in dialog box navigation.
            /// If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
            /// </summary>
            WS_EX_CONTROLPARENT = 0x00010000L,
            /// <summary>
            /// The window has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
            /// </summary>
            WS_EX_DLGMODALFRAME = 0x00000001L,
            /// <summary>
            /// The window is a layered window.
            /// This style cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
            /// Windows 8: The WS_EX_LAYERED style is supported for top-level windows and child windows.
            /// Previous Windows versions support WS_EX_LAYERED only for top-level windows.
            /// </summary>
            WS_EX_LAYERED = 0x00080000,
            /// <summary>
            /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the horizontal origin of the window is on the right edge.
            /// Increasing horizontal values advance to the left.
            /// </summary>
            WS_EX_LAYOUTRTL = 0x00400000L,
            /// <summary>
            /// The window has generic left-aligned properties.
            /// This is the default.
            /// </summary>
            WS_EX_LEFT = 0x00000000L,
            /// <summary>
            /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area.
            /// For other languages, the style is ignored.
            /// </summary>
            WS_EX_LEFTSCROLLBAR = 0x00004000L,
            /// <summary>
            /// The window text is displayed using left-to-right reading-order properties.
            /// This is the default.
            /// </summary>
            WS_EX_LTRREADING = 0x00000000L,
            /// <summary>
            /// The window is a MDI child window.
            /// </summary>
            WS_EX_MDICHILD = 0x00000040L,
            /// <summary>
            /// A top-level window created with this style does not become the foreground window when the user clicks it.
            /// The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
            /// The window should not be activated through programmatic access or via keyboard navigation by accessible technology, such as Narrator.
            /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
            /// The window does not appear on the taskbar by default.
            /// To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
            /// </summary>
            WS_EX_NOACTIVATE = 0x08000000L,
            /// <summary>
            /// The window does not pass its window layout to its child windows.
            /// </summary>
            WS_EX_NOINHERITLAYOUT = 0x00100000L,
            /// <summary>
            /// The child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
            /// </summary>
            WS_EX_NOPARENTNOTIFY = 0x00000004L,
            /// <summary>
            /// The window does not render to a redirection surface.
            /// This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
            /// </summary>
            WS_EX_NOREDIRECTIONBITMAP = 0x00200000L,
            /// <summary>
            /// The window is an overlapped window.
            /// </summary>
            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            /// <summary>
            /// The window is palette window, which is a modeless dialog box that presents an array of commands.
            /// </summary>
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
            /// <summary>
            /// The window has generic "right-aligned" properties.
            /// This depends on the window class.
            /// This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored.
            /// Using the WS_EX_RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively.
            /// Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
            /// </summary>
            WS_EX_RIGHT = 0x00001000L,
            /// <summary>
            /// The vertical scroll bar (if present) is to the right of the client area.
            /// This is the default.
            /// </summary>
            WS_EX_RIGHTSCROLLBAR = 0x00000000L,
            /// <summary>
            /// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties.
            /// For other languages, the style is ignored.
            /// </summary>
            WS_EX_RTLREADING = 0x00002000L,
            /// <summary>
            /// The window has a three-dimensional border style intended to be used for items that do not accept user input.
            /// </summary>
            WS_EX_STATICEDGE = 0x00020000L,
            /// <summary>
            /// The window is intended to be used as a floating toolbar.
            /// A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
            /// A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
            /// If a tool window has a system menu, its icon is not displayed on the title bar.
            /// However, you can display the system menu by right-clicking or by typing ALT+SPACE.
            /// </summary>
            WS_EX_TOOLWINDOW = 0x00000080L,
            /// <summary>
            /// The window should be placed above all non-topmost windows and should stay above them, even when the window is deactivated.
            /// To add or remove this style, use the SetWindowPos function.
            /// </summary>
            WS_EX_TOPMOST = 0x00000008L,
            /// <summary>
            /// The window should not be painted until siblings beneath the window (that were created by the same thread) have been painted.
            /// The window appears transparent because the bits of underlying sibling windows have already been painted.
            /// To achieve transparency without these restrictions, use the SetWindowRgn function.
            /// </summary>
            WS_EX_TRANSPARENT = 0x00000020L,
            /// <summary>
            /// The window has a border with a raised edge.
            /// </summary>
            WS_EX_WINDOWEDGE = 0x00000100L,
        }
        #endregion

        #region P/Invoke
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial nint SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        #endregion

        private static void RefreshWindow(nint hWnd) => SetWindowPos(hWnd, 0, 0, 0, 0, 0, 0x0010 | 0x0002 | 0x0200 | 0x0004 | 0x0001);
        public static bool WindowSetStyle(nint hWnd, WindowStyles windowStyle)
        {
            Marshal.SetLastPInvokeError(0);
            var functionSucceeded = SetWindowLongPtr(hWnd, -16, (nint)windowStyle) != 0;
            Thread.MemoryBarrier();
            var lastErrorModified = Marshal.GetLastWin32Error() != 0;
            RefreshWindow(hWnd);
            return functionSucceeded && !lastErrorModified;
        }
        public static bool WindowSetStyle(nint hWnd, ExtendedWindowStyles extendedWindowStyle)
        {
            Marshal.SetLastPInvokeError(0);
            var functionSucceeded = SetWindowLongPtr(hWnd, -20, (nint)extendedWindowStyle) != 0;
            Thread.MemoryBarrier();
            var lastErrorModified = Marshal.GetLastWin32Error() != 0;
            RefreshWindow(hWnd);
            return functionSucceeded && !lastErrorModified;
        }
    }
}
