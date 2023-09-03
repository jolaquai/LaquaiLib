using System.ComponentModel;

namespace LaquaiLib.Util;

/// <summary>
/// Extends the base functionality of the <see cref="BackgroundWorker"/> type.
/// </summary>
public class ExtendedBackgroundWorker : BackgroundWorker
{
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
