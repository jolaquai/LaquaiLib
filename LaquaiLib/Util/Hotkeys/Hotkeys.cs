using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Util.Hotkeys;

/// <summary>
/// Contains static methods for managing system-wide (henceforth referred to simply as 'global') hotkeys.
/// </summary>
public static partial class Hotkeys
{
    private static readonly nint _hostWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    private static readonly List<Hotkey> _hotkeys = [];

    private static readonly Task _messageProcessor = Task.Run(async () =>
    {
        while (true)
        {
            if (ProcessHotkeys && GetMessage(out var msg) > 0)
            {
                if (msg.message == WM_HOTKEY)
                {
                    var hotkey = _hotkeys
                        .Find(hk => hk.Key == (Keys)msg.VirtualKeyCode
                            && hk.Modifiers == (FsModifiers)msg.ModifierKeys
                            && hk.Id == (int)msg.wParam
                        );
                    if (hotkey is not null)
                    {
                        await OnHotkeyTriggered(hotkey).ConfigureAwait(false);
                    }
                }
            }

            // await Task.Delay(10);
        }
    });

    private const int WM_HOTKEY = 0x0312;
    private const int ID_MIN = 0x0000;
    private const int ID_MAX = 0xBFFF;
    private static volatile int id = ID_MIN;

    // Note: System.Windows.Forms.Keys contains all and more keys supported by RegisterHotKey
    // Just cast the enum to uint and you're done

    /// <summary>
    /// Gets the number of used hotkey slots.
    /// </summary>
    public static int UsedHotkeySlots => id - ID_MIN;
    /// <summary>
    /// Gets the number of free hotkey slots.
    /// </summary>
    public static int FreeHotkeySlots => ID_MAX - id;

    #region P/Invoke
    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);
    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int UnregisterHotKey(nint hWnd, int id);

    [LibraryImport("user32.dll", EntryPoint = "GetMessageW")]
    private static partial int GetMessage(out LPMSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [StructLayout(LayoutKind.Sequential)]
    private struct LPMSG
    {
        public HWND hWnd;
        public uint message;
        public WPARAM wParam;
        public LPARAM lParam;
        public DWORD time;
        public Point pt;
        public DWORD lPrivate;

        /// <summary>
        /// Retrieves the value of the low-order word of <see cref="lParam"/>.
        /// This specifies the modifier keys associated with the pressed hotkey.
        /// </summary>
        /// <returns>The modifier keys associated with the pressed hotkey.</returns>
        public nint ModifierKeys => lParam & 0xFFFF;
        /// <summary>
        /// Retrieves the value of the high-order word of <see cref="lParam"/>.
        /// This specifies the virtual key code of the pressed hotkey.
        /// </summary>
        /// <returns>The virtual key code of the pressed hotkey.</returns>
        public nint VirtualKeyCode => (lParam >> 16) & 0xFFFF;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int x;
        public int y;
    }
    #endregion

    private static bool hasStoppedManually;
    private static bool processHotkeys;
    /// <summary>
    /// Controls whether hotkeys are processed.
    /// </summary>
    public static bool ProcessHotkeys {
        get => processHotkeys;
        set
        {
            if (processHotkeys != value)
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }
    }
    /// <summary>
    /// Starts processing hotkeys.
    /// </summary>
    public static void Start() => processHotkeys = true;
    /// <summary>
    /// Stops processing hotkeys.
    /// </summary>
    public static void Stop()
    {
        hasStoppedManually = true;
        processHotkeys = false;
    }

    /// <summary>
    /// Registers a global hotkey and begins processing hotkeys if processing has not been explicitly stopped.
    /// </summary>
    /// <param name="fsModifiers">A <see cref="FsModifiers"/> value specifying the modifier keys to be associated with the hotkey.</param>
    /// <param name="vk">The virtual key code to be associated with the hotkey.</param>
    /// <param name="method">A <see cref="Delegate"/> representing the method to be invoked when the hotkey is pressed. Must be castable to either <see cref="Action"/> or a <see cref="Task"/>-returning <see cref="Func{TResult}"/>, otherwise an <see cref="ArgumentException"/> is thrown. Such an asynchronous <see langword="delegate"/> is <see langword="await"/>ed appropriately upon invocation.</param>
    /// <param name="hotkey"></param>
    /// <returns>The result of the registration attempt.</returns>
    public static bool RegisterHotkey(FsModifiers fsModifiers, Keys vk, Delegate method, [NotNullWhen(true)] out Hotkey? hotkey)
    {
        if (!processHotkeys && !hasStoppedManually)
        {
            Start();
        }

        hotkey = null;
        var registered = RegisterHotKey(_hostWindowHandle, id++, (uint)fsModifiers, (uint)vk) > 0;

        var newHotkey = method switch
        {
            Action action => new Hotkey(id, fsModifiers, vk, action),
            Func<Task> func => new Hotkey(id, fsModifiers, vk, func),
            _ => throw new ArgumentException($"The passed delegate must be castable to either {nameof(Action)} or {nameof(Func<Task>)}."),
        };
        if (registered)
        {
            _hotkeys.Add(newHotkey);
            hotkey = newHotkey;
        }
        return registered;
    }
    /// <summary>
    /// Unregisters a global hotkey.
    /// </summary>
    /// <param name="id">The ID of the hotkey to be unregistered.</param>
    /// <param name="hotkey">The <see cref="Hotkey"/> to be unregistered. This should not be specified if consuming code stores its <see cref="Hotkey"/> instances by ID.</param>
    /// <returns>The result of the unregistration attempt.</returns>
    public static bool UnregisterHotkey(int id, Hotkey? hotkey = null)
    {
        hotkey ??= _hotkeys.Find(hk => hk.Id == id);
        if (hotkey is not null)
        {
            _ = _hotkeys.Remove(hotkey);
            return UnregisterHotKey(_hostWindowHandle, id) != 0;
        }
        return false;
    }
    /// <summary>
    /// Unregisters a global hotkey.
    /// </summary>
    /// <param name="hotkey">The <see cref="Hotkey"/> to be unregistered.</param>
    /// <returns>The result of the unregistration attempt.</returns>
    public static bool UnregisterHotkey(Hotkey hotkey) => UnregisterHotkey(hotkey.Id, hotkey);
    private static int GetMessage(out LPMSG lpMsg) =>
        // Get only WM_HOTKEY messages
        // Also use GetMessage instead of PeekMessage to block until a message is available
        // This way, we don't need a timer or something similar to keep the application running
        // Just offload this into a separate thread and we're good
        GetMessage(out lpMsg, _hostWindowHandle, 0, 0);

    private static async Task OnHotkeyTriggered(Hotkey hotkey)
    {
        if (hotkey.SyncDelegate is not null)
        {
            hotkey.SyncDelegate();
        }
        else if (hotkey.AsyncDelegate is not null)
        {
            await hotkey.AsyncDelegate().ConfigureAwait(false);
        }
    }
}
