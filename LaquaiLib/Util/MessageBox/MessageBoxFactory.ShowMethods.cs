namespace LaquaiLib.Util.MessageBox;

// Contains just the Show method overloads
partial class MessageBoxFactory
{
    // TODO: Add XML documentation

    public uint Show(nint ownerHwnd = 0, string text = "", string caption = nameof(MessageBox), uint button = 0, uint icon = 0, uint modality = 0, uint otherOptions = 0) => Internals.PInvokeMessageBox(ownerHwnd, text, caption, button | icon | modality | otherOptions);
    public uint Show(string text) => Show(text, Caption, Button, Icon, Modality, OtherOptions);
    public uint Show(string text, string caption) => Show(text, caption, Button, Icon, Modality, OtherOptions);
    public uint Show(string text, string caption, uint button) => Show(text, caption, button, Icon, Modality, OtherOptions);
    public uint Show(string text, string caption, uint button, uint icon) => Show(text, caption, button, icon, Modality, OtherOptions);
    public uint Show(string text, string caption, uint button, uint icon, uint modality) => Show(text, caption, button, icon, modality, OtherOptions);
    public uint Show(string text, string caption, uint button, uint icon, uint modality, uint otherOptions) => Show(0, text, caption, button, icon, modality, otherOptions);
    public uint Show(MessageBoxConfiguration configuration) => Show(configuration.OwnerHwnd, configuration.Text, configuration.Caption, configuration.Button, configuration.Icon, configuration.Modality, configuration.OtherOptions);
}
