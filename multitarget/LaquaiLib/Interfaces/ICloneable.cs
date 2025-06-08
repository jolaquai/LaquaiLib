namespace LaquaiLib.Interfaces;

/// <summary>
/// Implements a typed version of the <see cref="ICloneable"/> interface.
/// </summary>
/// <typeparam name="TSelf">The type of the implementing type.</typeparam>
public interface ICloneable<TSelf> : ICloneable where TSelf : ICloneable<TSelf>
{
    /// <summary>
    /// Clones the current instance.
    /// </summary>
    /// <returns>A new instance of the current type with the same values as the original instance.</returns>
    public new TSelf Clone();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    object ICloneable.Clone() => Clone();
}
