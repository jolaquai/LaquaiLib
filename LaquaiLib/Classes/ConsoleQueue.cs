using System.Collections.Concurrent;

namespace LaquaiLib.Classes;

/// <summary>
/// Represents a (thread-safe) wrapper around the <see cref="Console"/>. It is used to postpone blocking console output until after any expensive computations are completed.
/// </summary>
public static class ConsoleQueue
{
    /// <summary>
    /// The <see cref="object"/> used to lock when attempting to write to the <see cref="Console"/>.
    /// </summary>
    private static readonly object consoleLock = new object();

    /// <summary>
    /// The internal queue of objects to eventually output to the <see cref="Console"/>.
    /// </summary>
    public static ConcurrentQueue<object> Queue { get; } = new ConcurrentQueue<object>();

    /// <summary>
    /// Adds an object to the end of the internal queue.
    /// </summary>
    /// <param name="obj">The object to eventually output to the <see cref="Console"/>.</param>
    /// <returns>The index of the position in the internal queue at which <paramref name="obj"/> is.</returns>
    public static int Enqueue(object obj)
    {
        Queue.Enqueue(obj);
        return Queue.Count;
    }

    /// <summary>
    /// Flushes all contents of the internal queue into the <see cref="Console"/>. This operation blocks until all objects in the internal queue have been output.
    /// </summary>
    /// <returns>The number of objects written to the <see cref="Console"/>.</returns>
    public static int Flush()
    {
        var cnt = Queue.Count;
        lock (consoleLock)
        {
            while (Queue.TryDequeue(out var obj))
            {
                Console.WriteLine(obj);
            }
        }
        Queue.Clear();
        return cnt;
    }

    /// <summary>
    /// Flushes all contents of the internal queue into the <see cref="Console"/> after invoking a transform function on them. This operation blocks until all objects in the internal queue have been output.
    /// </summary>
    /// <returns>The number of objects written to the <see cref="Console"/>.</returns>
    public static int Flush<T>(Func<object, T> transform)
    {
        var cnt = Queue.Count;
        lock (consoleLock)
        {
            while (Queue.TryDequeue(out var obj))
            {
                Console.WriteLine(transform(obj));
            }
        }
        return cnt;
    }
}
