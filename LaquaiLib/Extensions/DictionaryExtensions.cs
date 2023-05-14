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
        Dictionary<TValue, TKey> ret = new();
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
    {
        var result = new Dictionary<TValue, IEnumerable<TKey>>();

        foreach (var kv in source)
        {
            if (!result.ContainsKey(kv.Value))
            {
                result[kv.Value] = new List<TKey>();
            }

            ((List<TKey>)result[kv.Value]).Add(kv.Key);
        }

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
        where TKey : notnull => source.ToDictionary(kv => kv.Key, kv => kv.Value);
}
