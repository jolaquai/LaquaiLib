
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the Reset event.
/// </summary>
/// <typeparam name="T">The type of the new contents.</typeparam>
public class ResetEventArgs<T>
{
    /// <summary>
    /// Gets the new contents.
    /// </summary>
    public IEnumerable<T> NewContents { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetEventArgs{T}"/> class.
    /// </summary>
    /// <param name="newContents">The new contents.</param>
    public ResetEventArgs(IEnumerable<T> newContents) => NewContents = newContents;
}
