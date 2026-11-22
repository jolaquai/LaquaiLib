namespace LaquaiLib.Util.ShellInterfaces;

/// <summary>
/// Encapsulates a command dispatch result; that is, a command sent to a <see cref="IShellInterface"/> implementation and the output produced by the script that received the command in response to it.
/// </summary>
/// <param name="Input">The input sent to the <see cref="IShellInterface"/> implementation that produced this output.</param>
/// <param name="Output">The output produced by the script that received the <paramref name="Input"/>.</param>
public readonly record struct CommandDispatchResult(string Input, string Output);