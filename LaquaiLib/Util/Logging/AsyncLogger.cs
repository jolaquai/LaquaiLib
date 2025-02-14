using System.Collections.Concurrent;
using System.Text;

using LaquaiLib.Extensions;

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
    private static readonly ConcurrentQueue<LoggerMessage> _messages = [];

    /// <summary>
    /// The target <see cref="TextWriter"/> to write messages to.
    /// Changing this while messages are being processed is not recommended.
    /// If this is <see cref="Console.Out"/>, output colorization is supported automatically through <see cref="LoggerMessage.Type"/>.
    /// </summary>
    public static TextWriter Target { get; set; } = Console.Out;

    /// <summary>
    /// Gets a value indicating whether the <see cref="AsyncLogger"/> is currently processing messages, that is, the message queue is not empty.
    /// </summary>
    public static bool Processing => !_messages.IsEmpty;

    /// <summary>
    /// Initializes the <see cref="AsyncLogger"/>.
    /// It exits when the parent thread (the thread calling this method) exits.
    /// </summary>
    public static void Initialize() => Initialize(Thread.CurrentThread);
    /// <summary>
    /// Initializes the <see cref="AsyncLogger"/> using the given <paramref name="thread"/> as its parent.
    /// </summary>
    /// <param name="thread">The thread to use as the parent thread. May be <see langword="null"/> to explicitly leave the <see cref="AsyncLogger"/> running, even after all other threads have exited.</param>
    public static void Initialize(Thread thread)
    {
        _messageQueueHandler.Start(thread);
        initialized = true;
    }

    private static void MessageQueueHandlerProc(object state)
    {
        var parent = state as Thread;

        while (true)
        {
            if (_messages.TryDequeue(out var msg))
            {
                if (msg.Message is null)
                {
                    Target.WriteLine();
                }

                if (Target == Console.Out)
                {
                    Console.ForegroundColor = msg.Type switch
                    {
                        MessageType.Info => ConsoleColor.White,
                        MessageType.Warning => ConsoleColor.DarkYellow,
                        MessageType.Error => ConsoleColor.DarkRed,
                        MessageType.Success => ConsoleColor.DarkGreen,
                        _ => ConsoleColor.White
                    };
                    WriteMessage(msg);
                    Console.ResetColor();
                }
                else
                {
                    WriteMessage(msg);
                }

                continue;
            }
            if (IsBackground && parent?.ThreadState.HasFlag(ThreadState.Stopped) == true)
            {
                break;
            }
            Thread.Sleep(100);
        }
    }
    private static void WriteMessage(LoggerMessage msg)
    {
        var msgSpan = msg.Message.AsSpan();
        var first = true;
        var timestamp = $"[{msg.Timestamp:s}] ";
        var padding = new string(' ', timestamp.Length);

        foreach (var line in msgSpan.EnumerateLines())
        {
            if (first)
            {
                Target.WriteLine($"{timestamp}{line}");
                first = false;
            }
            else
            {
                Target.WriteLine($"{padding}{line}");
            }
        }
    }

    /// <summary>
    /// Creates and queues a new <see cref="LoggerMessage"/> with the given <paramref name="message"/> and <paramref name="type"/>. Its <see cref="LoggerMessage.Timestamp"/> is set to <see cref="DateTime.Now"/>.
    /// </summary>
    /// <param name="message">The object to convert to a string and use as the text of the message.</param>
    /// <param name="type">The type of the message.</param>
    public static void QueueMessage(object message, MessageType type = MessageType.Info) => QueueMessage(new LoggerMessage(message?.ToString(), DateTime.Now, type));
    /// <summary>
    /// Creates and queues a new <see cref="LoggerMessage"/> with the given <paramref name="message"/>, <paramref name="timestamp"/> and <paramref name="type"/>.
    /// </summary>
    /// <param name="message">The object to convert to a string and use as the text of the message.</param>
    /// <param name="timestamp">The timestamp of the message.</param>
    /// <param name="type">The type of the message.</param>
    public static void QueueMessage(object message, DateTime timestamp, MessageType type = MessageType.Info) => QueueMessage(new LoggerMessage(message?.ToString(), timestamp, type));
    /// <summary>
    /// Creates and queues a new <see cref="LoggerMessage"/> with the given <paramref name="message"/> and <paramref name="type"/>. Its <see cref="LoggerMessage.Timestamp"/> is set to <see cref="DateTime.Now"/>.
    /// </summary>
    /// <param name="message">The text of the message.</param>
    /// <param name="type">The type of the message.</param>
    public static void QueueMessage(string message, MessageType type = MessageType.Info) => QueueMessage(new LoggerMessage(message, DateTime.Now, type));
    /// <summary>
    /// Creates and queues a new <see cref="LoggerMessage"/> with the given <paramref name="message"/>, <paramref name="timestamp"/> and <paramref name="type"/>.
    /// </summary>
    /// <param name="message">The text of the message.</param>
    /// <param name="timestamp">The timestamp of the message.</param>
    /// <param name="type">The type of the message.</param>
    public static void QueueMessage(string message, DateTime timestamp, MessageType type = MessageType.Info) => QueueMessage(new LoggerMessage(message, timestamp, type));
    /// <summary>
    /// Creates and queues a new multi-line <see cref="LoggerMessage"/> with the given <paramref name="lines"/> and <paramref name="type"/>.
    /// </summary>
    /// <param name="lines">The lines of the message.</param>
    /// <param name="type">The type of the message.</param>
    public static void QueueMessage(ReadOnlySpan<string> lines, MessageType type = MessageType.Info) => QueueMessage(lines, DateTime.Now, type);
    /// <summary>
    /// Creates and queues a new multi-line <see cref="LoggerMessage"/> with the given <paramref name="lines"/>, <paramref name="timestamp"/> and <paramref name="type"/>.
    /// </summary>
    /// <param name="lines">The lines of the message.</param>
    /// <param name="timestamp">The timestamp of the message.</param>
    /// <param name="type">The type of the message.</param>
    public static void QueueMessage(ReadOnlySpan<string> lines, DateTime timestamp, MessageType type = MessageType.Info)
    {
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            _ = sb.AppendLine(line);
        }
        QueueMessage(sb.ToString(), timestamp, type);
    }
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

    /// <summary>
    /// Creates a <see cref="Task"/> that completes when the message queue is empty.
    /// </summary>
    public static Task FlushAsync()
    {
        return Task.Run(static async () =>
        {
            while (Processing)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        });
    }
    /// <summary>
    /// Blocks the calling thread until the message queue is empty.
    /// </summary>
    public static void Flush() => FlushAsync().GetAwaiter().GetResult();
}
