using DocumentFormat.OpenXml.Vml.Spreadsheet;
using System.Configuration;

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

    #region Synchronous
    /// <inheritdoc cref="MessageBoxFactory.Show(nint?, string?, string?, uint?, uint?, uint?, uint?, uint?)"/>
    public static uint Show(nint? ownerHwnd = null, string? text = null, string? caption = null, uint? button = null, uint? defaultButton = null, uint? icon = null, uint? modality = null, uint? otherOptions = 0) => Factory.Show(ownerHwnd, text, caption, button, defaultButton, icon, modality, otherOptions);
    /// <inheritdoc cref="MessageBoxFactory.Show(string)"/>
    public static uint Show(string text) => Factory.Show(text);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string)"/>
    public static uint Show(string text, string caption) => Factory.Show(text, caption);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string, uint)"/>
    public static uint Show(string text, string caption, uint button) => Factory.Show(text, caption, button);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string, uint, uint)"/>
    public static uint Show(string text, string caption, uint button, uint defaultButton) => Factory.Show(text, caption, button, defaultButton);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string, uint, uint, uint)"/>
    public static uint Show(string text, string caption, uint button, uint defaultButton, uint icon) => Factory.Show(text, caption, button, defaultButton, icon);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string, uint, uint, uint, uint)"/>
    public static uint Show(string text, string caption, uint button, uint defaultButton, uint icon, uint modality) => Factory.Show(text, caption, button, defaultButton, icon, modality);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string, uint, uint, uint, uint, uint)"/>
    public static uint Show(string text, string caption, uint button, uint defaultButton, uint icon, uint modality, uint otherOptions) => Factory.Show(0, text, caption, button, defaultButton, icon, modality, otherOptions);
    /// <inheritdoc cref="MessageBoxFactory.Show(MessageBoxConfiguration)"/>
    public static uint Show(MessageBoxConfiguration configuration) => Factory.Show(configuration);
    #endregion

    #region Asynchronous
    /// <inheritdoc cref="MessageBoxFactory.ShowAsync(nint?, string?, string?, uint?, uint?, uint?, uint?, uint?)"/>
    private static uint ShowAsync(nint? ownerHwnd = null, string? text = null, string? caption = null, uint? button = null, uint? defaultButton = null, uint? icon = null, uint? modality = null, uint? otherOptions = 0) => Factory.Show(ownerHwnd, text, caption, button, defaultButton, icon, modality, otherOptions);
    /// <inheritdoc cref="MessageBoxFactory.Show(string)"/>
    private static uint ShowAsync(string text) => Factory.Show(text);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string)"/>
    private static uint ShowAsync(string text, string caption) => Factory.Show(text, caption);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string, uint)"/>
    private static uint ShowAsync(string text, string caption, uint button) => Factory.Show(text, caption, button);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string, uint, uint)"/>
    private static uint ShowAsync(string text, string caption, uint button, uint defaultButton) => Factory.Show(text, caption, button, defaultButton);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string, uint, uint, uint)"/>
    private static uint ShowAsync(string text, string caption, uint button, uint defaultButton, uint icon) => Factory.Show(text, caption, button, defaultButton, icon);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string, uint, uint, uint, uint)"/>
    private static uint ShowAsync(string text, string caption, uint button, uint defaultButton, uint icon, uint modality) => Factory.Show(text, caption, button, defaultButton, icon, modality);
    /// <inheritdoc cref="MessageBoxFactory.Show(string, string, uint, uint, uint, uint, uint)"/>
    private static uint ShowAsync(string text, string caption, uint button, uint defaultButton, uint icon, uint modality, uint otherOptions) => Factory.Show(0, text, caption, button, defaultButton, icon, modality, otherOptions);
    /// <inheritdoc cref="MessageBoxFactory.Show(MessageBoxConfiguration)"/>
    private static uint ShowAsync(MessageBoxConfiguration configuration) => Factory.Show(configuration);
    #endregion
}
