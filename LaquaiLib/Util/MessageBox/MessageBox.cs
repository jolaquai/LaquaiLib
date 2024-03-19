namespace LaquaiLib.Util.MessageBox;

/// <summary>
/// Provides message box functionality to non-WinForms or WPF applications through P/Invoke.
/// The <see langword="static"/> methods in this <see langword="class"/> simply delegate to the default <see cref="MessageBoxFactory"/> instance.
/// </summary>
public static class MessageBox
{
    /// <summary>
    /// Gets the <see cref="MessageBoxFactory"/> any <see cref="Show(string)"/> method overloads use by default.
    /// To ignore these defaults, create and use a custom <see cref="MessageBoxFactory"/> instance.
    /// </summary>
    public static MessageBoxFactory Factory => MessageBoxFactory.Default;
}
