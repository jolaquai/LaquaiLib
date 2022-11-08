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
        
        public static IEnumerable<TResult> Cast<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> caster, Func<TSource, bool> predicate)
        {
            foreach (TSource item in source.Where(predicate))
            {
                yield return caster(item);
            }
        }
    }
}