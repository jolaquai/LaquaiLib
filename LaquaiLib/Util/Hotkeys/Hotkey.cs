namespace LaquaiLib.Util.Hotkeys;

/// <summary>
/// Represents a hotkey registered by <see cref="Hotkeys"/>.
/// </summary>
public record class Hotkey
{
    /// <summary>
    /// The ID this hotkey was registered to the system with.
    /// </summary>
    public int Id { get; init; }
    /// <summary>
    /// The modifier keys for this hotkey.
    /// </summary>
    public FsModifiers Modifiers { get; init; }
    /// <summary>
    /// The key for this hotkey.
    /// </summary>
    public Keys Key { get; init; }
    /// <summary>
    /// The action to perform when this hotkey is pressed.
    /// This property may be <see langword="null"/> and is as such mutually exclusive with <see cref="AsyncDelegate"/>, if the hotkey was created using such an asynchronous delegate.
    /// </summary>
    public Action? SyncDelegate { get; init; }
    /// <summary>
    /// The action to perform when this hotkey is pressed.
    /// This property may be <see langword="null"/> and is as such mutually exclusive with <see cref="SyncDelegate"/>, if the hotkey was created using such a synchronous delegate.
    /// </summary>
    public Func<Task>? AsyncDelegate { get; init; }

    private Hotkey(int id)
    {
        Id = id;
    }
    private Hotkey(int id, FsModifiers modifiers, Keys key) : this(id)
    {
        Modifiers = modifiers;
        Key = key;
    }
    internal Hotkey(int id, FsModifiers modifiers, Keys key, Action syncDelegate) : this(id, modifiers, key)
    {
        SyncDelegate = syncDelegate;
    }
    internal Hotkey(int id, FsModifiers modifiers, Keys key, Func<Task> asyncDelegate) : this(id, modifiers, key)
    {
        AsyncDelegate = asyncDelegate;
    }
}
