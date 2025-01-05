using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

using Microsoft.VisualBasic;

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
            throw new ArgumentException($"The {nameof(IDictionary<,>)} must be mutable.", nameof(source));
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
            throw new ArgumentException($"The {nameof(IDictionary<,>)} must be mutable.", nameof(source));
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
        if (source.IsReadOnly)
        {
            throw new ArgumentException($"The {nameof(IDictionary<,>)} must be mutable.", nameof(source));
        }

        // ConcurrentDictionary has a thread-safe AddOrUpdate method, so special-case it
        if (source is ConcurrentDictionary<TKey, TValue> concurrent)
        {
            concurrent.AddOrUpdate(key, addValue, (k, e) => updateValueFactory(e));
            return;
        }

        if (!source.TryGetValue(key, out var old))
        {
            source[key] = addValue;
        }
        else
        {
            // Validate null only when needed
            ArgumentNullException.ThrowIfNull(updateValueFactory);
            source[key] = updateValueFactory(old);
        }
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
        if (source.IsReadOnly)
        {
            throw new ArgumentException($"The {nameof(IDictionary<,>)} must be mutable.", nameof(source));
        }

        // ConcurrentDictionary has a thread-safe AddOrUpdate method, so special-case it
        if (source is ConcurrentDictionary<TKey, TValue> concurrent)
        {
            concurrent.AddOrUpdate(key, addValue, (k, e) => updateValueFactory(e, addValue));
            return;
        }

        if (!source.TryGetValue(key, out var old))
        {
            source[key] = addValue;
        }
        else
        {
            // Validate null only when needed
            ArgumentNullException.ThrowIfNull(updateValueFactory);
            source[key] = updateValueFactory(old, addValue);
        }
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
        if (source.IsReadOnly)
        {
            throw new ArgumentException($"The {nameof(IDictionary<,>)} must be mutable.", nameof(source));
        }

        // ConcurrentDictionary has a thread-safe AddOrUpdate method, so special-case it
        if (source is ConcurrentDictionary<TKey, TValue> concurrent)
        {
            concurrent.AddOrUpdate(key, k => addValueFactory(), (k, e) => updateValueFactory(e));
            return;
        }

        // Validate null only when needed
        if (!source.TryGetValue(key, out var old))
        {
            ArgumentNullException.ThrowIfNull(addValueFactory);
            source[key] = addValueFactory();
        }
        else
        {
            ArgumentNullException.ThrowIfNull(updateValueFactory);
            source[key] = updateValueFactory(old);
        }
    }

    /// <summary>
    /// Returns a <see langword="ref"/> into the storage of the specified <paramref name="dictionary"/> if the key-value pair was present, otherwise returns a <see langword="null"/> <see langword="ref"/>.
    /// If <paramref name="existed"/> is <see langword="false"/> when control returns to the caller, using the returned <see langword="ref"/> is undefined behavior and will likely result in a <see cref="NullReferenceException"/>.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="dictionary">The <see cref="Dictionary{TKey, TValue}"/> to get the value from.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="existed">An <see langword="out"/> variable that indicates whether the key-value pair was present in the <paramref name="dictionary"/>.</param>
    /// <returns>A <see langword="ref"/> into the storage of the <paramref name="dictionary"/> if the key-value pair was present, otherwise a <see langword="null"/> <see langword="ref"/>.</returns>
    public static ref TValue GetValueRefOrNullRef<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out bool existed)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(key);
        ref var reference = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
        existed = System.Runtime.CompilerServices.Unsafe.IsNullRef(ref reference);
        return ref reference;
    }
    /// <summary>
    /// Checks if the specified <paramref name="dictionary"/> contains a key-value pair with the specified <paramref name="key"/>, adds one with the specified <paramref name="value"/> if not and returns a <see langword="ref"/> into its storage.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="dictionary">The <see cref="Dictionary{TKey, TValue}"/> to get the value from or add to.</param>
    /// <param name="key">The key of the value to get or add.</param>
    /// <param name="value">The value to add to the <paramref name="dictionary"/> if the key does not exist.</param>
    /// <param name="existed">An <see langword="out"/> variable that indicates whether the key-value pair was present in the <paramref name="dictionary"/>.</param>
    /// <returns>A <see langword="ref"/> into the storage of the <paramref name="dictionary"/>.</returns>
    public static ref TValue GetValueRefOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value, out bool existed)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(key);
        ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out existed);
        if (!existed)
        {
            reference = value;
        }
        return ref reference;
    }
    /// <summary>
    /// Checks if the specified <paramref name="dictionary"/> contains a key-value pair with the specified <paramref name="key"/>, adds one with the value produced by <paramref name="valueFactory"/> if not and returns a <see langword="ref"/> into its storage.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The Type of the values of the <see cref="Dictionary{TKey, TValue}"/>.</typeparam>
    /// <param name="dictionary">The <see cref="Dictionary{TKey, TValue}"/> to get the value from or add to.</param>
    /// <param name="key">The key of the value to get or add.</param>
    /// <param name="valueFactory">A factory <see cref="Func{TResult}"/> that produces the value to add to the <paramref name="dictionary"/> if the key does not exist.</param>
    /// <param name="existed">An <see langword="out"/> variable that indicates whether the key-value pair was present in the <paramref name="dictionary"/>.</param>
    /// <returns>A <see langword="ref"/> into the storage of the <paramref name="dictionary"/>.</returns>
    public static ref TValue GetValueRefOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory, out bool existed)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(key);
        ref var reference = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out existed);
        if (!existed)
        {
            reference = valueFactory();
        }
        return ref reference;
    }

    // Expose the Dictionary<,> methods in CollectionsMarshal as extensions
    /// <inheritdoc cref="CollectionsMarshal.GetValueRefOrNullRef{TKey, TValue}(Dictionary{TKey, TValue}, TKey)"/>"
    public static ref TValue GetValueRefOrNullRef<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) => ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
    /// <inheritdoc cref="CollectionsMarshal.GetValueRefOrAddDefault{TKey, TValue}(Dictionary{TKey, TValue}, TKey, out bool)"/>
    public static ref TValue GetValueRefOrNullRef<TKey, TValue, TAlternateKey>(this Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey> dictionary, TAlternateKey key) => ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
    /// <inheritdoc cref="CollectionsMarshal.GetValueRefOrAddDefault{TKey, TValue}(Dictionary{TKey, TValue}, TKey, out bool)"/>
    public static ref TValue GetValueRefOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out bool existed) => ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out existed);
    /// <inheritdoc cref="CollectionsMarshal.GetValueRefOrAddDefault{TKey, TValue}(Dictionary{TKey, TValue}, TKey, out bool)"/>
    public static ref TValue GetValueRefOrAddDefault<TKey, TValue, TAlternateKey>(this Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey> dictionary, TAlternateKey key, out bool existed) => ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out existed);
}
