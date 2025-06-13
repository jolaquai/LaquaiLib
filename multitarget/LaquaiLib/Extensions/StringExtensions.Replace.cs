using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Extensions;
public static partial class StringExtensions
{
    extension(string source)
    {
        /// <summary>
        /// Creates a new string from this string with all occurrences of the strings in <paramref name="finds"/> replaced with <paramref name="replace"/>.
        /// </summary>
        /// <param name="source">The string to perform replacements in.</param>
        /// <param name="finds">A collection of strings to search for in <paramref name="source"/>.</param>
        /// <param name="replace">The replacement for occurrences of strings in <paramref name="finds"/>.</param>
        /// <returns>A string as described.</returns>
        public string Replace(IEnumerable<string> finds, string replace = "")
        {
            var input = source;
            foreach (var find in finds)
            {
                input = input.Replace(find, replace);
            }
            return input;
        }
        /// <summary>
        /// Creates a new string from this string with all occurrences of <paramref name="search"/> replaced with strings produced by <paramref name="replaceFactory"/>. Allows for stateful replacements.
        /// </summary>
        /// <param name="source">The string to perform replacements in.</param>
        /// <param name="search">The string to search for in <paramref name="source"/>.</param>
        /// <param name="replaceFactory">A <see cref="Func{TResult}"/> that produces the replacement for occurrences of <paramref name="search"/>. It is called once for each occurrence of <paramref name="search"/>.</param>
        /// <param name="recurse"><see langword="true"/> to not skip the substring produced by <paramref name="replaceFactory"/> calls when searching for the next occurrence of <paramref name="search"/>. <b>If <paramref name="replaceFactory"/> always returns strings containing <paramref name="search"/>, this will result in an infinite loop.</b> Defaults to <see langword="false"/> for this very reason.</param>
        /// <returns>The string with replacements as described.</returns>
        public string Replace(string search, Func<string> replaceFactory, bool recurse = false)
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
        /// Creates a new string from this string with all occurrences of <paramref name="search"/> replaced with strings produced by <paramref name="replaceFactory"/>. Allows for stateful replacements.
        /// </summary>
        /// <param name="source">The string to perform replacements in.</param>
        /// <param name="search">The string to search for in <paramref name="source"/>.</param>
        /// <param name="replaceFactory">A <see cref="Func{TResult}"/> that produces the replacement for occurrences of <paramref name="search"/>. It is called once for each occurrence of <paramref name="search"/> and passed the previous iteration's produced replacement or <see langword="null"/> on the first invocation.</param>
        /// <param name="recurse"><see langword="true"/> to not skip the substring produced by <paramref name="replaceFactory"/> calls when searching for the next occurrence of <paramref name="search"/>. <b>If <paramref name="replaceFactory"/> always returns strings containing <paramref name="search"/>, this will result in an infinite loop.</b> Defaults to <see langword="false"/> for this very reason.</param>
        /// <returns>The string with replacements as described.</returns>
        public string Replace(string search, Func<string, string> replaceFactory, bool recurse = false)
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
        public bool TryReplace(string search, string replacement, [NotNullWhen(true)] out string replaced, StringComparison stringComparison = StringComparison.CurrentCulture)
        {
            replaced = source.Replace(search, replacement, stringComparison);
            return replaced.Equals(source, stringComparison);
        }
    }
}
