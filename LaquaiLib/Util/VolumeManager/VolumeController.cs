using CoreAudio;

namespace LaquaiLib.Util.VolumeManager;

/// <summary>
/// Represents a controller for managing the volume of a specific window or process.
/// </summary>
public record class VolumeController
{
    private readonly string processNameOrTitle;
    /// <summary>
    /// The name of the process or window title this handler targets.
    /// </summary>
    public required string ProcessNameOrTitle
    {
        get => processNameOrTitle;
        init
        {
            processNameOrTitle = value;

            processNameOrTitle = processNameOrTitle.Trim();
            processNameOrTitle = Path.GetFileNameWithoutExtension(processNameOrTitle);
        }
    }
    /// <summary>
    /// The <see cref="Action{T}"/> that is invoked when the targeted window gains focus.
    /// It is passed the <see cref="AudioSessionControl2"/> representing the window.
    /// </summary>
    public required Action<AudioSessionControl2> OnFocusReceived { get; init; }
    /// <summary>
    /// The <see cref="Action{T}"/> that is invoked when the targeted window loses focus.
    /// It is passed the <see cref="AudioSessionControl2"/> representing the window.
    /// </summary>
    public required Action<AudioSessionControl2> OnFocusLost { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VolumeController"/> class.
    /// </summary>
    internal VolumeController()
    {
    }
}
