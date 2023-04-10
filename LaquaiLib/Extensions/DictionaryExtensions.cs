namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Dictionary{TKey, TValue}"/> Type.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Creates an inverted <see cref="Dictionary{TKey, TValue}"/>, where the original keys are now the values and vice versa.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
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

    public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> source)
        where TKey : notnull => source.ToDictionary(kv => kv.Key, kv => kv.Value);
}
