using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace LaquaiLib.Util.Threading;

// There's StackTraceHiddenAttribute and DebuggerStepThroughAttribute on all these to prevent the debugger from highlighting the throw statments in the GetResult methods if an exception is thrown

/// <summary>
/// Provides factory methods for creating <see cref="ExtendedDebugTask"/> instances.
/// </summary>
[StackTraceHidden]
[DebuggerStepThrough]
public static class ExtendedDebugTaskExtensions
{
    /// <summary>
    /// Creates a new <see cref="ExtendedDebugTask"/> that wraps the specified <see cref="System.Threading.Tasks.Task"/>.
    /// </summary>
    /// <param name="task">The task to wrap.</param>
    /// <returns>The wrapped task.</returns>
    public static ExtendedDebugTask AsDebuggable(this Task task) => new(task);
    /// <summary>
    /// Creates a new <see cref="ExtendedDebugTask{TResult}"/> that wraps the specified <see cref="System.Threading.Tasks.Task{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
    /// <param name="task">The task to wrap.</param>
    /// <returns>The wrapped task.</returns>
    public static ExtendedDebugTask<TResult> AsDebuggable<TResult>(this Task<TResult> task) => new(task);
}

/// <summary>
/// Wraps a <see cref="System.Threading.Tasks.Task"/> to provide additional debugging information.
/// </summary>
[StackTraceHidden]
[DebuggerStepThrough]
public class ExtendedDebugTask(Task task)
{
    internal const string awaitStackTemplate = $$"""
        {0}
        --- [{{nameof(ExtendedDebugTask)}}] await stack ---
        {1}
        """;
    internal const string creationStackTemplate = $$"""

        --- [{{nameof(ExtendedDebugTask)}}] Creation stack ---
        {0}
        """;

    private readonly StackTrace _creationStack = new StackTrace(true);

    #region .ctors
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="action"/> when started.
    /// </summary>
    /// <param name="action">The action to associate with the task.</param>
    public ExtendedDebugTask(Action action) : this(new Task(action)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="action"/> when started.
    /// </summary>
    /// <param name="action">The action to associate with the task.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
    public ExtendedDebugTask(Action action, CancellationToken cancellationToken) : this(new Task(action, cancellationToken)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="action"/> when started.
    /// </summary>
    /// <param name="action">The action to associate with the task.</param>
    /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
    public ExtendedDebugTask(Action action, TaskCreationOptions creationOptions) : this(new Task(action, creationOptions)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="action"/> when started.
    /// </summary>
    /// <param name="action">The action to associate with the task.</param>
    /// <param name="state">An object representing data to be used by the action.</param>
    public ExtendedDebugTask(Action<object> action, object state) : this(new Task(action, state)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="action"/> when started.
    /// </summary>
    /// <param name="action">The action to associate with the task.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
    /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
    public ExtendedDebugTask(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : this(new Task(action, cancellationToken, creationOptions)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="action"/> when started.
    /// </summary>
    /// <param name="action">The action to associate with the task.</param>
    /// <param name="state">An object representing data to be used by the action.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
    public ExtendedDebugTask(Action<object> action, object state, CancellationToken cancellationToken) : this(new Task(action, state, cancellationToken)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="action"/> when started.
    /// </summary>
    /// <param name="action">The action to associate with the task.</param>
    /// <param name="state">An object representing data to be used by the action.</param>
    /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
    public ExtendedDebugTask(Action<object> action, object state, TaskCreationOptions creationOptions) : this(new Task(action, state, creationOptions)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="action"/> when started.
    /// </summary>
    /// <param name="action">The action to associate with the task.</param>
    /// <param name="state">An object representing data to be used by the action.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
    /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
    public ExtendedDebugTask(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : this(new Task(action, state, cancellationToken, creationOptions)) { }
    #endregion

    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask Run(Action action) => new(System.Threading.Tasks.Task.Run(action));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels the task if it has not yet started.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask Run(Action action, CancellationToken cancellationToken) => new(System.Threading.Tasks.Task.Run(action, cancellationToken));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified action.
    /// </summary>
    /// <param name="function">The action to execute.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask Run(Func<Task> function) => new(System.Threading.Tasks.Task.Run(function));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified action.
    /// </summary>
    /// <param name="function">The action to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels the task if it has not yet started.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask Run(Func<Task> function, CancellationToken cancellationToken) => new(System.Threading.Tasks.Task.Run(function, cancellationToken));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified function.
    /// </summary>
    /// <typeparam name="TResult">The return type of the task.</typeparam>
    /// <param name="function">The function to execute.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask<TResult> Run<TResult>(Func<TResult> function) => new(System.Threading.Tasks.Task.Run(function));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified function.
    /// </summary>
    /// <typeparam name="TResult">The return type of the task.</typeparam>
    /// <param name="function">The function to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels the task if it has not yet started.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken) => new(System.Threading.Tasks.Task.Run(function, cancellationToken));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified function.
    /// </summary>
    /// <typeparam name="TResult">The return type of the task.</typeparam>
    /// <param name="function">The function to execute.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask<TResult> Run<TResult>(Func<Task<TResult>> function) => new(System.Threading.Tasks.Task.Run(function));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified function.
    /// </summary>
    /// <typeparam name="TResult">The return type of the task.</typeparam>
    /// <param name="function">The function to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels the task if it has not yet started.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken) => new(System.Threading.Tasks.Task.Run(function, cancellationToken));

    /// <summary>
    /// Gets the current instance typed as <see cref="System.Threading.Tasks.Task"/>.
    /// </summary>
    public Task Task { get; } = task;

    /// <summary>
    /// Gets an awaitable object that allows for configured awaits on the wrapped <see cref="System.Threading.Tasks.Task"/>.
    /// </summary>
    /// <param name="continueOnCapturedContext">Whether to attempt to marshal the continuation back to the original context captured.</param>
    /// <returns>An awaitable object.</returns>
    public new ConfiguredExtendedDebugTaskAwaitable ConfigureAwait(bool continueOnCapturedContext) => new(Task, this, _creationStack, continueOnCapturedContext);
    /// <summary>
    /// Gets an awaitable object that allows for configured awaits on the wrapped <see cref="System.Threading.Tasks.Task"/>.
    /// </summary>
    /// <param name="options">Options for configuring the await.</param>
    /// <returns>An awaitable object.</returns>
    public new ConfiguredExtendedDebugTaskAwaitable ConfigureAwait(ConfigureAwaitOptions options) => new(Task, this, _creationStack, options);
    /// <summary>
    /// Gets an awaiter for this task.
    /// </summary>
    /// <returns>The awaiter.</returns>
    public new ExtendedDebugTaskAwaiter GetAwaiter() => new(Task.GetAwaiter(), this, _creationStack);
}

#region ExtendedDebugTaskAwaiter
/// <summary>
/// Enables waiting for the task to complete execution while capturing the stack trace at the await point.
/// </summary>
[StackTraceHidden]
[DebuggerStepThrough]
public struct ExtendedDebugTaskAwaiter : INotifyCompletion
{
    private readonly TaskAwaiter _awaiter;
    private readonly ExtendedDebugTask _edt;
    private readonly StackTrace _creationStack;

    private ExtendedDebugTaskAwaiter(StackTrace creationStack)
    {
        _creationStack = creationStack;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedDebugTaskAwaiter"/> struct.
    /// </summary>
    /// <param name="awaiter">The wrapped awaiter.</param>
    /// <param name="edt">The <see cref="ExtendedDebugTask"/> that this awaiter is for.</param>
    /// <param name="creationStack">The stack trace at the creation point of the task.</param>
    public ExtendedDebugTaskAwaiter(TaskAwaiter awaiter, ExtendedDebugTask edt, StackTrace creationStack) : this(creationStack)
    {
        _awaiter = awaiter;
        _edt = edt;
    }

    public readonly bool IsCompleted => _awaiter.IsCompleted;

    public readonly void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);
    public void GetResult()
    {
        var awaitStack = new StackTrace(true);

        try
        {
            _awaiter.GetResult();
        }
        catch (Exception ex)
        {
            var mergedMessage = string.Format(ExtendedDebugTask.awaitStackTemplate, ex.Message, awaitStack);
            if (_creationStack is not null)
            {
                mergedMessage += string.Format(ExtendedDebugTask.creationStackTemplate, _creationStack);
            }
            throw new ExtendedDebugTaskException(mergedMessage, ex, _creationStack, awaitStack, _edt);
        }
    }
}
#endregion

#region ConfiguredExtendedDebugTaskAwaitable
/// <summary>
/// Provides an awaitable object that allows for configured awaits on <see cref="ExtendedDebugTask"/>.
/// </summary>
[StackTraceHidden]
[DebuggerStepThrough]
public readonly struct ConfiguredExtendedDebugTaskAwaitable
{
    private readonly Task _task;
    private readonly ExtendedDebugTask _edt;
    private readonly StackTrace _creationStack;
    private readonly ConfigureAwaitOptions _options = ConfigureAwaitOptions.None;

    private ConfiguredExtendedDebugTaskAwaitable(Task task, ExtendedDebugTask edt, StackTrace creationStack)
    {
        _task = task;
        _edt = edt;
        _creationStack = creationStack;
    }
    public ConfiguredExtendedDebugTaskAwaitable(Task task, ExtendedDebugTask edt, StackTrace creationStack, bool continueOnCapturedContext) : this(task, edt, creationStack)
    {
        _options = continueOnCapturedContext ? ConfigureAwaitOptions.ContinueOnCapturedContext : ConfigureAwaitOptions.None;
    }
    public ConfiguredExtendedDebugTaskAwaitable(Task task, ExtendedDebugTask edt, StackTrace creationStack, ConfigureAwaitOptions options) : this(task, edt, creationStack)
    {
        _options = options;
    }
    public ConfiguredExtendedDebugTaskAwaiter GetAwaiter() => new(_task.ConfigureAwait(_options).GetAwaiter(), _edt, _creationStack);

    #region ConfiguredExtendedDebugTaskAwaiter
    /// <summary>
    /// Enables waiting for the task to complete execution while capturing the stack trace at the await point.
    /// </summary>
    [StackTraceHidden]
    [DebuggerStepThrough]
    public struct ConfiguredExtendedDebugTaskAwaiter : INotifyCompletion
    {
        // I know this is ugly as hell, but we need to be compatible with all sorts of Task variants
        private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter _awaiter;
        private readonly ExtendedDebugTask _edt;
        private readonly StackTrace _creationStack;

        private ConfiguredExtendedDebugTaskAwaiter(StackTrace creationStack)
        {
            _creationStack = creationStack;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedDebugTaskAwaiter"/> struct.
        /// </summary>
        /// <param name="awaiter">The wrapped awaiter.</param>
        /// <param name="creationStack">The stack trace at the creation point of the task.</param>
        public ConfiguredExtendedDebugTaskAwaiter(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaiter, ExtendedDebugTask edt, StackTrace creationStack) : this(creationStack)
        {
            _awaiter = awaiter;
            _edt = edt;
        }

        public readonly bool IsCompleted => _awaiter.IsCompleted;

        public readonly void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);
        public void GetResult()
        {
            var awaitStack = new StackTrace(true);

            try
            {
                _awaiter.GetResult();
            }
            catch (Exception ex)
            {
                var mergedMessage = string.Format(ExtendedDebugTask.awaitStackTemplate, ex.Message, awaitStack);
                if (_creationStack is not null)
                {
                    mergedMessage += string.Format(ExtendedDebugTask.creationStackTemplate, _creationStack);
                }
                throw new ExtendedDebugTaskException(mergedMessage, ex, _creationStack, awaitStack, _edt);
            }
        }
    }
    #endregion
}
#endregion

/// <summary>
/// Wraps a <see cref="System.Threading.Tasks.Task{TResult}"/> to provide additional debugging information.
/// </summary>
/// <param name="task">The <see cref="System.Threading.Tasks.Task{TResult}"/> to wrap.</param>
[StackTraceHidden]
[DebuggerStepThrough]
public class ExtendedDebugTask<TResult>(Task<TResult> task)
{
    private readonly StackTrace _creationStack = new StackTrace(true);

    #region .ctors
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="function"/> when started.
    /// </summary>
    /// <param name="function">The action to associate with the task.</param>
    public ExtendedDebugTask(Func<TResult> function) : this(new Task<TResult>(function)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="function"/> when started.
    /// </summary>
    /// <param name="function">The action to associate with the task.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
    public ExtendedDebugTask(Func<TResult> function, CancellationToken cancellationToken) : this(new Task<TResult>(function, cancellationToken)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="function"/> when started.
    /// </summary>
    /// <param name="function">The action to associate with the task.</param>
    /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
    public ExtendedDebugTask(Func<TResult> function, TaskCreationOptions creationOptions) : this(new Task<TResult>(function, creationOptions)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="function"/> when started.
    /// </summary>
    /// <param name="function">The action to associate with the task.</param>
    /// <param name="state">An object representing data to be used by the action.</param>
    public ExtendedDebugTask(Func<object, TResult> function, object state) : this(new Task<TResult>(function, state)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="function"/> when started.
    /// </summary>
    /// <param name="function">The action to associate with the task.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
    /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
    public ExtendedDebugTask(Func<TResult> function, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : this(new Task<TResult>(function, cancellationToken, creationOptions)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="function"/> when started.
    /// </summary>
    /// <param name="function">The action to associate with the task.</param>
    /// <param name="state">An object representing data to be used by the action.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
    public ExtendedDebugTask(Func<object, TResult> function, object state, CancellationToken cancellationToken) : this(new Task<TResult>(function, state, cancellationToken)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="function"/> when started.
    /// </summary>
    /// <param name="function">The action to associate with the task.</param>
    /// <param name="state">An object representing data to be used by the action.</param>
    /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
    public ExtendedDebugTask(Func<object, TResult> function, object state, TaskCreationOptions creationOptions) : this(new Task<TResult>(function, state, creationOptions)) { }
    /// <summary>
    /// Initializes a new <see cref="ExtendedDebugTask"/> that executes the specified <paramref name="function"/> when started.
    /// </summary>
    /// <param name="function">The action to associate with the task.</param>
    /// <param name="state">An object representing data to be used by the action.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the new task.</param>
    /// <param name="creationOptions">The <see cref="TaskCreationOptions"/> used to customize the task's behavior.</param>
    public ExtendedDebugTask(Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : this(new Task<TResult>(function, state, cancellationToken, creationOptions)) { }
    #endregion

    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask Run(Action action) => new(System.Threading.Tasks.Task.Run(action));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels the task if it has not yet started.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask Run(Action action, CancellationToken cancellationToken) => new(System.Threading.Tasks.Task.Run(action, cancellationToken));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified action.
    /// </summary>
    /// <param name="function">The action to execute.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask Run(Func<Task> function) => new(System.Threading.Tasks.Task.Run(function));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified action.
    /// </summary>
    /// <param name="function">The action to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels the task if it has not yet started.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask Run(Func<Task> function, CancellationToken cancellationToken) => new(System.Threading.Tasks.Task.Run(function, cancellationToken));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified function.
    /// </summary>
    /// <param name="function">The function to execute.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask<TResult> Run(Func<TResult> function) => new(System.Threading.Tasks.Task.Run(function));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified function.
    /// </summary>
    /// <param name="function">The function to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels the task if it has not yet started.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask<TResult> Run(Func<TResult> function, CancellationToken cancellationToken) => new(System.Threading.Tasks.Task.Run(function, cancellationToken));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified function.
    /// </summary>
    /// <param name="function">The function to execute.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask<TResult> Run(Func<Task<TResult>> function) => new(System.Threading.Tasks.Task.Run(function));
    /// <summary>
    /// Creates and starts a <see cref="ExtendedDebugTask"/> that executes the specified function.
    /// </summary>
    /// <param name="function">The function to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels the task if it has not yet started.</param>
    /// <returns>The started task.</returns>
    public static ExtendedDebugTask<TResult> Run(Func<Task<TResult>> function, CancellationToken cancellationToken) => new(System.Threading.Tasks.Task.Run(function, cancellationToken));

    /// <summary>
    /// Gets the current instance typed as <see cref="System.Threading.Tasks.Task"/>.
    /// </summary>
    public Task<TResult> Task { get; } = task;

    /// <summary>
    /// Gets an awaitable object that allows for configured awaits on the wrapped <see cref="System.Threading.Tasks.Task"/>.
    /// </summary>
    /// <param name="continueOnCapturedContext">Whether to attempt to marshal the continuation back to the original context captured.</param>
    /// <returns>An awaitable object.</returns>
    public new ConfiguredExtendedDebugTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext) => new(Task, this, _creationStack, continueOnCapturedContext);
    /// <summary>
    /// Gets an awaitable object that allows for configured awaits on the wrapped <see cref="System.Threading.Tasks.Task"/>.
    /// </summary>
    /// <param name="options">Options for configuring the await.</param>
    /// <returns>An awaitable object.</returns>
    public new ConfiguredExtendedDebugTaskAwaitable<TResult> ConfigureAwait(ConfigureAwaitOptions options) => new(Task, this, _creationStack, options);
    /// <summary>
    /// Gets an awaiter for this task.
    /// </summary>
    /// <returns>The awaiter.</returns>
    public new ExtendedDebugTaskAwaiter<TResult> GetAwaiter() => new(Task.GetAwaiter(), this, _creationStack);
}

#region ExtendedDebugTaskAwaiter
/// <summary>
/// Enables waiting for the task to complete execution while capturing the stack trace at the await point.
/// </summary>
[StackTraceHidden]
[DebuggerStepThrough]
public struct ExtendedDebugTaskAwaiter<TResult> : INotifyCompletion
{
    private readonly TaskAwaiter<TResult> _awaiter;
    private readonly ExtendedDebugTask<TResult> _edt;
    private readonly StackTrace _creationStack;

    private ExtendedDebugTaskAwaiter(StackTrace creationStack)
    {
        _creationStack = creationStack;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedDebugTaskAwaiter"/> struct.
    /// </summary>
    /// <param name="awaiter">The wrapped awaiter.</param>
    /// <param name="creationStack">The stack trace at the creation point of the task.</param>
    public ExtendedDebugTaskAwaiter(TaskAwaiter<TResult> awaiter, ExtendedDebugTask<TResult> edt, StackTrace creationStack) : this(creationStack)
    {
        _awaiter = awaiter;
        _edt = edt;
    }

    public readonly bool IsCompleted => _awaiter.IsCompleted;

    public readonly void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);
    public TResult GetResult()
    {
        var awaitStack = new StackTrace(true);

        try
        {
            return _awaiter.GetResult();
        }
        catch (Exception ex)
        {
            var mergedMessage = string.Format(ExtendedDebugTask.awaitStackTemplate, ex.Message, awaitStack);
            if (_creationStack is not null)
            {
                mergedMessage += string.Format(ExtendedDebugTask.creationStackTemplate, _creationStack);
            }
            throw new ExtendedDebugTaskException<TResult>(mergedMessage, ex, _creationStack, awaitStack, _edt);
        }
    }
}
#endregion

#region ConfiguredExtendedDebugTaskAwaitable
/// <summary>
/// Provides an awaitable object that allows for configured awaits on <see cref="ExtendedDebugTask"/>.
/// </summary>
[StackTraceHidden]
[DebuggerStepThrough]
public readonly struct ConfiguredExtendedDebugTaskAwaitable<TResult>
{
    private readonly Task<TResult> _task;
    private readonly ExtendedDebugTask<TResult> _edt;
    private readonly StackTrace _creationStack;
    private readonly ConfigureAwaitOptions _options = ConfigureAwaitOptions.None;

    private ConfiguredExtendedDebugTaskAwaitable(Task<TResult> task, ExtendedDebugTask<TResult> edt, StackTrace creationStack)
    {
        _task = task;
        _edt = edt;
        _creationStack = creationStack;
    }
    public ConfiguredExtendedDebugTaskAwaitable(Task<TResult> task, ExtendedDebugTask<TResult> edt, StackTrace creationStack, bool continueOnCapturedContext) : this(task, edt, creationStack)
    {
        _options = continueOnCapturedContext ? ConfigureAwaitOptions.ContinueOnCapturedContext : ConfigureAwaitOptions.None;
    }
    public ConfiguredExtendedDebugTaskAwaitable(Task<TResult> task, ExtendedDebugTask<TResult> edt, StackTrace creationStack, ConfigureAwaitOptions options) : this(task, edt, creationStack)
    {
        _options = options;
    }
    public ConfiguredExtendedDebugTaskAwaiter GetAwaiter() => new(_task.ConfigureAwait(_options).GetAwaiter(), _edt, _creationStack);

    #region ConfiguredExtendedDebugTaskAwaiter
    /// <summary>
    /// Enables waiting for the task to complete execution while capturing the stack trace at the await point.
    /// </summary>
    [StackTraceHidden]
    [DebuggerStepThrough]
    public struct ConfiguredExtendedDebugTaskAwaiter : INotifyCompletion
    {
        // I know this is ugly as hell, but we need to be compatible with all sorts of Task variants
        private readonly ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter _awaiter;
        private readonly StackTrace _creationStack;
        private readonly ExtendedDebugTask<TResult> _edt;

        private ConfiguredExtendedDebugTaskAwaiter(StackTrace creationStack, ExtendedDebugTask<TResult> edt)
        {
            _creationStack = creationStack;
            _edt = edt;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedDebugTaskAwaiter"/> struct.
        /// </summary>
        /// <param name="awaiter">The wrapped awaiter.</param>
        /// <param name="creationStack">The stack trace at the creation point of the task.</param>
        public ConfiguredExtendedDebugTaskAwaiter(ConfiguredTaskAwaitable<TResult>.ConfiguredTaskAwaiter awaiter, ExtendedDebugTask<TResult> edt, StackTrace creationStack) : this(creationStack, edt)
        {
            _awaiter = awaiter;
        }

        public readonly bool IsCompleted => _awaiter.IsCompleted;

        public readonly void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);
        public TResult GetResult()
        {
            var awaitStack = new StackTrace(true);

            try
            {
                return _awaiter.GetResult();
            }
            catch (Exception ex)
            {
                var mergedMessage = string.Format(ExtendedDebugTask.awaitStackTemplate, ex.Message, awaitStack);
                if (_creationStack is not null)
                {
                    mergedMessage += string.Format(ExtendedDebugTask.creationStackTemplate, _creationStack);
                }
                throw new ExtendedDebugTaskException<TResult>(mergedMessage, ex, _creationStack, awaitStack, _edt);
            }
        }
    }
    #endregion
}
#endregion

#region ExtendedDebugTaskExceptions
/// <summary>
/// The exception that is thrown as a proxy to the original exception thrown by a <see cref="Task"/> that is wrapped by an <see cref="ExtendedDebugTask"/>.
/// </summary>
public class ExtendedDebugTaskException : Exception
{
    /// <summary>
    /// Gets a <see cref="StackTrace"/> that represents the call stack at the point where the <see cref="ExtendedDebugTask"/> was created.
    /// </summary>
    public StackTrace CreationStack { get; }
    /// <summary>
    /// Gets a <see cref="StackTrace"/> that represents the call stack at the point where the <see cref="ExtendedDebugTask"/> was awaited (or <c>.GetResult()</c> was called on an awaiter for it).
    /// </summary>
    public StackTrace AwaitStack { get; }
    /// <summary>
    /// Gets the <see cref="ExtendedDebugTask"/> that threw the exception.
    /// </summary>
    public ExtendedDebugTask ExtendedDebugTask { get; }

    internal ExtendedDebugTaskException(string message, Exception innerException, StackTrace creationStack, StackTrace awaitStack, ExtendedDebugTask throwingEdt) : base(message, innerException)
    {
        CreationStack = creationStack;
        AwaitStack = awaitStack;
        ExtendedDebugTask = throwingEdt;
    }
}

/// <summary>
/// The exception that is thrown as a proxy to the original exception thrown by a <see cref="Task{TResult}"/> that is wrapped by an <see cref="ExtendedDebugTask{TResult}"/>.
/// </summary>
public class ExtendedDebugTaskException<TResult> : Exception
{
    /// <summary>
    /// Gets a <see cref="StackTrace"/> that represents the call stack at the point where the <see cref="ExtendedDebugTask"/> was created.
    /// </summary>
    public StackTrace CreationStack { get; }
    /// <summary>
    /// Gets a <see cref="StackTrace"/> that represents the call stack at the point where the <see cref="ExtendedDebugTask"/> was awaited (or <c>.GetResult()</c> was called on an awaiter for it).
    /// </summary>
    public StackTrace AwaitStack { get; }
    /// <summary>
    /// Gets the <see cref="ExtendedDebugTask"/> that threw the exception.
    /// </summary>
    public ExtendedDebugTask<TResult> ExtendedDebugTask { get; }

    internal ExtendedDebugTaskException(string message, Exception innerException, StackTrace creationStack, StackTrace awaitStack, ExtendedDebugTask<TResult> throwingEdt) : base(message, innerException)
    {
        CreationStack = creationStack;
        AwaitStack = awaitStack;
        ExtendedDebugTask = throwingEdt;
    }
}
#endregion