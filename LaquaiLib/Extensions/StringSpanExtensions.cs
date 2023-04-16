using System.Text;

using DocumentFormat.OpenXml.Drawing;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="string"/> and <see cref="Span{T}"/> of <see cref="char"/> Types.
/// </summary>
public static class StringSpanExtensions
{
    /// <summary>
    /// Constructs a new string from this string repeated <paramref name="count"/> times.
    /// </summary>
    /// <param name="source">The string to repeat.</param>
    /// <param name="count">The number of times to repeat <paramref name="source"/>.</param>
    /// <returns>A string consisting of <paramref name="source"/> repeated <paramref name="count"/> times.</returns>
    public static string Repeat(this string source, int count) => string.Concat(Enumerable.Repeat(source, count));

    /// <summary>
    /// Creates a new string from this string with all occurrences of the strings in <paramref name="finds"/> replaced with <paramref name="replace"/>.
    /// </summary>
    /// <param name="source">The string to perform replacements in.</param>
    /// <param name="finds">A collection of strings to search for in <paramref name="source"/>.</param>
    /// <param name="replace">The replacement for occurrences of strings in <paramref name="finds"/>.</param>
    /// <returns>A string as described.</returns>
    public static string Replace(this string source, IEnumerable<string> finds, string replace)
    {
        var input = source;
        foreach (var find in finds)
        {
            input = input.Replace(find, replace);
        }
        return input;
    }

    /// <summary>
    /// Creates a new string from this string with all occurrences of any string that is not contained in <paramref name="except"/> replaced with <paramref name="replace"/>.
    /// </summary>
    /// <param name="source">The string to perform replacements in.</param>
    /// <param name="except">A collection of strings to except from replacement in <paramref name="source"/>.</param>
    /// <param name="replace">The replacement for occurrences of strings that are not in <paramref name="except"/>.</param>
    /// <returns>A string as described.</returns>
    public static string ReplaceExcept(this string source, IEnumerable<string> except, string replace)
    {
        var replacedChars = new HashSet<char>();
        foreach (var exceptString in except)
        {
            foreach (var c in exceptString)
            {
                replacedChars.Add(c);
            }
        }

        var result = new StringBuilder();
        foreach (var c in source)
        {
            if (replacedChars.Contains(c))
            {
                result.Append(c);
            }
            else
            {
                result.Append(replace);
            }
        }

        return result.ToString();
    }

    #region IndexOf... methods
    /// <summary>
    /// Reports the zero-based indices of all occurrences of the specified Unicode character in this string.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="search">A Unicode character to seek.</param>
    /// <returns>All zero-based index positions of <paramref name="search"/> if that character is found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOf(this string source, char search)
    {
        var find = source.IndexOf(search);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of the specified Unicode character in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="search">A Unicode character to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>All zero-based index positions of <paramref name="search"/> if that character is found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOf(this string source, char search, int startIndex)
    {
        var find = source.IndexOf(search, startIndex);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of the specified string in this instance.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="search">The string to seek.</param>
    /// <returns>All zero-based index positions of <paramref name="search"/> if that string is found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOf(this string source, string search)
    {
        var find = source.IndexOf(search);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of the specified string in this instance. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="search">The string to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>All zero-based index positions of <paramref name="search"/> if that string is found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOf(this string source, string search, int startIndex)
    {
        var find = source.IndexOf(search, startIndex);
        while (find != -1)
        {
            yield return find;
            find = source.IndexOf(search, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence in this instance of any string in a specified sequence of strings.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any string in <paramref name="searches"/> was found; -1 if no string in <paramref name="searches"/> was found.</returns>
    public static int IndexOfAny(this string source, IEnumerable<string> searches)
    {
        foreach (var search in searches)
        {
            return source.IndexOf(search);
        }
        return -1;
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence in this instance of any string in a specified sequence of strings. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any string in <paramref name="searches"/> was found; -1 if no string in <paramref name="searches"/> was found.</returns>
    public static int IndexOfAny(this string source, IEnumerable<string> searches, int startIndex)
    {
        foreach (var search in searches)
        {
            return source.IndexOf(search, startIndex);
        }
        return -1;
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences in this instance of any Unicode character in a specified sequence of characters.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any character in <paramref name="searches"/> was found; an empty collection if no character in <paramref name="searches"/> was found.</returns>
    public static IEnumerable<int> IndicesOfAny(this string source, IEnumerable<char> searches)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (var search in searches)
        {
            indexLists.Add(source.IndicesOf(search));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences in this instance of any Unicode character in a specified sequence of characters. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any character in <paramref name="searches"/> was found; an empty collection if no character in <paramref name="searches"/> was found.</returns>
    public static IEnumerable<int> IndicesOfAny(this string source, IEnumerable<char> searches, int startIndex)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (var search in searches)
        {
            indexLists.Add(source.IndicesOf(search, startIndex));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences in this instance of any string in a specified sequence of strings.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any string in <paramref name="searches"/> was found; an empty collection if no string in <paramref name="searches"/> was found.</returns>
    public static IEnumerable<int> IndicesOfAny(this string source, IEnumerable<string> searches)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (var search in searches)
        {
            indexLists.Add(source.IndicesOf(search));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences in this instance of any string in a specified sequence of strings. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any string in <paramref name="searches"/> was found; an empty collection if no string in <paramref name="searches"/> was found.</returns>
    public static IEnumerable<int> IndicesOfAny(this string source, IEnumerable<string> searches, int startIndex)
    {
        List<IEnumerable<int>> indexLists = new();
        foreach (var search in searches)
        {
            indexLists.Add(source.IndicesOf(search, startIndex));
        }
        return indexLists.Aggregate(new List<int>().Select(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }
    #endregion

    #region IndexOf...Except methods
    /// <summary>
    /// Reports the zero-based indices of all occurrences of Unicode characters other than the one specified in this string.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="except">A Unicode character to except.</param>
    /// <returns>All zero-based index positions of any characters that are left, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfExcept(this string source, char except)
    {
        var find = source.IndexOf(except);
        while (find != -1)
        {
            if (source[find] != except)
            {
                yield return find;
            }
            find = source.IndexOf(except, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of Unicode characters other than the one specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="except">A Unicode character to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>All zero-based index positions of any characters that are left, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfExcept(this string source, char except, int startIndex)
    {
        var find = source.IndexOf(except, startIndex);
        while (find != -1)
        {
            if (source[find] != except)
            {
                yield return find;
            }
            find = source.IndexOf(except, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of Unicode characters other than the ones contained in <paramref name="except"/> in this string.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="except">A string containing Unicode character to except.</param>
    /// <returns>All zero-based index positions of any characters that are left, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfExcept(this string source, string except)
    {
        var find = source.IndexOf(except);
        while (find != -1)
        {
            if (!except.Contains(source[find]))
            {
                yield return find;
            }
            find = source.IndexOf(except, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based indices of all occurrences of Unicode characters other than the ones contained in <paramref name="except"/> in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="except">A string containing Unicode character to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>All zero-based index positions of any characters that are left, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfExcept(this string source, string except, int startIndex)
    {
        var find = source.IndexOf(except, startIndex);
        while (find != -1)
        {
            if (!except.Contains(source[find]))
            {
                yield return find;
            }
            find = source.IndexOf(except, find + 1);
        }
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of a character other than the ones specified in this string.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="excepts">A sequence of characters to except.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any character other than the ones in <paramref name="excepts"/> was found, or -1 otherwise.</returns>
    public static int IndexOfAnyExcept(this string source, IEnumerable<char> excepts)
    {
        var index = -1;
        var minIndex = int.MaxValue;

        foreach (var c in source)
        {
            if (!excepts.Contains(c))
            {
                index = source.IndexOf(c);
                if (index >= 0 && index < minIndex)
                {
                    minIndex = index;
                }
            }
        }

        return minIndex == int.MaxValue ? -1 : minIndex;
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of a character other than the ones specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="excepts">A sequence of characters to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any character other than the ones in <paramref name="excepts"/> was found, or -1 otherwise.</returns>
    public static int IndexOfAnyExcept(this string source, IEnumerable<char> excepts, int startIndex)
    {
        var index = -1;
        var minIndex = int.MaxValue;

        foreach (var c in source.Substring(startIndex))
        {
            if (!excepts.Contains(c))
            {
                index = source.IndexOf(c, startIndex);
                if (index >= 0 && index < minIndex)
                {
                    minIndex = index;
                }
            }
            startIndex++;
        }

        return minIndex == int.MaxValue ? -1 : minIndex;
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of a string (or single character) other than the ones specified in this string.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="excepts">A sequence of strings to except.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any string other than the ones in <paramref name="excepts"/> was found, or -1 otherwise.</returns>
    public static int IndexOfAnyExcept(this string source, IEnumerable<string> excepts)
    {
        for (var i = 0; i < source.Length; i++)
        {
            var foundExcept = false;
            foreach (var except in excepts)
            {
                if (except == "")
                {
                    continue;
                }

                if (source.Substring(i).StartsWith(except))
                {
                    foundExcept = true;
                    break;
                }
            }

            if (!foundExcept)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Reports the zero-based index of the first occurrence of a string (or single character) other than the ones specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="excepts">A sequence of strings to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any string other than the ones in <paramref name="excepts"/> was found, or -1 otherwise.</returns>
    public static int IndexOfAnyExcept(this string source, IEnumerable<string> excepts, int startIndex)
    {
        if (startIndex >= source.Length)
        {
            return -1;
        }

        for (var i = startIndex; i < source.Length; i++)
        {
            var foundExcept = false;
            foreach (var except in excepts)
            {
                if (except == "")
                {
                    continue;
                }

                if (source.Substring(i).StartsWith(except))
                {
                    foundExcept = true;
                    break;
                }
            }

            if (!foundExcept)
            {
                return i;
            }
        }

        return -1;
    }


    /// <summary>
    /// Reports the zero-based indices of the all occurrences of any Unicode character other than the ones specified in this string.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="excepts">A sequence of characters to except.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any character not contained in <paramref name="excepts"/> was found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, IEnumerable<char> excepts)
    {
        var indexes = new List<int>();
        var index = -1;

        while ((index = source.IndexOfAny(excepts.ToArray(), index + 1)) != -1)
        {
            indexes.Add(index);
        }

        return indexes;
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences of any Unicode character other than the ones specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="excepts">A sequence of characters to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any character not contained in <paramref name="excepts"/> was found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, IEnumerable<char> excepts, int startIndex)
    {
        var indexes = new List<int>();
        var index = startIndex - 1;

        while ((index = source.IndexOfAny(excepts.ToArray(), index + 1)) != -1)
        {
            indexes.Add(index);
        }

        return indexes;
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences of any string (or single character) other than the ones specified in this string.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="excepts">A sequence of characters to except.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any string not contained in <paramref name="excepts"/> was found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, IEnumerable<string> excepts)
    {
        var indexes = new List<int>();
        var index = -1;

        while ((index = source.IndexOfAny(excepts.ToArray(), index + 1)) != -1)
        {
            indexes.Add(index);
        }

        return indexes;
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences of any string (or single character) other than the ones specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="excepts">A sequence of characters to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any string not contained in <paramref name="excepts"/> was found, or an empty collection otherwise.</returns>
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, IEnumerable<string> excepts, int startIndex)
    {
        var indexes = new List<int>();
        var index = startIndex - 1;

        while ((index = source.IndexOfAny(excepts.ToArray(), index + 1)) != -1)
        {
            indexes.Add(index);
        }

        return indexes;
    }
    #endregion

    /// <summary>
    /// Applies a <paramref name="transform"/> function to each line of a string.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="transform">The function used to transform each line of the input string.</param>
    /// <returns>The transformed string.</returns>
    public static string ForEachLine(this string source, Func<string, string> transform) => string.Join(Environment.NewLine, source.Split(Environment.NewLine).Select(line => transform(line)));
    /// <summary>
    /// Applies a <paramref name="transform"/> function to each line of a string, incorporating each line's index in the function.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="transform">The function used to transform each line of the input string.</param>
    /// <returns>The transformed string.</returns>
    public static string ForEachLine(this string source, Func<string, int, string> transform) => string.Join(Environment.NewLine, source.Split(Environment.NewLine).Select((line, index) => transform(line, index)));
    /// <summary>
    /// Applies a <paramref name="transform"/> function to each line of a string that satisfies conditions defined by <paramref name="predicate"/>. Lines that do not satisfy this condition are copied without applying <paramref name="transform"/>.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="transform">The function used to transform each line of the input string.</param>
    /// <param name="predicate">The function used to determine which lines are transformed using <paramref name="transform"/>.</param>
    /// <returns></returns>
    public static string ForEachLine(this string source, Func<string, string> transform, Func<string, bool> predicate) => string.Join(Environment.NewLine, source.Split(Environment.NewLine).Select(line => predicate(line) ? transform(line) : line));
    /// <summary>
    /// Applies a <paramref name="transform"/> function to each line of a string that satisfies conditions defined by <paramref name="predicate"/>, incorporating each line's index in the functions. Lines that do not satisfy this condition are copied without applying <paramref name="transform"/>.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="transform">The function used to transform each line of the input string.</param>
    /// <param name="predicate">The function used to determine which lines are transformed using <paramref name="transform"/>.</param>
    /// <returns></returns>
    public static string ForEachLine(this string source, Func<string, int, string> transform, Func<string, int, bool> predicate) => string.Join(Environment.NewLine, source.Split(Environment.NewLine).Select((line, index) => predicate(line, index) ? transform(line, index) : line));
}
