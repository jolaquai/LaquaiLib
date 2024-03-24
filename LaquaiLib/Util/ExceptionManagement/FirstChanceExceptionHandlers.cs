using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;

using LaquaiLib.Extensions;
using LaquaiLib.Util.Meta;

namespace LaquaiLib.Util.ExceptionManagement;

/// <summary>
/// Exposes <see cref="EventHandler{TEventArgs}"/> instances registerable for the <see cref="AppDomain.FirstChanceException"/> event.
/// </summary>
public static partial class FirstChanceExceptionHandlers
{
    private static readonly EventHandler<FirstChanceExceptionEventArgs>[] _handlers;

    static FirstChanceExceptionHandlers()
    {
        _handlers = typeof(FirstChanceExceptionHandlers)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(handler => !handler.IsConstructor && handler.Name != "RegisterAll")
            .Select(handler =>
            {
                try
                {
                    return handler.CreateDelegate<EventHandler<FirstChanceExceptionEventArgs>>();
                }
                catch
                {
                    throw new InvalidOperationException($"The method {handler.Name} must be a valid {nameof(EventHandler<FirstChanceExceptionEventArgs>)} (return void and take an object? and a {nameof(FirstChanceExceptionEventArgs)} instance) or it failed to register.");
                }
            })
            .ToArray();
    }

    private static bool isRegistered;
    /// <summary>
    /// Registers all <see cref="EventHandler{TEventArgs}"/> instances in the <see cref="FirstChanceExceptionHandlers"/> class for the <see cref="AppDomain.FirstChanceException"/> event.
    /// </summary>
    /// <remarks>
    /// Be aware that this will pollute your call stack like all hell...
    /// </remarks>
    public static void RegisterAll()
    {
        if (isRegistered)
        {
            return;
        }
        foreach (var handler in _handlers)
        {
            AppDomain.CurrentDomain.FirstChanceException += handler;
        }
        isRegistered = true;
    }

    private static string[] allPaths;
    private static string[] pathExts;
    /// <summary>
    /// Wraps <see cref="EntryPointNotFoundException"/>s in a <see cref="FirstChanceException"/> with additional information about the DLL and entry point.
    /// <para/>Rethrows the original exception if no additional information could be gathered.
    /// </summary>
    /// <param name="sender">Ignored.</param>
    /// <param name="e">The <see cref="FirstChanceExceptionEventArgs"/> instance containing the event data.</param>
    /// <exception cref="FirstChanceException">Thrown when an <see cref="EntryPointNotFoundException"/> is caught. Contains additional information about the DLL and entry point.</exception>
    public static void WrapEntryPointNotFoundException(object? sender, FirstChanceExceptionEventArgs e)
    {
        var capture = ExceptionDispatchInfo.Capture(e.Exception);

        if (e.Exception is EntryPointNotFoundException epnfEx)
        {
            GetEntryPointNotFoundExceptionData(epnfEx, out var entryPoint, out var dllName);

            allPaths ??=
            [
                .. Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process).Split(Path.PathSeparator),
                .. Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User).Split(Path.PathSeparator),
                .. Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine).Split(Path.PathSeparator)
            ];
            allPaths = allPaths.Select(p => p.Trim()).Distinct().OrderDescending().ToArray();
            pathExts ??=
            [
                .. Environment.GetEnvironmentVariable("PATHEXT", EnvironmentVariableTarget.Process).Split(Path.PathSeparator),
                .. Environment.GetEnvironmentVariable("PATHEXT", EnvironmentVariableTarget.User).Split(Path.PathSeparator),
                .. Environment.GetEnvironmentVariable("PATHEXT", EnvironmentVariableTarget.Machine).Split(Path.PathSeparator)
            ];
            pathExts = pathExts.Select(p => p.Trim()).Distinct().OrderDescending().ToArray();

            var fullPath = "";
            foreach (var path in allPaths)
            {
                var possiblePath = Path.Combine(path, dllName);
                if (File.Exists(possiblePath))
                {
                    fullPath = Path.GetFullPath(possiblePath);
                    break;
                }
            }

            string[] possibleEntryPoints;

            // Find dumpbin.exe
            var dumpbinPath = MetaHelpers.FindTool(MetaTool.Dumpbin).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(dumpbinPath))
            {
                // no wrapped exception here, as this is a critical error
                capture.Throw();
            }

            try
            {
                using (var proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = dumpbinPath,
                    ArgumentList =
                {
                    "/exports",
                    fullPath
                },
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }))
                {
                    var output = proc.StandardOutput.ReadToEnd();
                    proc.Kill();
                    proc.WaitForExit();
                    // Sample:
                    // 2459  3C6 0002A800 UnregisterDeviceNotification
                    // 2460  3C7 0002EB60 UnregisterHotKey
                    // 2461  3C8 000029B0 UnregisterMessagePumpHook
                    // 2462  3C9 000496C0 UnregisterPointerInputTarget
                    // 2463  3CA 000496E0 UnregisterPointerInputTargetEx
                    // 2464  3CB 0002A730 UnregisterPowerSettingNotification
                    var entryPoints = output
                        .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                        .SkipWhile(l => !l.Contains("ordinal hint RVA", StringComparison.OrdinalIgnoreCase))
                        .Skip(1)
                        .TakeWhile(l => !string.IsNullOrWhiteSpace(l))
                        .Select(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last())
                        .ToArray();
                    possibleEntryPoints = Array.FindAll(entryPoints, ep => ep.StartsWith(entryPoint, StringComparison.OrdinalIgnoreCase));
                    Array.Sort(possibleEntryPoints);
                }
            }
            catch
            {
                capture.Throw();
                // Unreachable
                throw;
            }

            throw new FirstChanceException($"""
                {e.Exception.Message}
                Are you missing a value for LibraryImportAttribute.EntryPoint with an 'A' or 'W' suffix?

                You were probably looking for one of these entry points:
                {string.Join(Environment.NewLine, possibleEntryPoints.Where(n => n.Length == entryPoint.Length + 1).Select(name => $"    - {name}"))}

                Other matching entry points in '{dllName}':
                {string.Join(Environment.NewLine, possibleEntryPoints.WhereNot(n => n.Length == entryPoint.Length + 1).Select(name => $"    - {name}"))}

                """, e.Exception);
        }
    }

    // Unable to find an entry point named 'GetMessage' in DLL 'user32.dll'.
    [GeneratedRegex(@"Unable to find an entry point named '(?<entryPoint>[^']+)' in DLL '(?<dllName>[^']+)'\.", RegexOptions.ExplicitCapture)]
    private static partial Regex EntryPointNotFoundExceptionDataRegex();

    internal static void GetEntryPointNotFoundExceptionData(EntryPointNotFoundException e, out string entryPoint, out string dllName)
    {
        var match = EntryPointNotFoundExceptionDataRegex().Match(e.Message);
        entryPoint = match.Groups["entryPoint"].Value;
        dllName = match.Groups["dllName"].Value;
    }
}
