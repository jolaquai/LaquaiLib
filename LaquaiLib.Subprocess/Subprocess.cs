using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LaquaiLib.Subprocess;

/// <summary>
/// Represents a <see cref="Task"/>-like object that offloads work into a separate process.
/// </summary>
public class Subprocess
{
    // While the design of this class is inspired by Task, there are obviously numerous drawbacks.
    // The biggest one is that delegates cannot be marshalled into a separate process, meaning loading the
    // calling assembly into the process is necessary. This means performance will be rather poor.
    // Additionally, methods must be static and need their parameters to be serializable.

    #region INotifyCompletion (await just delegates to _task)
    public TaskAwaiter<int> GetAwaiter() => _task.GetAwaiter();
    public ConfiguredTaskAwaitable<int> ConfigureAwait(bool continueOnCapturedContext) => _task.ConfigureAwait(continueOnCapturedContext);
    #endregion

    static Subprocess()
    {
        // Initialize by unpacking the prepared worker executable from our own resources
        var tempPath = Path.Combine(Path.GetTempPath(), "LaquaiLib.Subprocess.exe");
        if (!File.Exists(tempPath))
        {
            using (var file = File.Create(tempPath))
            {
                file.Write(RuntimeResources.SatelliteExecutable);
            }
        }
    }

    public static ISubprocess Run(SubprocessWork work)
    {

    }
}

/// <summary>
/// Represents a method that can be executed in a subprocess.
/// </summary>
/// <param name="args">Arbitrary data to pass to the subprocess as command-line arguments. May be <see langword="null"/>.</param>
/// <returns>A <see cref="Task{TResult}"/> that represents the work to be done in the subprocess. It resolves to the exit code of the subprocess.</returns>
public delegate Task<int> SubprocessWork(object[] args);

/// <summary>
/// Specifies the state an <see cref="ISubprocess"/> implementation is in.
/// </summary>
public enum SubprocessStatus
{
    /// <summary>
    /// Specifies that the subprocess has been created but has not yet started.
    /// </summary>
    Created,
    /// <summary>
    /// Specifies that starting the subprocess has been requested, but it is not yet running.
    /// </summary>
    WaitingToRun,
    /// <summary>
    /// Specifies that the subprocess is running.
    /// </summary>
    Running,
    /// <summary>
    /// Specifies that the subprocess has completed successfully.
    /// </summary>
    RanToCompletion,
    /// <summary>
    /// Specifies that the subprocess has been canceled.
    /// </summary>
    Canceled,
    /// <summary>
    /// Specifies that the subprocess has been terminated forcefully (that is, not through cooperative cancellation, but by terminating the process).
    /// </summary>
    ForceTerminated,
    /// <summary>
    /// Specifies that the subprocess has faulted (that is, exited with an exit code unequal to its <see cref="ISubprocess.SuccessExitCode"/>).
    /// </summary>
    Faulted,
}

/// <summary>
/// Represents work that is run in a subprocess and the information required to do so.
/// </summary>
public interface ISubprocess
{
    /// <summary>
    /// Gets an <see cref="Expression{TDelegate}"/> that represents the work to be done in the subprocess.
    /// </summary>
    public SubprocessWork Work { get; }
    /// <summary>
    /// Gets a dictionary that maps from process exit codes to human-readable descriptions.
    /// If the subprocess exits and returns a code that is not equal to <see cref="SuccessExitCode"/>, an exception is thrown using the exit code and the provided description.
    /// </summary>
    public IReadOnlyDictionary<int, string> ExitCodes { get; }
    /// <summary>
    /// The exit code returned by the subprocess that indicates success.
    /// </summary>
    public int SuccessExitCode { get; }
    /// <summary>
    /// Gets a <see cref="SubprocessStatus"/> value that indicates the current status of the subprocess.
    /// </summary>
    public SubprocessStatus Status { get; }
}

/// <summary>
/// Abstractly mplements a <see cref="ISubprocess"/> that does not return an exit code.
/// All exits by this subprocess are considered successful.
/// If your work needs to convey information about the success of the work, implement <see cref="ISubprocess"/> directly.
/// </summary>
public abstract class SubprocessBase : ISubprocess
{
    SubprocessWork ISubprocess.Work { get; }

    public abstract Func<object[], Task> Work { get; }
    public IReadOnlyDictionary<int, string> ExitCodes { get; } = new Dictionary<int, string>()
    {
        { 0, "" }
    };
    public int SuccessExitCode { get; } = 0;
}
