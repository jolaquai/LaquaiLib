
namespace LaquaiLib.Collections.Observable;

/// <summary>
/// Represents the event arguments for the Reset event.
/// </summary>
/// <typeparam name="T">The type of the new contents.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ResetEventArgs{T}"/> class.
/// </remarks>
/// <param name="newContents">The new contents.</param>
public class ResetEventArgs<T>(IEnumerable<T> newContents)
{
    /// <summary>
    /// Gets the new contents.
    /// </summary>
    public IEnumerable<T> NewContents { get; } = newContents;
}
