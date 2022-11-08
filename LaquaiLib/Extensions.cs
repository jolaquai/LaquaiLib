using System.Runtime.CompilerServices;

namespace LaquaiLib
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TResult> Cast<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> caster)
        {
            foreach (TSource item in source)
            {
                yield return caster(item);
            }
        }

        public static IEnumerable<TResult> Cast<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> caster, Func<TSource, bool> predicate) => Cast(source.Where(predicate), caster);
        
        public static string Join<T>(this IEnumerable<T> source) => source.Aggregate("", (seed, item) => seed += item!.ToString());
        public static string Join<T>(this IEnumerable<T> source, string separator) => source.Aggregate("", (seed, item) => seed += item!.ToString() + separator, seed => seed[..^separator.Length]);

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            Random ran = new();
            List<T> input = source.ToList();
            List<int> indicesLeft = Miscellaneous.Range(0, source.Count() - 1).ToList();
            while (indicesLeft.Any())
            {
                int r = ran.Next(indicesLeft.Count);
                int index = indicesLeft[r];
                indicesLeft.RemoveAt(r);
                yield return input[index];
            }
        }
    }

    public static class IEnumerableBoolExtensions
    {
        public static bool All(this IEnumerable<bool> source) => source.All(x => x);
    }
}