using System.IO;
using System.Windows.Interop;
using System.Windows.Media;

namespace LaquaiLib;

public static class IEnumerableExtensions
{
    public static string Join<T>(this IEnumerable<T> source) => source.Aggregate("", (seed, item) => seed += item!.ToString());

    public static string Join<T>(this IEnumerable<T> source, string separator) => source.Aggregate("", (seed, item) => seed += item!.ToString() + separator, seed => seed[..^separator.Length]);

    public static IEnumerable<T> Select<T>(this IEnumerable<T> source) => source.Select(item => item);

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        Random ran = new();
        List<T> input = source.ToList();
        List<int> indicesLeft = LaquaiLib.Range(source.Count() - 1).ToList();
        while (indicesLeft.Any())
        {
            int r = ran.Next(indicesLeft.Count);
            int index = indicesLeft[r];
            indicesLeft.RemoveAt(r);
            yield return input[index];
        }
    }

    public static IEnumerable<T> FromTo<T>(this IEnumerable<T> source, int from, int to) => source.Where((_, i) => i >= from && i <= to);
}

public static class IEnumerableBoolExtensions
{
    public static bool All(this IEnumerable<bool> source) => source.All(x => x);
}

public static class ArrayExtensions
{
    public static void Swap<T>(this T[] source, int i, int j) => (source[j], source[i]) = (source[i], source[j]);
}

public static class DictionaryExtensions
{
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
    /// Reads all bytes available until the end of the stream.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<byte> ReadToEnd(this Stream source)
    {
        int read = -2;
        while (read != -1)
        {
            read = source.ReadByte();
            yield return (byte)read;
        }
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

public static class ObjectExtensions
{
    public static IEnumerable<object> Repeat(this object source, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return source;
        }
    }
}

public static class StringExtensions
{
    public static IEnumerable<char> SplitEach(this string source) => source.Select();

    public static string Shift(this string source)
    {
        string input = source;
        string ret = source + "\r\n";
        int i = 1;
        while (i < source.Length)
        {
            char[] split = input.SplitEach().ToArray();
            input = split.FromTo(1, split.Length).Join() + split[0];
            ret += input + "\r\n";
            i++;
        }
        return ret;
    }

    public static string Repeat(this string source, int times)
    {
        string repeat = "";
        for (int i = 0; i < times; i++)
        {
            repeat += source;
        }
        return repeat;
    }

    public static string Replace(this string source, IEnumerable<string> finds, string replace)
    {
        string input = source;
        foreach (string find in finds)
        {
            input = input.Replace(find, replace);
        }
        return input;
    }

    public static IEnumerable<int> IndicesOf(this string source, char search)
    {
        int find = source.IndexOf(search);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    public static IEnumerable<int> IndicesOf(this string source, string search)
    {
        int find = source.IndexOf(search);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    public static IEnumerable<int> IndexOfAny(this string source, IEnumerable<string> searches)
    {
        List<int> indices = new();
        foreach (string search in searches)
        {
            indices.Add(source.IndexOf(search));
        }
        return indices.Distinct().Order();
    }

    public static IEnumerable<int> IndexOfAny(this string source, IEnumerable<string> searches, int startIndex)
    {
        List<int> indices = new();
        foreach (string search in searches)
        {
            indices.Add(source.IndexOf(search, startIndex));
        }
        return indices.Distinct().Order();
    }

    public static IEnumerable<int> IndicesOfAny(this string source, IEnumerable<char> searches)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (char search in searches)
        {
            indexLists.Add(source.IndicesOf(search));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

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