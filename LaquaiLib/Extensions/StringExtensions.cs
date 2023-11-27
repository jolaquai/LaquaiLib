using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="string"/> Type.
/// </summary>
public static class StringExtensions
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
    public static string Replace(this string source, IEnumerable<string> finds, string replace = "")
    {
        var input = source;
        foreach (var find in finds)
        {
            input = input.Replace(find, replace);
        }
        return input;
    }

    /// <summary>
    /// Searches the specified input string for occurrences of a specified regex pattern.
    /// </summary>
    /// <param name="source">The <see cref="string"/> to search.</param>
    /// <param name="pattern">The pattern to search for.</param>
    /// <returns>The <see cref="MatchCollection"/> instance returned by <see cref="Regex.Matches(string, string)"/></returns>
    public static MatchCollection Match(this string source, string pattern) => Regex.Matches(source, pattern);
    /// <summary>
    /// Searches the specified input string for occurrences of a specified regex pattern represented by a <see cref="Regex"/> instance.
    /// </summary>
    /// <param name="source">The <see cref="string"/> to search.</param>
    /// <param name="regex">The pattern to search for.</param>
    /// <returns>The <see cref="MatchCollection"/> instance returned by <see cref="Regex.Matches(string, string)"/></returns>
    public static MatchCollection Match(this string source, Regex regex) => regex.Matches(source);

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

    /// <summary>
    /// Converts the specified input string to sentence case (that is, the first character is capitalized and all other characters are lower case).
    /// </summary>
    /// <param name="source">The <see cref="string"/> to convert.</param>
    /// <returns><paramref name="source"/> in sentence case.</returns>
    public static string ToSentence(this string source)
    {
        var lower = source.ToLowerInvariant();
        return char.ToUpperInvariant(lower[0]) + lower[1..];
    }
    /// <summary>
    /// Converts the specified input string to title case according to the rules of the specified <paramref name="culture"/>.
    /// </summary>
    /// <param name="source">The <see cref="string"/> to convert.</param>
    /// <param name="culture">The <see cref="CultureInfo"/> to use for casing rules.</param>
    /// <returns><paramref name="source"/> in title case according to <paramref name="culture"/>.</returns>
    public static string ToTitle(this string source, CultureInfo? culture = null) => (culture ?? CultureInfo.CurrentCulture).TextInfo.ToTitleCase(source);
    /// <summary>
    /// Converts the specified input string to title case according to the rules of the invariant culture.
    /// </summary>
    /// <param name="source">The <see cref="string"/> to convert.</param>
    /// <returns><paramref name="source"/> in title case according to the invariant culture.</returns>
    public static string ToTitleInvariant(this string source) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(source);

    #region (Actually useful) Remove methods / overloads
    /// <summary>
    /// Removes all occurrences of the specified <see cref="char"/>s from this <see cref="string"/>.
    /// </summary>
    /// <param name="source">The <see cref="string"/> to modify.</param>
    /// <param name="remove">The <see cref="char"/>s to remove.</param>
    /// <returns>The original string with all occurrences of the <paramref name="remove"/> chars removed.</returns>
    public static string Remove(this string source, params char[] remove) => Remove(source, (IEnumerable<char>)remove);
    /// <summary>
    /// Removes all occurrences of the specified <see cref="char"/>s from this <see cref="string"/>.
    /// </summary>
    /// <param name="source">The <see cref="string"/> to modify.</param>
    /// <param name="remove">The <see cref="char"/>s to remove.</param>
    /// <returns>The original string with all occurrences of the <paramref name="remove"/> chars removed.</returns>
    public static string Remove(this string source, IEnumerable<char> remove) => string.Concat(source.Except(remove));

    /// <summary>
    /// Removes all occurrences of the specified <see cref="char"/>s from this <see cref="string"/> starting at the specified index.
    /// </summary>
    /// <param name="source">The <see cref="string"/> to modify.</param>
    /// <param name="startIndex">The zero-based index at which to begin removing <see cref="char"/>s.</param>
    /// <param name="remove">The <see cref="char"/>s to remove.</param>
    /// <returns>The original string with all occurrences of the <paramref name="remove"/> chars removed.</returns>
    public static string Remove(this string source, int startIndex, params char[] remove) => source[..(startIndex - 1)] + source[startIndex..].Remove(remove);

    /// <summary>
    /// Removes all occurrences of the specified <see cref="string"/>s from this <see cref="string"/>.
    /// </summary>
    /// <param name="source">The <see cref="string"/> to modify.</param>
    /// <param name="remove">The <see cref="string"/>s to remove.</param>
    /// <returns>The original string with all occurrences of the <paramref name="remove"/> chars removed.</returns>
    public static string Remove(this string source, params string[] remove)
    {
        var ret = source;
        foreach (var r in remove)
        {
            ret = ret.Replace(r, "");
        }
        return ret;
    }

    /// <summary>
    /// Removes all occurrences of the specified <see cref="string"/>s from this <see cref="string"/> starting at the specified index.
    /// </summary>
    /// <param name="source">The <see cref="string"/> to modify.</param>
    /// <param name="startIndex">The zero-based index at which to begin removing <see cref="string"/>s.</param>
    /// <param name="remove">The <see cref="string"/>s to remove.</param>
    /// <returns>The original string with all occurrences of the <paramref name="remove"/> chars removed.</returns>
    public static string Remove(this string source, int startIndex, params string[] remove) => source[..(startIndex - 1)] + source[startIndex..].Remove(remove);
    #endregion

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
        List<IEnumerable<int>> indexLists = [];
        foreach (var search in searches)
        {
            indexLists.Add(source.IndicesOf(search));
        }
        return indexLists.Aggregate(Enumerable.Empty<int>(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
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
        List<IEnumerable<int>> indexLists = [];
        foreach (var search in searches)
        {
            indexLists.Add(source.IndicesOf(search, startIndex));
        }
        return indexLists.Aggregate(Enumerable.Empty<int>(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences in this instance of any string in a specified sequence of strings.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any string in <paramref name="searches"/> was found; an empty collection if no string in <paramref name="searches"/> was found.</returns>
    public static IEnumerable<int> IndicesOfAny(this string source, IEnumerable<string> searches)
    {
        List<IEnumerable<int>> indexLists = [];
        foreach (var search in searches)
        {
            indexLists.Add(source.IndicesOf(search));
        }
        return indexLists.Aggregate(Enumerable.Empty<int>(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
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
        List<IEnumerable<int>> indexLists = [];
        foreach (var search in searches)
        {
            indexLists.Add(source.IndicesOf(search, startIndex));
        }
        return indexLists.Aggregate(Enumerable.Empty<int>(), (seed, next) => seed = seed.Concat(next), seed => seed.Distinct()).Order();
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
        for (var i = 0; i < source.Length; i++)
        {
            if (source[i] != except)
            {
                yield return i;
            }
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
        for (var i = startIndex; i < source.Length; i++)
        {
            if (source[i] != except)
            {
                yield return i;
            }
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
        var exceptSet = new HashSet<char>(except);

        for (var i = 0; i < source.Length; i++)
        {
            if (!exceptSet.Contains(source[i]))
            {
                yield return i;
            }
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
        var exceptSet = new HashSet<char>(except);

        for (var i = startIndex; i < source.Length; i++)
        {
            if (!exceptSet.Contains(source[i]))
            {
                yield return i;
            }
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

        foreach (var c in source[startIndex..])
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

                if (source[i..].StartsWith(except))
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

                if (source[i..].StartsWith(except))
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

    #region Line transformation methods
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
    #endregion

    #region String similarity
    /// <summary>
    /// Computes a value that indicates the similarity between two strings. "Similarity" is defined as the number of characters that are the same in both strings, divided by the length of the longer string. As such, the value returned by this method is always between <c>0</c> (the strings are have no characters in common) and <c>1</c> (the strings are equal), inclusive.
    /// </summary>
    /// <param name="first">The first <see cref="string"/> to use for the comparison.</param>
    /// <param name="second">The second <see cref="string"/> to use for the comparison.</param>
    /// <param name="stringComparer">A <see cref="StringComparer"/> instance to use when comparing the <see cref="string"/>s. Defaults to <see cref="StringComparer.OrdinalIgnoreCase"/>.</param>
    /// <returns>The computed similarity as described.</returns>
    public static double GetSimilarity(this string first, string second, StringComparer? stringComparer = null)
    {
        ArgumentNullException.ThrowIfNull(second);

        if (!first.Intersect(second).Any()
            || string.IsNullOrEmpty(first)
            || string.IsNullOrEmpty(second))
        {
            return 0;
        }

        stringComparer ??= StringComparer.OrdinalIgnoreCase;

        if (stringComparer.Compare(first, second) == 0)
        {
            return 1;
        }

        _ = first.Select(c => c.ToString()).Intersect(second.Select(c => c.ToString()), stringComparer);
        return (double)first.Select(c => c.ToString()).Intersect(second.Select(c => c.ToString()), stringComparer).Count() / new List<string>() { first, second }.Max(str => str.Length);
    }
    #endregion
}
