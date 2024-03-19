namespace LaquaiLib.Util.WpfForms.MessageBox;
/// <summary>
/// Specifies the icon that is displayed in a message box.
/// </summary>
public static class MessageBoxIcon
{
    /// <summary>
    /// Specifies that the message box contain no symbols.
    /// </summary>
    public const uint None = 0x0;
    /// <summary>
    /// Specifies that the message box contain a symbol consisting of a white X in a circle with a red background.
    /// </summary>
    public const uint Hand = 0x10;
    /// <summary>
    /// Specifies that the message box contain a symbol consisting of white X in a circle with a red background.
    /// </summary>
    public const uint Question = 0x20;
    /// <summary>
    /// Specifies that the message box contain a symbol consisting of an exclamation point in a triangle with a yellow background.
    /// </summary>
    public const uint Exclamation = 0x30;
    /// <summary>
    /// Specifies that the message box contain a symbol consisting of a lowercase letter i in a circle.
    /// </summary>
    public const uint Asterisk = 0x40;
    /// <summary>
    /// Same as <see cref="Hand"/>.
    /// </summary>
    public const uint Stop = Hand;
    /// <summary>
    /// Same as <see cref="Hand"/>.
    /// </summary>
    public const uint Error = Hand;
    /// <summary>
    /// Same as <see cref="Exclamation"/>.
    /// </summary>
    public const uint Warning = Exclamation;
    /// <summary>
    /// Same as <see cref="Asterisk"/>.
    /// </summary>
    public const uint Information = Asterisk;
}
