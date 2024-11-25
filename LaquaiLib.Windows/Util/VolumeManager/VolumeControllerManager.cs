using CoreAudio;

namespace LaquaiLib.Windows.Util.VolumeManager;

/// <summary>
/// Implements a manager for creating <see cref="VolumeController"/>s.
/// </summary>
public class VolumeControllerManager
{
    private readonly Action<AudioSessionControl2> _defaultOnFocusReceived;
    private readonly Action<AudioSessionControl2> _defaultOnFocusLost;

    /// <summary>
    /// Initializes a new <see cref="VolumeControllerManager"/> with the specified default focus received and lost actions.
    /// </summary>
    /// <param name="defaultOnFocusReceived">The <see cref="Action{T}"/> to be called when a window or process represented by a <see cref="AudioSessionControl2"/> receives focus.</param>
    /// <param name="defaultOnFocusLost">The <see cref="Action{T}"/> to be called when a window or process represented by a <see cref="AudioSessionControl2"/> loses focus.</param>
    public VolumeControllerManager(Action<AudioSessionControl2> defaultOnFocusReceived, Action<AudioSessionControl2> defaultOnFocusLost)
    {
        _defaultOnFocusReceived = defaultOnFocusReceived;
        _defaultOnFocusLost = defaultOnFocusLost;
    }

    /// <summary>
    /// Creates a new <see cref="VolumeController"/> with the specified process name or title and focus received and lost actions.
    /// </summary>
    /// <param name="processNameOrTitle">The name or title of the window or process to be controlled by the <see cref="VolumeController"/>.</param>
    /// <param name="onFocusReceived">The <see cref="Action{T}"/> to be called when the window or process represented by the <see cref="VolumeController"/> receives focus. If omitted or <see langword="null"/>, the default focus received action will be used.</param>
    /// <param name="onFocusLost">The <see cref="Action{T}"/> to be called when the window or process represented by the <see cref="VolumeController"/> loses focus. If omitted or <see langword="null"/>, the default focus lost action will be used.</param>
    /// <returns>The created <see cref="VolumeController"/>.</returns>
    public VolumeController CreateController(string processNameOrTitle, Action<AudioSessionControl2> onFocusReceived = null, Action<AudioSessionControl2> onFocusLost = null)
    {
        return new VolumeController()
        {
            ProcessNameOrTitle = processNameOrTitle,
            OnFocusReceived = onFocusReceived ?? _defaultOnFocusReceived,
            OnFocusLost = onFocusLost ?? _defaultOnFocusLost
        };
    }
}
