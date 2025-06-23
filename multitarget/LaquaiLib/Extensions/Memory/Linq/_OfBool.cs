namespace LaquaiLib.Extensions.Memory.Linq;

public static partial class LinqMemoryExtensions
{
    extension(ReadOnlySpan<bool> source)
    {
        /// <summary>
        /// Determines whether all values in the source <see cref="ReadOnlySpan{T}"/> are <see langword="true"/>.
        /// </summary>
        /// <returns><see langword="true"/> if all values in the source <see cref="ReadOnlySpan{T}"/> are <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
        public bool All() => source.Length > 0 && source.All(static x => x);
    }
}
