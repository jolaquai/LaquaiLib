using System.IO;

using CoreAudio;

namespace LaquaiLib.Windows.Util.VolumeManager;

/// <summary>
/// Represents a controller for managing the volume of a specific window or process.
/// </summary>
public class VolumeController
{
    /// <summary>
    /// The name of the process or window title this handler targets.
    /// </summary>
    public required string ProcessNameOrTitle {
        get;
        init
        {
            field = value;

            field = field.Trim();
            field = Path.GetFileNameWithoutExtension(field);
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
