using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

using LaquaiLib.Extensions;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a handler for a progress display on the current window's taskbar icon.
/// </summary>
public class TaskbarProgress
{
    private readonly TaskbarItemInfo? _taskbar;

    /// <summary>
    /// Initializes a new <see cref="TaskbarProgress"/> with reference to a specified <paramref name="window"/>.
    /// </summary>
    /// <param name="window">The <see cref="Window"/> the taskbar icon of which is to display progress.</param>
    public TaskbarProgress(Window window)
    {
        _taskbar = window.TaskbarItemInfo ??= new TaskbarItemInfo();

        _taskbar.ProgressState = TaskbarItemProgressState.Normal;
        _taskbar.ProgressValue = 0;
    }
    /// <summary>
    /// Creates or directly returns an existing instance of <see cref="TaskbarProgress"/> with reference to a <see cref="Window"/> identified by its <paramref name="hwnd"/>.
    /// </summary>
    /// <remarks>
    /// The application calling this method or using its return value must own the <see cref="Window"/> pointed to by <paramref name="hwnd"/>.
    /// </remarks>
    /// <param name="hwnd">The hwnd to the <see cref="Window"/> the taskbar icon of which is to display progress.</param>
    /// <returns>A <see cref="TaskbarProgress"/> instance.</returns>
    public TaskbarProgress(nint hwnd)
    {
        if (HwndSource.FromHwnd(hwnd).RootVisual is Window target)
        {
            _taskbar = target.TaskbarItemInfo;
        }
        else
        {
            throw new ArgumentException("Window hwnd was 0 or resolved to null.", nameof(hwnd));
        }
    }

    /// <summary>
    /// Sets the state of the taskbar progress visual.
    /// </summary>
    /// <param name="state">The new state of the taskbar progress visual.</param>
    /// <returns>The value of the <see cref="TaskbarItemInfo.ProgressState"/> property after the attempted set operation.</returns>
    public TaskbarItemProgressState SetState(TaskbarItemProgressState state)
    {
        _taskbar!.ProgressState = state;
        return _taskbar.ProgressState;
    }

    /// <summary>
    /// Animates towards a specified progress <paramref name="target"/> within a specified <paramref name="duration"/>.
    /// </summary>
    /// <param name="target">The value to animate progress towards.</param>
    /// <param name="duration">The amount of time for the animation to take in milliseconds. It may not be possible to obey this (exactly) in all cases.</param>
    /// <param name="steps">The number of steps to take to reach the target value. This is ignored if greater than <paramref name="duration"/>.</param>
    /// <returns>A <see cref="Task"/> that completes when the animation has finished.</returns>
    public async Task AnimateToValueAsync(double target, int duration, int steps = 100)
    {
        var from = _taskbar!.ProgressValue;
        var to = double.Clamp(target, 0, 1);

        // Can't use any of the animation classes because the animation just never starts
        steps = steps > duration ? duration : steps;
        var wait = TimeSpan.FromMilliseconds(duration) / steps;

        static double RoundToMultiple(double value, double multiple) => Math.Round(value / multiple) * multiple;

        var values =
            Miscellaneous.Range(from, to, (to - from) / steps)
                     .Select(d => RoundToMultiple(d, 0.05))
                     .ToArray();
        // Consolidate the values
        List<KeyValuePair<double, TimeSpan>> sequence = [];
        foreach (var value in values.Distinct())
        {
            sequence.Add(new KeyValuePair<double, TimeSpan>(value, wait * values.Count(d => d == value)));
        }

        foreach (var (value, time) in sequence)
        {
            _taskbar.ProgressValue = value;
            await Task.Delay(time);
        }

        _taskbar!.ProgressValue = to;
    }

    /// <summary>
    /// Gets or sets the current value of the taskbar progress bar.
    /// </summary>
    public double Value
    {
        get => _taskbar!.ProgressValue;
        set => _taskbar!.ProgressValue = value;
    }
}
