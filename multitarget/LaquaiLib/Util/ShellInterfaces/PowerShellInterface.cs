using System.Diagnostics;

namespace LaquaiLib.Util.ShellInterfaces;

/// <summary>
/// Implements <see cref="IShellInterface"/> using a PowerShell instance.
/// </summary>
public sealed class PowerShellInterface : IShellInterface
{
    /// <inheritdoc/>
    public Process Process { get; init; }
    /// <inheritdoc/>
    public bool Ready => Process.StandardInput?.BaseStream?.CanWrite ?? false;
    /// <inheritdoc/>
    public StreamReader StdOut => Process.StandardOutput;
    /// <inheritdoc/>
    public StreamReader StdErr => Process.StandardError;
    /// <inheritdoc/>
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
    /// <inheritdoc/>
    public bool SupportsMultilineCommands => true;

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
        _ = instance.Process.Start();

        // Before returning, the working directory MUST be set to the current directory
        // Unfortunately, PS doesn't care about passing ProcessStartInfo.WorkingDirectory it seems
        _ = await instance.DispatchAsync($"Set-Location '{Environment.CurrentDirectory}'").ConfigureAwait(false);
        return instance;
    }
    private PowerShellInterface()
    {
    }
    /// <inheritdoc/>
    public async Task<CommandDispatchResult> DispatchAsync(string input)
    {
        // 1st semaphore entry: write
        await _syncSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            await Task.Run(async () =>
            {
                while (!Ready)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

            if (input.Contains(Environment.NewLine))
            {
                input = $$$"""
                    & {{
                    {{{input}}}
                    }}
                    """;
            }

            await Process.StandardInput.WriteLineAsync(input).ConfigureAwait(false);
            await Process.StandardInput.FlushAsync().ConfigureAwait(false);

            var readLines = TryReadOutput().Split(Environment.NewLine);
            return new CommandDispatchResult()
            {
                Input = input,
                Output = string.Join(Environment.NewLine, readLines[0] == input ? readLines[1..] : readLines),
            };
        }
        finally
        {
            _ = _syncSemaphore.Release();
        }
    }
    /// <inheritdoc/>
    public async Task Close()
    {
        await _syncSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            Process.StandardInput.Close();
        }
        finally
        {
            _ = _syncSemaphore.Release();
        }
    }
    /// <inheritdoc/>
    public async Task WhenReady()
    {
        while (!Ready)
        {
            await Task.Delay(100).ConfigureAwait(false);
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
                    await _syncSemaphore.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        // Have to use async methods to make the operation cancellable
                        var line = await StdOut.ReadLineAsync(cts.Token).ConfigureAwait(false);
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
                        _ = _syncSemaphore.Release();
                    }
                }, cts.Token);
                _ = readTask.Wait(300);
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
    /// <inheritdoc/>
    #region public async ValueTask DisposeAsync()
    public async ValueTask DisposeAsync()
    {
        _ = await DispatchAsync("break").ConfigureAwait(false);
        await (Process.WaitForExitAsync() ?? Task.CompletedTask).ConfigureAwait(false);
        Process.Dispose();
    }
    #endregion
}
