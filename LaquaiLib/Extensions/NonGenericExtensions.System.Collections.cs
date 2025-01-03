using System.Collections;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions methods for non-generic types from the <see cref="System.Collections"/> namespace.
/// </summary>
public static class NonGenericExtensionsSystemCollections
{
    /// <summary>
    /// Treats the specified <see cref="ICollection"/> as an <see cref="ICollection{T}"/> of the specified type.
    /// This is done through unsafe type coercion and should only be used when the type is known to be correct, but in those cases, it can drastically improve performance over a typical cast.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <param name="source">The collection to treat as an <see cref="ICollection{T}"/>.</param>
    /// <returns>The specified <see cref="ICollection"/> typed as an <see cref="ICollection{T}"/> of the specified type.</returns>
    public static ICollection<T> Of<T>(this ICollection source) => System.Runtime.CompilerServices.Unsafe.As<ICollection<T>>(source);
    /// <summary>
    /// Treats the specified <see cref="IComparer"/> as an <see cref="IComparer{T}"/> of the specified type.
    /// This is done through unsafe type coercion and should only be used when the type is known to be correct, but in those cases, it can drastically improve performance over a typical cast.
    /// </summary>
    /// <typeparam name="T">The type of the elements to compare.</typeparam>
    /// <param name="source">The collection to treat as an <see cref="IComparer{T}"/>.</param>
    /// <returns>The specified <see cref="IComparer"/> typed as an <see cref="IComparer{T}"/> of the specified type.</returns>
    public static IComparer<T> Of<T>(this IComparer source) => System.Runtime.CompilerServices.Unsafe.As<IComparer<T>>(source);
    /// <summary>
    /// Treats the specified <see cref="IDictionary"/> as an <see cref="IDictionary{TKey, TValue}"/> of the specified types.
    /// This is done through unsafe type coercion and should only be used when the type is known to be correct, but in those cases, it can drastically improve performance over a typical cast.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="source">The collection to treat as an <see cref="IDictionary{TKey, TValue}"/>.</param>
    /// <returns>The specified <see cref="IDictionary"/> typed as an <see cref="IDictionary{TKey, TValue}"/> of the specified type.</returns>
    public static IDictionary<TKey, TValue> Of<TKey, TValue>(this IDictionary source) => System.Runtime.CompilerServices.Unsafe.As<IDictionary<TKey, TValue>>(source);
    /// <summary>
    /// Treats the specified <see cref="IEnumerable"/> as an <see cref="IEnumerable{T}"/> of the specified type.
    /// This is done through unsafe type coercion and should only be used when the type is known to be correct, but in those cases, it can drastically improve performance over a typical cast.
    /// </summary>
    /// <typeparam name="T">The type of the elements to enumerate.</typeparam>
    /// <param name="source">The collection to treat as an <see cref="IEnumerable{T}"/>.</param>
    /// <returns>The specified <see cref="IEnumerable"/> typed as an <see cref="IEnumerable{T}"/> of the specified type.</returns>
    public static IEnumerable<T> Of<T>(this IEnumerable source) => System.Runtime.CompilerServices.Unsafe.As<IEnumerable<T>>(source);
    /// <summary>
    /// Treats the specified <see cref="IEnumerator"/> as an <see cref="IEnumerator{T}"/> of the specified type.
    /// This is done through unsafe type coercion and should only be used when the type is known to be correct, but in those cases, it can drastically improve performance over a typical cast.
    /// </summary>
    /// <typeparam name="T">The type of the elements to enumerate.</typeparam>
    /// <param name="source">The collection to treat as an <see cref="IEnumerator{T}"/>.</param>
    /// <returns>The specified <see cref="IEnumerator"/> typed as an <see cref="IEnumerator{T}"/> of the specified type.</returns>
    public static IEnumerator<T> Of<T>(this IEnumerator source) => System.Runtime.CompilerServices.Unsafe.As<IEnumerator<T>>(source);
    /// <summary>
    /// Treats the specified <see cref="IEqualityComparer"/> as an <see cref="IEqualityComparer{T}"/> of the specified type.
    /// This is done through unsafe type coercion and should only be used when the type is known to be correct, but in those cases, it can drastically improve performance over a typical cast.
    /// </summary>
    /// <typeparam name="T">The type of the elements to compare.</typeparam>
    /// <param name="source">The collection to treat as an <see cref="IEqualityComparer{T}"/>.</param>
    /// <returns>The specified <see cref="IEqualityComparer"/> typed as an <see cref="IEqualityComparer{T}"/> of the specified type.</returns>
    public static IEqualityComparer<T> Of<T>(this IEqualityComparer source) => System.Runtime.CompilerServices.Unsafe.As<IEqualityComparer<T>>(source);
    /// <summary>
    /// Treats the specified <see cref="IList"/> as an <see cref="IList{T}"/> of the specified type.
    /// This is done through unsafe type coercion and should only be used when the type is known to be correct, but in those cases, it can drastically improve performance over a typical cast.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="source">The collection to treat as an <see cref="IList{T}"/>.</param>
    /// <returns>The specified <see cref="IList"/> typed as an <see cref="IList{T}"/> of the specified type.</returns>
    public static IList<T> Of<T>(this IList source) => System.Runtime.CompilerServices.Unsafe.As<IList<T>>(source);
}
