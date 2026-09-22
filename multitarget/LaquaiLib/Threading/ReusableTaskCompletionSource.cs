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
    /// <summary>
    /// Attempts to set the currently pending <see cref="ValueTask"/> or <see cref="Task"/> as completed successfully.
    /// </summary>
    /// <returns><see langword="true"/> if the completion state was successfully set; otherwise, <see langword="false"/>.</returns>
    public bool TrySetResult() => TrySetResultCore(default);
    /// <summary>
    /// Sets the currently pending <see cref="ValueTask"/> or <see cref="Task"/> to the same completion state as the specified <paramref name="task"/>.
    /// <paramref name="task"/> must be completed, otherwise an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <param name="task">The <see cref="System.Threading.Tasks.Task"/> to set the result from.</param>
    public void SetFromTask(Task task)
    {
        if (!TrySetFromTask(task))
            ThrowCannotChangeSetResult(Token);
    }
    /// <summary>
    /// Attempts to set the currently pending <see cref="ValueTask"/> or <see cref="Task"/> to the same completion state as the specified <paramref name="task"/>.
    /// <paramref name="task"/> must be completed, otherwise an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <param name="task">The <see cref="System.Threading.Tasks.Task"/> to set the result from.</param>
    /// <returns><see langword="true"/> if the completion state was successfully set; otherwise, <see langword="false"/>.</returns>
    public bool TrySetFromTask(Task task)
    {
        if (!task.IsCompleted)
            ThrowCannotCopyResultStateFromUncompletedTask();

        if (task.IsCanceled)
            return TrySetException(new TaskCanceledException(task));
        if (task.IsFaulted)
            return TrySetException(Unwrap(task.Exception));
        return TrySetResult();
    }

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
    /// <summary>
    /// Attempts to set the result of the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> to <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The result to set.</param>
    /// <returns><see langword="true"/> if the completion state was successfully set; otherwise, <see langword="false"/>.</returns>
    public bool TrySetResult(T result) => TrySetResultCore(result);
    /// <summary>
    /// Sets the currently pending <see cref="ValueTask"/> or <see cref="Task"/> to the same completion state as the specified <paramref name="task"/>.
    /// <paramref name="task"/> must be completed, otherwise an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <param name="task">The <see cref="Task{TResult}"/> to set the result from.</param>
    public void SetFromTask(Task<T> task)
    {
        if (!TrySetFromTask(task))
            ThrowCannotChangeSetResult(Token);
    }
    /// <summary>
    /// Attempts to set the currently pending <see cref="ValueTask"/> or <see cref="Task"/> to the same completion state as the specified <paramref name="task"/>.
    /// <paramref name="task"/> must be completed, otherwise an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <param name="task">The <see cref="Task{TResult}"/> to set the result from.</param>
    /// <returns><see langword="true"/> if the completion state was successfully set; otherwise, <see langword="false"/>.</returns>
    public bool TrySetFromTask(Task<T> task)
    {
        if (!task.IsCompleted)
            ThrowCannotCopyResultStateFromUncompletedTask();

        if (task.IsCanceled)
            return TrySetException(new TaskCanceledException(task));
        if (task.IsFaulted)
            return TrySetException(Unwrap(task.Exception));
        return TrySetResult(task.Result);
    }

    /// <inheritdoc cref="ReusableTaskCompletionSourceBase{T}.ValueTaskOfTCore" />
    public ValueTask<T> ValueTask => ValueTaskOfTCore;
    /// <inheritdoc cref="ReusableTaskCompletionSourceBase{T}.TaskOfTCore" />
    public Task<T> Task => TaskOfTCore;
}

/// <summary>
/// An empty <see langword="struct"/> definition.
/// </summary>
public readonly struct Nothing : IEquatable<Nothing>, IEquatable<Nothing?>
{
    /// <summary>
    /// Determines whether this <see cref="Nothing"/> is equal to another <see cref="Nothing"/>.
    /// Always returns <see langword="true"/>.
    /// </summary>
    public bool Equals(Nothing other) => true;
    /// <summary>
    /// Determines whether this <see cref="Nothing"/> is equal to a nullable <see cref="Nothing"/>.
    /// Returns <see langword="true"/> if the nullable <see cref="Nothing"/> has a value.
    /// </summary>
    public bool Equals(Nothing? other) => other.HasValue;
    /// <summary>
    /// Determines whether this <see cref="Nothing"/> is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with this <see cref="Nothing"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Nothing"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object obj) => obj is Nothing nothing && Equals(nothing);
    /// <summary>
    /// Gets a hash code for this <see cref="Nothing"/>. Always returns 0.
    /// </summary>
    /// <returns>0.</returns>
    public override int GetHashCode() => 0;
    /// <summary>
    /// Determines whether two <see cref="Nothing"/> instances are equal. See <see cref="Equals(Nothing)"/>.
    /// </summary>
    public static bool operator ==(Nothing left, Nothing right) => left.Equals(right);
    /// <summary>
    /// Determines whether two <see cref="Nothing"/> instances are not equal. See <see cref="Equals(Nothing)"/>.
    /// </summary>
    public static bool operator !=(Nothing left, Nothing right) => !(left == right);
}
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
    private int _claimed;

    /// <summary>
    /// Gets the token of the currently pending operation.
    /// </summary>
    protected internal short Token
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _source.Core.Version;
    }

    /// <summary>
    /// Sets the result of the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> to <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The result to set. Ignored for <see cref="ReusableTaskCompletionSource"/>.</param>
    protected void SetResultCore(T result)
    {
        if (!TrySetResultCore(result))
            ThrowCannotChangeSetResult(Token);
    }
    /// <summary>
    /// Attempts to set the result of the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> to <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The result to set. Ignored for <see cref="ReusableTaskCompletionSource"/>.</param>
    /// <returns><see langword="true"/> if the completion state was successfully set; otherwise, <see langword="false"/>.</returns>
    protected bool TrySetResultCore(T result)
    {
        if (Interlocked.CompareExchange(ref _claimed, 1, 0) != 0)
            return false;

        _source.Core.SetResult(result);
        return true;
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

    private const int NoTaskToken = int.MinValue;
    private int _taskToken = NoTaskToken;
    private Task<T> _task;
    /// <summary>
    /// Gets a <see cref="Task{TResult}"/> that represents the currently pending operation.
    /// </summary>
    protected Task<T> TaskOfTCore
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var token = Token;
            return Volatile.Read(ref _taskToken) == token ? _task : CreateTask(token);
        }
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private Task<T> CreateTask(short token)
    {
        var vt = new ValueTask<T>(_source, token);
        Task<T> task;
        if (vt.IsCompletedSuccessfully)
        {
            var result = vt.Result;
            task = _task is { IsCompletedSuccessfully: true } previous && BitwiseEquals(previous.Result, result) ? previous : Task.FromResult(result);
        }
        else
            task = vt.AsTask();

        _task = task;
        Volatile.Write(ref _taskToken, token);
        return task;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool BitwiseEquals(T left, T right) => !RuntimeHelpers.IsReferenceOrContainsReferences<T>()
        && MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref left), Unsafe.SizeOf<T>()).SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref right), Unsafe.SizeOf<T>()));

    /// <summary>
    /// Gets whether the currently pending operation has been completed.
    /// </summary>
    public bool IsCompleted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => HasResult();
    }
    /// <summary>
    /// Gets whether the currently pending operation has been completed successfully.
    /// </summary>
    public bool IsCompletedSuccessfully
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _source.GetStatus(Token) == ValueTaskSourceStatus.Succeeded;
    }
    /// <summary>
    /// Gets whether the currently pending operation has been completed with an exception.
    /// </summary>
    public bool IsFaulted
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _source.GetStatus(Token) == ValueTaskSourceStatus.Faulted;
    }
    /// <summary>
    /// Gets whether the currently pending operation has been completed with a cancellation.
    /// </summary>
    public bool IsCanceled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _source.GetStatus(Token) == ValueTaskSourceStatus.Canceled;
    }

    /// <summary>
    /// Sets the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> as faulted with the specified <paramref name="exception"/>.
    /// </summary>
    /// <param name="exception">The exception to set.</param>
    public void SetException(Exception exception)
    {
        if (!TrySetException(exception))
            ThrowCannotChangeSetResult(Token);
    }
    /// <summary>
    /// Attempts to set the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> as faulted with the specified <paramref name="exception"/>.
    /// </summary>
    /// <param name="exception">The exception to set.</param>
    /// <returns><see langword="true"/> if the completion state was successfully set; otherwise, <see langword="false"/>.</returns>
    public bool TrySetException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (Interlocked.CompareExchange(ref _claimed, 1, 0) != 0)
            return false;

        _source.Core.SetException(exception);
        return true;
    }
    /// <summary>
    /// Sets the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> as canceled with the specified <paramref name="cancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to set.</param>
    public void SetCanceled(CancellationToken cancellationToken = default) => SetException(new OperationCanceledException(cancellationToken));
    /// <summary>
    /// Attempts to set the currently pending <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> as canceled with the specified <paramref name="cancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to set.</param>
    /// <returns><see langword="true"/> if the completion state was successfully set; otherwise, <see langword="false"/>.</returns>
    public bool TrySetCanceled(CancellationToken cancellationToken = default) => TrySetException(new OperationCanceledException(cancellationToken));
    /// <summary>
    /// Resets the <see cref="ReusableTaskCompletionSource{T}"/> to its initial state, allowing it to be reused for another operation.
    /// </summary>
    /// <remarks>
    /// Calls to this method are only allowed when the current <see cref="ValueTask{TResult}"/> or <see cref="Task{TResult}"/> has been completed.
    /// </remarks>
    public void Reset()
    {
        if (!TryReset())
            ThrowTokenUnknown(Token);
    }
    /// <summary>
    /// Attempts to reset the <see cref="ReusableTaskCompletionSource{T}"/> to its initial state, allowing it to be reused for another operation.
    /// </summary>
    /// <returns><see langword="true"/> if state was reset successfully; otherwise, <see langword="false"/>.</returns>
    public bool TryReset()
    {
        if (!HasResult())
            return false;
        // A still-queued AsTask callback reads the core with this token; resetting first makes it throw on the thread pool.
        if (Volatile.Read(ref _taskToken) == Token && _task is { IsCompleted: false } task)
        {
            var spinner = new SpinWait();
            while (!task.IsCompleted)
                spinner.SpinOnce();
        }
        _source.Core.Reset();
        _taskToken = NoTaskToken;
        Volatile.Write(ref _claimed, 0);
        return true;
    }

    /// <summary>
    /// Gets whether the specified <paramref name="token"/> is the current token for the underlying <see cref="ManualResetValueTaskSourceCore{TResult}"/>.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><see langword="true"/> if the specified <paramref name="token"/> is the current token; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] protected internal bool IsCurrent(short token) => token == Token;
    /// <summary>
    /// Gets whether the result for the specified <paramref name="token"/> has been set.
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><see langword="true"/> if the result has been set; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] protected internal bool HasResult(short? token = null) => _source.GetStatus(token ?? Token) != ValueTaskSourceStatus.Pending;
    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the specified <paramref name="token"/> has already been completed.
    /// </summary>
    /// <param name="token">The token to check.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal void ThrowIfTokenHasResult(short token)
    {
        if (HasResult(token))
            ThrowCannotChangeSetResult(token);
    }
    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the specified <paramref name="token"/> has not yet been completed.
    /// </summary>
    /// <param name="token">The token to check.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected internal void ThrowIfTokenHasNoResult(short token)
    {
        if (!HasResult(token))
            ThrowTokenUnknown(token);
    }
    private protected static Exception Unwrap(AggregateException exception) => exception.InnerExceptions.Count == 1 ? exception.InnerExceptions[0] : exception;
    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] private protected static void ThrowCannotChangeSetResult(short token) => throw new InvalidOperationException($"Cannot change the already-set result for token {token}. Call {nameof(ReusableTaskCompletionSource)}.{nameof(Reset)}.");
    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] private static void ThrowTokenUnknown(short token) => throw new InvalidOperationException($"The result for token {token} is unset.");
    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> indicating that the result state cannot be copied from an uncompleted task.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] protected internal static void ThrowCannotCopyResultStateFromUncompletedTask() => throw new InvalidOperationException($"Cannot copy the result state from an uncompleted task.");
}