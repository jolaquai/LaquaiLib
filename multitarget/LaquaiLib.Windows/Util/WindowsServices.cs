using System.Diagnostics;
using System.Management;

namespace LaquaiLib.Windows.Util;

/// <summary>
/// Contains helper methods for Windows services.
/// </summary>
public static class WindowsServices
{
    /// <summary>
    /// Determines whether the process with the specified <paramref name="processId"/> is running as a Windows service with the specified <paramref name="serviceName"/>.
    /// </summary>
    /// <param name="serviceName">The name of the Windows service.</param>
    /// <param name="processId">The ID of the process to check.</param> 
    /// <returns><see langword="true"/> if the process is running as a Windows service, otherwise <see langword="false"/>.</returns>
    public static bool IsRunningAsService(string serviceName, int processId) => IsRunningAsService(serviceName, Process.GetProcessById(processId));
    /// <summary>
    /// Determines whether the specified <paramref name="proc"/> is running as a Windows service with the specified <paramref name="serviceName"/>.
    /// </summary>
    /// <param name="serviceName">The name of the Windows service.</param>
    /// <param name="proc">The process to check.</param>
    /// <returns><see langword="true"/> if the process is running as a Windows service, otherwise <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">
    public static bool IsRunningAsService(string serviceName, Process proc)
    {
        var process = Process.GetProcessById(proc.Id);

        if (Environment.UserInteractive)
        {
            return false;
        }

        try
        {
            using var serviceController = new System.ServiceProcess.ServiceController(serviceName);

            if (serviceController.Status != System.ServiceProcess.ServiceControllerStatus.Running)
            {
                return false;
            }

            // The service exists, but this could still be a non-service instance
            var parentPid = GetParentProcessId(proc.Id);
            if (parentPid is -1)
            {
                return false;
            }
            using var parent = Process.GetProcessById(parentPid);
            if (parent != null && (parent.ProcessName.Contains("services", StringComparison.OrdinalIgnoreCase)
               || parent.ProcessName.Contains("svchost", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return true;
        }
        catch
        {
            // If the service doesn't exist, no process can be running as that service
            return false;
        }
    }
    private static int GetParentProcessId(int processId)
    {
        using var query = new ManagementObjectSearcher($"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {processId}");
        foreach (var mo in query.Get())
        {
            return Convert.ToInt32(mo["ParentProcessId"]);
        }

        return -1;
    }
}
