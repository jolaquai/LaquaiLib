using LaquaiLib.Threading;

namespace LaquaiLib.Extensions;

public static class ThreadingExtensions
{
    extension(SynchronizationContext synchronizationContext)
    {
        /// <summary>
        /// Gets an awaitable that causes continuations on <see langword="await"/>s on it to be posted to the specified <paramref name="synchronizationContext"/>.
        /// </summary>
        /// <returns>The awaitable object bound to the specified <paramref name="synchronizationContext"/>.</returns>
        public SynchronizationContextAwaitable SwitchTo() => new SynchronizationContextAwaitable(synchronizationContext);
    }
    extension(TaskScheduler taskScheduler)
    {
        /// <summary>
        /// Gets an awaitable that causes continuations on <see langword="await"/>s on it to be scheduled as <see cref="Task"/>s using the specified <paramref name="taskScheduler"/>.
        /// </summary>
        /// <returns>The awaitable object bound to the specified <paramref name="taskScheduler"/>.</returns>
        public TaskSchedulerAwaitable SwitchTo() => new TaskSchedulerAwaitable(taskScheduler);
    }
}
