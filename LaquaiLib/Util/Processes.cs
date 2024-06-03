using System.Diagnostics;

using LaquaiLib.Extensions;

using Timer = System.Threading.Timer;

namespace LaquaiLib.Util;

/// <summary>
/// Provides methods and events for working with processes.
/// </summary>
public static class Processes
{
    private static readonly Timer timer = new Timer(ConditionalRaiseEvents, null, Timeout.Infinite, 10);
    private static IEnumerable<Process> _previousProcessList = GetAllProcesses();
    private static readonly object _syncRoot = new object();

    private static event Action<Process>? processStarted;
    private static event Action<Process>? processStopped;

    /// <summary>
    /// Returns a sequence of all processes running on the local computer.
    /// </summary>
    public static IEnumerable<Process> GetAllProcesses() => Process.GetProcesses();

    /// <summary>
    /// Occurs for each process that is started on the local computer.
    /// </summary>
    public static event Action<Process>? ProcessStarted {
        add
        {
            lock (_syncRoot)
            {
                processStarted += value;
            }
        }
        remove
        {
            lock (_syncRoot)
            {
                processStarted -= value;
            }
        }
    }
    /// <summary>
    /// Occurs for each process that is stopped on the local computer.
    /// </summary>
    public static event Action<Process>? ProcessStopped {
        add
        {
            lock (_syncRoot)
            {
                processStopped += value;
            }
        }
        remove
        {
            lock (_syncRoot)
            {
                processStopped -= value;
            }
        }
    }

    /// <summary>
    /// Removes all entries in the invocation lists of the events defined in <see cref="Processes"/>.
    /// </summary>
    public static void Clear()
    {
        processStarted = null;
        processStopped = null;
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
    private static void ConditionalRaiseEvents(object? state)
    {
        var currentProcessList = GetAllProcesses();
        var newProcesses = currentProcessList.ExceptBy(_previousProcessList, proc => proc.Id);
        var stoppedProcesses = _previousProcessList.ExceptBy(currentProcessList, proc => proc.Id);

        foreach (var process in newProcesses)
        {
            processStarted?.Invoke(process);
        }

        foreach (var process in stoppedProcesses)
        {
            processStopped?.Invoke(process);
        }

        _previousProcessList = currentProcessList;
    }
}
