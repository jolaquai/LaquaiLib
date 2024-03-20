using System.ComponentModel;

namespace LaquaiLib.Util;

/// <summary>
/// Extends the base functionality of the <see cref="BackgroundWorker"/> type.
/// </summary>
public class ExtendedBackgroundWorker : BackgroundWorker
{
    /// <summary>
    /// Initializes a new <see cref="ExtendedBackgroundWorker"/> that, by default, supports progress reporting and cancellation.
    /// </summary>
    public ExtendedBackgroundWorker()
    {
        WorkerReportsProgress = true;
        WorkerSupportsCancellation = true;
    }
    /// <summary>
    /// Initializes a new <see cref="ExtendedBackgroundWorker"/> that, by default, supports progress reporting and cancellation and executes the work represented by the <paramref name="work"/> delegate when started.
    /// </summary>
    /// <param name="work">A <see cref="Delegate"/> that encapsulates a method that is executed when the <see cref="ExtendedBackgroundWorker"/> is started. If explicitly convertible to <see cref="DoWorkEventHandler"/>, it is cast and queued as work as such, otherwise dynamic invocation with no parameters is used.</param>
    public ExtendedBackgroundWorker(Delegate work) : this()
    {
        if (work is DoWorkEventHandler doWork)
        {
            DoWork += doWork;
        }
        else if (Unsafe.As<DoWorkEventHandler>(work) is DoWorkEventHandler _doWork)
        {
            DoWork += _doWork;
        }
        else
        {
            DoWork += (sender, e) => work.DynamicInvoke(null);
        }
    }
    /// <summary>
    /// Initializes a new <see cref="ExtendedBackgroundWorker"/> that, by default, supports progress reporting and cancellation and executes the work represented by the <paramref name="work"/> delegate when started, passing the specified <paramref name="args"/> as parameters.
    /// </summary>
    /// <param name="work">A <see cref="Delegate"/> that encapsulates a method that is executed when the <see cref="ExtendedBackgroundWorker"/> is started. If explicitly convertible to <see cref="DoWorkEventHandler"/>, it is cast and queued as work as such, otherwise dynamic invocation with the specified <paramref name="args"/> is used.</param>
    /// <param name="args">The arguments to pass to the <paramref name="work"/> delegate or <see langword="null"/> if the delegate does not take any arguments.</param>
    /// <remarks>This constructor forces wrapping of the passed <paramref name="work"/> delegate in a <see cref="DoWorkEventHandler"/> delegate, even if it is already explicitly convertible to <see cref="DoWorkEventHandler"/>. Use the <see cref="ExtendedBackgroundWorker(Delegate)"/> constructor to avoid this.</remarks>
    public ExtendedBackgroundWorker(Delegate work, params object?[]? args) : this()
    {
        DoWork += (sender, e) => work.DynamicInvoke(args);
    }
    /// <summary>
    /// Initializes a new <see cref="ExtendedBackgroundWorker"/> that, by default, supports progress reporting and cancellation and executes the work represented by the <paramref name="work"/> delegates when started.
    /// </summary>
    /// <param name="work">The <see cref="Delegate"/>s that encapsulate methods that are executed when the <see cref="ExtendedBackgroundWorker"/> is started. Delegates explicitly convertible to <see cref="DoWorkEventHandler"/> are cast and queued as work as such, otherwise dynamic invocation with no parameters is used.</param>
    public ExtendedBackgroundWorker(params Delegate[] work) : this()
    {
        ArgumentNullException.ThrowIfNull(work);
        foreach (var w in work)
        {
            if (w is DoWorkEventHandler doWork)
            {
                DoWork += doWork;
            }
            else if (Unsafe.As<DoWorkEventHandler>(work) is DoWorkEventHandler _doWork)
            {
                DoWork += _doWork;
            }
            else
            {
                DoWork += (sender, e) => w.DynamicInvoke(null);
            }
        }
    }
    /// <summary>
    /// Initializes a new <see cref="ExtendedBackgroundWorker"/> that, by default, supports progress reporting and cancellation and executes the work represented by the <paramref name="work"/> delegates when started.
    /// </summary>
    /// <param name="work">The <see cref="Delegate"/>s that encapsulate methods that are executed when the <see cref="ExtendedBackgroundWorker"/> is started. Delegates explicitly convertible to <see cref="DoWorkEventHandler"/> are cast and queued as work as such, otherwise dynamic invocation with no parameters is used.</param>
    /// <param name="args">The arguments to pass to the <paramref name="work"/> delegates or <see langword="null"/> if the delegates do not take any arguments.</param>
    public ExtendedBackgroundWorker(IEnumerable<Delegate> work, IEnumerable<object?>? args) : this()
    {
        var theArgs = args?.ToArray();
        foreach (var w in work)
        {
            if (w is DoWorkEventHandler doWork)
            {
                DoWork += doWork;
            }
            else
            {
                DoWork += (sender, e) => w.DynamicInvoke(theArgs);
            }
        }
    }

    /// <summary>
    /// The last reported progress of the <see cref="ExtendedBackgroundWorker"/>.
    /// </summary>
    public int Progress { get; private set; }

    /// <inheritdoc/>
    protected override void OnProgressChanged(ProgressChangedEventArgs e)
    {
        Progress = e.ProgressPercentage;
        base.OnProgressChanged(e);
    }

    /// <summary>
    /// Occurs when the <see cref="ExtendedBackgroundWorker"/> is started.
    /// </summary>
    public event EventHandler WorkerStarted;

    /// <inheritdoc/>
    protected override void OnDoWork(DoWorkEventArgs e)
    {
        WorkerStarted?.Invoke(this, EventArgs.Empty);
        base.OnDoWork(e);
    }
}
