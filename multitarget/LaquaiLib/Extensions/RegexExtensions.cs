using System.Text.RegularExpressions;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Regex"/> Types.
/// </summary>
public static class RegexExtensions
{
    extension(Match match)
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field)] private extern ref Regex _regex();

        /// <summary>
        /// Retrieves the <see cref="Regex"/> instance that was used to create this <see cref="Match"/>.
        /// </summary>
        /// <param name="match">The <see cref="Match"/> instance to retrieve the <see cref="Regex"/> instance from.</param>
        /// <returns>The <see cref="Regex"/> instance that was used to create this <see cref="Match"/>.</returns>
        public Regex GetRegex()
        {
            ArgumentNullException.ThrowIfNull(match);
            return match._regex();
        }
    }

    extension(Capture capture)
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Text>k__BackingField")] private extern ref string get_Text();

        /// <summary>
        /// Retrieves the original <see cref="string"/> that was matched by a <see cref="Regex"/> instance to produce this <paramref name="capture"/>.
        /// </summary>
        /// <param name="capture">The <see cref="Capture"/> instance to retrieve the original <see cref="string"/> from.</param>
        /// <returns>The original <see cref="string"/> that was matched by a <see cref="Regex"/> instance to produce this <paramref name="capture"/>.</returns>
        public string GetOriginalText()
        {
            ArgumentNullException.ThrowIfNull(capture);
            return capture.get_Text();
        }
    }
}
