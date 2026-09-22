using System.Windows.Threading;

using LaquaiLib.Interfaces;

namespace LaquaiLib.Threading;

/// <summary>
/// Implements an awaitable object that causes <see langword="await"/> continuations to be posted to the specified <paramref name="dispatcher"/> using the specified <paramref name="dispatcherPriority"/>.
/// </summary>
public readonly struct DispatcherAwaitable(Dispatcher dispatcher, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal) : IAwaitable<DispatcherAwaitable, DispatcherAwaiter>
{
    private readonly Dispatcher _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

    /// <summary>
    /// Gets a <see cref="DispatcherAwaiter"/> instance that resumes continuations on the specified <see cref="Dispatcher"/> with the specified <see cref="DispatcherPriority"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly DispatcherAwaiter GetAwaiter() => new DispatcherAwaiter(_dispatcher, dispatcherPriority);
}
/// <summary>
/// Implements the awaiter side for <see cref="DispatcherAwaitable"/>.
/// </summary>
public readonly struct DispatcherAwaiter(Dispatcher dispatcher, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal) : IAwaiter<DispatcherAwaiter>
{
    private readonly Dispatcher _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

    /// <summary>
    /// Gets whether the thread <see langword="await"/>ing this instance is the same as the thread associated with the specified <see cref="Dispatcher"/>.
    /// </summary>
    public readonly bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _dispatcher?.CheckAccess() ?? true;
    }
    /// <inheritdoc cref="UnsafeOnCompleted"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);
    /// <summary>
    /// Schedules the specified <paramref name="continuation"/> to be executed on the thread associated with the specified <see cref="Dispatcher"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly void UnsafeOnCompleted(Action continuation) => _dispatcher.InvokeAsync(continuation, dispatcherPriority);
    /// <summary>
    /// Ends the await on this awaiter.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public void GetResult() { }
}