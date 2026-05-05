using LaquaiLib.Interfaces;

namespace LaquaiLib.Threading;

/// <summary>
/// Implements an awaitable object that causes <see langword="await"/> continuations to be posted to the specified <paramref name="synchronizationContext"/>.
/// </summary>
public readonly struct SynchronizationContextAwaitable(SynchronizationContext synchronizationContext) : IAwaitable<SynchronizationContextAwaitable, SynchronizationContextAwaiter>
{
    private readonly SynchronizationContext _synchronizationContext = synchronizationContext ?? throw new ArgumentNullException(nameof(synchronizationContext));

    /// <summary>
    /// Gets a <see cref="SynchronizationContextAwaiter"/> instance that resumes the <see langword="await"/>'s continuation on the specified <paramref name="synchronizationContext"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly SynchronizationContextAwaiter GetAwaiter() => new SynchronizationContextAwaiter(_synchronizationContext);
}
/// <summary>
/// Implements the awaiter side for <see cref="SynchronizationContextAwaitable"/>.
/// </summary>
public readonly struct SynchronizationContextAwaiter(SynchronizationContext synchronizationContext) : IAwaiter<SynchronizationContextAwaiter>
{
    private readonly SynchronizationContext _synchronizationContext = synchronizationContext ?? throw new ArgumentNullException(nameof(synchronizationContext));

    /// <summary>
    /// Gets whether the thread <see langword="await"/>ing this instance is the same as the thread associated with the specified <paramref name="synchronizationContext"/>.
    /// </summary>
    public readonly bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => SynchronizationContext.Current == _synchronizationContext;
    }
    /// <inheritdoc cref="UnsafeOnCompleted"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);
    /// <summary>
    /// Posts the specified <paramref name="continuation"/> to the specified <paramref name="synchronizationContext"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly void UnsafeOnCompleted(Action continuation) => _synchronizationContext.Post(static x => Unsafe.As<Action>(x)(), continuation);
    /// <summary>
    /// Ends the await on this awaiter.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public void GetResult() { }
}

/// <summary>
/// Implements an awaitable object that causes <see langword="await"/> continuations to be started as <see cref="Task"/>s scheduled using the specified <paramref name="taskScheduler"/>.
/// </summary>
public readonly struct TaskSchedulerAwaitable(TaskScheduler taskScheduler) : IAwaitable<TaskSchedulerAwaitable, TaskSchedulerAwaiter>
{
    private readonly TaskScheduler _taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));

    /// <summary>
    /// Gets a <see cref="TaskSchedulerAwaiter"/> instance that schedule's the <see langword="await"/>'s continuation using the specified <paramref name="taskScheduler"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly TaskSchedulerAwaiter GetAwaiter() => new TaskSchedulerAwaiter(_taskScheduler);
}
/// <summary>
/// Implements the awaiter side for <see cref="TaskSchedulerAwaitable"/>.
/// </summary>
public readonly struct TaskSchedulerAwaiter(TaskScheduler taskScheduler) : IAwaiter<TaskSchedulerAwaiter>
{
    private readonly TaskScheduler _taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));

    /// <summary>
    /// Gets whether the <see cref="TaskScheduler"/> that scheduled the <see cref="Task"/> <see langword="await"/>ing this instance is the same as the specified <paramref name="taskScheduler"/>.
    /// </summary>
    public readonly bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TaskScheduler.Current == _taskScheduler;
    }
    /// <inheritdoc cref="UnsafeOnCompleted"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);
    /// <summary>
    /// Schedules the specified <paramref name="continuation"/> to be executed on the thread associated with the specified <paramref name="synchronizationContext"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly void UnsafeOnCompleted(Action continuation) => Task.Factory.StartNew(continuation, default, TaskCreationOptions.None, _taskScheduler);
    /// <summary>
    /// Ends the await on this awaiter.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public void GetResult() { }
}