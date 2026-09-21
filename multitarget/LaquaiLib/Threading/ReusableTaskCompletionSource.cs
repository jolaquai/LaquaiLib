using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Sources;

namespace LaquaiLib.Threading;

/// <summary>
/// Implements a <see cref="TaskCompletionSource"/> that can be reused, remaining effectively zero-alloc beyond the wrapper.
/// When using exclusively the <see cref="ValueTask"/> property, this is even slightly faster than <see cref="TaskCompletionSource"/>.
/// Using the <see cref="Task"/> property makes this slower than <see cref="TaskCompletionSource"/>, but still zero-alloc.
/// </summary>
public sealed class ReusableTaskCompletionSource : ReusableTaskCompletionSourceBase<Nothing>
{
    /// <summary>
    /// Sets the currently pending <see cref="ValueTask"/> or <see cref="Task"/> as completed successfully.
    /// </summary>
    public void SetResult() => SetResultCore(default);

    /// <inheritdoc cref="ReusableTaskCompletionSourceBase{T}.ValueTaskCore" />
    public ValueTask ValueTask => ValueTaskCore;
    /// <inheritdoc cref="ReusableTaskCompletionSourceBase{T}.TaskCore" />
    public Task Task => TaskCore;
}
/// <summary>
/// Implements a <see cref="TaskCompletionSource{TResult}"/> that can be reused.
/// When using exclusively the <see cref="ValueTask"/> property, this is even slightly faster than <see cref="TaskCompletionSource{TResult}"/>.
/// Using the <see cref="Task"/> property makes this slower but still less allocation-heavy than <see cref="TaskCompletionSource{TResult}"/>.
/// </summary>
public sealed class ReusableTaskCompletionSource<T> : ReusableTaskCompletionSourceBase<T>
{
    /// <summary>
    /// Sets the result of the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> to <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The result to set.</param>
    public void SetResult(T result) => SetResultCore(result);

    /// <inheritdoc cref="ReusableTaskCompletionSourceBase{T}.ValueTaskOfTCore" />
    public ValueTask<T> ValueTask => ValueTaskOfTCore;
    /// <inheritdoc cref="ReusableTaskCompletionSourceBase{T}.TaskOfTCore" />
    public Task<T> Task => TaskOfTCore;
}

/// <summary>
/// An empty <see langword="struct"/> definition.
/// </summary>
public readonly struct Nothing;
/// <summary>
/// Implements base functionality for <see cref="ReusableTaskCompletionSource"/> and <see cref="ReusableTaskCompletionSource{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the result. Ignored for <see cref="ReusableTaskCompletionSource"/>.</typeparam>
public closed class ReusableTaskCompletionSourceBase<T>
{
    private sealed class Source : IValueTaskSource, IValueTaskSource<T>
    {
        public ManualResetValueTaskSourceCore<T> Core = new() { RunContinuationsAsynchronously = true };

        public ValueTaskSourceStatus GetStatus(short token) => Core.GetStatus(token);
        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags) => Core.OnCompleted(continuation, state, token, flags);

        void IValueTaskSource.GetResult(short token) => Core.GetResult(token);
        T IValueTaskSource<T>.GetResult(short token) => Core.GetResult(token);
    }
    private readonly Source _source = new();

    private short Token
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _source.Core.Version;
    }

    /// <summary>
    /// When overridden in a derived class, sets the result of the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> to <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The result to set. Ignored for <see cref="ReusableTaskCompletionSource"/>.</param>
    protected void SetResultCore(T result)
    {
        ThrowIfTokenHasResult(Token);
        _source.Core.SetResult(result);
    }
    /// <summary>
    /// Gets a <see cref="ValueTask"/> that represents the currently pending operation.
    /// </summary>
    protected ValueTask ValueTaskCore
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_source, Token);
    }
    /// <summary>
    /// Gets a <see cref="ValueTask{TResult}"/> that represents the currently pending operation.
    /// </summary>
    protected ValueTask<T> ValueTaskOfTCore
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(_source, Token);
    }
    /// <summary>
    /// Gets a <see cref="Task"/> that represents the currently pending operation.
    /// </summary>
    protected Task TaskCore
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => TaskOfTCore;
    }

    private int _taskToken = -1;
    /// <summary>
    /// Gets a <see cref="Task{TResult}"/> that represents the currently pending operation.
    /// </summary>
    protected Task<T> TaskOfTCore
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var token = Token;
            var task = field;
            if (_taskToken == token)
                return task;

            var vt = new ValueTask<T>(_source, token);
            if (vt.IsCompletedSuccessfully)
            {
                var result = vt.Result;
                if (typeof(T).IsValueType && task is { IsCompletedSuccessfully: true } && EqualityComparer<T>.Default.Equals(task.Result, result))
                {
                    _taskToken = token;
                    return task;
                }
                task = Task.FromResult(result);
            }
            else
                task = vt.AsTask();

            _taskToken = token;
            return field = task;
        }
    }

    /// <summary>
    /// Sets the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> as faulted with the specified <paramref name="exception"/>.
    /// </summary>
    /// <param name="exception">The exception to set.</param>
    public void SetException(Exception exception)
    {
        ThrowIfTokenHasResult(Token);
        _source.Core.SetException(exception);
    }
    /// <summary>
    /// Sets the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> as canceled with the specified <paramref name="cancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to set.</param>
    public void SetCanceled(CancellationToken cancellationToken = default) => SetException(new OperationCanceledException(cancellationToken));
    /// <summary>
    /// Resets the <see cref="ReusableTaskCompletionSource{T}"/> to its initial state, allowing it to be reused for another operation.
    /// </summary>
    /// <remarks>
    /// Calls to this method are only allowed when the current <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> has been completed.
    /// </remarks>
    public void Reset()
    {
        ThrowIfTokenHasNoResult(Token);
        _source.Core.Reset();
        _taskToken = -1;
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the specified <paramref name="token"/> has already been completed.
    /// </summary>
    /// <param name="token">The token to check.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfTokenHasResult(short token)
    {
        if (_source.GetStatus(token) != ValueTaskSourceStatus.Pending)
            ThrowCannotChangeSetResult(token);
    }
    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the specified <paramref name="token"/> has not yet been completed.
    /// </summary>
    /// <param name="token">The token to check.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ThrowIfTokenHasNoResult(short token)
    {
        if (_source.GetStatus(token) == ValueTaskSourceStatus.Pending)
            ThrowTokenUnknown(token);
    }
    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] private static void ThrowCannotChangeSetResult(short token) => throw new InvalidOperationException($"Cannot change the already-set result for token {token}. Call {nameof(ReusableTaskCompletionSource)}.{nameof(Reset)}.");
    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] private static void ThrowTokenUnknown(short token) => throw new InvalidOperationException($"The result for token {token} is unset.");
}