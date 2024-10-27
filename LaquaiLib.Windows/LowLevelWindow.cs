using System.Windows;
using System.Windows.Interop;

namespace LaquaiLib.Windows;

/// <summary>
/// Proxies low-level <see cref="Window"/> functionality out of <see cref="HwndSource"/> to inheriting classes.
/// </summary>
public abstract class LowLevelWindow : Window
{
    /// <summary>
    /// Retrieves the <see cref="HwndSource"/> of the window.
    /// </summary>
    protected HwndSource? HwndSource => field ??= PresentationSource.FromVisual(this) as HwndSource;
    /// <summary>
    /// Retrieves the handle of the window.
    /// </summary>
    protected nint Handle => HwndSource.Handle;

    protected sealed override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var source = PresentationSource.FromVisual(this) as HwndSource;
        source.AddHook(WndProc);
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        handled = false;
        return OnMessageReceived(new MSG()
        {
            hwnd = hwnd,
            message = msg,
            wParam = wParam,
            lParam = lParam
        }, ref handled);
    }
    /// <summary>
    /// Invoked when a message is received by the window.
    /// Inheriting classes should delegate to <see langword="base"/> if their own logic does not handle the message, otherwise the window may become unresponsive.
    /// </summary>
    /// <param name="message">A <see cref="MSG"/> struct representing the message.</param>
    /// <param name="handled">A <see langword="ref"/> <see cref="bool"/> that should be set to <see langword="true"/> if the message was handled.</param>
    /// <returns>A return value dependent on the message. Check the MSDN documentation on the message you are processing to determine the appropriate return value(s).</returns>
    public virtual nint OnMessageReceived(MSG message, ref bool handled) => nint.Zero;
}
