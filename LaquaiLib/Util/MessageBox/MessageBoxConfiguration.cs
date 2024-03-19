namespace LaquaiLib.Util.MessageBox;

/// <summary>
/// Encapsulates configuration values for <see cref="MessageBox"/> and <see cref="MessageBoxFactory"/>.
/// After creation, an instance of this <see langword="class"/> is immutable.
/// </summary>
public sealed record class MessageBoxConfiguration
{
    /// <summary>
    /// Gets the default <see cref="MessageBoxConfiguration"/> instance.
    /// </summary>
    public static MessageBoxConfiguration Default { get; } = new MessageBoxConfiguration();

    /// <summary>
    /// Gets the HWND of the owner window used to create message boxes.
    /// </summary>
    public nint OwnerHwnd { get; }
    /// <summary>
    /// Gets the text to display in message boxes if none is specified.
    /// </summary>
    public string Text { get; }
    /// <summary>
    /// Gets the caption to display in message boxes if none is specified.
    /// </summary>
    public string Caption { get; }
    /// <summary>
    /// Gets the <see cref="MessageBoxButton"/> used to create message boxes.
    /// </summary>
    public uint Button { get; }
    /// <summary>
    /// Gets the <see cref="MessageBoxDefaultButton"/> used to create message boxes.
    /// </summary>
    public uint DefaultButton { get; }
    /// <summary>
    /// Gets the <see cref="MessageBoxIcon"/> used to create message boxes.
    /// </summary>
    public uint Icon { get; }
    /// <summary>
    /// Gets the <see cref="MessageBoxModality"/> used to create message boxes.
    /// </summary>
    public uint Modality { get; }
    /// <summary>
    /// Gets the <see cref="MessageBoxOtherOptions"/> used to create message boxes.
    /// </summary>
    public uint OtherOptions { get; }

    /// <summary>
    /// Bitwise-ORs all the properties of this <see cref="MessageBoxConfiguration"/> instance and returns the result.
    /// </summary>
    /// <returns>The result of the bitwise-OR operation on all properties of this <see cref="MessageBoxConfiguration"/> instance.</returns>
    public uint TypeParameterValue => Button | DefaultButton | Icon | Modality | OtherOptions;

    /// <summary>
    /// Initializes a new <see cref="MessageBoxConfiguration"/> by reconstructing the properties from an existing <see cref="TypeParameterValue"/>. This does not preserve <see cref="OwnerHwnd"/>, <see cref="Text"/> or <see cref="Caption"/>, which are set to their respective default values.
    /// </summary>
    /// <param name="typeParameterValue">The <see cref="TypeParameterValue"/> to reconstruct the properties from.</param>
    public MessageBoxConfiguration(uint typeParameterValue)
    {
        OwnerHwnd = 0;
        Text = "";
        Caption = nameof(MessageBox);
        Button = typeParameterValue & 0b1111;
        DefaultButton = typeParameterValue & 0b1111_0000;
        Icon = typeParameterValue & 0b1111_0000_0000;
        Modality = typeParameterValue & 0b1111_0000_0000_0000;
        OtherOptions = typeParameterValue & 0b1111_0000_0000_0000_0000;
    }
    /// <summary>
    /// Initializes a new <see cref="MessageBoxConfiguration"/> by copying the properties from an existing <see cref="MessageBoxConfiguration"/>.
    /// </summary>
    /// <param name="other">The <see cref="MessageBoxConfiguration"/> to copy from.</param>
    public MessageBoxConfiguration(MessageBoxConfiguration other)
    {
        OwnerHwnd = other.OwnerHwnd;
        Text = other.Text;
        Caption = other.Caption;
        Button = other.Button;
        DefaultButton = other.DefaultButton;
        Icon = other.Icon;
        Modality = other.Modality;
        OtherOptions = other.OtherOptions;
    }
    /// <summary>
    /// Initializes a new <see cref="MessageBoxConfiguration"/> with the specified properties. Any are omissible and will default to their respective default values.
    /// </summary>
    /// <param name="ownerHwnd">The HWND of the owner window to use.</param>
    /// <param name="text">The text to use.</param>
    /// <param name="caption">The caption to use.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <param name="icon">The <see cref="Icon"/> value to use.</param>
    /// <param name="modality">The <see cref="Modality"/> value to use.</param>
    /// <param name="otherOptions">The <see cref="OtherOptions"/> value to use.</param>
    public MessageBoxConfiguration(nint ownerHwnd = 0, string text = "", string caption = nameof(MessageBox), uint button = 0, uint defaultButton = 0, uint icon = 0, uint modality = 0, uint otherOptions = 0)
    {
        OwnerHwnd = ownerHwnd;
        Text = text;
        Caption = caption;
        Button = button;
        DefaultButton = defaultButton;
        Icon = icon;
        Modality = modality;
        OtherOptions = otherOptions;
    }
}
