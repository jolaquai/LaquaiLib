using System.Diagnostics;
using System.Management;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Process"/> Type.
/// </summary>
public static class ProcessExtensions
{
    /// <summary>
    /// Retrieves the command line of the specified <see cref="Process"/>.
    /// This is done either by using the <see cref="ProcessStartInfo"/> property of the <see cref="Process"/> instance or, if that is <see langword="null"/>, by using WMI.
    /// </summary>
    /// <param name="process">A <see cref="Process"/> instance.</param>
    /// <returns>The command line of the specified <see cref="Process"/> or <see langword="null"/> if it could not be retrieved.</returns>
    public static string? GetCommandLine(this Process process)
    {
        if (process.StartInfo is ProcessStartInfo psi)
        {
            if (psi.ArgumentList.Count > 0)
            {
                return $"\"{psi.FileName}\" {string.Join(' ', psi.ArgumentList.Select(a => '"' + a + '"'))}";
            }
            else
            {
                return $"\"{psi.FileName}\" {psi.Arguments}";
            }
        }
        {
            using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (var objects = searcher.Get())
            using (var obj = Enumerable.Cast<ManagementBaseObject>(objects).SingleOrDefault())
            {
                return obj?["CommandLine"]?.ToString();
            }
        }
    }
}
