using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using LaquaiLib.Extensions;

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

    #region (Try)Replace overloads
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
    /// Creates a new string from this string with all occurrences <paramref name="search"/> replaced with strings produced by <paramref name="replaceFactory"/>. Allows for stateful replacements.
    /// </summary>
    /// <param name="source">The string to perform replacements in.</param>
    /// <param name="search">The string to search for in <paramref name="source"/>.</param>
    /// <param name="replaceFactory">A <see cref="Func{TResult}"/> that produces the replacement for occurrences of <paramref name="search"/>. It is called once for each occurrence of <paramref name="search"/>.</param>
    /// <param name="recurse"><see langword="true"/> to not skip the substring produced by <paramref name="replaceFactory"/> calls when searching for the next occurrence of <paramref name="search"/>. <b>If <paramref name="replaceFactory"/> always returns strings containing <paramref name="search"/>, this will result in an infinite loop.</b> Defaults to <see langword="false"/> for this very reason.</param>
    /// <returns>The string with replacements as described.</returns>
    public static string Replace(this string source, string search, Func<string> replaceFactory, bool recurse = false)
    {
        var index = source.IndexOf(search);
        var searchLen = search.Length;
        while (index > -1)
        {
            var currentReplacement = replaceFactory();
            source = source.Remove(index, searchLen).Insert(index, currentReplacement);
            if (!recurse)
            {
                index += currentReplacement.Length;
            }
            index = source.IndexOf(search, index);
        }
        return source;
    }
    /// <summary>
    /// Creates a new string from this string with all occurrences <paramref name="search"/> replaced with strings produced by <paramref name="replaceFactory"/>. Allows for stateful replacements.
    /// </summary>
    /// <param name="source">The string to perform replacements in.</param>
    /// <param name="search">The string to search for in <paramref name="source"/>.</param>
    /// <param name="replaceFactory">A <see cref="Func{TResult}"/> that produces the replacement for occurrences of <paramref name="search"/>. It is called once for each occurrence of <paramref name="search"/> and passed the previous iteration's produced replacement or <see langword="null"/> on the first invocation.</param>
    /// <param name="recurse"><see langword="true"/> to not skip the substring produced by <paramref name="replaceFactory"/> calls when searching for the next occurrence of <paramref name="search"/>. <b>If <paramref name="replaceFactory"/> always returns strings containing <paramref name="search"/>, this will result in an infinite loop.</b> Defaults to <see langword="false"/> for this very reason.</param>
    /// <returns>The string with replacements as described.</returns>
    public static string Replace(this string source, string search, Func<string?, string> replaceFactory, bool recurse = false)
    {
        var index = source.IndexOf(search);
        var searchLen = search.Length;
        string lastReplacement = null;
        while (index > -1)
        {
            lastReplacement = replaceFactory(lastReplacement);
            source = source.Remove(index, searchLen).Insert(index, lastReplacement);
            if (!recurse)
            {
                index += lastReplacement.Length;
            }
            index = source.IndexOf(search, index);
        }
        return source;
    }
    /// <summary>
    /// Replaces all occurrences of the specified <paramref name="search"/> <see langword="string"/> with the specified <paramref name="replacement"/> <see langword="string"/> using the specified <paramref name="stringComparison"/> and returns whether the replacement resulted in a change to the original string.
    /// </summary>
    /// <param name="source">The <see langword="string"/> to search.</param>
    /// <param name="search">The <see langword="string"/> to search for.</param>
    /// <param name="replacement">The <see langword="string"/> to replace <paramref name="search"/> with.</param>
    /// <param name="replaced">An <see langword="out"/> variable that receives the result of the replacement. It is assigned the result of the <see cref="string.Replace(string, string?, StringComparison)"/> call regardless of whether this results in a change.</param>
    /// <param name="stringComparison">The <see cref="StringComparison"/> to use for the replacement <b>and</b> the comparison of the original and replaced strings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
    /// <returns><see langword="true"/> if the replace operation resulted in a change to the original string, <see langword="false"/> otherwise.</returns>
    public static bool TryReplace(this string source, string search, string replacement, [NotNullWhen(true)] out string replaced, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        replaced = source.Replace(search, replacement, stringComparison);
        return replaced.Equals(source, stringComparison);
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
    #endregion

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

    #region Remove overloads
    /// <summary>
    /// Removes all occurrences of the specified <see cref="char"/>s from this <see cref="string"/>.
    /// </summary>
    /// <param name="source">The <see cref="string"/> to modify.</param>
    /// <param name="remove">The <see cref="char"/>s to remove.</param>
    /// <returns>The original string with all occurrences of the <paramref name="remove"/> chars removed.</returns>
    public static string Remove(this string source, params IEnumerable<char> remove) => string.Concat(source.Except(remove));
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
    /// Reports the zero-based index of the first occurrence of any of the specified Unicode <see langword="char"/>s in this <see langword="string"/>.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">The Unicode <see langword="char"/>s to seek.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any <see langword="char"/> in <paramref name="searches"/> was found; -1 if no <see langword="char"/> in <paramref name="searches"/> was found.</returns>
    public static int IndexOfAny(this string source, ReadOnlySpan<char> searches) => source.AsSpan().IndexOfAny(searches);
    /// <summary>
    /// Reports the zero-based index of the first occurrence of any of the specified Unicode <see langword="char"/>s in this <see langword="string"/>. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">The Unicode <see langword="char"/>s to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any <see langword="char"/> in <paramref name="searches"/> was found; -1 if no <see langword="char"/> in <paramref name="searches"/> was found.</returns>
    public static int IndexOfAny(this string source, ReadOnlySpan<char> searches, int startIndex) => source.AsSpan(startIndex).IndexOfAny(searches);
    /// <summary>
    /// Reports the zero-based index of the first occurrence in this instance of any string in a specified sequence of strings.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <param name="stringComparison">The <see cref="StringComparison"/> behavior to employ when searching for the delimiters. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any <see langword="string"/> in <paramref name="searches"/> was found; -1 if no <see langword="string"/> in <paramref name="searches"/> was found.</returns>
    public static int IndexOfAny(this string source, ReadOnlySpan<string> searches, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        return source.AsSpan().IndexOfAny(SearchValues.Create(searches, stringComparison));
    }
    /// <summary>
    /// Reports the zero-based index of the first occurrence in this instance of any <see langword="string"/> in a specified sequence of <see langword="string"/>s. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <param name="stringComparison">The <see cref="StringComparison"/> behavior to employ when searching for the delimiters. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any <see langword="string"/> in <paramref name="searches"/> was found; -1 if no string in <paramref name="searches"/> was found.</returns>
    public static int IndexOfAny(this string source, ReadOnlySpan<string> searches, int startIndex, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        return source.AsSpan(startIndex).IndexOfAny(SearchValues.Create(searches, stringComparison));
    }

    /// <summary>
    /// Reports the zero-based indices of the all occurrences in this instance of any Unicode character in a specified sequence of characters.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="searches">A sequence of strings to seek.</param>
    /// <returns>The zero-based index positions of all occurrences in this instance where any character in <paramref name="searches"/> was found; an empty collection if no character in <paramref name="searches"/> was found.</returns>
    public static IEnumerable<int> IndicesOfAny(this string source, ReadOnlySpan<char> searches)
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
    public static IEnumerable<int> IndicesOfAny(this string source, ReadOnlySpan<char> searches, int startIndex)
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
    public static IEnumerable<int> IndicesOfAny(this string source, ReadOnlySpan<string> searches)
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
    public static IEnumerable<int> IndicesOfAny(this string source, ReadOnlySpan<string> searches, int startIndex)
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
    /// Reports the zero-based index of the first occurrence of a character other than the ones specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="excepts">A sequence of characters to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any character other than the ones in <paramref name="excepts"/> was found, or -1 otherwise.</returns>
    public static int IndexOfAnyExcept(this string source, ReadOnlySpan<char> excepts, int startIndex) => source.AsSpan(startIndex).IndexOfAnyExcept(excepts);
    /// <summary>
    /// Reports the zero-based index of the first occurrence of a string (or single character) other than the ones specified in this string. The search starts at a specified character position.
    /// </summary>
    /// <param name="source">The string to search.</param>
    /// <param name="excepts">A sequence of strings to except.</param>
    /// <param name="startIndex">The search starting position.</param>
    /// <returns>The zero-based index position of the first occurrence in this instance where any string other than the ones in <paramref name="excepts"/> was found, or -1 otherwise.</returns>
    public static int IndexOfAnyExcept(this string source, ReadOnlySpan<string> excepts, int startIndex, StringComparison stringComparison = StringComparison.CurrentCulture)
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
                if (except?.Length == 0)
                {
                    continue;
                }

                if (source[i..].StartsWith(except, stringComparison))
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
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, ReadOnlySpan<char> excepts)
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
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, ReadOnlySpan<char> excepts, int startIndex)
    {
        var indexes = new List<int>();
        var index = startIndex - 1;

        while ((index = source.IndexOfAny(excepts, index + 1)) != -1)
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
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, ReadOnlySpan<string> excepts, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        var indexes = new List<int>();
        var index = -1;

        while ((index = source.IndexOfAny(excepts, index + 1, stringComparison)) != -1)
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
    public static IEnumerable<int> IndicesOfAnyExcept(this string source, ReadOnlySpan<string> excepts, int startIndex, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        var indexes = new List<int>();
        var index = startIndex - 1;

        while ((index = source.IndexOfAny(excepts, index + 1, stringComparison)) != -1)
        {
            indexes.Add(index);
        }

        return indexes;
    }
    #endregion

    #region Line transformation/action methods
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

    /// <summary>
    /// Executes an <paramref name="action"/> for each line of a <see langword="string"/>. It is passed a <see cref="ReadOnlySpan{T}"/> of the line.
    /// </summary>
    /// <param name="source">The <see langword="string"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{T}"/> to execute for each line.</param>
    public static void ForEachLine(this string source, Action<ReadOnlySpan<char>> action)
    {
        foreach (var line in source.AsSpan().EnumerateLines())
        {
            action(line);
        }
    }
    /// <summary>
    /// Executes an <paramref name="action"/> for each line of a <see langword="string"/>. It is passed a <see cref="ReadOnlySpan{T}"/> of the line and the index of the line.
    /// </summary>
    /// <param name="source">The <see langword="string"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{T1, T2}"/> to execute for each line.</param>
    public static void ForEachLine(this string source, Action<ReadOnlySpan<char>, int> action)
    {
        var index = 0;
        foreach (var line in source.AsSpan().EnumerateLines())
        {
            action(line, index++);
        }
    }
    /// <summary>
    /// Executes an <paramref name="action"/> for each line of a <see langword="string"/> that satisfies conditions defined by <paramref name="predicate"/>. It is passed a <see cref="ReadOnlySpan{T}"/> of the line.
    /// </summary>
    /// <param name="source">The <see langword="string"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{T}"/> to execute for each line.</param>
    /// <param name="predicate">The <see cref="Func{T, TResult}"/> used to determine which lines are processed by <paramref name="action"/>.</param>
    public static void ForEachLine(this string source, Action<ReadOnlySpan<char>> action, Func<ReadOnlySpan<char>, bool> predicate)
    {
        foreach (var line in source.AsSpan().EnumerateLines())
        {
            if (predicate(line))
            {
                action(line);
            }
        }
    }
    /// <summary>
    /// Executes an <paramref name="action"/> for each line of a <see langword="string"/> that satisfies conditions defined by <paramref name="predicate"/>. It is passed a <see cref="ReadOnlySpan{T}"/> of the line and the index of the line.
    /// </summary>
    /// <param name="source">The <see langword="string"/> to iterate over.</param>
    /// <param name="action">The <see cref="Action{T1, T2}"/> to execute for each line.</param>
    /// <param name="predicate">The <see cref="Func{T1, T2, TResult}"/> used to determine which lines are processed by <paramref name="action"/>.</param>
    public static void ForEachLine(this string source, Action<ReadOnlySpan<char>, int> action, Func<ReadOnlySpan<char>, int, bool> predicate)
    {
        var index = 0;
        foreach (var line in source.AsSpan().EnumerateLines())
        {
            if (predicate(line, index))
            {
                action(line, index);
            }
            index++;
        }
    }

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

        return (double)first.Select(c => c.ToString()).Intersect(second.Select(c => c.ToString()), stringComparer).Count() / new List<string>() { first, second }.Max(str => str.Length);
    }
    #endregion

    #region Span shit
    /// <summary>
    /// Returns a <see cref="SpanSplitByCharEnumerator"/> that enumerates the segments of a <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>s that are separated by any of the <see langword="char"/>s specified by <paramref name="chars"/>.
    /// </summary>
    /// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
    /// <param name="chars">The <see langword="char"/>s to use as delimiters.</param>
    /// <returns>The created <see cref="SpanSplitByCharEnumerator"/>.</returns>
    public static SpanSplitByCharEnumerator EnumerateSplits(this ReadOnlySpan<char> source, ReadOnlySpan<char> chars)
        => new SpanSplitByCharEnumerator(source, chars);
    /// <summary>
    /// Enumerates the segments of a <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>s that are separated by any of the <see langword="string"/>s specified by <paramref name="strings"/>.
    /// </summary>
    /// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
    /// <param name="strings">The <see langword="string"/>s to use as delimiters.</param>
    /// <param name="stringComparison">The <see cref="StringComparison"/> behavior to employ when searching for the delimiters. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ReadOnlySpan{T}"/>s representing the segments of the input <see cref="ReadOnlySpan{T}"/>.</returns>
    /// <remarks>
    /// This overload expects specifically one or more <see langword="string"/>s as the delimiter(s). To use one or more <see langword="char"/>s as the delimiter(s), use <see cref="EnumerateSplits(ReadOnlySpan{char}, ReadOnlySpan{char})"/> instead.
    /// </remarks>
    public static SpanSplitByStringEnumerator EnumerateSplits(this ReadOnlySpan<char> source, ReadOnlySpan<string> strings, StringComparison stringComparison = StringComparison.CurrentCulture)
        => new SpanSplitByStringEnumerator(source, strings, stringComparison);

    /// <summary>
    /// Finds the number of occurrences of any of the specified <see langword="char"/>s in the input <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>s.
    /// </summary>
    /// <param name="source">The <see cref="ReadOnlySpan{T}"/> to search.</param>
    /// <param name="chars">The <see langword="char"/>s to search for.</param>
    /// <returns>The number of occurrences of any of the <paramref name="chars"/> in the input <see cref="ReadOnlySpan{T}"/>.</returns>
    public static int FindCount(this ReadOnlySpan<char> source, params ReadOnlySpan<char> chars)
    {
        var count = 0;
        while (source.Length > 0)
        {
            var index = source.IndexOfAny(chars);
            if (index == -1)
            {
                break;
            }
            count++;
            source = source[(index + 1)..];
        }
        return count;
    }
    /// <summary>
    /// Finds the number of occurrences of any of the specified <see langword="char"/>s in the input <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>s.
    /// </summary>
    /// <param name="source">The <see cref="ReadOnlySpan{T}"/> to search.</param>
    /// <param name="strings">The <paramref name="string"/>s to search for.</param>
    /// <param name="stringComparison">The <see cref="StringComparison"/> behavior to employ when searching for the strings. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
    /// <returns>The number of occurrences of any of the <paramref name="strings"/> in the input <see cref="ReadOnlySpan{T}"/>.</returns>
    public static int FindCount(this ReadOnlySpan<char> source, ReadOnlySpan<string> strings, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        var searchValues = SearchValues.Create(strings, stringComparison);
        var count = 0;
        while (source.Length > 0)
        {
            var index = source.IndexOfAny(searchValues);
            if (index == -1)
            {
                break;
            }
            count++;
            source = source[(index + 1)..];
        }
        return count;
    }
    /// <inheritdoc cref="FindCount(ReadOnlySpan{char}, ReadOnlySpan{string}, StringComparison)"/>
    public static int FindCount(this ReadOnlySpan<char> source, params ReadOnlySpan<string> strings) => FindCount(source, strings, stringComparison: StringComparison.CurrentCulture);
    #endregion
}

#region Span enumerators
/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>s that are separated by any of the <paramref name="char"/>s specified by <paramref name="chars"/>.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="chars">The <see langword="char"/>s to use as delimiters.</param>
public ref struct SpanSplitByCharEnumerator(ReadOnlySpan<char> source, ReadOnlySpan<char> chars)
{
    private ReadOnlySpan<char> _source = source;
    private readonly ReadOnlySpan<char> _chars = chars;

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<char> Current { get; private set; }

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    /// <returns>A reference to the current instance.</returns>
    public readonly SpanSplitByCharEnumerator GetEnumerator() => this;
    /// <summary>
    /// Attempts to advance the enumerator to the next segment in the source span.
    /// </summary>
    /// <returns><see langword="true"/> if the advancement has succeeded, otherwise <see langword="false"/> if the enumerator has passed the end of the span.</returns>
    public bool MoveNext()
    {
        if (_source.Length == 0)
        {
            return false;
        }

        var end = _source.IndexOfAny(_chars);
        if (end == -1)
        {
            Current = _source;
            _source = [];
            return true;
        }
        Current = _source[..end];
        _source = _source[++end..];
        return true;
    }
}
/// <summary>
/// Implements the enumerator pattern to enumerate the segments in a source <see cref="ReadOnlySpan{T}"/> of <see langword="char"/>s that are separated by any of the <see langword="string"/>s specified by <paramref name="strings"/>.
/// </summary>
/// <param name="source">The <see cref="ReadOnlySpan{T}"/> to enumerate the segments of.</param>
/// <param name="strings">The <see cref="ReadOnlySpan{T}"/>s to use as delimiters.</param>
/// <param name="stringComparison">The <see cref="StringComparison"/> behavior to employ when searching for the delimiters. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
public ref struct SpanSplitByStringEnumerator(ReadOnlySpan<char> source, ReadOnlySpan<string> strings, StringComparison stringComparison = StringComparison.CurrentCulture)
{
    private ReadOnlySpan<char> _source = source;
    private readonly SearchValues<string> _searchValues = SearchValues.Create(strings, stringComparison);

    /// <summary>
    /// Retrieves the current segment at which the enumerator is positioned.
    /// </summary>
    public ReadOnlySpan<char> Current { get; private set; }

    /// <summary>
    /// Returns the current instance. For use in <see langword="foreach"/> statements.
    /// </summary>
    /// <returns>A reference to the current instance.</returns>
    public readonly SpanSplitByStringEnumerator GetEnumerator() => this;
    /// <summary>
    /// Attempts to advance the enumerator to the next segment in the source span.
    /// </summary>
    public bool MoveNext()
    {
        if (_source.Length == 0)
        {
            return false;
        }

        var end = _source.IndexOfAny(_searchValues);
        if (end == -1)
        {
            Current = _source;
            _source = [];
            return true;
        }
        Current = _source[..end];
        _source = _source[++end..];
        return true;
    }
}
#endregion