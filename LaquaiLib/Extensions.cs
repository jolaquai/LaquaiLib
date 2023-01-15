using System.IO;
using System.Windows.Interop;
using System.Windows.Media;

namespace LaquaiLib.Extensions;

public static class IEnumerableTExtensions
{
    public static IEnumerable<T> Select<T>(this IEnumerable<T> source) => source.Select(item => item);

    /// <summary>
    /// Returns the first element from a sequence and removes it from the source sequence.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    /// <returns>An object of type  that contains the specified range of elements from the source sequence.</returns>
    public static T Consume<T>(this IEnumerable<T> source)
    {
        T extracted = source.First();
        List<T> newList = source.ToList();
        newList.Remove(extracted);
        source = newList;
        return extracted;
    }

    /// <summary>
    /// Returns a specified number of contiguous elements from the start of a sequence and removes them from the source sequence.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    /// <param name="count">The number of elements to return.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the specified number of elements from the start of the input sequence.</returns>
    public static IEnumerable<T> Consume<T>(this IEnumerable<T> source, int count)
    {
        IEnumerable<T> extracted = source.Take(count);
        source = source.Except(extracted);
        return extracted;
    }

    /// <summary>
    /// Returns a specified range of contiguous elements from a sequence and removes them from the source sequence.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    /// <param name="range">The range of elements to return, which has start and end indexes either from the beginning or the end of the sequence.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the specified range of elements from the source sequence.</returns>
    public static IEnumerable<T> Consume<T>(this IEnumerable<T> source, Range range)
    {
        IEnumerable<T> extracted = source.Take(range);
        source = source.Except(extracted);
        return extracted;
    }
}

public static class IEnumerableBoolExtensions
{
    public static bool All(this IEnumerable<bool> source) => source.All(x => x);
}

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

public static class StreamExtensions
{
    /// <summary>
    /// Reads all characters from the current position to the end of the stream.
    /// </summary>
    /// <returns>The rest of the stream as a String, from the current position to the end.</returns>
    public static string ReadToEnd(this Stream stream)
    {
        using StreamReader sr = new(stream);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.
    /// </summary>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public static async Task<string> ReadToEndAsync(this Stream stream)
    {
        using StreamReader sr = new(stream);
        return await sr.ReadToEndAsync();
    }

    /// <summary>
    /// Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public static async Task<string> ReadToEndAsync(this Stream stream, CancellationToken cancellationToken)
    {
        using StreamReader sr = new(stream);
        return await sr.ReadToEndAsync(cancellationToken);
    }
}

public static class IconExtensions
{
    /// <summary>
    /// Converts an icon to an <see cref="ImageSource"/> object.
    /// </summary>
    /// <param name="icon"></param>
    /// <returns></returns>
    public static ImageSource ToImageSource(this Icon icon) => Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
}

public static class GenericExtensions
{

}

public static class StringExtensions
{
    public static string Repeat(this string source, int times) => string.Join("", Enumerable.Repeat(source, times));

    public static string Replace(this string source, IEnumerable<string> finds, string replace)
    {
        string input = source;
        foreach (string find in finds)
        {
            input = input.Replace(find, replace);
        }
        return input;
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of the specified Unicode character in this string.
    /// </summary>
    /// <param name="search">A Unicode character to seek.</param>
    /// <returns>All zero-based index positions of <paramref name="search"/> if that character is found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOf(this string source, char search)
    {
        int find = source.IndexOf(search);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of the specified Unicode character in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="search">A Unicode character to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>All zero-based index positions of <paramref name="search"/> if that character is found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOf(this string source, char search, int startIndex)
    {
        int find = source.IndexOf(search, startIndex);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of the specified string in this instance.
    /// </summary>
    /// <param name="search">The string to seek.</param>
    /// <returns>All zero-based index positions of <paramref name="search"/> if that string is found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOf(this string source, string search)
    {
        int find = source.IndexOf(search);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of the specified string in this instance. The search starts at a specified character position.
    /// </summary>
    /// <param name="search">The string to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>All zero-based index positions of <paramref name="search"/> if that string is found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOf(this string source, string search, int startIndex)
    {
        int find = source.IndexOf(search, startIndex);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence in this instance of any string in a specified sequence of strings.
    /// </summary>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any string in <paramref name="searches"/> was found; -1 if no string in <paramref name="searches"/> was found.</returns>
    public static int IndexOfAny(this string source, IEnumerable<string> searches)
    {
        foreach (string search in searches)
        {
            return source.IndexOf(search);
        }
        return -1;
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence in this instance of any string in a specified sequence of strings. The search starts at a specified character position.
    /// </summary>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any string in <paramref name="searches"/> was found; -1 if no string in <paramref name="searches"/> was found.</returns>
    public static int IndexOfAny(this string source, IEnumerable<string> searches, int startIndex)
    {
        foreach (string search in searches)
        {
            return source.IndexOf(search, startIndex);
        }
        return -1;
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences in this instance of any Unicode character in a specified sequence of characters.
    /// </summary>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any character in <paramref name="searches"/> was found; an empty collection if no character in <paramref name="searches"/> was found.</returns>
    public static IEnumerable<int> IndicesOfAny(this string source, IEnumerable<char> searches)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (char search in searches)
        {
            indexLists.Add(source.IndicesOf(search));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences in this instance of any Unicode character in a specified sequence of characters. The search starts at a specified character position.
    /// </summary>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any character in <paramref name="searches"/> was found; an empty collection if no character in <paramref name="searches"/> was found.</returns>
    public static IEnumerable<int> IndicesOfAny(this string source, IEnumerable<char> searches, int startIndex)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (char search in searches)
        {
            indexLists.Add(source.IndicesOf(search, startIndex));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences in this instance of any string in a specified sequence of strings.
    /// </summary>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any string in <paramref name="searches"/> was found; an empty collection if no string in <paramref name="searches"/> was found.</returns>
    public static IEnumerable<int> IndicesOfAny(this string source, IEnumerable<string> searches)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (string search in searches)
        {
            indexLists.Add(source.IndicesOf(search));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences in this instance of any string in a specified sequence of strings. The search starts at a specified character position.
    /// </summary>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any string in <paramref name="searches"/> was found; an empty collection if no string in <paramref name="searches"/> was found.</returns>
    public static IEnumerable<int> IndicesOfAny(this string source, IEnumerable<string> searches, int startIndex)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (string search in searches)
        {
            indexLists.Add(source.IndicesOf(search, startIndex));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Applies a <paramref name="transform"/> function to each line of a string.
    /// </summary>
    /// <param name="transform">The function used to transform each line of the input string.</param>
    /// <returns>The transformed string.</returns>
    public static string ForEachLine(this string source, Func<string, string> transform) => string.Join(Environment.NewLine, source.Split(Environment.NewLine).Select(line => transform(line)));
    /// <summary>
    /// Applies a <paramref name="transform"/> function to each line of a string, incorporating each line's index in the function.
    /// </summary>
    /// <param name="transform">The function used to transform each line of the input string.</param>
    /// <returns>The transformed string.</returns>
    public static string ForEachLine(this string source, Func<string, int, string> transform) => string.Join(Environment.NewLine, source.Split(Environment.NewLine).Select((line, index) => transform(line, index)));
    /// <summary>
    /// Applies a <paramref name="transform"/> function to each line of a string that satisfies conditions defined by <paramref name="predicate"/>. Lines that do not satisfy this condition are copied without applying <paramref name="transform"/>.
    /// </summary>
    /// <param name="transform">The function used to transform each line of the input string.</param>
    /// <param name="predicate">The function used to determine which lines are transformed using <paramref name="transform"/>.</param>
    /// <returns></returns>
    public static string ForEachLine(this string source, Func<string, string> transform, Func<string, bool> predicate) => string.Join(Environment.NewLine, source.Split(Environment.NewLine).Select(line => predicate(line) ? transform(line) : line));
    /// <summary>
    /// Applies a <paramref name="transform"/> function to each line of a string that satisfies conditions defined by <paramref name="predicate"/>, incorporating each line's index in the functions. Lines that do not satisfy this condition are copied without applying <paramref name="transform"/>.
    /// </summary>
    /// <param name="transform">The function used to transform each line of the input string.</param>
    /// <param name="predicate">The function used to determine which lines are transformed using <paramref name="transform"/>.</param>
    /// <returns></returns>
    public static string ForEachLine(this string source, Func<string, int, string> transform, Func<string, int, bool> predicate) => string.Join(Environment.NewLine, source.Split(Environment.NewLine).Select((line, index) => predicate(line, index) ? transform(line, index) : line));
}
