using System.Globalization;

using static LaquaiLib.Util.WpfForms.VirtualKey;

namespace LaquaiLib.Util.WpfForms;

/// <summary>
/// Provides utility methods for working with virtual key codes.
/// </summary>
public static partial class VirtualKeyUtils
{
    /// <summary>
    /// Gets an array of <see cref="VirtualKey"/> values that represent toggle keys.
    /// </summary>
    public static ReadOnlySpan<VirtualKey> ToggleKeys => [VK_CAPITAL, VK_NUMLOCK, VK_SCROLL];

    // Listen I KNOW this is ugly but I refuse to allocate for this when I can just hand out a span
    /// <summary>
    /// Gets an array of <see cref="VirtualKey"/> values that represent all keys that are not a toggle key.
    /// </summary>
    public static ReadOnlySpan<VirtualKey> NormalKeys => [A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, VK_0, VK_1, VK_2, VK_3, VK_4, VK_5, VK_6, VK_7, VK_8, VK_9, VK_ACCEPT, VK_ADD, VK_APPS, VK_ATTN, VK_BACK, VK_BROWSER_BACK, VK_BROWSER_FAVORITES, VK_BROWSER_FORWARD, VK_BROWSER_HOME, VK_BROWSER_REFRESH, VK_BROWSER_SEARCH, VK_BROWSER_STOP, VK_CANCEL, VK_CAPITAL, VK_CLEAR, VK_CONTROL, VK_CONVERT, VK_CRSEL, VK_DECIMAL, VK_DELETE, VK_DIVIDE, VK_DOWN, VK_END, VK_EREOF, VK_ESCAPE, VK_EXECUTE, VK_EXSEL, VK_F1, VK_F10, VK_F11, VK_F12, VK_F13, VK_F14, VK_F15, VK_F16, VK_F17, VK_F18, VK_F19, VK_F2, VK_F20, VK_F21, VK_F22, VK_F23, VK_F24, VK_F3, VK_F4, VK_F5, VK_F6, VK_F7, VK_F8, VK_F9, VK_FINAL, VK_HANGUL, VK_HANJA, VK_HELP, VK_HOME, VK_IME_OFF, VK_IME_ON, VK_INSERT, VK_JUNJA, VK_KANA, VK_KANJI, VK_LAUNCH_APP1, VK_LAUNCH_APP2, VK_LAUNCH_MAIL, VK_LAUNCH_MEDIA_SELECT, VK_LBUTTON, VK_LCONTROL, VK_LEFT, VK_LMENU, VK_LSHIFT, VK_LWIN, VK_MBUTTON, VK_MEDIA_NEXT_TRACK, VK_MEDIA_PLAY_PAUSE, VK_MEDIA_PREV_TRACK, VK_MEDIA_STOP, VK_MENU, VK_MODECHANGE, VK_MULTIPLY, VK_NEXT, VK_NONAME, VK_NONCONVERT, VK_NUMLOCK, VK_NUMPAD0, VK_NUMPAD1, VK_NUMPAD2, VK_NUMPAD3, VK_NUMPAD4, VK_NUMPAD5, VK_NUMPAD6, VK_NUMPAD7, VK_NUMPAD8, VK_NUMPAD9, VK_OEM_1, VK_OEM_102, VK_OEM_2, VK_OEM_3, VK_OEM_4, VK_OEM_5, VK_OEM_6, VK_OEM_7, VK_OEM_8, VK_OEM_CLEAR, VK_OEM_COMMA, VK_OEM_MINUS, VK_OEM_PERIOD, VK_OEM_PLUS, VK_PA1, VK_PACKET, VK_PAUSE, VK_PLAY, VK_PRINT, VK_PRIOR, VK_PROCESSKEY, VK_RBUTTON, VK_RCONTROL, VK_RETURN, VK_RIGHT, VK_RMENU, VK_RSHIFT, VK_RWIN, VK_SCROLL, VK_SELECT, VK_SEPARATOR, VK_SHIFT, VK_SLEEP, VK_SNAPSHOT, VK_SPACE, VK_SUBTRACT, VK_TAB, VK_UP, VK_VOLUME_DOWN, VK_VOLUME_MUTE, VK_VOLUME_UP, VK_XBUTTON1, VK_XBUTTON2, VK_ZOOM, W, X, Y, Z];

    private static partial class Interop
    {
        internal const byte lsb = 1;
        internal const byte msgByte = 0x80;
        internal const ushort msbShort = 0x8000;

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetKeyboardState(Span<byte> lpKeyState);
        [LibraryImport("user32.dll")]
        internal static partial int ToUnicodeEx(uint wVirtKey, uint wScanCode, ReadOnlySpan<byte> lpKeyState, [MarshalAs(UnmanagedType.LPWStr)] string pwszBuff, int cchBuff, uint wFlags, [Optional] int dwhkl);
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
    public static bool LCtrl => GetKeyState(VK_LCONTROL);
    /// <summary>
    /// Gets whether the right control key is pressed.
    /// </summary>
    public static bool RCtrl => GetKeyState(VK_RCONTROL);
    /// <summary>
    /// Gets whether any shift key is pressed.
    /// </summary>
    public static bool Shift => LShift || RShift;
    /// <summary>
    /// Gets whether the left shift key is pressed.
    /// </summary>
    public static bool LShift => GetKeyState(VK_LSHIFT);
    /// <summary>
    /// Gets whether the right shift key is pressed.
    /// </summary>
    public static bool RShift => GetKeyState(VK_RSHIFT);
    /// <summary>
    /// Gets whether any alt key is pressed.
    /// </summary>
    public static bool Alt => LAlt || RAlt;
    /// <summary>
    /// Gets whether the left alt key is pressed.
    /// </summary>
    public static bool LAlt => GetKeyState(VK_LMENU);
    /// <summary>
    /// Gets whether the right alt key is pressed.
    /// </summary>
    public static bool RAlt => GetKeyState(VK_RMENU);
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
    public static bool LWin => GetKeyState(VK_LWIN);
    /// <summary>
    /// Gets whether the right Windows key is pressed.
    /// </summary>
    public static bool RWin => GetKeyState(VK_RWIN);
    /// <summary>
    /// Gets whether Caps Lock is toggled on.
    /// </summary>
    public static bool CapsLock => GetToggleState(VK_CAPITAL);
    /// <summary>
    /// Gets whether Num Lock is toggled on.
    /// </summary>
    public static bool NumLock => GetToggleState(VK_NUMLOCK);
    /// <summary>
    /// Gets whether Scroll Lock is toggled on.
    /// </summary>
    public static bool ScrollLock => GetToggleState(VK_SCROLL);
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

        Span<byte> kbStateBuffer = stackalloc byte[256];
        _ = Interop.GetKeyboardState(kbStateBuffer);

        var keyboardLayout = cultureInfo.KeyboardLayoutId;
        var receiver = new string('\0', 2);
        var result = Interop.ToUnicodeEx((uint)vk, 0, kbStateBuffer, receiver, 2, 0, keyboardLayout);
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
#if NET9_0
        var index = -1;
        var tk = ToggleKeys;
        for (var i = 0; i < tk.Length; i++)
        {
            if (tk[i] == vk)
            {
                index = i;
                break;
            }
        }
#elif NET10_0_OR_GREATER
        var index = ToggleKeys.IndexOf(vk);
#endif
        if (index == -1)
        {
            throw new ArgumentException("The specified virtual key is not a toggle key.", nameof(vk));
        }

        return (Interop.GetKeyState((uint)vk) & Interop.lsb) != 0;
    }

    /// <summary>
    /// Returns an array all <see cref="VirtualKey"/>s that are currently pressed or toggled on if the key is a toggle key.
    /// </summary>
    /// <returns>The array of all <see cref="VirtualKey"/>s that are currently pressed or toggled on.</returns>
    public static VirtualKey[] GetKeyStates()
    {
        var nk = NormalKeys;
        var tk = ToggleKeys;
        // Encode the keys into the LSBs of the uints and their state into the MSB
        var arr = new uint[nk.Length + tk.Length];
        const uint msb = 1u << 31;

        var offset = 0;
        for (var i = 0; i < NormalKeys.Length; i++)
        {
            if (GetKeyState(nk[i]))
            {
                arr[offset] = (uint)nk[i] | msb;
            }
            offset++;
        }
        for (var i = 0; i < ToggleKeys.Length; i++)
        {
            if (GetToggleState(tk[i]))
            {
                arr[offset] = (uint)tk[i] | msb;
            }
            offset++;
        }
        return [.. arr.Where(static x => (x & msb) == msb).Select(static x => (VirtualKey)(x & ~msb))];
    }
}
