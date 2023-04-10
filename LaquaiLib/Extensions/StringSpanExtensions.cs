namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="string"/> and <see cref="Span{T}"/> of <see cref="char"/> Types.
/// </summary>
public static class StringSpanExtensions
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

    #region IndexOf... methods
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
    #endregion

    /* #region IndexOf...Except methods
    /// <summary>
    /// Reports the zero-based indices of all occurrences of Unicode characters other than the one specified in this string.
    /// </summary>
    /// <param name="search">A Unicode character to except.</param>
    /// <returns>All zero-based index positions of any characters that are left, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfExcept(this string source, char search)
    {
        int find = source.IndexOf(search);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of Unicode characters other than the one specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="search">A Unicode character to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>All zero-based index positions of any characters that are left, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfExcept(this string source, char search, int startIndex)
    {
        int find = source.IndexOf(search, startIndex);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of Unicode characters other than the ones contained in <paramref name="search"/> in this string.
    /// </summary>
    /// <param name="search">A string containing Unicode character to except.</param>
    /// <returns>All zero-based index positions of any characters that are left, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfExcept(this string source, string search)
    {
        int find = source.IndexOf(search);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of Unicode characters other than the ones contained in <paramref name="search"/> in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="search">A string containing Unicode character to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>All zero-based index positions of any characters that are left, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfExcept(this string source, string search, int startIndex)
    {
        int find = source.IndexOf(search, startIndex);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of a string (or single character) other than the ones specified in this string.
    /// </summary>
    /// <param name="searches">A sequence of strings to except.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any string other than the ones in <paramref name="searches"/> was found, or -1 otherwise.</returns>
    public static int IndexOfAnyExcept(this string source, IEnumerable<string> searches)
    {
        foreach (string search in searches)
        {
            return source.IndexOf(search);
        }
        return -1;
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of a string (or single character) other than the ones specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="searches">A sequence of strings to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any string other than the ones in <paramref name="searches"/> was found, or -1 otherwise.</returns>
    public static int IndexOfAnyExcept(this string source, IEnumerable<string> searches, int startIndex)
    {
        foreach (string search in searches)
        {
            return source.IndexOf(search, startIndex);
        }
        return -1;
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences of any Unicode character other than the ones specified in this string.
    /// </summary>
    /// <param name="searches">A sequence of characters to except.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any character not contained in <paramref name="searches"/> was found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, IEnumerable<char> searches)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (char search in searches)
        {
            indexLists.Add(source.IndicesOf(search));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences of any Unicode character other than the ones specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="searches">A sequence of characters to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any character not contained in <paramref name="searches"/> was found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, IEnumerable<char> searches, int startIndex)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (char search in searches)
        {
            indexLists.Add(source.IndicesOf(search, startIndex));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences of any string (or single character) other than the ones specified in this string.
    /// </summary>
    /// <param name="searches">A sequence of characters to except.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any string not contained in <paramref name="searches"/> was found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, IEnumerable<string> searches)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (string search in searches)
        {
            indexLists.Add(source.IndicesOf(search));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences of any string (or single character) other than the ones specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="searches">A sequence of characters to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any string not contained in <paramref name="searches"/> was found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, IEnumerable<string> searches, int startIndex)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (string search in searches)
        {
            indexLists.Add(source.IndicesOf(search, startIndex));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }
    #endregion
    */

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
