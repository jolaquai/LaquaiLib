namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for <see cref="ICloneable"/>.
/// </summary>
public static class ICloneableExtensions
{
    extension<T>(T cloneable) where T : ICloneable
    {
        /// <summary>
        /// Creates a copy of the object using its <see cref="ICloneable"/> implementation.
        /// Forces boxing for value type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The copied <see langword="object"/>.</returns>
        public T Copy()
        {
            var clone = cloneable.Clone();
            // no ref return
            return Unsafe.As<object, T>(ref clone);
        }
    }
}
