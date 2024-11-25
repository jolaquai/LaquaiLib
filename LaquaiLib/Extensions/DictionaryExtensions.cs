using System.Collections.Concurrent;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IDictionary{TKey, TValue}"/> Type.
/// </summary>
public static class IDictionaryExtensions
{
    /// <summary>
    /// Creates an inverted <see cref="Dictionary{TKey, TValue}"/>, where the original keys are now the values and vice versa.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the original <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the original <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The original <see cref="Dictionary{TKey, TValue}"/>. Must be mutable.</param>
    /// <returns>A new <see cref="Dictionary{TKey, TValue}"/> where the keys are the values of the original <see cref="Dictionary{TKey, TValue}"/> and vice versa.</returns>
    public static Dictionary<TValue, TKey> Invert<TKey, TValue>(this IDictionary<TKey, TValue> source)
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
    /// <typeparam name="TKey">The Type of the keys of the original <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the original <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The original <see cref="IDictionary{TKey, TValue}"/>. Must be mutable.</param>
    /// <returns>An inverted <see cref="Dictionary{TKey, TValue}"/> as described.</returns>
    public static Dictionary<TValue, IEnumerable<TKey>> InvertContentAware<TKey, TValue>(this IDictionary<TKey, TValue> source)
        where TKey : notnull
        where TValue : notnull
    {
        var grouping = source.GroupBy(static kv => kv.Value);
        return grouping.Aggregate(
            new Dictionary<TValue, IEnumerable<TKey>>(),
            static (acc, grouping) =>
            {
                acc.Add(grouping.Key, grouping.Select(static x => x.Key));
                return acc;
            }
        );
    }
    /// <summary>
    /// Creates a mutable shallow copy of the <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">They Type of the values of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey, TValue}"/> to clone. Must be mutable.</param>
    /// <returns>A shallow copy of the <see cref="IDictionary{TKey, TValue}"/>.</returns>
    public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this IDictionary<TKey, TValue> source)
        where TKey : notnull => source.ToDictionary();

    /// <summary>
    /// Gets the value associated with the specified key or adds a new key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not already exist.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey, TValue}"/> to get the value from or add to. Must be mutable.</param>
    /// <param name="key">The key of the value to get or add.</param>
    /// <param name="addValue">The value to be added for an absent key. If the key is absent, the return value is this value.</param>
    /// <returns>The value associated with the specified key, if the key is found, otherwise <paramref name="addValue"/>.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue addValue)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(key);
        if (source.IsReadOnly)
        {
            throw new ArgumentException($"The {nameof(IDictionary<TKey, TValue>)} must be mutable.", nameof(source));
        }

        // ConcurrentDictionary has a thread-safe GetOrAdd method, so special-case it
        if (source is ConcurrentDictionary<TKey, TValue> concurrent)
        {
            return concurrent.GetOrAdd(key, addValue);
        }

        if (!source.TryGetValue(key, out var v))
        {
            source[key] = addValue;
            return addValue;
        }
        else
        {
            return v;
        }
    }
    /// <summary>
    /// Gets the value associated with the specified key or adds a new key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not already exist.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey, TValue}"/> to get the value from or add to. Must be mutable.</param>
    /// <param name="key">The key of the value to get or add.</param>
    /// <param name="addValueFactory">A factory <see cref="Func{TResult}"/> that produces the value to be added for an absent key. If the key is absent, the return value is the produced value.</param>
    /// <returns>The value associated with the specified key, if the key is found, otherwise the value produced by <paramref name="addValueFactory"/>.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TValue> addValueFactory)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(addValueFactory);
        if (source.IsReadOnly)
        {
            throw new ArgumentException($"The {nameof(IDictionary<TKey, TValue>)} must be mutable.", nameof(source));
        }

        // ConcurrentDictionary has a thread-safe GetOrAdd method, so special-case it
        if (source is ConcurrentDictionary<TKey, TValue> concurrent)
        {
            return concurrent.GetOrAdd(key, k => addValueFactory());
        }

        if (!source.TryGetValue(key, out var v))
        {
            var addValue = addValueFactory();
            source[key] = addValue;
            return addValue;
        }
        else
        {
            return v;
        }
    }
    /// <summary>
    /// Gets the value associated with the specified key or adds a new key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The type of the elements in the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey, TValue}"/> to get the value from or add the value to. Must be mutable.</param>
    /// <param name="key">The key of the value to get or add.</param>
    /// <param name="addValue">The value to add to the <see cref="IDictionary{TKey, TValue}"/> if the key does not exist.</param>
    /// <param name="element">An <see langword="out"/> variable that receives the value associated with the specified key or the added value.</param>
    /// <returns><see langword="true"/> if the key was found in the <see cref="IDictionary{TKey, TValue}"/>, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the <see cref="IDictionary{TKey, TValue}"/> is not mutable.</exception>
    public static bool GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue addValue, out TValue element)
    {
        if (source.IsReadOnly)
        {
            throw new ArgumentException($"{nameof(source)} must be mutable to use {nameof(GetOrAdd)} overloads.");
        }
        if (source.TryGetValue(key, out element))
        {
            return true;
        }

        // ConcurrentDictionary has a thread-safe GetOrAdd method, so special-case it
        if (source is ConcurrentDictionary<TKey, TValue> concurrent)
        {
            element = concurrent.GetOrAdd(key, addValue);
            return false;
        }

        source.Add(key, addValue);
        element = addValue;
        return false;
    }
    /// <summary>
    /// Gets the value associated with the specified key or adds a new key/value pair produced by a factory to the <see cref="IDictionary{TKey, TValue}"/> if the key does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The type of the elements in the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey, TValue}"/> to get the value from or add the value to. Must be mutable.</param>
    /// <param name="key">The key of the value to get or add.</param>
    /// <param name="addValueFactory">A factory <see cref="Func{TResult}"/> that produces the value to add to the <see cref="IDictionary{TKey, TValue}"/> if the key does not exist. This overload is useful when constructing the value is expensive and should only be done when necessary.</param>
    /// <param name="element">An <see langword="out"/> variable that receives the value associated with the specified key or the added value.</param>
    /// <returns><see langword="true"/> if the key was found in the <see cref="IDictionary{TKey, TValue}"/>, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the dictionary is not mutable.</exception>
    public static bool GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TValue> addValueFactory, out TValue element)
    {
        if (source.IsReadOnly)
        {
            throw new ArgumentException($"{nameof(source)} must be mutable to use {nameof(GetOrAdd)} overloads.");
        }
        if (source.TryGetValue(key, out element))
        {
            return true;
        }

        // ConcurrentDictionary has a thread-safe GetOrAdd method, so special-case it
        if (source is ConcurrentDictionary<TKey, TValue> concurrent)
        {
            element = concurrent.GetOrAdd(key, k => addValueFactory());
            return false;
        }

        var newValue = addValueFactory();
        source.Add(key, newValue);
        element = newValue;
        return false;
    }

    /// <summary>
    /// Adds a key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not already exist. Otherwise, a factory <see cref="Func{T, TResult}"/> that produces a new value is invoked with the existing value.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey, TValue}"/> to add to or update. Must be mutable.</param>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="addValue">The value to be added for an absent key.</param>
    /// <param name="updateValueFactory">A factory <see cref="Func{T, TResult}"/> that takes the existing value for a key and produces a new value.</param>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue addValue, Func<TValue, TValue> updateValueFactory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(updateValueFactory);
        if (source.IsReadOnly)
        {
            throw new ArgumentException($"The {nameof(IDictionary<TKey, TValue>)} must be mutable.", nameof(source));
        }

        // ConcurrentDictionary has a thread-safe AddOrUpdate method, so special-case it
        if (source is ConcurrentDictionary<TKey, TValue> concurrent)
        {
            concurrent.AddOrUpdate(key, addValue, (k, e) => updateValueFactory(e));
        }

        source[key] = !source.TryGetValue(key, out var old) ? addValue : updateValueFactory(old);
    }
    /// <summary>
    /// Adds a key/value pair to the <see cref="IDictionary{TKey, TValue}"/> if the key does not already exist. Otherwise, a factory <see cref="Func{T1, T2, TResult}"/> that produces a new value is invoked with the existing value and <paramref name="addValue"/>.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey, TValue}"/> to add to or update. Must be mutable.</param>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="addValue">The value to be added for an absent key.</param>
    /// <param name="updateValueFactory">A factory <see cref="Func{T1, T2, TResult}"/> that takes the existing value for a key and <paramref name="addValue"/> itself and produces a new value. This avoids having to materialize the value twice.</param>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue addValue, Func<TValue, TValue, TValue> updateValueFactory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(updateValueFactory);
        if (source.IsReadOnly)
        {
            throw new ArgumentException($"The {nameof(IDictionary<TKey, TValue>)} must be mutable.", nameof(source));
        }

        // ConcurrentDictionary has a thread-safe AddOrUpdate method, so special-case it
        if (source is ConcurrentDictionary<TKey, TValue> concurrent)
        {
            concurrent.AddOrUpdate(key, addValue, (k, e) => updateValueFactory(e, addValue));
        }

        source[key] = !source.TryGetValue(key, out var old) ? addValue : updateValueFactory(old, addValue);
    }
    /// <summary>
    /// Adds a key/value pair to the <see cref="IDictionary{TKey, TValue}"/> where the value is produced by <paramref name="addValueFactory"/> if the key does not already exist. Otherwise, a factory <see cref="Func{T, TResult}"/> that produces a new value is invoked with the existing value.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="source">The <see cref="IDictionary{TKey, TValue}"/> to add to or update. Must be mutable.</param>
    /// <param name="key">The key of the element to add or update.</param>
    /// <param name="addValueFactory">A <see cref="Func{TResult}"/> that produces the value to be added for an absent key. It is only invoked if the key does not already exist in the <see cref="IDictionary{TKey, TValue}"/>.</param>
    /// <param name="updateValueFactory">A factory <see cref="Func{T1, T2, TResult}"/> that takes the existing value for a key and produces a new value.</param>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TValue> addValueFactory, Func<TValue, TValue> updateValueFactory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(addValueFactory);
        ArgumentNullException.ThrowIfNull(updateValueFactory);
        if (source.IsReadOnly)
        {
            throw new ArgumentException($"The {nameof(IDictionary<TKey, TValue>)} must be mutable.", nameof(source));
        }

        // ConcurrentDictionary has a thread-safe AddOrUpdate method, so special-case it
        if (source is ConcurrentDictionary<TKey, TValue> concurrent)
        {
            concurrent.AddOrUpdate(key, k => addValueFactory(), (k, e) => updateValueFactory(e));
        }

        source[key] = !source.TryGetValue(key, out var old) ? addValueFactory() : updateValueFactory(old);
    }
}
