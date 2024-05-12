namespace LaquaiLib.Util;

/// <summary>
/// Represents a <see cref="IComparer{T}"/> that compares strings using a natural sort order, that is, like Windows Explorer sorts file names.
/// </summary>
public class NaturalStringComparer : IComparer<string>
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
    public int Compare(string? x, string? y)
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
            number = number * 10 + s[index] - '0';
            index++;
        }
        return number;
    }
}
