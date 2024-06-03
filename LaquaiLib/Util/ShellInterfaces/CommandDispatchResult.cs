namespace LaquaiLib.Util.ShellInterfaces;

/// <summary>
/// Encapsulates a command dispatch result; that is, a command sent to a <see cref="IShellInterface"/> implementation and the output produced by the script that received the command in response to it.
/// </summary>
public record class CommandDispatchResult
{
    /// <summary>
    /// The input sent to the <see cref="IShellInterface"/> implementation that produced this output.
    /// </summary>
    public required string Input { get; init; }
    /// <summary>
    /// The output produced by the script that received the <see cref="Input"/>.
    /// </summary>
    public required string Output { get; init; }
}
