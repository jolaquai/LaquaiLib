using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using LaquaiLib.Core;

namespace LaquaiLib.Util;

/// <summary>
/// Implements an <see cref="IComparer{T}"/> that compares <see langword="string"/>s and <see cref="ReadOnlySpan{T}"/> of <see langword="char"/> using a natural sort order, that is, like Windows Explorer sorts file names.
/// </summary>
public partial class NaturalStringComparer : IComparer<string>, IComparer<ReadOnlySpan<char>>, IEqualityComparer<string>, IEqualityComparer<ReadOnlySpan<char>>
{
    /// <summary>
    /// Gets the default instance of <see cref="NaturalStringComparer"/>. Character comparisons are done using case-insensitive ordinal rules.
    /// </summary>
    public static NaturalStringComparer Default { get; } = new NaturalStringComparer();
    /// <summary>
    /// Gets an instance of <see cref="NaturalStringComparer"/> that compares all non-digit, non-letter and non-Roman numeral characters as equal.
    /// </summary>
    public static NaturalStringComparer LenientEquality { get; } = new NaturalStringComparer(true);

    private readonly bool _lenient;

    /// <summary>
    /// Initializes a new <see cref="NaturalStringComparer"/>.
    /// </summary>
    public NaturalStringComparer() { }
    /// <summary>
    /// Initializes a new <see cref="NaturalStringComparer"/> with the specified <paramref name="lenientEquality"/> mode. See <see cref="LenientEquality"/>.
    /// </summary>
    /// <param name="lenientEquality">Whether to treat all non-digit, non-letter and non-Roman numeral characters as equal.</param>
    public NaturalStringComparer(bool lenientEquality) => _lenient = lenientEquality;

    /// <summary>
    /// Compares two <see langword="string"/>s and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// Empty <see langword="string"/>s are sorted to the bottom.
    /// </summary>
    /// <param name="x">The first <see langword="string"/> to compare.</param>
    /// <param name="y">The second <see langword="string"/> to compare.</param>
    /// <returns>A signed integer that indicates the result of the comparison. A negative value indicates that <paramref name="x"/> is less than <paramref name="y"/>, zero indicates that they are equal, and a positive value indicates that <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public int Compare(string x, string y) => Compare(x.AsSpan(), y.AsSpan());
    /// <summary>
    /// Compares two <see cref="ReadOnlySpan{T}"/> of <see langword="char"/> and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// Empty spans are sorted to the bottom.
    /// </summary>
    /// <param name="x">The first <see cref="ReadOnlySpan{T}"/> of <see langword="char"/> to compare.</param>
    /// <param name="y">The second <see cref="ReadOnlySpan{T}"/> of <see langword="char"/> to compare.</param>
    /// <returns>A signed integer that indicates the result of the comparison. A negative value indicates that <paramref name="x"/> is less than <paramref name="y"/>, zero indicates that they are equal, and a positive value indicates that <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
    public int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
    {
        if (x.IsEmpty && y.IsEmpty)
        {
            return 0;
        }
        if (x.IsEmpty)
        {
            return -1;
        }
        if (y.IsEmpty)
        {
            return 1;
        }

        var len = x.Length + y.Length;
        Span<char> chars = len <= Configuration.MaxStackallocSize / sizeof(char) ? stackalloc char[len] : new char[len];
        var left = chars[..x.Length];
        var right = chars[x.Length..];
        x.ToUpperInvariant(left);
        y.ToUpperInvariant(right);

        if (left.SequenceEqual(right))
        {
            return 0;
        }

        int ix = 0, iy = 0;
        while (ix < left.Length && iy < right.Length)
        {
            // Check for Arabic numerals
            if (char.IsDigit(left[ix]) && char.IsDigit(right[iy]))
            {
                // Extract numerical parts
                var nx = GetNumber(left, ref ix);
                var ny = GetNumber(right, ref iy);
                if (nx != ny)
                {
                    return nx.CompareTo(ny);
                }
            }
            // Check for Roman numerals
            else if (IsRomanNumeral(left, ix) && IsRomanNumeral(right, iy))
            {
                // Extract Roman numeral values
                var rx = GetRomanValue(left, ref ix);
                var ry = GetRomanValue(right, ref iy);
                if (rx != ry)
                {
                    return rx.CompareTo(ry);
                }
            }
            else if (_lenient && !char.IsLetterOrDigit(left[ix]) && !char.IsLetterOrDigit(right[iy]))
            {
                // Ignore non-letter, non-digit characters
                ix++;
                iy++;
            }
            else if (_lenient && !char.IsLetterOrDigit(left[ix]))
            {
                ix++;
            }
            else if (_lenient && !char.IsLetterOrDigit(right[iy]))
            {
                iy++;
            }
            else
            {
                if (left[ix] != right[iy])
                {
                    return left[ix].CompareTo(right[iy]);
                }
                ix++;
                iy++;
            }
        }
        return left.Length - right.Length;
    }
    private static long GetNumber(ReadOnlySpan<char> s, ref int index)
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
    private static bool IsRomanNumeral(ReadOnlySpan<char> s, int index)
    {
        if (index >= s.Length)
        {
            return false;
        }

        // Escape hatch if our previous char is a Roman numeral character
        // If it was valid, it should be impossible to be in here.
        // If it wasn't, we shouldn't be in here because starting Roman identification partway through can't be correct
        if (index > 0 && s[index - 1] is 'I' or 'V' or 'X' or 'L' or 'C' or 'D' or 'M')
        {
            return false;
        }

        // Check if character is a valid Roman numeral character
        var c = s[index];
        if (c is not ('I' or 'V' or 'X' or 'L' or 'C' or 'D' or 'M'))
        {
            return false;
        }

        // Look ahead to see if we have a sequence of Roman numeral characters
        var i = index;
        var romanChars = 0;

        while (i < s.Length)
        {
            c = s[i];
            if (c is not ('I' or 'V' or 'X' or 'L' or 'C' or 'D' or 'M'))
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
        return romanChars > 0 && IsValidRomanNumeral(s.Slice(index, romanChars));
    }
    /// <summary>
    /// Extracts and computes the value of a Roman numeral from a string starting at the specified index.
    /// </summary>
    private static int GetRomanValue(ReadOnlySpan<char> s, ref int index)
    {
        var startIndex = index;
        var length = 0;

        // Find the end of the Roman numeral
        while (index < s.Length)
        {
            var c = s[index];
            if (c is not ('I' or 'V' or 'X' or 'L' or 'C' or 'D' or 'M'))
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
        return RomanToInt(s.Slice(startIndex, length));
    }

    #region private static readonly FrozenDictionary<string, int> _romanMap
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
    #endregion

    /// <summary>
    /// Converts a Roman numeral string to an integer.
    /// </summary>
    private static int RomanToInt(ReadOnlySpan<char> roman)
    {
        if (roman.Length == 0)
        {
            return 0;
        }

        var result = 0;
        var prevValue = 0;

        var alt = AlternateLookup;
        if (alt.TryGetValue(roman, out var value))
        {
            return value;
        }

        // Process from right to left
        Span<char> upper = roman.Length <= Configuration.MaxStackallocSize / sizeof(char) ? stackalloc char[roman.Length] : new char[roman.Length];
        roman.ToUpperInvariant(upper);
        for (var i = roman.Length - 1; i >= 0; i--)
        {
            var currentValue = alt[upper[i..(i + 1)]];

            // If current value is greater than or equal to previous value, add it
            // Otherwise subtract it (handles cases like IV, IX, etc.)
            if (currentValue >= prevValue)
            {
                result += prevValue = currentValue;
            }
            else
            {
                result -= prevValue = currentValue;
            }
        }

        return result;
    }
    /// <summary>
    /// Validates that a string is a properly formatted Roman numeral.
    /// </summary>
    private static bool IsValidRomanNumeral(ReadOnlySpan<char> roman)
    {
        if (roman.Length == 0)
        {
            return false;
        }

        return _romanNumeralRegex.IsMatch(roman);
    }

    private static readonly Regex _romanNumeralRegex = RomanNumeralRegex();
    [GeneratedRegex("^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$", RegexOptions.ExplicitCapture)]
    private static partial Regex RomanNumeralRegex();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public bool Equals(string x, string y) => Equals(x.AsSpan(), y.AsSpan());
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y) => Compare(x, y) == 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    public int GetHashCode([DisallowNull] string obj) => GetHashCode(obj.AsSpan());
    public int GetHashCode([DisallowNull] ReadOnlySpan<char> span)
    {
        HashCode hashCode = default;

        var comp = CharComparer.OrdinalIgnoreCase;
        for (var i = span.Length >= 8 ? span.Length - 8 : 0; i < span.Length; i++)
        {
            if (!_lenient || char.IsLetterOrDigit(span[i]))
            {
                hashCode.Add(comp.GetHashCode(span[i]));
            }
            else
            {
                hashCode.Add('\uE000');
            }
        }

        return hashCode.ToHashCode();
    }
}
