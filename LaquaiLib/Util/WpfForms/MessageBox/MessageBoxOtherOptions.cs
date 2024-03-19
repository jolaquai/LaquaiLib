namespace LaquaiLib.Util.WpfForms.MessageBox;

/// <summary>
/// Specifies miscellaneous options for a message box.
/// </summary>
public static class MessageBoxOtherOptions
{
    /// <summary>
    /// Specifies that the message box should become the foreground window upon creation.
    /// </summary>
    public const uint SetForeground = 0x10000;
    /// <summary>
    /// Specifies that, if the current input desktop is not the default desktop, the call to <c>MessageBox</c> will not return until the user switches to the default desktop.
    /// </summary>
    public const uint DefaultDesktopOnly = 0x20000;
    /// <summary>
    /// Specifies that the message box is created with the <c>WS_EX_TOPMOST</c> window style.
    /// </summary>
    public const uint Topmost = 0x40000;
    /// <summary>
    /// Specifies that the message box display its contents right-justified.
    /// </summary>
    public const uint RightJustify = 0x80000;
    /// <summary>
    /// Specifies that the message box use right-to-left reading layout for Hebrew and Arabic systems.
    /// </summary>
    public const uint RtlReading = 0x100000;
    /// <summary>
    /// Specifies that the creating thread is a service notifying the user of an event. The message box is displayed on the current active desktop, even if there is no user logged on to the computer. This requires the owner HWND to be <see langword="null"/>.
    /// </summary>
    public const uint ServiceNotification = 0x200000;
}
