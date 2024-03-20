
using System.Collections.Concurrent;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a <see cref="TaskScheduler"/> implementation that limits the amount of <see cref="Task"/>s that run concurrently at any given time.
/// </summary>
public class LimitedConcurrencyTaskScheduler : TaskScheduler
{
    private readonly BlockingCollection<Task> _tasks = [];
    private readonly int _maxDegreeOfParallelism;
    private int _delegatesQueuedOrRunning;

    /// <summary>
    /// Returns an instance of <see cref="LimitedConcurrencyTaskScheduler"/> that is configured to allow a maximum of <see cref="Environment.ProcessorCount"/> <see cref="Task"/>s to run concurrently.
    /// </summary>
    public static new LimitedConcurrencyTaskScheduler Default { get; } = new LimitedConcurrencyTaskScheduler(Environment.ProcessorCount);
    /// <summary>
    /// Returns an instance of <see cref="TaskFactory"/> that is configured to use <see cref="Default"/>.
    /// </summary>
    public static TaskFactory TaskFactory { get; } = new TaskFactory(Default);

    /// <summary>
    /// Initializes a new <see cref="LimitedConcurrencyTaskScheduler"/> with the specified maximum degree of parallelism.
    /// </summary>
    /// <param name="maxDegreeOfParallelism">The maximum number of <see cref="Task"/>s that are allowed to run concurrently.</param>
    public LimitedConcurrencyTaskScheduler(int maxDegreeOfParallelism)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDegreeOfParallelism, 1);

        _maxDegreeOfParallelism = maxDegreeOfParallelism;
    }
    /// <inheritdoc/>
    protected override void QueueTask(Task task)
    {
        _tasks.Add(task);
        if (Interlocked.Increment(ref _delegatesQueuedOrRunning) <= _maxDegreeOfParallelism)
        {
            NotifyThreadPoolOfPendingWork();
        }
    }
    /// <inheritdoc/>
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        if (taskWasPreviouslyQueued && !TryDequeue(task))
        {
            return false;
        }

        return TryExecuteTask(task);
    }
    private void NotifyThreadPoolOfPendingWork()
    {
        ThreadPool.UnsafeQueueUserWorkItem(_ =>
        {
            Interlocked.Increment(ref _delegatesQueuedOrRunning);
            try
            {
                while (_tasks.TryTake(out var task, Timeout.Infinite))
                {
                    TryExecuteTask(task);
                }
            }
            finally
            {
                Interlocked.Decrement(ref _delegatesQueuedOrRunning);
            }
        }, null);
    }
    /// <inheritdoc/>
    protected override bool TryDequeue(Task task) => _tasks.TryTake(out _);
    /// <inheritdoc/>
    protected override IEnumerable<Task> GetScheduledTasks() => _tasks.ToArray();
    /// <inheritdoc/>
    public override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;
}
