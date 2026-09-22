namespace LaquaiLib.Extensions;

public static partial class LinqMemoryExtensions
{
    extension(in ReadOnlySpan<bool> source)
    {
        /// <summary>
        /// Determines whether all values in the source <see cref="ReadOnlySpan{T}"/> are <see langword="true"/>.
        /// </summary>
        /// <returns><see langword="true"/> if all values in the source <see cref="ReadOnlySpan{T}"/> are <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        public bool All() => source.All(static x => x);
    }
}
