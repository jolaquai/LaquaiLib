namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> of <see cref="Task"/> Type.
/// </summary>
public static class IEnumerableTaskExtensions
{
    /// <summary>
    /// Starts all the tasks in this collection of <see cref="Task"/>s.
    /// </summary>
    /// <param name="tasks">The <see cref="IEnumerable{T}"/> of <see cref="Task"/> that contains the <see cref="Task"/>s that are to be started.</param>
    /// <remarks>Note that this does not await any of the <see cref="Task"/>s, they are merely started. The calling code is expected to await the availability of and process their results.</remarks>
    public static void Start(this IEnumerable<Task> tasks) => tasks.ForEach(t => t.Start());

    /// <summary>
    /// Starts all the tasks in this collection of <see cref="Task"/>s and returns a <see cref="Task"/> that represents their completion.
    /// </summary>
    /// <param name="tasks">The <see cref="IEnumerable{T}"/> of <see cref="Task"/> that contains the <see cref="Task"/>s that are to be started.</param>
    /// <returns>A <see cref="Task"/> that completes when all of the <see cref="Task"/>s in this collection have completed.</returns>
    public static Task StartAsync(this IEnumerable<Task> tasks)
    {
        tasks.Start();
        return Task.WhenAll(tasks);
    }
}