namespace LaquaiLib.Util.Logging;

/// <summary>
/// Represents an immutable message that can be queued for logging.
/// </summary>
public readonly struct LoggerMessage : IEquatable<LoggerMessage>
{
    /// <summary>
    /// The text of the message.
    /// If explicitly <see langword="null"/>, an empty line is written (i.e. without the default formatting including a timestamp).
    /// If this contains multiple lines, each line is written separately. For each but the first, padding is added to align the text with the first line, after the timestamp.
    /// </summary>
    public readonly string? Message { get; }
    /// <summary>
    /// The timestamp of the message.
    /// </summary>
    public readonly DateTime Timestamp { get; }
    /// <summary>
    /// The type of the message.
    /// </summary>
    public readonly MessageType Type { get; }

    /// <summary>
    /// Initializes a new <see cref="LoggerMessage"/> with the given <paramref name="message"/>.
    /// The default <see cref="MessageType"/> is <see cref="MessageType.Info"/> and its <see cref="Timestamp"/> is set to <see cref="DateTime.Now"/>.
    /// </summary>
    /// <param name="message">The text of the message.</param>
    public LoggerMessage(string? message)
    {
        Message = message;
        Timestamp = DateTime.Now;
        Type = MessageType.Info;
    }
    /// <summary>
    /// Initializes a new <see cref="LoggerMessage"/> with the given <paramref name="message"/> and <paramref name="type"/>.
    /// </summary>
    /// <param name="message">The text of the message.</param>
    /// <param name="type">The type of the message.</param>
    public LoggerMessage(string? message, MessageType type)
    {
        Message = message;
        Timestamp = DateTime.Now;
        Type = type;
    }
    /// <summary>
    /// Initializes a new <see cref="LoggerMessage"/> with the given data.
    /// </summary>
    /// <param name="message">The text of the message.</param>
    /// <param name="timestamp">The timestamp of the message.</param>
    /// <param name="type">The type of the message.</param>
    public LoggerMessage(string? message, DateTime timestamp, MessageType type)
    {
        Message = message;
        Timestamp = timestamp;
        Type = type;
    }

    /// <inheritdoc/>
    public bool Equals(LoggerMessage other) => Message == other.Message && Timestamp.Equals(other.Timestamp) && Type == other.Type;
    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is LoggerMessage lm && Equals(lm);
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.Add(Timestamp);
        hc.Add(Type);
        if (Message is not null)
        {
            hc.Add(Message);
        }
        return hc.ToHashCode();
    }

    /// <summary>
    /// Indicates whether two <see cref="LoggerMessage"/>s are equal.
    /// </summary>
    /// <param name="left">The first <see cref="LoggerMessage"/>.</param>
    /// <param name="right">The second <see cref="LoggerMessage"/>.</param>
    /// <returns><see langword="true"/> if the two <see cref="LoggerMessage"/>s are equal, otherwise <see langword="false"/>.</returns>
    public static bool operator ==(LoggerMessage left, LoggerMessage right) => left.Equals(right);
    /// <summary>
    /// Indicates whether two <see cref="LoggerMessage"/>s are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="LoggerMessage"/>.</param>
    /// <param name="right">The second <see cref="LoggerMessage"/>.</param>
    /// <returns><see langword="true"/> if the two <see cref="LoggerMessage"/>s are not equal, otherwise <see langword="false"/>.</returns>
    public static bool operator !=(LoggerMessage left, LoggerMessage right) => !(left == right);
}
