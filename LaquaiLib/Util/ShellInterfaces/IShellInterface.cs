using System.Diagnostics;

namespace LaquaiLib.Util.ShellInterfaces;

/// <summary>
/// Defines a type that interfaces and handles communication with a shell (such as PowerShell).
/// </summary>
public interface IShellInterface : IAsyncDisposable
{
    /// <summary>
    /// Gets the <see cref="System.Diagnostics.Process"/> component that represents the underlying shell.
    /// </summary>
    protected Process Process { get; }
    /// <summary>
    /// Gets the standard output stream of the underlying PowerShell instance.
    /// </summary>
    StreamReader StdOut { get; }
    /// <summary>
    /// Gets the standard error stream of the underlying PowerShell instance.
    /// </summary>
    StreamReader StdErr { get; }
    /// <summary>
    /// Indicates whether the underlying shell is ready to receive input.
    /// </summary>
    bool Ready { get; }
    /// <summary>
    /// Indicates whether the underlying shell still exists.
    /// </summary>
    bool Exists { get; }
    /// <summary>
    /// Indicates whether the underlying shell supports multiline commands; that is, whether the underlying shell behaves correctly when being passed a multiline command through <see cref="DispatchAsync(string)"/>, OR whether that method preprocesses the input to ensure that the shell behaves correctly. This may include wrapping the input in a block, using a different method to send the input or splitting the input into multiple commands.
    /// </summary>
    bool SupportsMultilineCommands { get; }

    /// <summary>
    /// Sends a string to the standard input of the underlying shell, then closes the input stream to allow the script to execute.
    /// </summary>
    /// <param name="input">The string to send. See remarks for further information.</param>
    /// <returns>A <see cref="Task{TResult}"/> that completes when the command has been dispatched and the output has been received.</returns>
    /// <remarks>
    /// Before sending multiline input, ensure that the shell you are sending to supports this. For example, a multiline PowerShell block must be enclosed in a block: <c>&amp; { }</c>
    /// <para/>
    /// </remarks>
    Task<CommandDispatchResult> DispatchAsync(string input);
    /// <summary>
    /// Requests that the interactive shell represented by this instance terminate at the earliest opportunity.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes when the shell has terminated.</returns>
    Task Close();
    /// <summary>
    /// Returns a <see cref="Task"/> that completes when the shell is ready to receive input.
    /// Note that it may never complete if the shell has been disposed or is uninitialized.
    /// </summary>
    /// <returns>The <see cref="Task"/> as described.</returns>
    Task WhenReady();
}
