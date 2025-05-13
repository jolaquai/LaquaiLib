namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    extension(IEnumerable<bool> source)
    {
        /// <summary>
        /// Determines whether all elements of a sequence of <see cref="bool"/> values are true.
        /// </summary>
        /// <param name="source">The sequence of <see cref="bool"/> values to check.</param>
        /// <returns>A value that indicates whether all elements of the sequence are true.</returns>
        public bool All() => source.All(static x => x);
    }
}
