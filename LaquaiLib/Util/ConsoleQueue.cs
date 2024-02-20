using System.Collections.Concurrent;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a queue of console messages that is asynchronously emptied to the console.
/// </summary>
public static class ConsoleQueue
{
    private static readonly Thread _parentThread;
    private static readonly Thread _emptyQueueThread;
    private static bool forceEmptyBeforeShutdown;
    /// <summary>
    /// Controls whether the queue should be emptied before the application can shut down.
    /// This defaults to <see langword="false"/> and may be freely changed at any time.
    /// </summary>
    public static bool ForceEmptyBeforeShutdown
    {
        get => forceEmptyBeforeShutdown;
        set
        {
            if (forceEmptyBeforeShutdown == value)
            {
                return;
            }

            forceEmptyBeforeShutdown = value;
            _emptyQueueThread.IsBackground = !value;
        }
    }
    private static readonly ConcurrentQueue<object?> _queue;

    static ConsoleQueue()
    {
        _parentThread = Thread.CurrentThread;
        _emptyQueueThread = new Thread(EmptyQueueProc);
        ForceEmptyBeforeShutdown = false;
        _queue = [];
        _emptyQueueThread.Start();
    }

    private static void EmptyQueueProc()
    {
        while (_parentThread.IsAlive || ForceEmptyBeforeShutdown)
        {
            if (_queue.TryDequeue(out var message))
            {
                Console.WriteLine(message);
            }
            else
            {
                Thread.Sleep(100);
            }
        }
    }

    /// <summary>
    /// Enqueues a message to the queue.
    /// </summary>
    /// <param name="message">The message to enqueue.</param>
    public static void Enqueue(object? message) => _queue.Enqueue(message);
    /// <summary>
    /// Enqueues multiple messages to the queue.
    /// </summary>
    /// <param name="messages">The messages to enqueue.</param>
    public static void Enqueue(params object?[] messages) => Array.ForEach(messages, _queue.Enqueue);
    /// <summary>
    /// Creates and returns a <see cref="Task"/> that completes when the queue has been emptied.
    /// </summary>
    public static Task FlushAsync() => Task.Run(Flush);
    /// <summary>
    /// Blocks until the queue has been emptied.
    /// </summary>
    public static void Flush() => SpinWait.SpinUntil(() => _queue.IsEmpty);

    /// <summary>
    /// Returns an enumerator that iterates through the queue.
    /// </summary>
    public static IEnumerator<object?> GetEnumerator() => _queue.GetEnumerator();
}
