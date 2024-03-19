namespace LaquaiLib.Util.WpfForms.MessageBox;
/// <summary>
/// Specifies the default button on a message box.
/// </summary>
public static class MessageBoxDefaultButton
{
    /// <summary>
    /// Specifies that the first button on the message box is the default button. This is the default.
    /// </summary>
    public const uint Button1 = 0x0;
    /// <summary>
    /// Specifies that the second button on the message box is the default button.
    /// </summary>
    public const uint Button2 = 0x100;
    /// <summary>
    /// Specifies that the third button on the message box is the default button.
    /// </summary>
    public const uint Button3 = 0x200;
    /// <summary>
    /// Specifies that the fourth button on the message box is the default button. This requires the Help button to be present (see <see cref="MessageBoxOtherOptions.HelpButton"/>).
    /// </summary>
    public const uint ButtonHelp = 0x400;
}
