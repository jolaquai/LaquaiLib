namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    extension(IEnumerable<Task> tasks)
    {
        /// <summary>
        /// Gets a <see cref="TaskAwaiter"/> that can be used to await the completion of all tasks in the specified collection.
        /// </summary>
        /// <param name="tasks">The collection of tasks.</param>
        /// <returns>A <see cref="TaskAwaiter"/> that can be used to await the completion of all tasks in the specified collection.</returns>
        public TaskAwaiter GetAwaiter() => Task.WhenAll(tasks).GetAwaiter();
        /// <summary>
        /// Starts all tasks in the specified collection.
        /// </summary>
        /// <param name="tasks">The collection of tasks.</param>
        public void Start() => tasks.ForEach(static t => t.Start());

        /// <summary>
        /// Synchronously waits for all <see cref="Task"/>s in the specified enumerable to complete unless that wait is cancelled.
        /// </summary>
        /// <param name="tasks">The enumerable of <see cref="Task"/>s.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the wait.</param>
        public void WaitAll(CancellationToken cancellationToken = default) => Task.WaitAll(tasks, cancellationToken);
        /// <summary>
        /// Synchronously waits for any <see cref="Task"/> in the specified enumerable to complete unless that wait is cancelled.
        /// </summary>
        /// <param name="tasks">The enumerable of <see cref="Task"/>s.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the wait.</param>
        /// <returns>The <see cref="Task"/> that completed.</returns>
        public Task WaitAny(CancellationToken cancellationToken = default)
        {
            var taskArray = tasks as Task[] ?? tasks.ToArray();
            return taskArray[Task.WaitAny(taskArray, cancellationToken)];
        }
        /// <summary>
        /// Asynchronously waits for all <see cref="Task"/>s in the specified enumerable to complete.
        /// </summary>
        /// <param name="tasks">The enumerable of <see cref="Task"/>s.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous wait operation.</returns>
        public Task WhenAll() => Task.WhenAll(tasks);
        /// <summary>
        /// Asynchronously waits for any <see cref="Task"/> in the specified enumerable to complete.
        /// </summary>
        /// <param name="tasks">The enumerable of <see cref="Task"/>s.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous wait operation. Its result resolves to the <see cref="Task"/> that completed.</returns>
        public Task<Task> WhenAny() => Task.WhenAny(tasks);
        /// <summary>
        /// Gets an <see cref="IAsyncEnumerable{T}"/> that will yield the specified <see cref="Task"/>s as they complete.
        /// </summary>
        /// <param name="tasks">The enumerable of <see cref="Task"/>s.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> that iterates through the specified <see cref="Task"/>s as they complete.</returns>
        public IAsyncEnumerable<Task> WhenEach() => Task.WhenEach(tasks);
    }

    extension<TResult>(IEnumerable<Task<TResult>> tasks)
    {
        /// <summary>
        /// Synchronously waits for all <see cref="Task"/>s in the specified enumerable to complete unless that wait is cancelled.
        /// </summary>
        /// <param name="tasks">The enumerable of <see cref="Task"/>s.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the wait.</param>
        public void WaitAll(CancellationToken cancellationToken = default) => Task.WaitAll(tasks, cancellationToken);
        /// <summary>
        /// Synchronously waits for any <see cref="Task"/> in the specified enumerable to complete unless that wait is cancelled.
        /// </summary>
        /// <param name="tasks">The enumerable of <see cref="Task"/>s.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the wait.</param>
        /// <returns>The <see cref="Task"/> that completed.</returns>
        public Task<TResult> WaitAny(CancellationToken cancellationToken = default)
        {
            var taskArray = tasks as Task<TResult>[] ?? tasks.ToArray();
            return taskArray[Task.WaitAny(taskArray, cancellationToken)];
        }
        /// <summary>
        /// Asynchronously waits for all <see cref="Task"/>s in the specified enumerable to complete.
        /// </summary>
        /// <param name="tasks">The enumerable of <see cref="Task"/>s.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous wait operation.</returns>
        public Task<TResult[]> WhenAll() => Task.WhenAll(tasks);
        /// <summary>
        /// Asynchronously waits for any <see cref="Task"/> in the specified enumerable to complete.
        /// </summary>
        /// <param name="tasks">The enumerable of <see cref="Task"/>s.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous wait operation. Its result resolves to the <see cref="Task"/> that completed.</returns>
        public Task<Task<TResult>> WhenAny() => Task.WhenAny(tasks);
        /// <summary>
        /// Gets an <see cref="IAsyncEnumerable{T}"/> that will yield the specified <see cref="Task"/>s as they complete.
        /// </summary>
        /// <param name="tasks">The enumerable of <see cref="Task"/>s.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> that iterates through the specified <see cref="Task"/>s as they complete.</returns>
        public IAsyncEnumerable<Task<TResult>> WhenEach() => Task.WhenEach(tasks);
    }
}
