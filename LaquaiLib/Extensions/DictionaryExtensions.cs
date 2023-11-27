namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Dictionary{TKey, TValue}"/> Type.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Creates an inverted <see cref="Dictionary{TKey, TValue}"/>, where the original keys are now the values and vice versa.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the original <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the original <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The original <see cref="Dictionary{TKey, TValue}"/>.</param>
    /// <returns>A new <see cref="Dictionary{TKey, TValue}"/> where the keys are the values of the original <see cref="Dictionary{TKey, TValue}"/> and vice versa.</returns>
    public static Dictionary<TValue, TKey> Invert<TKey, TValue>(this Dictionary<TKey, TValue> source)
        where TKey : notnull
        where TValue : notnull
    {
        Dictionary<TValue, TKey> ret = [];
        foreach (var kv in source)
        {
            ret.Add(kv.Value, kv.Key);
        }
        return ret;
    }

    /// <summary>
    /// Creates a content-aware inverse <see cref="Dictionary{TKey, TValue}"/> where the original keys are now values grouped by the original values.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the original <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the original <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The original <see cref="Dictionary{TKey, TValue}"/>.</param>
    /// <returns>An inverted <see cref="Dictionary{TKey, TValue}"/> as described.</returns>
    public static Dictionary<TValue, IEnumerable<TKey>> InvertContentAware<TKey, TValue>(this Dictionary<TKey, TValue> source)
        where TKey : notnull
        where TValue : notnull
    {
        var grouping = source.GroupBy(kv => kv.Value);
        var result = grouping.Aggregate(
            new Dictionary<TValue, IEnumerable<TKey>>(),
            (acc, grouping) =>
            {
                acc.Add(grouping.Key, grouping.Select(x => x.Key));
                return acc;
            }
        );

        return result;
    }

    /// <summary>
    /// Creates a shallow copy of the <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">They Type of the values of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="Dictionary{TKey, TValue}"/> to clone.</param>
    /// <returns>A shallow copy of the <see cref="Dictionary{TKey, TValue}"/>.</returns>
    public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> source)
        where TKey : notnull => source.ToDictionary();
}
