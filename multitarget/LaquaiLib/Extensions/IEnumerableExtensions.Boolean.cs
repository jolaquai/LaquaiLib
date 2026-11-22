namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    extension(IEnumerable<bool> source)
    {
        /// <summary>
        /// Determines whether all elements of a sequence of <see cref="bool"/> values are true.
        /// </summary>
        /// <returns>A value that indicates whether all elements of the sequence are true.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool All() => source.All(static x => x);
    }
}
