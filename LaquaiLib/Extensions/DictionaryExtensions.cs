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

    /// <summary>
    /// Adds a key/value pair to the <see cref="Dictionary{TKey, TValue}"/> if the key does not already exist. Otherwise, a factory <see cref="Func{T, TResult}"/> that produces a new value is invoked with the existing value.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="Dictionary{TKey, TValue}"/> to add to or update.</param>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="addValue">The value to be added for an absent key.</param>
    /// <param name="updateValueFactory">A factory <see cref="Func{T, TResult}"/> that takes the existing value for a key and produces a new value.</param>
    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue addValue, Func<TValue, TValue> updateValueFactory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(updateValueFactory);

        source[key] = !source.TryGetValue(key, out var value) ? addValue : updateValueFactory(value);
    }
    /// <summary>
    /// Adds a key/value pair to the <see cref="Dictionary{TKey, TValue}"/> if the key does not already exist. Otherwise, a factory <see cref="Func{T1, T2, TResult}"/> that produces a new value is invoked with the existing value and <paramref name="addValue"/>.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="Dictionary{TKey, TValue}"/> to add to or update.</param>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="addValue">The value to be added for an absent key.</param>
    /// <param name="updateValueFactory">A factory <see cref="Func{T1, T2, TResult}"/> that takes the existing value for a key and <paramref name="addValue"/> itself and produces a new value. This avoids having to materialize the value twice.</param>
    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue addValue, Func<TValue, TValue, TValue> updateValueFactory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(updateValueFactory);

        source[key] = !source.TryGetValue(key, out var old) ? addValue : updateValueFactory(old, addValue);
    }
    /// <summary>
    /// Adds a key/value pair to the <see cref="Dictionary{TKey, TValue}"/> where the value is produced by <paramref name="addValueFactory"/> if the key does not already exist. Otherwise, a factory <see cref="Func{T, TResult}"/> that produces a new value is invoked with the existing value.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="Dictionary{TKey, TValue}"/> to add to or update.</param>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="addValueFactory">A <see cref="Func{TResult}"/> that produces the value to be added for an absent key. It is only invoked if the key does not already exist in the <see cref="Dictionary{TKey, TValue}"/>.</param>
    /// <param name="updateValueFactory">A factory <see cref="Func{T1, T2, TResult}"/> that takes the existing value for a key and produces a new value.</param>
    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, Func<TValue> addValueFactory, Func<TValue, TValue> updateValueFactory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(addValueFactory);
        ArgumentNullException.ThrowIfNull(updateValueFactory);

        source[key] = !source.TryGetValue(key, out var old) ? addValueFactory() : updateValueFactory(old);
    }
}
