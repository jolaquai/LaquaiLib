﻿using System.ComponentModel;

namespace LaquaiLib.Util;

/// <summary>
/// Extends the base functionality of the <see cref="BackgroundWorker"/> type.
/// </summary>
public class ExtendedBackgroundWorker : BackgroundWorker
{
    /// <summary>
    /// Instantiates a new <see cref="ExtendedBackgroundWorker"/> that, by default, supports progress reporting and cancellation.
    /// </summary>
    public ExtendedBackgroundWorker()
    {
        WorkerReportsProgress = true;
        WorkerSupportsCancellation = true;
    }
    /// <summary>
    /// Instantiates a new <see cref="ExtendedBackgroundWorker"/> that, by default, supports progress reporting and cancellation and executes the work represented by the <paramref name="work"/> delegate when started.
    /// </summary>
    /// <param name="work">A <see cref="Delegate"/> that encapsulates a method that is executed when the <see cref="ExtendedBackgroundWorker"/> is started. If explicitly convertible to <see cref="DoWorkEventHandler"/>, it is cast and queued as work as such, otherwise dynamic invocation with no parameters is used.</param>
    public ExtendedBackgroundWorker(Delegate work) : this()
    {
        if (work is DoWorkEventHandler doWork)
        {
            DoWork += doWork;
        }
        else
        {
            DoWork += (sender, e) => work.DynamicInvoke(null);
        }
    }
    /// <summary>
    /// Instantiates a new <see cref="ExtendedBackgroundWorker"/> that, by default, supports progress reporting and cancellation and executes the work represented by the <paramref name="work"/> delegate when started, passing the specified <paramref name="args"/> as parameters.
    /// </summary>
    /// <param name="work">A <see cref="Delegate"/> that encapsulates a method that is executed when the <see cref="ExtendedBackgroundWorker"/> is started. If explicitly convertible to <see cref="DoWorkEventHandler"/>, it is cast and queued as work as such, otherwise dynamic invocation with the specified <paramref name="args"/> is used.</param>
    /// <param name="args">The arguments to pass to the <paramref name="work"/> delegate or <see langword="null"/> if the delegate does not take any arguments.</param>
    /// <remarks>This constructor forces wrapping of the passed <paramref name="work"/> delegate in a <see cref="DoWorkEventHandler"/> delegate, even if it is already explicitly convertible to <see cref="DoWorkEventHandler"/>. Use the <see cref="ExtendedBackgroundWorker(Delegate)"/> constructor to avoid this.</remarks>
    public ExtendedBackgroundWorker(Delegate work, params object?[]? args) : this()
    {
        DoWork += (sender, e) => work.DynamicInvoke(args);
    }
    /// <summary>
    /// Instantiates a new <see cref="ExtendedBackgroundWorker"/> that, by default, supports progress reporting and cancellation and executes the work represented by the <paramref name="work"/> delegates when started.
    /// </summary>
    /// <param name="work">The <see cref="Delegate"/>s that encapsulate methods that are executed when the <see cref="ExtendedBackgroundWorker"/> is started. Delegates explicitly convertible to <see cref="DoWorkEventHandler"/> are cast and queued as work as such, otherwise dynamic invocation with no parameters is used.</param>
    public ExtendedBackgroundWorker(params Delegate[] work) : this()
    {
        foreach (var w in work)
        {
            if (w is DoWorkEventHandler doWork)
            {
                DoWork += doWork;
            }
            else
            {
                DoWork += (sender, e) => w.DynamicInvoke(null);
            }
        }
    }
    /// <summary>
    /// Instantiates a new <see cref="ExtendedBackgroundWorker"/> that, by default, supports progress reporting and cancellation and executes the work represented by the <paramref name="work"/> delegates when started.
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

    private int progress;
    /// <summary>
    /// The last reported progress of the <see cref="ExtendedBackgroundWorker"/>.
    /// </summary>
    public int Progress => progress;

    /// <inheritdoc/>
    protected override void OnProgressChanged(ProgressChangedEventArgs e)
    {
        progress = e.ProgressPercentage;
        base.OnProgressChanged(e);
    }

    /// <summary>
    /// Represents a method that is invoked when the <see cref="ExtendedBackgroundWorker"/> is started.
    /// </summary>
    public delegate void WorkerStartedEventHandler(object sender, EventArgs e);
    /// <summary>
    /// Represents a method that is invoked when cancellation of the <see cref="ExtendedBackgroundWorker"/> is requested.
    /// </summary>
    public delegate void WorkerCancelledEventHandler(object sender, EventArgs e);
    /// <summary>
    /// Occurs when the <see cref="ExtendedBackgroundWorker"/> is started.
    /// </summary>
    public event WorkerStartedEventHandler WorkerStarted;
    /// <summary>
    /// Occurs when cancellation of the <see cref="ExtendedBackgroundWorker"/> is requested.
    /// </summary>
    public event WorkerCancelledEventHandler WorkerCancelled;

    /// <inheritdoc/>
    protected override void OnDoWork(DoWorkEventArgs e)
    {
        WorkerStarted?.Invoke(this, new EventArgs());
        base.OnDoWork(e);
    }

    /// <inheritdoc cref="BackgroundWorker.CancelAsync"/>
    public new void CancelAsync()
    {
        WorkerCancelled?.Invoke(this, new EventArgs());
        base.CancelAsync();
    }
}