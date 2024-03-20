using System.Collections.Concurrent;

namespace LaquaiLib.Util.Logging;

/// <summary>
/// Represents a logger that writes messages to the <see cref="Console"/> in a separate thread to avoid blocking operations on the main thread. This type self-initializes when it is first used, that is, when the first message is queued. Before that, the background thread is not running.
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
