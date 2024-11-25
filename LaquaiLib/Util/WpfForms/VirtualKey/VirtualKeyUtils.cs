using System.Globalization;
using System.Runtime.InteropServices;

namespace LaquaiLib.Util.WpfForms;

/// <summary>
/// Provides utility methods for working with virtual key codes.
/// </summary>
public static partial class VirtualKeyUtils
{
    /// <summary>
    /// Gets an array of <see cref="VirtualKey"/> values that represent toggle keys.
    /// </summary>
    public static VirtualKey[] ToggleKeys => field ??=
    [
        VirtualKey.VK_CAPITAL,
        VirtualKey.VK_NUMLOCK,
        VirtualKey.VK_SCROLL
    ];

    /// <summary>
    /// Gets an array of <see cref="VirtualKey"/> values that represent all keys that are not a toggle key.
    /// </summary>
    public static VirtualKey[] NormalKeys => field ??= Enum.GetValues<VirtualKey>().Except(ToggleKeys).ToArray();

    private static partial class Interop
    {
        internal const byte lsb = 1;
        internal const byte msgByte = 0x80;
        internal const ushort msbShort = 0x8000;

        internal static byte[] KeyboardStateBuffer => field ??= new byte[256];
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetKeyboardState([Out] byte[] lpKeyState);
        [LibraryImport("user32.dll")]
        internal static partial int ToUnicodeEx(uint wVirtKey, uint wScanCode, [In] byte[] lpKeyState, [MarshalAs(UnmanagedType.LPWStr)] string pwszBuff, int cchBuff, uint wFlags, [Optional] int dwhkl);
        [LibraryImport("user32.dll")]
        internal static partial short GetKeyState(uint wVirtKey);
    }

    #region Properties
    /// <summary>
    /// Gets whether any control key is pressed.
    /// </summary>
    public static bool Ctrl => LCtrl || RCtrl;
    /// <summary>
    /// Gets whether the left control key is pressed.
    /// </summary>
    public static bool LCtrl => GetKeyState(VirtualKey.VK_LCONTROL);
    /// <summary>
    /// Gets whether the right control key is pressed.
    /// </summary>
    public static bool RCtrl => GetKeyState(VirtualKey.VK_RCONTROL);
    /// <summary>
    /// Gets whether any shift key is pressed.
    /// </summary>
    public static bool Shift => LShift || RShift;
    /// <summary>
    /// Gets whether the left shift key is pressed.
    /// </summary>
    public static bool LShift => GetKeyState(VirtualKey.VK_LSHIFT);
    /// <summary>
    /// Gets whether the right shift key is pressed.
    /// </summary>
    public static bool RShift => GetKeyState(VirtualKey.VK_RSHIFT);
    /// <summary>
    /// Gets whether any alt key is pressed.
    /// </summary>
    public static bool Alt => LAlt || RAlt;
    /// <summary>
    /// Gets whether the left alt key is pressed.
    /// </summary>
    public static bool LAlt => GetKeyState(VirtualKey.VK_LMENU);
    /// <summary>
    /// Gets whether the right alt key is pressed.
    /// </summary>
    public static bool RAlt => GetKeyState(VirtualKey.VK_RMENU);
    /// <summary>
    /// Gets whether the AltGr key is pressed. This is equivalent to <c><see cref="LCtrl"/> &amp;&amp; <see cref="RAlt"/></c>.
    /// </summary>
    public static bool AltGr => LCtrl && RAlt;
    /// <summary>
    /// Gets whether any Windows key is pressed.
    /// </summary>
    public static bool Win => LWin || RWin;
    /// <summary>
    /// Gets whether the left Windows key is pressed.
    /// </summary>
    public static bool LWin => GetKeyState(VirtualKey.VK_LWIN);
    /// <summary>
    /// Gets whether the right Windows key is pressed.
    /// </summary>
    public static bool RWin => GetKeyState(VirtualKey.VK_RWIN);
    /// <summary>
    /// Gets whether Caps Lock is toggled on.
    /// </summary>
    public static bool CapsLock => GetToggleState(VirtualKey.VK_CAPITAL);
    /// <summary>
    /// Gets whether Num Lock is toggled on.
    /// </summary>
    public static bool NumLock => GetToggleState(VirtualKey.VK_NUMLOCK);
    /// <summary>
    /// Gets whether Scroll Lock is toggled on.
    /// </summary>
    public static bool ScrollLock => GetToggleState(VirtualKey.VK_SCROLL);
    #endregion

    /// <summary>
    /// Translates a virtual key code to a Unicode character.
    /// </summary>
    /// <param name="vk">The virtual key code to translate.</param>
    /// <param name="cultureInfo">The <see cref="CultureInfo"/> instance to use for the translation. It specifies the keyboard layout to use, which influences the actual character of <c>VK_OEM_*</c> keys. If omitted, <see cref="CultureInfo.CurrentCulture"/> is used.</param>
    /// <returns>The Unicode character that corresponds to the specified virtual key code, or <see langword="null"/> if the key does not correspond to a character or the translation has failed.</returns>
    public static string GetOemKeyData(VirtualKey vk, CultureInfo cultureInfo = null)
    {
        cultureInfo ??= CultureInfo.CurrentCulture;

        Interop.GetKeyboardState(Interop.KeyboardStateBuffer);

        var keyboardLayout = cultureInfo.KeyboardLayoutId;
        var receiver = new string('\0', 2);
        var result = Interop.ToUnicodeEx((uint)vk, 0, Interop.KeyboardStateBuffer, receiver, 2, 0, keyboardLayout);
        return result > 0 ? receiver.Trim('\0') : null;
    }
    /// <summary>
    /// Returns an array of all <see cref="VirtualKey"/>s that are currently pressed.
    /// The return value does not reflect the toggle state, but the current state of the key. To accurately determine toggle states, use <see cref="GetKeyStates"/>.
    /// </summary>
    /// <returns>The array of all <see cref="VirtualKey"/>s that are currently pressed.</returns>
    public static VirtualKey[] GetPressedKeys()
    {
        var keys = Enum.GetValues<VirtualKey>();
        return Array.FindAll(keys, static vk => (Interop.GetKeyState((uint)vk) & Interop.msgByte) != 0);
    }
    /// <summary>
    /// Gets the state of the specified virtual key.
    /// The return value does not reflect the toggle state, but the current state of the key.
    /// </summary>
    /// <param name="vk">The virtual key to get the state of.</param>
    /// <returns><see langword="true"/> if the key is pressed, otherwise <see langword="false"/>.</returns>
    public static bool GetKeyState(VirtualKey vk) => (Interop.GetKeyState((uint)vk) & Interop.msbShort) != 0;
    /// <summary>
    /// Gets the toggle state of the specified virtual key.
    /// </summary>
    /// <param name="vk">The virtual key to get the toggle state of.</param>
    /// <returns><see langword="true"/> if the key is toggled on, otherwise <see langword="false"/>.</returns>
    public static bool GetToggleState(VirtualKey vk)
    {
        if (!Array.Exists(ToggleKeys, k => k == vk))
            throw new ArgumentException("The specified virtual key is not a toggle key.", nameof(vk));

        return (Interop.GetKeyState((uint)vk) & Interop.lsb) != 0;
    }

    /// <summary>
    /// Returns an array all <see cref="VirtualKey"/>s that are currently pressed or toggled on if the key is a toggle key.
    /// </summary>
    /// <returns>The array of all <see cref="VirtualKey"/>s that are currently pressed or toggled on.</returns>
    public static VirtualKey[] GetKeyStates()
    {
        VirtualKey[] states =
        [
            .. Array.FindAll(NormalKeys, GetKeyState),
            .. Array.FindAll(ToggleKeys, GetToggleState)
        ];
        Array.Sort(states);
        return states;
    }
}
