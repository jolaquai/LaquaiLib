namespace LaquaiLib.Util.WpfForms.MessageBox;

/// <summary>
/// Specifies the return value a message box interaction produced.
/// </summary>
public static class MessageBoxResult
{
    /// <summary>
    /// The "OK" button was selected.
    /// </summary>
    public const uint OK = 1;
    /// <summary>
    /// The "Cancel" button was selected.
    /// </summary>
    public const uint Cancel = 2;
    /// <summary>
    /// The "Abort" button was selected.
    /// </summary>
    public const uint Abort = 3;
    /// <summary>
    /// The "Retry" button was selected.
    /// </summary>
    public const uint Retry = 4;
    /// <summary>
    /// The "Ignore" button was selected.
    /// </summary>
    public const uint Ignore = 5;
    /// <summary>
    /// The "Yes" button was selected.
    /// </summary>
    public const uint Yes = 6;
    /// <summary>
    /// The "No" button was selected.
    /// </summary>
    public const uint No = 7;
    /// <summary>
    /// The "Continue" button was selected.
    /// </summary>
    public const uint Continue = 11;
    /// <summary>
    /// The "Try Again" button was selected.
    /// </summary>
    public const uint TryAgain = 10;
}
