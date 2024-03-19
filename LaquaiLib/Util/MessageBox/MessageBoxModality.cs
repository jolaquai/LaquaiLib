namespace LaquaiLib.Util.MessageBox;

/// <summary>
/// Specifies the modality of a message box.
/// </summary>
public static class MessageBoxModality
{
    /// <summary>
    /// Specifies that the message box is application modal. This blocks interaction with the associated owner window of the message box, if it has one, but windows belonging to other threads can be interacted with. This is the default.
    /// </summary>
    public const uint Application = 0x0;
    /// <summary>
    /// Specifies that the message box is system modal. Behaves exactly like <see cref="Application"/>, but promotes the message box's window to be always-on-top.
    /// </summary>
    public const uint System = 0x1000;
    /// <summary>
    /// Specifies that the message box is task modal. Behaves exactly like <see cref="Application"/>, but blocks interaction with all windows belonging to the creating thread, if it has no owner window.
    /// </summary>
    public const uint Task = 0x2000;
}
