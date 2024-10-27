using System.Runtime.InteropServices;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> Type.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="Array"/>, starting at the specified index.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="Array"/> to copy elements to.</param>
    /// <param name="startIndex">The index in <paramref name="target"/> at which to start copying elements.</param>
    /// <param name="allowUnsafeMutation">Whether the method is allowed to begin mutating the array if it is unable to ascertain whether all elements will fit. Defaults to <see langword="false"/>.</param>
    public static void Into<T>(this IEnumerable<T> source, T[] target, int startIndex = 0, bool allowUnsafeMutation = false)
    {
        switch (source)
        {
            case List<T> other:
            {
                var src = other.AsSpan();
                var length = src.Length;
                if (startIndex + length > target.Length)
                {
                    throw new ArgumentException("The destination array cannot accommodate the source collection.", nameof(target));
                }
                var dest = target.AsSpan(startIndex, length);
                src.CopyTo(dest);
                break;
            }
            case ICollection<T> collection:
            {
                if (startIndex + collection.Count > target.Length)
                {
                    throw new ArgumentException("The destination array cannot accommodate the source collection.", nameof(target));
                }
                collection.CopyTo(target, startIndex);
                break;
            }
            case IReadOnlyList<T> list:
                if (startIndex + list.Count > target.Length)
                {
                    throw new ArgumentException("The destination array cannot accommodate the source collection.", nameof(target));
                }
                for (var i = 0; i < list.Count; i++)
                {
                    target[startIndex++] = list[i];
                }
                break;
            case IReadOnlyCollection<T> coll:
            {
                if (startIndex + coll.Count > target.Length)
                {
                    throw new ArgumentException("The destination array cannot accommodate the source collection.", nameof(target));
                }
                var dest = target.AsSpan(startIndex, coll.Count);
                foreach (var item in source)
                {
                    dest[startIndex++] = item;
                }
                break;
            }
            default:
            {
                if (source.TryGetNonEnumeratedCount(out var length))
                {
                    if (startIndex + length > target.Length)
                    {
                        throw new ArgumentException("The destination array cannot accommodate the source collection.", nameof(target));
                    }
                    var dest = target.AsSpan(startIndex, length);
                    foreach (var item in source)
                    {
                        dest[startIndex++] = item;
                    }
                }
                else if (allowUnsafeMutation)
                {
                    // dangerous fallback since it will mutate the array without knowing whether all elements will fit
                    foreach (var item in source)
                    {
                        target[startIndex++] = item;
                    }
                }
                break;
            }
        }
    }
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="List{T}"/>, starting at the specified index.
    /// The <see cref="List{T}"/> will be resized if the input sequence contains more elements than the <see cref="List{T}"/>'s current capacity.
    /// Efficient <see cref="Span{T}"/>-based copying is employed when possible, otherwise falling back to <see cref="List{T}.AddRange(IEnumerable{T})"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="Array"/> to copy elements to.</param>
    /// <param name="startIndex">The index in <paramref name="target"/> at which to start copying elements.</param>
    public static void Into<T>(this IEnumerable<T> source, List<T> target, int startIndex = 0)
    {
        switch (source)
        {
            case List<T> other:
            {
                var src = other.AsSpan();
                var length = src.Length;
                if (startIndex + length > target.Capacity)
                {
                    CollectionsMarshal.SetCount(target, startIndex + length);
                }
                var dest = target.AsSpan(startIndex, length);
                src.CopyTo(dest);
                break;
            }
            case IReadOnlyList<T> list:
            {
                var length = list.Count;
                if (startIndex + length > target.Capacity)
                {
                    CollectionsMarshal.SetCount(target, startIndex + length);
                }
                for (var i = 0; i < length; i++)
                {
                    target[startIndex++] = list[i];
                }
                break;
            }
            case IReadOnlyCollection<T> collection:
            {
                var length = collection.Count;
                if (startIndex + length > target.Capacity)
                {
                    CollectionsMarshal.SetCount(target, startIndex + length);
                }
                var dest = target.AsSpan(startIndex, length);
                foreach (var item in source)
                {
                    dest[startIndex++] = item;
                }
                break;
            }
            default:
            {
                if (source.TryGetNonEnumeratedCount(out var length))
                {
                    if (startIndex + length > target.Capacity)
                    {
                        CollectionsMarshal.SetCount(target, startIndex + length);
                    }
                    var dest = target.AsSpan(startIndex, length);
                    foreach (var item in source)
                    {
                        dest[startIndex++] = item;
                    }
                }
                else
                {
                    target.AddRange(source);
                }
                break;
            }
        }
    }
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="Span{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="Span{T}"/> to copy elements to.</param>
    /// <param name="allowUnsafeMutation">Whether the method is allowed to begin mutating the span if it is unable to ascertain whether all elements will fit. Defaults to <see langword="false"/>.</param>
    public static void Into<T>(this IEnumerable<T> source, Span<T> target, bool allowUnsafeMutation = false)
    {
        switch (source)
        {
            case List<T> other:
            {
                var src = other.AsSpan();
                var length = src.Length;
                if (length > target.Length)
                {
                    throw new ArgumentException("The destination span cannot accommodate the source collection.", nameof(target));
                }
                src.CopyTo(target);
                break;
            }
            case IReadOnlyList<T> list:
            {
                var length = list.Count;
                if (length > target.Length)
                {
                    throw new ArgumentException("The destination span cannot accommodate the source collection.", nameof(target));
                }
                for (var i = 0; i < length; i++)
                {
                    target[i] = list[i];
                }
                break;
            }
            case IReadOnlyCollection<T> collection:
            {
                var length = collection.Count;
                if (length > target.Length)
                {
                    throw new ArgumentException("The destination span cannot accommodate the source collection.", nameof(target));
                }
                var i = 0;
                foreach (var item in source)
                {
                    target[i++] = item;
                }
                break;
            }
            default:
            {
                if (source.TryGetNonEnumeratedCount(out var length))
                {
                    if (length > target.Length)
                    {
                        throw new ArgumentException("The destination span cannot accommodate the source collection.", nameof(target));
                    }
                    var i = 0;
                    foreach (var item in source)
                    {
                        target[i++] = item;
                    }
                }
                else if (allowUnsafeMutation)
                {
                    // dangerous fallback since it will mutate the array without knowing whether all elements will fit
                    var i = 0;
                    foreach (var item in source)
                    {
                        target[i++] = item;
                    }
                }
                break;
            }
        }
    }
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="Dictionary{TKey, TValue}"/> using the specified <paramref name="valueFactory"/> to generate values for each key.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys in the input sequence.</typeparam>
    /// <typeparam name="TValue">The Type of the values in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="Dictionary{TKey, TValue}"/> to copy elements to.</param>
    /// <param name="valueFactory">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence and produces a value for the corresponding key.</param>
    /// <param name="overwrite">Whether to overwrite existing values in the <paramref name="target"/> dictionary. Defaults to <see langword="false"/>.</param>
    public static void Into<TKey, TValue>(this IEnumerable<TKey> source, Dictionary<TKey, TValue> target, Func<TKey, TValue> valueFactory, bool overwrite = false)
    {
        foreach (var key in source)
        {
            ref var dest = ref CollectionsMarshal.GetValueRefOrAddDefault(target, key, out var exists);
            if (!exists || overwrite)
            {
                dest = valueFactory(key);
            }
        }
    }
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="Dictionary{TKey, TValue}"/> using the specified <paramref name="keySelector"/> to generate keys for each value.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys in the input sequence.</typeparam>
    /// <typeparam name="TValue">The Type of the values in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="Dictionary{TKey, TValue}"/> to copy elements to.</param>
    /// <param name="keySelector">A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence and produces a key for the corresponding value.</param>
    /// <param name="overwrite">Whether to overwrite existing values in the <paramref name="target"/> dictionary. Defaults to <see langword="false"/>.</param>
    public static void Into<TKey, TValue>(this IEnumerable<TValue> source, Dictionary<TKey, TValue> target, Func<TValue, TKey> keySelector, bool overwrite = false)
    {
        foreach (var value in source)
        {
            var key = keySelector(value);
            ref var dest = ref CollectionsMarshal.GetValueRefOrAddDefault(target, key, out var exists);
            if (!exists || overwrite)
            {
                dest = value;
            }
        }
    }
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="ICollection{T}"/>.
    /// Efficient overloads of this method are used, if possible.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="ICollection{T}"/> to copy elements to.</param>
    public static void Into<T>(this IEnumerable<T> source, ICollection<T> target)
    {
        switch (target)
        {
            case T[] arr:
                source.Into(arr);
                break;
            case List<T> list:
                source.Into(list);
                break;
        }

        var targetType = target.GetType();
        if (targetType.GetMethod("AddRange", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) is System.Reflection.MethodInfo method)
        {
            method.Invoke(target, [source]);
            return;
        }

        if (target.IsReadOnly)
        {
            throw new InvalidOperationException("The target collection is read-only.");
        }

        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}
