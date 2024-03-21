using LaquaiLib.Extensions;

namespace LaquaiLib.Util.WpfForms.MessageBox;

// Contains just the Show method overloads
partial class MessageBoxFactory
{
    #region Synchronous
    /// <summary>
    /// Shows a message box with the specified properties. Any are omissible and will default to their respective default value configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="ownerHwnd">The HWND of the owner window to associate the message box with.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <param name="icon">The <see cref="Icon"/> value to use.</param>
    /// <param name="modality">The <see cref="Modality"/> value to use.</param>
    /// <param name="otherOptions">The <see cref="OtherOptions"/> value to use.</param>
    /// <returns>The result of the message box.</returns>
    public uint Show(nint? ownerHwnd = null, string? text = null, string? caption = null, uint? button = null, uint? defaultButton = null, uint? icon = null, uint? modality = null, uint? otherOptions = 0)
    {
        ownerHwnd ??= OwnerHwnd;
        text ??= Text;
        caption ??= Caption;
        button ??= Button;
        defaultButton ??= DefaultButton;
        icon ??= Icon;
        modality ??= Modality;
        otherOptions ??= OtherOptions;

        if (ownerHwnd == 0 && button.Value.HasFlag(MessageBoxButton.HelpButton))
        {
            ownerHwnd = Interop.CreateWindowExW(0, "STATIC", "DummyWindow", 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        return Interop.PInvokeMessageBox(ownerHwnd.Value, text, caption, button.Value | defaultButton.Value | icon.Value | modality.Value | otherOptions.Value);
    }
    /// <summary>
    /// Shows a message box with the specified <paramref name="text"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <returns>The result of the message box.</returns>
    public uint Show(string text) => Show(text, Caption, Button, DefaultButton, Icon, Modality, OtherOptions);
    /// <summary>
    /// Shows a message box with the specified <paramref name="text"/> and <paramref name="caption"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <returns>The result of the message box.</returns>
    public uint Show(string text, string caption) => Show(text, caption, Button, DefaultButton, Icon, Modality, OtherOptions);
    /// <summary>
    /// Shows a message box with the specified <paramref name="text"/>, <paramref name="caption"/> and <paramref name="button"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <returns>The result of the message box.</returns>
    public uint Show(string text, string caption, uint button) => Show(text, caption, button, DefaultButton, Icon, Modality, OtherOptions);
    /// <summary>
    /// Shows a message box with the specified <paramref name="text"/>, <paramref name="caption"/>, <paramref name="button"/> and <paramref name="defaultButton"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <returns>The result of the message box.</returns>
    public uint Show(string text, string caption, uint button, uint defaultButton) => Show(text, caption, button, defaultButton, Icon, Modality, OtherOptions);
    /// <summary>
    /// Shows a message box with the specified <paramref name="text"/>, <paramref name="caption"/>, <paramref name="button"/>, <paramref name="defaultButton"/> and <paramref name="icon"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <param name="icon">The <see cref="Icon"/> value to use.</param>
    /// <returns>The result of the message box.</returns>
    public uint Show(string text, string caption, uint button, uint defaultButton, uint icon) => Show(text, caption, button, defaultButton, icon, Modality, OtherOptions);
    /// <summary>
    /// Shows a message box with the specified <paramref name="text"/>, <paramref name="caption"/>, <paramref name="button"/>, <paramref name="defaultButton"/>, <paramref name="icon"/> and <paramref name="modality"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <param name="icon">The <see cref="Icon"/> value to use.</param>
    /// <param name="modality">The <see cref="Modality"/> value to use.</param>
    /// <returns>The result of the message box.</returns>
    public uint Show(string text, string caption, uint button, uint defaultButton, uint icon, uint modality) => Show(text, caption, button, defaultButton, icon, modality, OtherOptions);
    /// <summary>
    /// Shows a message box with the specified <paramref name="text"/>, <paramref name="caption"/>, <paramref name="button"/>, <paramref name="defaultButton"/>, <paramref name="icon"/>, <paramref name="modality"/> and <paramref name="otherOptions"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <param name="icon">The <see cref="Icon"/> value to use.</param>
    /// <param name="modality">The <see cref="Modality"/> value to use.</param>
    /// <param name="otherOptions">The <see cref="OtherOptions"/> value to use.</param>
    /// <returns>The result of the message box.</returns>
    public uint Show(string text, string caption, uint button, uint defaultButton, uint icon, uint modality, uint otherOptions) => Show(0, text, caption, button, defaultButton, icon, modality, otherOptions);
    /// <summary>
    /// Shows a message box using the specified <paramref name="configuration"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="MessageBoxConfiguration"/> to use.</param>
    /// <returns>The result of the message box.</returns>
    public uint Show(MessageBoxConfiguration configuration) => Show(configuration.OwnerHwnd, configuration.Text, configuration.Caption, configuration.Button, configuration.DefaultButton, configuration.Icon, configuration.Modality, configuration.OtherOptions);
    #endregion

    #region Asynchronous
    /// <summary>
    /// Asynchronously shows a message box with the specified properties. Any are omissible and will default to their respective default value configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="ownerHwnd">The HWND of the owner window to associate the message box with.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <param name="icon">The <see cref="Icon"/> value to use.</param>
    /// <param name="modality">The <see cref="Modality"/> value to use.</param>
    /// <param name="otherOptions">The <see cref="OtherOptions"/> value to use.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the user's interaction with the message box. It's <see cref="Task{TResult}.Result"/> is the result of that interaction.</returns>
    private Task<uint> ShowAsync(nint? ownerHwnd = null, string? text = null, string? caption = null, uint? button = null, uint? defaultButton = null, uint? icon = null, uint? modality = null, uint? otherOptions = 0)
    {
        ownerHwnd ??= OwnerHwnd;
        text ??= Text;
        caption ??= Caption;
        button ??= Button;
        defaultButton ??= DefaultButton;
        icon ??= Icon;
        modality ??= Modality;
        otherOptions ??= OtherOptions;

        var tcs = new TaskCompletionSource<uint>();
        var msgBoxThread = new Thread(state =>
        {
            Thread.CurrentThread.Name = nameof(WpfForms.MessageBox.MessageBoxFactory) + '.' + nameof(ShowAsync);
            var result = Interop.PInvokeMessageBox(ownerHwnd.Value, text, caption, button.Value | defaultButton.Value | icon.Value | (modality.Value | MessageBoxModality.Application) | otherOptions.Value);
            tcs.SetResult(result);
        });
        msgBoxThread.Start();
        return tcs.Task;
    }
    /// <summary>
    /// Asynchronously shows a message box with the specified <paramref name="text"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the user's interaction with the message box. It's <see cref="Task{TResult}.Result"/> is the result of that interaction.</returns>
    private Task<uint> ShowAsync(string text) => ShowAsync(text, Caption, Button, DefaultButton, Icon, Modality, OtherOptions);
    /// <summary>
    /// Asynchronously shows a message box with the specified <paramref name="text"/> and <paramref name="caption"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the user's interaction with the message box. It's <see cref="Task{TResult}.Result"/> is the result of that interaction.</returns>
    private Task<uint> ShowAsync(string text, string caption) => ShowAsync(text, caption, Button, DefaultButton, Icon, Modality, OtherOptions);
    /// <summary>
    /// Asynchronously shows a message box with the specified <paramref name="text"/>, <paramref name="caption"/> and <paramref name="button"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the user's interaction with the message box. It's <see cref="Task{TResult}.Result"/> is the result of that interaction.</returns>
    private Task<uint> ShowAsync(string text, string caption, uint button) => ShowAsync(text, caption, button, DefaultButton, Icon, Modality, OtherOptions);
    /// <summary>
    /// Asynchronously shows a message box with the specified <paramref name="text"/>, <paramref name="caption"/>, <paramref name="button"/> and <paramref name="defaultButton"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the user's interaction with the message box. It's <see cref="Task{TResult}.Result"/> is the result of that interaction.</returns>
    private Task<uint> ShowAsync(string text, string caption, uint button, uint defaultButton) => ShowAsync(text, caption, button, defaultButton, Icon, Modality, OtherOptions);
    /// <summary>
    /// Asynchronously shows a message box with the specified <paramref name="text"/>, <paramref name="caption"/>, <paramref name="button"/>, <paramref name="defaultButton"/> and <paramref name="icon"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <param name="icon">The <see cref="Icon"/> value to use.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the user's interaction with the message box. It's <see cref="Task{TResult}.Result"/> is the result of that interaction.</returns>
    private Task<uint> ShowAsync(string text, string caption, uint button, uint defaultButton, uint icon) => ShowAsync(text, caption, button, defaultButton, icon, Modality, OtherOptions);
    /// <summary>
    /// Asynchronously shows a message box with the specified <paramref name="text"/>, <paramref name="caption"/>, <paramref name="button"/>, <paramref name="defaultButton"/>, <paramref name="icon"/> and <paramref name="modality"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <param name="icon">The <see cref="Icon"/> value to use.</param>
    /// <param name="modality">The <see cref="Modality"/> value to use.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the user's interaction with the message box. It's <see cref="Task{TResult}.Result"/> is the result of that interaction.</returns>
    private Task<uint> ShowAsync(string text, string caption, uint button, uint defaultButton, uint icon, uint modality) => ShowAsync(text, caption, button, defaultButton, icon, modality, OtherOptions);
    /// <summary>
    /// Asynchronously shows a message box with the specified <paramref name="text"/>, <paramref name="caption"/>, <paramref name="button"/>, <paramref name="defaultButton"/>, <paramref name="icon"/>, <paramref name="modality"/> and <paramref name="otherOptions"/>. All other properties will use the defaults configured in this <see cref="MessageBoxFactory"/>'s <see cref="Configuration"/> instance.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="caption">The caption to display.</param>
    /// <param name="button">The <see cref="Button"/> value to use.</param>
    /// <param name="defaultButton">The <see cref="DefaultButton"/> value to use.</param>
    /// <param name="icon">The <see cref="Icon"/> value to use.</param>
    /// <param name="modality">The <see cref="Modality"/> value to use.</param>
    /// <param name="otherOptions">The <see cref="OtherOptions"/> value to use.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the user's interaction with the message box. It's <see cref="Task{TResult}.Result"/> is the result of that interaction.</returns>
    private Task<uint> ShowAsync(string text, string caption, uint button, uint defaultButton, uint icon, uint modality, uint otherOptions) => ShowAsync(0, text, caption, button, defaultButton, icon, modality, otherOptions);
    /// <summary>
    /// Asynchronously shows a message box using the specified <paramref name="configuration"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="MessageBoxConfiguration"/> to use.</param>
    /// <returns>A <see cref="Task{TResult}"/&gt;  that represents the user's interaction with the message box. It's <see cref="Task{TResult}.Result"/> is the result of that interaction.</returns>
    private Task<uint> ShowAsync(MessageBoxConfiguration configuration) => ShowAsync(configuration.OwnerHwnd, configuration.Text, configuration.Caption, configuration.Button, configuration.DefaultButton, configuration.Icon, configuration.Modality, configuration.OtherOptions);
    #endregion
}
