using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;

namespace LaquaiLib.Util.Threading;

/// <summary>
/// Implements a custom equivalent to <see cref="TaskCompletionSource"/> that can be reused without further allocation.
/// </summary>
public sealed class ReusableTaskCompletionSource() : IValueTaskSource
{
    private struct VoidResult;
    private ManualResetValueTaskSourceCore<VoidResult> _core = new ManualResetValueTaskSourceCore<VoidResult>()
    {
        RunContinuationsAsynchronously = true
    };

    /// <summary>
    /// Gets a <see cref="System.Threading.Tasks.ValueTask"/> that represents the current state of the <see cref="ReusableTaskCompletionSource"/>.
    /// </summary>
    public ValueTask ValueTask => new ValueTask(this, _core.Version);

    private bool completed;

    /// <summary>
    /// Successfully completes the underlying <see cref="ValueTask"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the underlying <see cref="ValueTask"/> has already been completed. The instance must be reset before doing so again.</exception>
    public void SetResult()
    {
        if (completed)
        {
            throw new InvalidOperationException("The underlying ValueTask has already been completed. The instance must be reset before doing so again.");
        }

        _core.SetResult(default);
        completed = true;
    }
    /// <summary>
    /// Faults the underlying <see cref="ValueTask"/> with the specified <paramref name="exception"/>.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> to fault the <see cref="ValueTask"/> with.</param>
    /// <exception cref="InvalidOperationException">Thrown when the underlying <see cref="ValueTask"/> has already been completed. The instance must be reset before doing so again.</exception>
    public void SetException(Exception exception)
    {
        if (completed)
        {
            throw new InvalidOperationException("The underlying ValueTask has already been completed. The instance must be reset before doing so again.");
        }

        _core.SetException(exception);
        completed = true;
    }

    /// <summary>
    /// Attempts to successfully complete the underlying <see cref="ValueTask"/>. Becomes a no-op if the underlying <see cref="ValueTask"/> is already completed.
    /// </summary>
    public bool TrySetResult()
    {
        if (completed)
        {
            return false;
        }
        SetResult();
        return true;
    }
    /// <summary>
    /// Attempts to fault the underlying <see cref="ValueTask"/> with the specified <paramref name="exception"/>. Becomes a no-op if the underlying <see cref="ValueTask"/> is already completed.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> to fault the <see cref="ValueTask"/> with.</param
    public bool TrySetException(Exception exception)
    {
        if (completed)
        {
            return false;
        }
        SetException(exception);
        return true;
    }

    /// <summary>
    /// Resets the underlying state so that the next <see cref="ValueTask"/> retrieved represents an uncompleted task.
    /// </summary>
    public void Reset()
    {
        if (completed)
        {
            _core.Reset();
            completed = false;
        }
    }

    public void GetResult(short token) => _core.GetResult(token);
    public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);
    public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags) => _core.OnCompleted(continuation, state, token, flags);
}

/// <summary>
/// Implements a custom equivalent to <see cref="TaskCompletionSource{TResult}"/> that can be reused without further allocation.
/// </summary>
/// <typeparam name="TResult">The type of the result of the underlying <see cref="ValueTask{TResult}"/>.</typeparam>
public class ReusableTaskCompletionSource<TResult>() : IValueTaskSource<TResult>
{
    private ManualResetValueTaskSourceCore<TResult> _core = new ManualResetValueTaskSourceCore<TResult>()
    {
        RunContinuationsAsynchronously = true
    };

    /// <summary>
    /// Gets a <see cref="ValueTask{TResult}"/> that represents the current state of the <see cref="ReusableTaskCompletionSource"/>.
    /// </summary>
    public ValueTask<TResult> ValueTask => new ValueTask<TResult>(this, _core.Version);
    private bool completed;

    /// <summary>
    /// Successfully completes the underlying <see cref="ValueTask"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the underlying <see cref="ValueTask"/> has already been completed. The instance must be reset before doing so again.</exception>
    public void SetResult(TResult result)
    {
        if (completed)
        {
            throw new InvalidOperationException("The underlying ValueTask has already been completed. The instance must be reset before doing so again.");
        }

        _core.SetResult(result);
        completed = true;
    }
    /// <summary>
    /// Faults the underlying <see cref="ValueTask"/> with the specified <paramref name="exception"/>.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> to fault the <see cref="ValueTask"/> with.</param>
    /// <exception cref="InvalidOperationException">Thrown when the underlying <see cref="ValueTask"/> has already been completed. The instance must be reset before doing so again.</exception>
    public void SetException(Exception exception)
    {
        if (completed)
        {
            throw new InvalidOperationException("The underlying ValueTask has already been completed. The instance must be reset before doing so again.");
        }

        _core.SetException(exception);
        completed = true;
    }
    /// <summary>
    /// Faults the underlying <see cref="ValueTask"/> with an <see cref="OperationCanceledException"/> using the specified <paramref name="cancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to fault the <see cref="ValueTask"/> with.</param>
    /// <exception cref="InvalidOperationException">Thrown when the underlying <see cref="ValueTask"/> has already been completed. The instance must be reset before doing so again.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="cancellationToken"/> is not canceled.</exception>
    public void SetCanceled(CancellationToken cancellationToken = default)
    {
        if (completed)
        {
            throw new InvalidOperationException("The underlying ValueTask has already been completed. The instance must be reset before doing so again.");
        }
        if (!cancellationToken.IsCancellationRequested)
        {
            throw new ArgumentException("The CancellationToken must be canceled.", nameof(cancellationToken));
        }
        _core.SetException(new OperationCanceledException(cancellationToken));
        completed = true;
    }
    /// <summary>
    /// Sets the state of the underlying <see cref="ValueTask"/> to that of the specified <paramref name="task"/>.
    /// If it is not completed, its result is awaited synchronously.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> to set the state of the underlying <see cref="ValueTask"/> to.</param>
    /// <exception cref="InvalidOperationException">Thrown when the underlying <see cref="ValueTask"/> has already been completed. The instance must be reset before doing so again.</exception>
    /// <exception cref="ArgumentException">T
    public void SetFromResult(Task<TResult> task)
    {
        if (completed)
        {
            throw new InvalidOperationException("The underlying ValueTask has already been completed. The instance must be reset before doing so again.");
        }

        if (!task.IsCompleted)
        {
            task.Wait();
        }
        if (task.IsCompletedSuccessfully)
        {
            _core.SetResult(task.Result);
            completed = true;
        }
        else if (task.IsFaulted)
        {
            _core.SetException(task.Exception);
            completed = true;
        }
        else if (task.IsCanceled)
        {
            _core.SetException(task.Exception.InnerException);
            completed = true;
        }
        else
        {
            throw new ArgumentException("The task is not completed, faulted or canceled.", nameof(task));
        }
    }
    /// <summary>
    /// Sets the result of the underlying <see cref="ValueTask"/> to that of the specified <paramref name="task"/>.
    /// If it is not completed, its result is awaited asynchronously.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> to set the state of the underlying <see cref="ValueTask"/> to.</param>
    /// <exception cref="InvalidOperationException">Thrown when the underlying <see cref="ValueTask"/> has already been completed. The instance must be reset before doing so again.</exception>
    public async ValueTask SetFromResultAsync(Task<TResult> task)
    {
        if (completed)
        {
            throw new InvalidOperationException("The underlying ValueTask has already been completed. The instance must be reset before doing so again.");
        }

        await task.ConfigureAwait(false);
        SetFromResult(task);
    }

    /// <summary>
    /// Attempts to successfully complete the underlying <see cref="ValueTask"/>. Becomes a no-op if the underlying <see cref="ValueTask"/> is already completed.
    /// </summary>
    public bool TrySetResult(TResult result)
    {
        if (completed)
        {
            return false;
        }
        SetResult(result);
        return true;
    }
    /// <summary>
    /// Attempts to fault the underlying <see cref="ValueTask"/> with the specified <paramref name="exception"/>. Becomes a no-op if the underlying <see cref="ValueTask"/> is already completed.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> to fault the <see cref="ValueTask"/> with.</param
    public bool TrySetException(Exception exception)
    {
        if (completed)
        {
            return false;
        }
        SetException(exception);
        return true;
    }

    /// <summary>
    /// Resets the underlying state so that the next <see cref="ValueTask"/> retrieved represents an uncompleted task.
    /// Becomes a no-op if the underlying <see cref="ValueTask"/> is not completed.
    /// </summary>
    public void Reset()
    {
        if (completed)
        {
            _core.Reset();
            completed = false;
        }
    }

    public TResult GetResult(short token) => _core.GetResult(token);
    public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);
    public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags) => _core.OnCompleted(continuation, state, token, flags);
}
