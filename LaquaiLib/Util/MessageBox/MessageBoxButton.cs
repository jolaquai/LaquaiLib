namespace LaquaiLib.Util.MessageBox;

/// <summary>
/// Specifies the buttons that are displayed on a message box.
/// </summary>
public static class MessageBoxButton
{
    /// <summary>
    /// Specifies that only an "OK" button is displayed.
    /// </summary>
    public const uint OK = 0x0;
    /// <summary>
    /// Specifies that "OK" and "Cancel" buttons are displayed.
    /// </summary>
    public const uint OKCancel = 0x1;
    /// <summary>
    /// Specifies that "Abort", "Retry", and "Ignore" buttons are displayed.
    /// </summary>
    public const uint AbortRetryIgnore = 0x2;
    /// <summary>
    /// Specifies that "Yes" and "No" buttons are displayed.
    /// </summary>
    public const uint YesNoCancel = 0x3;
    /// <summary>
    /// Specifies that "Yes" and "No" buttons are displayed.
    /// </summary>
    public const uint YesNo = 0x4;
    /// <summary>
    /// Specifies that "Retry" and "Cancel" buttons are displayed.
    /// </summary>
    public const uint RetryCancel = 0x5;
    /// <summary>
    /// Specifies that "Cancel", "Try Again", and "Continue" buttons are displayed.
    /// </summary>
    public const uint CancelTryAgainContinue = 0x6;
    /// <summary>
    /// Specifies that a "Help" button is displayed in the message box, <i>in addition</i> to any other buttons specified.
    /// </summary>
    public const uint HelpButton = 0x4000;
}
