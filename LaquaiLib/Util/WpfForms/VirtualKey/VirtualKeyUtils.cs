using System.Globalization;

namespace LaquaiLib.Util.WpfForms;

/// <summary>
/// Provides utility methods for working with virtual key codes.
/// </summary>
public static partial class VirtualKeyUtils
{
    private static partial class Interop
    {
        private static byte[] _keyboardStateBuffer;
        internal static byte[] KeyboardStateBuffer => _keyboardStateBuffer ??= new byte[256];
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetKeyboardState([Out] byte[] lpKeyState);
        [LibraryImport("user32.dll")]
        internal static partial int ToUnicodeEx(uint wVirtKey, uint wScanCode, [In] byte[] lpKeyState, [MarshalAs(UnmanagedType.LPWStr)] string pwszBuff, int cchBuff, uint wFlags, [Optional] int dwhkl);
    }

    /// <summary>
    /// Translates a virtual key code to a Unicode character.
    /// </summary>
    /// <param name="vk">The virtual key code to translate.</param>
    /// <param name="cultureInfo">The <see cref="CultureInfo"/> instance to use for the translation. It specifies the keyboard layout to use, which influences the actual character of <c>VK_OEM_*</c> keys. If omitted, <see cref="CultureInfo.CurrentCulture"/> is used.</param>
    /// <returns>The Unicode character that corresponds to the specified virtual key code, or <see langword="null"/> if the key does not correspond to a character or the translation has failed.</returns>
    public static string? GetOemKeyData(VirtualKey vk, CultureInfo? cultureInfo = null)
    {
        cultureInfo ??= CultureInfo.CurrentCulture;

        Interop.GetKeyboardState(Interop.KeyboardStateBuffer);

        var keyboardLayout = cultureInfo.KeyboardLayoutId;
        var reciever = new string('\0', 2);
        var result = Interop.ToUnicodeEx((uint)vk, 0, Interop.KeyboardStateBuffer, reciever, 2, 0, keyboardLayout);
        if (result > 0)
        {
            return reciever.Trim('\0');
        }
        return null;
    }
}
