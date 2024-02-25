namespace LaquaiLib.Interfaces;

/// <summary>
/// Implements a typed version of the <see cref="ICloneable"/> interface.
/// </summary>
/// <typeparam name="TSelf">The type of the implementing class.</typeparam>
public interface ICloneable<TSelf> :
    ICloneable
    where TSelf : ICloneable<TSelf>
{
    /// <summary>
    /// Clones the current instance.
    /// </summary>
    /// <returns>A new instance of the current class with the same values as the original instance.</returns>
    public TSelf Clone();

    object ICloneable.Clone() => Clone();
}
