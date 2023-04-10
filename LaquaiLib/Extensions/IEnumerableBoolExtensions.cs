namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> of <see cref="bool"/> Type.
/// </summary>
public static class IEnumerableBoolExtensions
{
    public static bool All(this IEnumerable<bool> source) => source.All(x => x);
}
