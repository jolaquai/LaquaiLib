using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace LaquaiLib.Util.ShellInterfaces;

/// <summary>
/// Implements <see cref="IShellInterface"/> using a PowerShell instance.
/// </summary>
public class PowerShellInterface : IShellInterface
{
    public Process Process { get; init; }
    public bool Ready => Process.StandardInput?.BaseStream?.CanWrite ?? false;
    public StreamReader? StdOut => Process.StandardOutput;
    public StreamReader? StdErr => Process.StandardError;
    public bool Exists
    {
        get
        {
            try
            {
                _ = Process.Handle;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }

    // Don't ever Write-Host in here, it will make consuming the output unnecessarily difficult.
    private static readonly string _command = Convert.ToBase64String(Encoding.Unicode.GetBytes("""
        while ($true) {
            $cmdline = Read-Host
            & $cmdline
        }
        """));
    private readonly SemaphoreSlim _syncSemaphore = new SemaphoreSlim(2, 2);

    /// <summary>
    /// Asynchronously creates and configures a new instance of <see cref="PowerShellInterface"/>.
    /// </summary>
    /// <returns>The configured instance of <see cref="PowerShellInterface"/>.</returns>
    public static async Task<PowerShellInterface> CreateInstanceAsync()
    {
        var instance = new PowerShellInterface()
        {
            Process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = "powershell",
                    ArgumentList =
                    {
                        "-NoProfile",
                        "-NoLogo",
                        "-EncodedCommand",
                        _command
                    },
                    CreateNoWindow = false,
                    WorkingDirectory = Environment.CurrentDirectory
                },
                EnableRaisingEvents = true
            }
        };
        instance.Process.Start();

        // Before returning, the working directory MUST be set to the current directory
        // Unfortunately, PS doesn't care about passing ProcessStartInfo.WorkingDirectory it seems
        await instance.DispatchAsync($"Set-Location '{Environment.CurrentDirectory}'");
        return instance;
    }
    private PowerShellInterface()
    {
    }

    public async Task<CommandDispatchResult> DispatchAsync(string input)
    {
        // 1st semaphore entry: write
        await _syncSemaphore.WaitAsync();
        try
        {
            await Task.Run(async () =>
            {
                while (!Ready)
                {
                    await Task.Delay(100);
                }
            });
            await Process.StandardInput.WriteLineAsync(input);
            await Process.StandardInput.FlushAsync();

            var readLines = TryReadOutput().Split(Environment.NewLine);
            var commandDispatchResult = new CommandDispatchResult()
            {
                Input = input,
                Output = string.Join(Environment.NewLine, readLines[0] == input ? readLines[1..] : readLines),
            };
            return commandDispatchResult;
        }
        finally
        {
            _syncSemaphore.Release();
        }
    }
    public async Task Close()
    {
        await _syncSemaphore.WaitAsync();
        try
        {
            Process.StandardInput.Close();
        }
        finally
        {
            _syncSemaphore.Release();
        }
    }

    // This was part of the interface before, but since DispatchAsync now always returns the output of a command, calling this would cause more problems than it solves
    private string TryReadOutput()
    {
        var cts = new CancellationTokenSource();
        BlockingCollection<string> lines = [];
        try
        {
            while (true)
            {
                var readTask = Task.Run(async () =>
                {
                    // 2nd semaphore entry: reading
                    await _syncSemaphore.WaitAsync();
                    try
                    {
                        // Have to use async methods to make the operation cancellable
                        var line = await StdOut.ReadLineAsync(cts.Token);
                        if (line is not null)
                        {
                            lines.Add(line);
                        }
                        else
                        {
                            cts.Cancel();
                        }
                    }
                    finally
                    {
                        _syncSemaphore.Release();
                    }
                }, cts.Token);
                readTask.Wait(300);
                if (!readTask.IsCompleted)
                {
                    cts.Cancel();
                    break;
                }
                readTask.Wait();
            }
        }
        catch (Exception)
        {
            { }
        }
        return string.Join(Environment.NewLine, lines).Trim();
    }

    #region public async ValueTask DisposeAsync()
    public async ValueTask DisposeAsync()
    {
        await DispatchAsync("break");
        await (Process.WaitForExitAsync() ?? Task.CompletedTask);
        Process.Dispose();
    }
    #endregion
}
