using System.Diagnostics;
using System.Text.RegularExpressions;

using LaquaiLib.Extensions;

using Timer = System.Threading.Timer;

namespace LaquaiLib.Util;

/// <summary>
/// Provides methods and events for working with processes.
/// </summary>
public static class Processes
{
    private static readonly Timer timer = new Timer(ConditionalRaiseEvents, null, Timeout.Infinite, 10);
    private static Process[] _previousProcessList = GetAllProcesses();
    private static readonly Lock _syncRoot = new Lock();

    /// <summary>
    /// Returns a sequence of all processes running on the local computer.
    /// </summary>
    public static Process[] GetAllProcesses() => Process.GetProcesses();

    /// <summary>
    /// Occurs for each process that is started on the local computer.
    /// </summary>
    public static event Action<Process> ProcessStarted;
    /// <summary>
    /// Occurs for each process that is stopped on the local computer.
    /// </summary>
    public static event Action<Process> ProcessStopped;

    /// <summary>
    /// Removes all entries in the invocation lists of the events defined in <see cref="Processes"/>.
    /// </summary>
    public static void Clear()
    {
        ProcessStarted = null;
        ProcessStopped = null;
    }
    /// <summary>
    /// Starts raising the events defined in <see cref="Processes"/>.
    /// </summary>
    public static void Start() => timer.Change(0, 10);
    /// <summary>
    /// Stops raising the events defined in <see cref="Processes"/>.
    /// </summary>
    public static void Stop() => timer.Change(Timeout.Infinite, 10);

    /// <summary>
    /// Raises the events defined in <see cref="Processes"/> if their conditions are met.
    /// </summary>
    /// <param name="state">Unused / ignored unconditionally.</param>
    private static void ConditionalRaiseEvents(object state)
    {
        var currentProcessList = GetAllProcesses();
        var newProcesses = currentProcessList.ExceptBy(_previousProcessList, static proc => proc.Id);
        var stoppedProcesses = _previousProcessList.ExceptBy(currentProcessList, static proc => proc.Id);

        foreach (var process in newProcesses)
        {
            ProcessStarted?.Invoke(process);
        }

        foreach (var process in stoppedProcesses)
        {
            ProcessStopped?.Invoke(process);
        }

        _previousProcessList = currentProcessList;
    }

    /// <summary>
    /// Gets all processes whose name contains the specified string.
    /// </summary>
    /// <param name="name">The string to search for in the process names.</param>
    /// <returns>An array of processes whose name contains the specified string.</returns>
    public static Process[] FindProcesses(string name) => [.. Process.GetProcesses().Where(p => p.ProcessName.Contains(name, StringComparison.OrdinalIgnoreCase))];
    /// <summary>
    /// Gets all processes whose name matches the specified regular expression.
    /// </summary>
    /// <param name="regex">The regular expression to match against the process names.</param>
    /// <returns>An array of processes whose name matches the specified regular expression.</returns>
    public static Process[] FindProcesses(Regex regex) => [.. GetAllProcesses().Where(proc => regex.IsMatch(proc.ProcessName))];
}