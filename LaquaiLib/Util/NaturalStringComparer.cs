using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace LaquaiLib.Util;

/// <summary>
/// Represents a <see cref="IComparer{T}"/> that compares strings using a natural sort order, that is, like Windows Explorer sorts file names.
/// </summary>
public partial class NaturalStringComparer : IComparer<string>
{
    /// <summary>
    /// Gets the default instance of the <see cref="NaturalStringComparer"/> class.
    /// </summary>
    public static NaturalStringComparer Instance { get; } = new NaturalStringComparer();

    /// <summary>
    /// Compares two strings and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    /// <returns>A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>.</returns>
    public int Compare(string x, string y)
    {
        if (x == y)
        {
            return 0;
        }
        if (x == null)
        {
            return -1;
        }
        if (y == null)
        {
            return 1;
        }

        int ix = 0, iy = 0;
        while (ix < x.Length && iy < y.Length)
        {
            // Check for Arabic numerals
            if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
            {
                // Extract numerical parts
                var nx = GetNumber(x, ref ix);
                var ny = GetNumber(y, ref iy);
                if (nx != ny)
                {
                    return nx.CompareTo(ny);
                }
            }
            // Check for Roman numerals
            else if (IsRomanNumeral(x, ix) && IsRomanNumeral(y, iy))
            {
                // Extract Roman numeral values
                var rx = GetRomanValue(x, ref ix);
                var ry = GetRomanValue(y, ref iy);
                if (rx != ry)
                {
                    return rx.CompareTo(ry);
                }
            }
            else
            {
                if (x[ix] != y[iy])
                {
                    return x[ix].CompareTo(y[iy]);
                }
                ix++;
                iy++;
            }
        }
        return x.Length - y.Length;
    }
    private static long GetNumber(string s, ref int index)
    {
        long number = 0;
        while (index < s.Length && char.IsDigit(s[index]))
        {
            number = (number * 10) + s[index] - '0';
            index++;
        }
        return number;
    }

    /// <summary>
    /// Checks if the characters starting at the given index form a Roman numeral.
    /// </summary>
    private static bool IsRomanNumeral(string s, int index)
    {
        if (index >= s.Length)
        {
            return false;
        }

        // Check if character is a valid Roman numeral character
        var c = char.ToUpper(s[index]);
        if (c != 'I' && c != 'V' && c != 'X' && c != 'L' && c != 'C' && c != 'D' && c != 'M')
        {
            return false;
        }

        // Look ahead to see if we have a sequence of Roman numeral characters
        var i = index;
        var romanChars = 0;

        while (i < s.Length)
        {
            c = char.ToUpper(s[i]);
            if (c != 'I' && c != 'V' && c != 'X' && c != 'L' && c != 'C' && c != 'D' && c != 'M')
            {
                break;
            }

            romanChars++;
            i++;

            // To avoid misidentifying regular words with Roman numeral characters
            if (romanChars > 15) // Arbitrary limit to prevent infinite loops
            {
                return false;
            }
        }

        // Verify there's at least one valid Roman numeral
        return romanChars > 0 && IsValidRomanNumeral(s.Substring(index, romanChars).ToUpper());
    }

    /// <summary>
    /// Extracts and computes the value of a Roman numeral from a string starting at the specified index.
    /// </summary>
    private static int GetRomanValue(string s, ref int index)
    {
        var startIndex = index;
        var length = 0;

        // Find the end of the Roman numeral
        while (index < s.Length)
        {
            var c = char.ToUpper(s[index]);
            if (c != 'I' && c != 'V' && c != 'X' && c != 'L' && c != 'C' && c != 'D' && c != 'M')
            {
                break;
            }

            length++;
            index++;

            // Safety check
            if (length > 15)
            {
                break;
            }
        }

        // Extract and parse the Roman numeral
        var roman = s.Substring(startIndex, length).ToUpper();
        return RomanToInt(roman);
    }

    private static readonly FrozenDictionary<string, int> _romanMap = FrozenDictionary.ToFrozenDictionary(
    [
        // Add most common cases here too, enables a fast path
        new KeyValuePair<string, int>("I", 1),
        new KeyValuePair<string, int>("II", 2),
        new KeyValuePair<string, int>("III", 3),
        new KeyValuePair<string, int>("IV", 4),
        new KeyValuePair<string, int>("V", 5),
        new KeyValuePair<string, int>("VI", 6),
        new KeyValuePair<string, int>("VII", 7),
        new KeyValuePair<string, int>("VIII", 8),
        new KeyValuePair<string, int>("IX", 9),
        new KeyValuePair<string, int>("X", 10),
        new KeyValuePair<string, int>("XI", 11),
        new KeyValuePair<string, int>("XII", 12),
        new KeyValuePair<string, int>("XIII", 13),
        new KeyValuePair<string, int>("XIV", 14),
        new KeyValuePair<string, int>("XV", 15),
        new KeyValuePair<string, int>("XVI", 16),
        new KeyValuePair<string, int>("XVII", 17),
        new KeyValuePair<string, int>("XVIII", 18),
        new KeyValuePair<string, int>("XIX", 19),
        new KeyValuePair<string, int>("XX", 20),

        new KeyValuePair<string, int>("L", 50),
        new KeyValuePair<string, int>("C", 100),
        new KeyValuePair<string, int>("D", 500),
        new KeyValuePair<string, int>("M", 1000)
    ]);
    private static FrozenDictionary<string, int>.AlternateLookup<ReadOnlySpan<char>> AlternateLookup
    {
        get
        {
            _ = _romanMap.TryGetAlternateLookup<ReadOnlySpan<char>>(out var lookup);
            return lookup;
        }
    }

    /// <summary>
    /// Converts a Roman numeral string to an integer.
    /// </summary>
    private static int RomanToInt(string roman)
    {
        if (string.IsNullOrEmpty(roman))
        {
            return 0;
        }

        var result = 0;
        var prevValue = 0;

        if (_romanMap.TryGetValue(roman, out var value))
        {
            return value;
        }

        // Process from right to left
        var alt = AlternateLookup;
        var span = roman.AsSpan();
        for (var i = roman.Length - 1; i >= 0; i--)
        {
            var currentValue = alt[span[i..i]];

            // If current value is greater than or equal to previous value, add it
            // Otherwise subtract it (handles cases like IV, IX, etc.)
            if (currentValue >= prevValue)
            {
                result += currentValue;
            }
            else
            {
                result -= currentValue;
            }

            prevValue = currentValue;
        }

        return result;
    }

    /// <summary>
    /// Validates that a string is a properly formatted Roman numeral.
    /// </summary>
    private static bool IsValidRomanNumeral(string roman)
    {
        if (string.IsNullOrEmpty(roman))
        {
            return false;
        }

        return _romanNumeralRegex.IsMatch(roman);
    }

    private static readonly Regex _romanNumeralRegex = RomanNumeralRegex();
    [GeneratedRegex("^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$")]
    private static partial Regex RomanNumeralRegex();
}
