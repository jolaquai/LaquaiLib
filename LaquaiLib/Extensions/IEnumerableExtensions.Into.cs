using System.Runtime.InteropServices;

namespace LaquaiLib.Extensions;

public partial class IEnumerableExtensions
{
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="Array"/>, starting at the specified
    /// index.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="Array"/> to copy elements to.</param>
    /// <param name="startIndex">The index in <paramref name="target"/> at which to start copying elements.</param>
    /// <param name="allowUnsafeMutation">
    /// Whether the method is allowed to begin mutating the array if it is unable to ascertain whether all elements will fit. Defaults to <see langword="false"/>.
    /// </param>
    /// <returns>The number of elements written to the target collection.</returns>
    public static int Into<T>(this IEnumerable<T> source, T[] target, int startIndex = 0, bool allowUnsafeMutation = false)
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
                return src.Length;
            }
            case ICollection<T> collection:
            {
                if (startIndex + collection.Count > target.Length)
                {
                    throw new ArgumentException("The destination array cannot accommodate the source collection.", nameof(target));
                }
                collection.CopyTo(target, startIndex);
                return collection.Count;
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
                return list.Count;
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
                return dest.Length;
            }
            default:
            {
                var start = startIndex;
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
                return startIndex - start;
            }
        }
    }
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="List{T}"/>, starting at the specified
    /// index. The <see cref="List{T}"/> will be resized if the input sequence contains more elements than the <see
    /// cref="List{T}"/>'s current capacity. Efficient <see cref="Span{T}"/>-based copying is employed when possible,
    /// otherwise falling back to <see cref="List{T}.AddRange(IEnumerable{T})"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="List{T}"/> to copy elements to.</param>
    /// <param name="startIndex">The index in <paramref name="target"/> at which to start copying elements.</param>
    /// <returns>The number of elements written to the target collection.</returns>
    /// <remarks>
    /// <paramref name="startIndex"/> defaults to <c>0</c>, meaning items will be overwritten from the beginning of the <see cref="List{T}"/>. To force appending them, explicitly pass the current <see cref="List{T}.Count"/> of <paramref name="target"/>.
    /// <para/>Note that this method inherently exposes unsafe behavior, such as results from setting <paramref name="startIndex"/> to a value greater than the current <see cref="List{T}.Count"/>. Unassigned elements are left in an undefined state.
    /// </remarks>
    public static int Into<T>(this IEnumerable<T> source, List<T> target, int startIndex = 0)
    {
        switch (source)
        {
            case List<T> other:
            {
                var src = other.AsSpan();
                var length = src.Length;
                if (startIndex + length > target.Count)
                {
                    CollectionsMarshal.SetCount(target, startIndex + length);
                }
                var dest = target.AsSpan(startIndex, length);
                src.CopyTo(dest);
                return length;
            }
            case IReadOnlyList<T> list:
            {
                var length = list.Count;
                if (startIndex + length > target.Count)
                {
                    CollectionsMarshal.SetCount(target, startIndex + length);
                }
                for (var i = 0; i < length; i++)
                {
                    target[startIndex++] = list[i];
                }
                return length;
            }
            case IReadOnlyCollection<T> collection:
            {
                var length = collection.Count;
                if (startIndex + length > target.Count)
                {
                    CollectionsMarshal.SetCount(target, startIndex + length);
                }
                var dest = target.AsSpan(startIndex, length);
                foreach (var item in source)
                {
                    dest[startIndex++] = item;
                }
                return length;
            }
            default:
            {
                if (source.TryGetNonEnumeratedCount(out var length))
                {
                    if (startIndex + length > target.Count)
                    {
                        CollectionsMarshal.SetCount(target, startIndex + length);
                    }
                    var dest = target.AsSpan(startIndex, length);
                    var start = startIndex;
                    foreach (var item in source)
                    {
                        dest[startIndex++] = item;
                    }
                    return startIndex - start;
                }
                else
                {
                    var start = target.Count;
                    target.AddRange(source);
                    return target.Count - start;
                }
            }
        }
    }
    /// <summary>
    /// Adds the elements of the input sequence to the end of the specified <see cref="List{T}"/>. The <see cref="List{T}"/> will be resized if the input sequence contains more elements than the <see cref="List{T}"/>'s current capacity (beyond its count). Efficient <see cref="Span{T}"/>-based copying is employed when possible, otherwise falling back to <see cref="List{T}.AddRange(IEnumerable{T})"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="List{T}"/> to copy elements to.</param>
    /// <returns>The number of elements written to the target collection.</returns>
    public static int AddTo<T>(this IEnumerable<T> source, List<T> target) => source.Into(target, target.Count);
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="Span{T}"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="Span{T}"/> to copy elements to.</param>
    /// <param name="startIndex">The index in <paramref name="target"/> at which to start copying elements.</param>
    /// <param name="allowUnsafeMutation">
    /// Whether the method is allowed to begin mutating the span if it is unable to ascertain whether all elements will
    /// fit. Defaults to <see langword="false"/>.
    /// </param>
    /// <returns>The number of elements written to the target collection.</returns>
    public static int Into<T>(this IEnumerable<T> source, Span<T> target, int startIndex = 0, bool allowUnsafeMutation = false)
    {
        if (startIndex > 0)
        {
            target = target[startIndex..];
        }
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
                return length;
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
                return length;
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
                return length;
            }
            default:
            {
                var i = 0;
                if (source.TryGetNonEnumeratedCount(out var length))
                {
                    if (length > target.Length)
                    {
                        throw new ArgumentException("The destination span cannot accommodate the source collection.", nameof(target));
                    }
                    foreach (var item in source)
                    {
                        target[i++] = item;
                    }
                }
                else if (allowUnsafeMutation)
                {
                    // dangerous fallback since it will mutate the array without knowing whether all elements will fit
                    foreach (var item in source)
                    {
                        target[i++] = item;
                    }
                }
                return i;
            }
        }
    }
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="Dictionary{TKey, TValue}"/> using the
    /// specified <paramref name="valueFactory"/> to generate values for each key.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys in the input sequence.</typeparam>
    /// <typeparam name="TValue">The Type of the values in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="Dictionary{TKey, TValue}"/> to copy elements to.</param>
    /// <param name="valueFactory">
    /// A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence and produces a value for the
    /// corresponding key.
    /// </param>
    /// <param name="overwrite">
    /// Whether to overwrite existing values in the <paramref name="target"/> dictionary. Defaults to <see
    /// langword="false"/>.
    /// </param>
    /// <returns>The number of elements written to the target collection.</returns>
    public static int Into<TKey, TValue>(this IEnumerable<TKey> source, IDictionary<TKey, TValue> target, Func<TKey, TValue> valueFactory, bool overwrite = false)
    {
        var i = 0;
        if (target is Dictionary<TKey, TValue> concreteDict)
        {
            foreach (var key in source)
            {
                ref var dest = ref CollectionsMarshal.GetValueRefOrAddDefault(concreteDict, key, out var exists);
                if (overwrite || !exists)
                {
                    i++;
                    dest = valueFactory(key);
                }
                else if (exists)
                {
                    throw new InvalidOperationException($"The target dictionary already contains a value for the key '{key}' and 'overwrite' is set to false.");
                }
            }
        }
        else
        {
            foreach (var key in source)
            {
                var exists = target.ContainsKey(key);
                if (overwrite || !exists)
                {
                    i++;
                    target[key] = valueFactory(key);
                }
                else if (exists)
                {
                    throw new InvalidOperationException($"The target dictionary already contains a value for the key '{key}' and 'overwrite' is set to false.");
                }
            }
        }
        return i;
    }
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="Dictionary{TKey, TValue}"/> using the
    /// specified <paramref name="keyFactory"/> to generate keys for each value.
    /// </summary>
    /// <typeparam name="TKey">The Type of the keys in the input sequence.</typeparam>
    /// <typeparam name="TValue">The Type of the values in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="Dictionary{TKey, TValue}"/> to copy elements to.</param>
    /// <param name="keyFactory">
    /// A <see cref="Func{T, TResult}"/> that is passed each element of the input sequence and produces a key for the
    /// corresponding value.
    /// </param>
    /// <param name="overwrite">
    /// Whether to overwrite existing values in the <paramref name="target"/> dictionary. Defaults to <see
    /// langword="false"/>.
    /// </param>
    /// <returns>The number of elements written to the target collection.</returns>
    public static int Into<TKey, TValue>(this IEnumerable<TValue> source, IDictionary<TKey, TValue> target, Func<TValue, TKey> keyFactory, bool overwrite = false)
    {
        var i = 0;
        if (target is Dictionary<TKey, TValue> concreteDict)
        {
            foreach (var value in source)
            {
                var key = keyFactory(value);
                ref var dest = ref CollectionsMarshal.GetValueRefOrAddDefault(concreteDict, key, out var exists);
                if (overwrite || !exists)
                {
                    i++;
                    dest = value;
                }
                else if (exists)
                {
                    throw new InvalidOperationException($"The target dictionary already contains the key-value pair {{{key}, {value}}} and 'overwrite' is set to false.");
                }
            }
        }
        else
        {
            foreach (var value in source)
            {
                var key = keyFactory(value);
                var exists = target.ContainsKey(key);
                if (overwrite || !exists)
                {
                    i++;
                    target[key] = value;
                }
                else if (exists)
                {
                    throw new InvalidOperationException($"The target dictionary already contains the key-value pair {{{key}, {value}}} and 'overwrite' is set to false.");
                }
            }
        }
        return i;
    }
    /// <summary>
    /// Copies the elements of the input sequence into the specified <see cref="ICollection{T}"/>. Efficient overloads
    /// of this method are used, if possible.
    /// </summary>
    /// <typeparam name="T">The Type of the elements in the input sequence.</typeparam>
    /// <param name="source">The sequence to copy elements from.</param>
    /// <param name="target">The <see cref="ICollection{T}"/> to copy elements to.</param>
    /// <returns>The number of elements written to the target collection.</returns>
    public static int Into<T>(this IEnumerable<T> source, ICollection<T> target)
    {
        switch (target)
        {
            case T[] arr:
                return source.Into(arr);
            case List<T> list:
                return source.Into(list);
        }

        var targetType = target.GetType();
        if (targetType.GetMethod("AddRange", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) is System.Reflection.MethodInfo method)
        {
            // Assumes sane behavior of that method
            var count = target.Count;
            method.Invoke(target, [source]);
            return target.Count - count;
        }

        if (target.IsReadOnly)
        {
            throw new InvalidOperationException("The target collection is read-only.");
        }

        var i = 0;
        foreach (var item in source)
        {
            i++;
            target.Add(item);
        }
        return i;
    }
}
