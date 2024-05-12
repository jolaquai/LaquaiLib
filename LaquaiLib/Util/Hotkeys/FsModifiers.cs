namespace LaquaiLib.Util.Hotkeys;

/// <summary>
/// Identifies hotkey modifier keys.
/// </summary>
[Flags]
public enum FsModifiers : uint
{
    /// <summary>
    /// Any Alt key.
    /// </summary>
    Alt = 0x0001,
    /// <summary>
    /// Any Control key.
    /// </summary>
    Control = 0x0002,
    /// <summary>
    /// Any Shift key.
    /// </summary>
    Shift = 0x0004
}
