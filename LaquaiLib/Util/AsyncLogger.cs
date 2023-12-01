using System.Collections.Concurrent;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a logger that writes messages to the <see cref="Console"/> in a separate thread to avoid blocking operations on the main thread.
/// </summary>
/// <remarks>
/// This type is thread-safe.
/// </remarks>
public static class AsyncLogger
{
    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="AsyncLogger"/> should run in the background.
    /// If <see langword="true"/>, the <see cref="AsyncLogger"/> will exit when the parent thread exits.
    /// By default, this is <see langword="true"/>.
    /// </summary>
    public static bool IsBackground { get; set; } = true;

    private static bool initialized;

    private static readonly Thread _messageQueueHandler = new Thread(MessageQueueHandlerProc);
    private static readonly ConcurrentQueue<LoggerMessage> _messages = new ConcurrentQueue<LoggerMessage>();

    /// <summary>
    /// Initializes the <see cref="AsyncLogger"/>.
    /// It exits when the parent thread (the thread calling this method) exits.
    /// </summary>
    public static void Initialize() => Initialize(Thread.CurrentThread);
    /// <summary>
    /// Initializes the <see cref="AsyncLogger"/> usnig the given <paramref name="thread"/> as its parent.
    /// </summary>
    /// <param name="thread">The thread to use as the parent thread. May be <see langword="null"/> to explicitly leave the <see cref="AsyncLogger"/> running, even after all other threads have exited.</param>
    public static void Initialize(Thread? thread)
    {
        _messageQueueHandler.Start(thread);
        initialized = true;
    }

    private static void MessageQueueHandlerProc(object? state)
    {
        var parent = state as Thread;

        while (true)
        {
            if (_messages.TryDequeue(out var msg))
            {
                if (msg.Message is null)
                {
                    Console.WriteLine();
                }

                Console.ForegroundColor = msg.Type switch
                {
                    MessageType.Info => ConsoleColor.White,
                    MessageType.Warning => ConsoleColor.Yellow,
                    MessageType.Error => ConsoleColor.Red,
                    MessageType.Success => ConsoleColor.Green,
                    _ => ConsoleColor.White
                };
                Console.WriteLine($"[{msg.Timestamp:s}] {msg.Message}");
                Console.ResetColor();

                continue;
            }
            if (IsBackground && parent?.ThreadState.HasFlag(ThreadState.Stopped) == true)
            {
                break;
            }
            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// Creates and queues a new <see cref="LoggerMessage"/> with the given <paramref name="message"/> and <paramref name="type"/>. Its <see cref="LoggerMessage.Timestamp"/> is set to <see cref="DateTime.Now"/>.
    /// </summary>
    /// <param name="message">The text of the message.</param>
    /// <param name="type">The type of the message.</param>
    public static void QueueMessage(string? message, MessageType type = MessageType.Info) => QueueMessage(new LoggerMessage(message, DateTime.Now, type));
    /// <summary>
    /// Creates and queues a new <see cref="LoggerMessage"/> with the given <paramref name="message"/>, <paramref name="timestamp"/> and <paramref name="type"/>.
    /// </summary>
    /// <param name="message">The text of the message.</param>
    /// <param name="timestamp">The timestamp of the message.</param>
    /// <param name="type">The type of the message.</param>
    public static void QueueMessage(string? message, DateTime timestamp, MessageType type = MessageType.Info) => QueueMessage(new LoggerMessage(message, timestamp, type));
    /// <summary>
    /// Queues the given <paramref name="message"/>.
    /// </summary>
    /// <param name="message">The <see cref="LoggerMessage"/> to queue.</param>
    public static void QueueMessage(LoggerMessage message)
    {
        if (!initialized)
        {
            Initialize();
            initialized = true;
        }

        _messages.Enqueue(message);
    }
}

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

/// <summary>
/// Represents an immutable message that can be queued for logging.
/// </summary>
public readonly struct LoggerMessage : IEquatable<LoggerMessage>
{
    /// <summary>
    /// The text of the message.
    /// If explicitly <see langword="null"/>, an empty line is written (i.e. without the default formatting including a timestamp).
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
    /// Instantiates a new <see cref="LoggerMessage"/> with the given <paramref name="message"/>.
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
    /// Instantiates a new <see cref="LoggerMessage"/> with the given <paramref name="message"/> and <paramref name="type"/>.
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
    /// Instantiates a new <see cref="LoggerMessage"/> with the given data.
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
    public override bool Equals(object? obj)
    {
        return obj is LoggerMessage msg && Equals(msg);
    }
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.Add(Timestamp);
        hc.Add(Type);
        if (Message is not null)
        {
            hc.Add(string.Empty);
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
