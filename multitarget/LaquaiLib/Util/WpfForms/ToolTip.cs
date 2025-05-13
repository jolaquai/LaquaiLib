using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Util.WpfForms;

/// <summary>
/// Provides tooltip functionality to non-WinForms or WPF applications through P/Invoke, even without a window to host it.
/// <b>The current implementation is not functional.</b>
/// </summary>
public static partial class ToolTip
{
    internal static partial class Interop
    {
        public const int OPERATION_COMPLETED_SUCCESSFULLY = -0x7FF90000;

        private static int? screenDpi;
        internal static int ScreenDpi { get; } = screenDpi ??= GetScreenDpi();
        [LibraryImport("user32.dll")]
        private static partial nint GetDC(nint hWnd);
        [LibraryImport("gdi32.dll")]
        private static partial int GetDeviceCaps(nint hdc, int nIndex);
        [LibraryImport("user32.dll")]
        private static partial int ReleaseDC(nint hWnd, nint hDC);
        private static int GetScreenDpi()
        {
            var hdc = GetDC(nint.Zero);
            var dpi = GetDeviceCaps(hdc, 88);
            _ = ReleaseDC(nint.Zero, hdc);
            return dpi;
        }

        internal const int CW_USEDEFAULT = unchecked((int)0x80000000);
        private const uint WM_USER = 0x0400;
        internal const uint TTM_ADDTOOL = WM_USER + 4;
        internal const uint TTM_ADJUSTRECT = WM_USER + 31;
        internal const uint TTM_SETMAXTIPWIDTH = WM_USER + 24;
        internal const uint TTM_TRACKPOSITION = WM_USER + 18;
        internal const uint TTM_TRACKACTIVATE = WM_USER + 17;
        internal const uint TTM_UPDATETIPTEXT = WM_USER + 57; // TTM_UPDATETIPTEXTW

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
            public int x, y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
            public readonly bool Contains(POINT point) => point.x >= left && point.x <= right && point.y >= top && point.y <= bottom;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [LibraryImport("user32.dll")]
        [return: MaybeNull]
        internal static partial nint GetForegroundWindow();
        [LibraryImport("user32.dll")]
        private static partial nint MonitorFromPoint(POINT pt, uint dwFlags);

        [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

        [LibraryImport("user32.dll", EntryPoint = "CreateWindowExA", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint CreateWindowEx(int exstyle, string classname, string windowname, uint style, int x, int y, int width, int height, nint hwndParent, nint hMenu, nint hInstance, nint lpParam);
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool DestroyWindow(nint hWnd);
        [LibraryImport("user32.dll", EntryPoint = "SendMessageA")]
        internal static partial nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetCursorPos(out POINT lpPoint);
        internal static MONITORINFO GetMonitorInfoFromCursor()
        {
            _ = GetCursorPos(out var cursorPos);

            var hMonitor = MonitorFromPoint(cursorPos, 2);

            var monitorInfo = default(MONITORINFO) with
            {
                cbSize = Unsafe.SizeOf<MONITORINFO>()
            };

            if (GetMonitorInfo(hMonitor, ref monitorInfo))
            {
                return monitorInfo;
            }
            else
            {
                throw new Exception("Unable to get monitor information");
            }
        }

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetWindowRect(nint hWnd, out RECT lpRect);

        internal static readonly Version _win8 = new Version(6, 2);
        internal static readonly bool _win8OrGreater = Environment.OSVersion.Version >= _win8;

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public nint hwnd;
            public uint message;
            public nint wParam;
            public nint lParam;
            public uint time;
            public POINT pt;
            public uint lPrivate;
        }
        [LibraryImport("user32.dll", EntryPoint = "GetMessageW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [LibraryImport("user32.dll")]
        private static partial nint DispatchMessage(ref MSG lpmsg);
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool TranslateMessage(ref MSG lpMsg);

        private static volatile bool _runningMessageLoop;
        internal static void RunMessageLoop(nint hWnd)
        {
            if (_runningMessageLoop)
            {
                return;
            }
            _runningMessageLoop = true;
            {
                var msg = default(MSG);
                while (GetMessage(out msg, hWnd, 0, 0))
                {
                    _ = TranslateMessage(ref msg);
                    _ = DispatchMessage(ref msg);
                }
            }
            _runningMessageLoop = false;
        }
    }

    /// <summary>
    /// Displays a tooltip with the specified <paramref name="text"/> at the cursor's position (plus the default offset of 5 pixels down and right) for its default lifetime of 1 second.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    /// <returns>A <see cref="ToolTipHandle"/> that allows monitoring the tooltip's lifetime.</returns>
    /// <remarks>
    /// This method behaves like a <c>Begin*</c> method in that it returns immediately after displaying the tooltip. The tooltip will be removed from the screen after the specified <see cref="ToolTipHandle.DisplayTask"/> completes.
    /// </remarks>
    public static ToolTipHandle ShowTooltip(string text) => ShowTooltip(text, ToolTipDisplay.Cursor, null, 5, 5);
    /// <summary>
    /// Displays a tooltip with the specified <paramref name="text"/> using the specified <paramref name="displayMode"/> which controls where the tooltip is displayed (plus the default offset of 5 pixels down and right) for its default lifetime of 1 second.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    /// <param name="displayMode">A <see cref="ToolTipDisplay"/> value that specifies where the tooltip should be displayed.</param>
    /// <returns>A <see cref="ToolTipHandle"/> that allows monitoring the tooltip's lifetime.</returns>
    /// <remarks>
    /// This method behaves like a <c>Begin*</c> method in that it returns immediately after displaying the tooltip. The tooltip will be removed from the screen after the specified <see cref="ToolTipHandle.DisplayTask"/> completes.
    /// </remarks>
    public static ToolTipHandle ShowTooltip(string text, ToolTipDisplay displayMode) => ShowTooltip(text, displayMode, TimeSpan.FromMilliseconds(1000), 5, 5);
    /// <summary>
    /// Displays a tooltip with the specified <paramref name="text"/> at the cursor's position (plus the default offset of 5 pixels down and right) for the specified <paramref name="displayTime"/>.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    /// <param name="displayTime">A <see cref="TimeSpan"/> value that specifies how long the tooltip should be displayed for.</param>
    /// <returns>A <see cref="ToolTipHandle"/> that allows monitoring the tooltip's lifetime.</returns>
    /// <remarks>
    /// This method behaves like a <c>Begin*</c> method in that it returns immediately after displaying the tooltip. The tooltip will be removed from the screen after the specified <see cref="ToolTipHandle.DisplayTask"/> completes.
    /// </remarks>
    public static ToolTipHandle ShowTooltip(string text, TimeSpan displayTime) => ShowTooltip(text, ToolTipDisplay.Cursor, displayTime, 5, 5);
    /// <summary>
    /// Displays a tooltip with the specified <paramref name="text"/> using the specified <paramref name="displayMode"/> which controls where the tooltip is displayed for the specified <paramref name="displayTime"/>.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    /// <param name="displayMode">A <see cref="ToolTipDisplay"/> value that specifies where the tooltip should be displayed.</param>
    /// <param name="displayTime">A <see cref="TimeSpan"/> value that specifies how long the tooltip should be displayed for.</param>
    /// <returns>A <see cref="ToolTipHandle"/> that allows monitoring the tooltip's lifetime.</returns>
    /// <remarks>
    /// This method behaves like a <c>Begin*</c> method in that it returns immediately after displaying the tooltip. The tooltip will be removed from the screen after the specified <see cref="ToolTipHandle.DisplayTask"/> completes.
    /// </remarks>
    public static ToolTipHandle ShowTooltip(string text, ToolTipDisplay displayMode, TimeSpan? displayTime) => ShowTooltip(text, displayMode, displayTime, 5, 5);
    /// <summary>
    /// Displays a tooltip with the specified <paramref name="text"/> using the specified <paramref name="displayMode"/> which controls where the tooltip is displayed for the specified <paramref name="displayTime"/> at the specified <paramref name="x"/> and <paramref name="y"/> position.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    /// <param name="displayMode">A <see cref="ToolTipDisplay"/> value that specifies where the tooltip should be displayed.</param>
    /// <param name="displayTime">A <see cref="TimeSpan"/> value that specifies how long the tooltip should be displayed for.</param>
    /// <param name="x">The <c>x</c>-coordinate of the tooltip's position if <c><paramref name="displayMode"/> == <see cref="ToolTipDisplay.Absolute"/></c>, otherwise the horizontal offset from the cursor's position.</param>
    /// <param name="y">The <c>y</c>-coordinate of the tooltip's position if <c><paramref name="displayMode"/> == <see cref="ToolTipDisplay.Absolute"/></c>, otherwise the vertical offset from the cursor's position.</param>
    /// <returns>A <see cref="ToolTipHandle"/> that allows monitoring the tooltip's lifetime.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="displayMode"/> is <see cref="ToolTipDisplay.Absolute"/> and <paramref name="x"/> or <paramref name="y"/> are not specified.</exception>
    /// <exception cref="ArgumentException">Thrown when an unsupported <see cref="ToolTipDisplay"/> value is specified for <paramref name="displayMode"/>.</exception>
    /// <remarks>
    /// This method behaves like a <c>Begin*</c> method in that it returns immediately after displaying the tooltip. The tooltip will be removed from the screen after the specified <see cref="ToolTipHandle.DisplayTask"/> completes.
    /// </remarks>
    public static ToolTipHandle ShowTooltip(string text, ToolTipDisplay displayMode = ToolTipDisplay.Cursor, TimeSpan? displayTime = null, int? x = null, int? y = null)
    {
        if (displayTime == Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(displayTime), "Tooltips cannot be persistent (they must auto-hide after a finite amount of time).");
        }

        var monitorinfo = Interop.GetMonitorInfoFromCursor();
        _ = Interop.GetCursorPos(out var cursorPos);
        var textRect = Interop._win8OrGreater ? monitorinfo.rcWork : monitorinfo.rcMonitor;

        int xActual;
        int yActual;
        switch (displayMode)
        {
            case ToolTipDisplay.Cursor:
                xActual = cursorPos.x;
                yActual = cursorPos.y;
                break;
            case ToolTipDisplay.Center:
                var width = textRect.right - textRect.left;
                var height = textRect.bottom - textRect.top;

                xActual = width / 2;
                yActual = height / 2;
                break;
            case ToolTipDisplay.Absolute when x == int.MinValue || y == int.MinValue:
                throw new ArgumentException($"Invalid tooltip position. {nameof(x)} and {nameof(y)} must be specified when using {nameof(ToolTipDisplay)}.{nameof(ToolTipDisplay.Absolute)}.");
            default:
                throw new ArgumentException($"Invalid display mode '{displayMode}'.");
        }
        if (x is not null)
        {
            xActual += x.Value;
        }
        if (y is not null)
        {
            yActual += y.Value;
        }

        var tooltipHwnd = Interop.CreateWindowEx(0x8, "tooltips_class32", null, 0x3, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, Interop.CW_USEDEFAULT, nint.Zero, nint.Zero, nint.Zero, nint.Zero);
        if (tooltipHwnd == nint.Zero)
        {
            throw new Exception("Unable to create tooltip window.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
        }

        _ = Task.Run(() => Interop.RunMessageLoop(tooltipHwnd));

        unsafe
        {
            var ti = default(Interop.TOOLINFO) with
            {
                cbSize = 64,
                uFlags = 0,
                hwnd = tooltipHwnd,
                hinst = nint.Zero,
                lpszText = Marshal.StringToHGlobalAuto(text),
                rect = default,
                lParam = nint.Zero, // No lParam data
                lpReserved = nint.Zero // Reserved, must be zero
            };

            displayTime ??= TimeSpan.FromMilliseconds(milliseconds: 1000);

            var block = (nint)(&ti);

            // Technically, because of pointer bullshit and P/Invoke source generation, this could still be somewhat fast xd
            _ = Interop.SendMessage(tooltipHwnd, Interop.TTM_ADJUSTRECT, 0, (nint)(&textRect));
            _ = Interop.SendMessage(tooltipHwnd, Interop.TTM_SETMAXTIPWIDTH, 0, (textRect.right - textRect.left) * 96 / Interop.ScreenDpi);

            _ = Interop.SendMessage(tooltipHwnd, Interop.TTM_TRACKPOSITION, 0, (nint)MakeLong(xActual, yActual));
            _ = Interop.SendMessage(tooltipHwnd, Interop.TTM_TRACKACTIVATE, 1, block);

            _ = Interop.SendMessage(tooltipHwnd, Interop.TTM_UPDATETIPTEXT, 0, block);

            _ = Interop.GetWindowRect(tooltipHwnd, out var ttw);
            var height = ttw.bottom - ttw.top;
            var width = ttw.right - ttw.left;
            if (xActual + width > textRect.right)
            {
                xActual = textRect.right - width - 1;
            }
            if (yActual + height > textRect.bottom)
            {
                yActual = textRect.bottom - height - 1;
            }
            ttw.left = xActual;
            ttw.top = yActual;
            ttw.right = xActual + width;
            ttw.bottom = yActual + height;
            // This should never matter because of the implicit shift, UNLESS they're set to 0 explicitly
            if ((x is 0 || y is 0) && ttw.Contains(cursorPos))
            {
                xActual = cursorPos.x - width - 5;
                yActual = cursorPos.y - height - 5;
            }

            _ = Interop.SendMessage(tooltipHwnd, Interop.TTM_TRACKPOSITION, 0, (nint)MakeLong(xActual, yActual));
            _ = Interop.SendMessage(tooltipHwnd, Interop.TTM_TRACKACTIVATE, 1, block);

            return new ToolTipHandle(tooltipHwnd, ti.lpszText, displayTime.Value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long MakeLong(int low, int high) => (uint)low | ((uint)high << 16);
}

/// <summary>
/// Specifies how a tooltip should be displayed.
/// </summary>
public enum ToolTipDisplay
{
    /// <summary>
    /// Specifies that the tooltip should be displayed at the cursor's position.
    /// </summary>
    Cursor,
    /// <summary>
    /// Specifies that the tooltip should be displayed at the center of the screen on which the cursor is located.
    /// </summary>
    Center,
    /// <summary>
    /// Specifies that the tooltip should be displayed at the specified absolute position.
    /// </summary>
    Absolute
}

/// <summary>
/// Represents a P/Invoke handle to a tooltip.
/// </summary>
public struct ToolTipHandle
{
    /// <summary>
    /// Gets whether this <see cref="ToolTipHandle"/> has been disposed; that is, whether the tooltip has been removed from the screen.
    /// </summary>
    public bool IsDisposed { get; private set; }
    /// <summary>
    /// Gets a <see cref="Task"/> that completes when the tooltip is no longer displayed.
    /// </summary>
    public Task DisplayTask { get; }

    private readonly nint _lpszText;
    private readonly nint _tooltipHwnd;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolTipHandle"/> <see langword="struct"/>.
    /// </summary>
    /// <param name="tooltipHwnd">The unmanaged handle to the tooltip window.</param>
    /// <param name="lpszText">The unmanaged handle to the tooltip text.</param>
    /// <param name="displayTime">The length of time to display the tooltip for. The instance of <see cref="ToolTipHandle"/> initialized with this value will automatically dispose after this time has elapsed.</param>
    public ToolTipHandle(nint tooltipHwnd, nint lpszText, TimeSpan displayTime)
    {
        _lpszText = lpszText;
        _tooltipHwnd = tooltipHwnd;

        DisplayTask = Task.Delay(displayTime);
        var self = this;
        _ = DisplayTask.ContinueWith(_ => Dispose(self));
    }

    private static void Dispose(ToolTipHandle handle)
    {
        if (!handle.IsDisposed)
        {
            Marshal.FreeHGlobal(handle._lpszText);
            _ = ToolTip.Interop.DestroyWindow(handle._tooltipHwnd);
            handle.DisplayTask.Dispose();
            handle.IsDisposed = true;
        }
    }
}