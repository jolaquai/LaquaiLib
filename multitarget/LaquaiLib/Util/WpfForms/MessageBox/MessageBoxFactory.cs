namespace LaquaiLib.Util.WpfForms.MessageBox;

/// <summary>
/// Provides a factory that creates message boxes using non-standard default values.
/// After creation, an instance of this <see langword="class"/> is immutable.
/// </summary>
public sealed partial class MessageBoxFactory
{
    /// <summary>
    /// Gets the default <see cref="MessageBoxFactory"/> instance.
    /// </summary>
    public static MessageBoxFactory Default { get; } = new MessageBoxFactory();

    /// <summary>
    /// Gets the entire <see cref="MessageBoxConfiguration"/> instance this factory uses to create message boxes.
    /// </summary>
    public MessageBoxConfiguration Configuration { get; }
    /// <summary>
    /// Gets the <see cref="MessageBoxConfiguration.OwnerHwnd"/> value in the <see cref="Configuration"/> instance this factory uses to create message boxes.
    /// </summary>
    public nint OwnerHwnd => Configuration.OwnerHwnd;
    /// <summary>
    /// Gets the <see cref="MessageBoxConfiguration.Text"/> value in the <see cref="Configuration"/> instance this factory uses to create message boxes.
    /// </summary>
    public string Text => Configuration.Text;
    /// <summary>
    /// Gets the <see cref="MessageBoxConfiguration.Caption"/> value in the <see cref="Configuration"/> instance this factory uses to create message boxes.
    /// </summary>
    public string Caption => Configuration.Caption;
    /// <summary>
    /// Gets the <see cref="MessageBoxConfiguration.Button"/> value in the <see cref="Configuration"/> instance this factory uses to create message boxes.
    /// </summary>
    public uint Button => Configuration.Button;
    /// <summary>
    /// Gets the <see cref="MessageBoxConfiguration.DefaultButton"/> value in the <see cref="Configuration"/> instance this factory uses to create message boxes.
    /// </summary>
    public uint DefaultButton => Configuration.DefaultButton;
    /// <summary>
    /// Gets the <see cref="MessageBoxConfiguration.Icon"/> value in the <see cref="Configuration"/> instance this factory uses to create message boxes.
    /// </summary>
    public uint Icon => Configuration.Icon;
    /// <summary>
    /// Gets the <see cref="MessageBoxConfiguration.Modality"/> value in the <see cref="Configuration"/> instance this factory uses to create message boxes.
    /// </summary>
    public uint Modality => Configuration.Modality;
    /// <summary>
    /// Gets the <see cref="MessageBoxConfiguration.OtherOptions"/> value in the <see cref="Configuration"/> instance this factory uses to create message boxes.
    /// </summary>
    public uint OtherOptions => Configuration.OtherOptions;

    private MessageBoxFactory() => Configuration = MessageBoxConfiguration.Default;
    /// <summary>
    /// Initialies a new <see cref="MessageBoxFactory"/> with the same default values as <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The <see cref="MessageBoxFactory"/> to copy from.</param>
    public MessageBoxFactory(MessageBoxFactory other) => Configuration = new MessageBoxConfiguration(other.Configuration);
    /// <summary>
    /// Initializes a new <see cref="MessageBoxFactory"/> with the specified <paramref name="config"/>.
    /// </summary>
    /// <param name="config">The <see cref="MessageBoxConfiguration"/> to use.</param>
    public MessageBoxFactory(MessageBoxConfiguration config) => Configuration = config;
    /// <summary>
    /// Initializes a new <see cref="MessageBoxFactory"/> with the specified parameters. Any are omissible and will default to their respective default values.
    /// </summary>
    /// <param name="ownerHwnd">The <see cref="OwnerHwnd"/> value to use.</param>
    /// <param name="text">The <see cref="Text"/> value to use.</param>
    /// <param name="caption">The <see cref="Caption"/> value to use.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <param name="icon">The <see cref="Icon"/> value to use.</param>
    /// <param name="modality">The <see cref="Modality"/> value to use.</param>
    /// <param name="otherOptions">The <see cref="OtherOptions"/> value to use.</param>
    public MessageBoxFactory(nint ownerHwnd = 0, string text = "", string caption = nameof(MessageBox), uint button = 0, uint defaultButton = 0, uint icon = 0, uint modality = 0, uint otherOptions = 0) => Configuration = new MessageBoxConfiguration(ownerHwnd, text, caption, button, defaultButton, icon, modality, otherOptions);

    private static partial class Interop
    {
        [LibraryImport("user32.dll", EntryPoint = "MessageBoxW")]
        internal static partial uint PInvokeMessageBox(nint hWnd, [MarshalAs(UnmanagedType.LPWStr)] string lpText, [MarshalAs(UnmanagedType.LPWStr)] string lpCaption, uint uType);
        [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW")]
        internal static partial nint CreateWindowExW(uint dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName, [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam);
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool DestroyWindow(nint hwnd);
    }
}
