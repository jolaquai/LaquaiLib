using System.Runtime.CompilerServices;

namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    /// <summary>
    /// Gets a <see cref="TaskAwaiter"/> that can be used to await the completion of all tasks in the specified collection.
    /// </summary>
    /// <param name="tasks">The collection of tasks.</param>
    /// <returns>A <see cref="TaskAwaiter"/> that can be used to await the completion of all tasks in the specified collection.</returns>
    public static TaskAwaiter GetAwaiter(this IEnumerable<Task> tasks) => Task.WhenAll(tasks).GetAwaiter();
    /// <summary>
    /// Starts all tasks in the specified collection.
    /// </summary>
    /// <param name="tasks">The collection of tasks.</param>
    public static void Start(this IEnumerable<Task> tasks) => tasks.ForEach(t => t.Start());
}
