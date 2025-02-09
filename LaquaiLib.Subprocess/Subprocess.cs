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
}
