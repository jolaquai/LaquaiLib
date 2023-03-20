using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using System.Windows.Threading;

namespace LaquaiLib.Classes;

public class TaskbarProgress
{
    private static TaskbarProgress _instance;

    private TaskbarItemInfo _taskbar;

    internal TaskbarProgress(Window window)
    {
        window.TaskbarItemInfo ??= new();
        _taskbar = window.TaskbarItemInfo;

        _taskbar.ProgressState = TaskbarItemProgressState.Normal;
        _taskbar.ProgressValue = 0;
    }

    public static TaskbarProgress GetInstance(Window window)
    {
        _instance ??= new(window);
        return _instance;
    }
    public static TaskbarProgress GetInstance(nint window)
    {
        if (HwndSource.FromHwnd(window).RootVisual is Window target)
        {
            _instance ??= new(target);
            return _instance;
        }
        else
        {
            throw new ArgumentException("Window pointer was 0 or resolved to null.", nameof(window));
        }
    }
    public static TaskbarProgress GetInstance()
    {
        if (_instance == null)
        {
            try
            {
                if (HwndSource.FromHwnd(Process.GetCurrentProcess().MainWindowHandle).RootVisual is Window target)
                {
                    _instance ??= new(target);
                    return _instance;
                }
                else
                {
                    throw new InvalidOperationException("Cannot set taskbar progress information when calling from a non-WPF context (calling context's window handle was 0).");
                }
            }
            catch (ArgumentException argumentException)
            {
                throw new InvalidOperationException("Cannot set taskbar progress information when calling from a non-WPF context (calling context's window handle was 0).");
            }
        }
        else
        {
            return _instance;
        }
    }

    public static void ResetInstance() => _instance = null;

    public TaskbarItemProgressState SetState(TaskbarItemProgressState state)
    {
        return _taskbar.ProgressState = state;
    }
    public double SetValue(int percent)
    {
        if (percent < 0)
        {
            percent = 0;
        }
        else if (percent > 100)
        {
            percent = 100;
        }
        return _taskbar.ProgressValue = percent / 100d;
    }
    public double SetValue(double value)
    {
        if (value < 0)
        {
            return _taskbar.ProgressValue = 0;
        }
        else if (value > 1)
        {
            return _taskbar.ProgressValue = 1;
        }
        return _taskbar.ProgressValue = value;
    }
    public double IncreaseValue(double value)
    {
        if (_taskbar.ProgressValue + value < 0)
        {
            return _taskbar.ProgressValue = 0;
        }
        else if (_taskbar.ProgressValue + value > 1)
        {
            return _taskbar.ProgressValue = 1;
        }
        return _taskbar.ProgressValue += value;
    }
    public double DecreaseValue(double value)
    {
        if (_taskbar.ProgressValue - value < 0)
        {
            return _taskbar.ProgressValue = 0;
        }
        else if (_taskbar.ProgressValue - value > 1)
        {
            return _taskbar.ProgressValue = 1;
        }
        return _taskbar.ProgressValue -= value;
    }

    /// <summary>
    /// Animates towards a specified progress <paramref name="value"/> within a specified <paramref name="timeSpan"/>. Must be called from the UI thread or the <see cref="Dispatcher"/> of your main <see cref="Window"/>, otherwise the animation will not work.
    /// </summary>
    /// <param name="value">The value to animate progress towards.</param>
    /// <param name="timeSpan">The amount of time for the animation to take in milliseconds. It may not be possible to obey this in all cases.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> that may be used to request cancellation of the asynchronous animation operation.</param>
    public async Task AnimateToValueAsync(double value, long timeSpan, CancellationToken cancellationToken = default)
    {
        if (timeSpan <= 200)
        {
            SetValue(value);
            return;
        }

        timeSpan -= 200;

        _taskbar.ProgressState = TaskbarItemProgressState.Normal;

        double diff = value - (double)_taskbar.ProgressValue;
        double step = diff / 100;
        int delay = Math.Abs((int)(timeSpan / 100));

        while (diff > 0 ? _taskbar.ProgressValue < value : _taskbar.ProgressValue > value)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _taskbar.ProgressValue += step;
            await Task.Delay(delay, cancellationToken);
        }
    }

    public double GetValue() => _taskbar.ProgressValue;
}
