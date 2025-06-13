namespace LaquaiLib.Util.Logging;

/// <summary>
/// Identifies the type of a <see cref="LoggerMessage"/>.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Indicates that the <see cref="LoggerMessage"/> is informational.
    /// </summary>
    Info,
    /// <summary>
    /// Indicates that the <see cref="LoggerMessage"/> is a warning.
    /// </summary>
    Warning,
    /// <summary>
    /// Indicates that the <see cref="LoggerMessage"/> is an error.
    /// </summary>
    Error,
    /// <summary>
    /// Indicates that the <see cref="LoggerMessage"/> is a success message.
    /// </summary>
    Success,
}
