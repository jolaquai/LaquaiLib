namespace LaquaiLib.Extensions;

public static class IEnumerableBoolExtensions
{
    public static bool All(this IEnumerable<bool> source) => source.All(x => x);
}
