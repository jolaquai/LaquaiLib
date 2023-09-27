using System.Globalization;
using System.Text.RegularExpressions;

using LaquaiLib.Extensions;

namespace LaquaiLib.Util;

/// <summary>
/// Provides helper methods for <see cref="CultureInfo"/>.
/// </summary>
public static class CultureInfoHelper
{
    /// <summary>
    /// The similarity threshold for <see cref="CultureInfo"/> names (used for <see cref="ResolveData(string?)"/>). If the similarity of a <see cref="CultureInfo"/> name to the given data is greater than or equal to this value, the <see cref="CultureInfo"/> is considered a match.
    /// </summary>
    /// <remarks>
    /// The default value for this property is <c>0.7</c>.
    /// </remarks>
    public static double SimilarityThreshold { get; set; } = 0.7;

    private static readonly Dictionary<string, CultureInfo[]> _cultureMap;

    private static readonly Regex _similarityCleanerRegex = new Regex("[^a-zà-ÿ ]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// A <see cref="Dictionary{TKey, TValue}"/> of <see cref="CultureInfo"/> mapping overrides. When a <see cref="CultureInfo"/> is resolved using <see cref="ResolveData(string?)"/>, before any other matching is done, the input to the method is checked against this <see cref="Dictionary{TKey, TValue}"/>. If a match is found, the <see cref="CultureInfo"/> from the <see cref="KeyValuePair{TKey, TValue}.Value"/> is returned.
    /// </summary>
    /// <remarks>
    /// Contrary to all other matching, this matching is case-sensitive. If an override is added for, for example, <c>"united states"</c> with a value of <c>"en-US"</c>, the inputs <c>"United states"</c>, <c>"united States"</c>, <c>"United States"</c> or <c>"UNITED STATES"</c> will not match the override.
    /// </remarks>
    public static Dictionary<string, CultureInfo> Overrides { get; } = new Dictionary<string, CultureInfo>();

    static CultureInfoHelper()
    {
        _cultureMap = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .GroupBy(ci => ci.Name.Split('-')[0])
            .ToArrayDictionary();
    }

    /// <summary>
    /// Resolves a <see cref="CultureInfo"/> from data in a <see cref="string"/>. See the parameter documentation for more information.
    /// </summary>
    /// <param name="cultureName">
    /// The data to analyze and resolve to a <see cref="CultureInfo"/>. The following formats are supported:
    /// <list type="bullet">
    /// <item/>A standard <see cref="CultureInfo"/> identifier in <c>languagecode2-country/regioncode2</c> format (<see cref="CultureInfo.Name"/>), e.g. <c>en-US</c>.
    /// <item/>A standard (generic) <see cref="CultureInfo"/> identifier in <c>languagecode2</c> format (<see cref="CultureInfo.TwoLetterISOLanguageName"/>), e.g. <c>en</c>.
    /// <item/>One of the following special values: <see langword="null"/>, <see cref="string.Empty"/>, whitespace-only, <c>Invariant</c> (which all resolve to <see cref="CultureInfo.InvariantCulture"/>), <c>Current</c> (which resolves to <see cref="CultureInfo.CurrentCulture"/>), <c>UI</c> (which resolves to <see cref="CultureInfo.CurrentUICulture"/>).
    /// <item/>One of the generic culture identifiers that are returned by <see cref="CultureInfo.ThreeLetterISOLanguageName"/>.
    /// <item/>One of the specific culture identifiers that are returned by <see cref="CultureInfo.DisplayName"/>, <see cref="CultureInfo.EnglishName"/>, <see cref="CultureInfo.NativeName"/> or <see cref="CultureInfo.ThreeLetterWindowsLanguageName"/>, or a partial match of one of these (however, exact matches are always higher priority and, as such, evaluated first).
    /// </list>
    /// </param>
    /// <returns>A <see cref="CultureInfo"/> instance that matches the given data, or <see langword="null"/> if no match was found.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item/>"Natural" preferences (such as the expectation that <c>United States</c> should return the <see cref="CultureInfo"/> for <c>en-US</c> and not <c>chr-US</c>) cannot be taken into account. As such, you may use <see cref="Overrides"/> to manually cater to preferences within your application.
    /// </list>
    /// </remarks>
    public static CultureInfo? ResolveData(string? cultureName)
    {
        if (cultureName is not null && Overrides.TryGetValue(cultureName, out var overrideCulture))
        {
            return overrideCulture;
        }

        if (string.IsNullOrWhiteSpace(cultureName) || cultureName.Equals("Invariant", StringComparison.OrdinalIgnoreCase))
        {
            return CultureInfo.InvariantCulture;
        }
        if (cultureName.Equals("Current", StringComparison.OrdinalIgnoreCase))
        {
            return CultureInfo.CurrentCulture;
        }
        if (cultureName.Equals("UI", StringComparison.OrdinalIgnoreCase))
        {
            return CultureInfo.CurrentUICulture;
        }

        // Try some shortcuts first before doing the heavy lifting of iterating through all cultures
        if (cultureName.Length == 2 && _cultureMap.TryGetValue(cultureName, out var twoLetterCultures))
        {
            return twoLetterCultures.FirstOrDefault(ci => ci.TwoLetterISOLanguageName.Equals(cultureName, StringComparison.OrdinalIgnoreCase));
        }
        if (cultureName.Length == 5 && cultureName[2] == '-' && _cultureMap.TryGetValue(cultureName[..2], out var threeLetterCultures))
        {
            if (threeLetterCultures.FirstOrDefault(ci => ci.Name.Equals(cultureName, StringComparison.OrdinalIgnoreCase)) is CultureInfo exactMatch)
            {
                return exactMatch;
            }
            // If there is no exact match, return the first culture that matches the language code and ignore the region code
            return threeLetterCultures.FirstOrDefault(ci => ci.TwoLetterISOLanguageName.Equals(cultureName[..2], StringComparison.OrdinalIgnoreCase));
        }

        // If we didn't find a match yet, iterate through all cultures
        var flattened = _cultureMap.Values.SelectMany().AsParallel();
        // Specific culture matches
        switch (cultureName.Length)
        {
            case 2:
                // CultureInfo.TwoLetterISOLanguageName match
                if (flattened.FirstOrDefault(ci => ci.TwoLetterISOLanguageName.Equals(cultureName, StringComparison.OrdinalIgnoreCase)) is CultureInfo twoLetterMatch)
                {
                    return twoLetterMatch;
                }
                break;
            case 3:
                // CultureInfo.ThreeLetterISOLanguageName or CultureInfo.ThreeLetterWindowsLanguageName match
                // LastOrDefault because we want to prioritize more specific matches
                if (flattened.LastOrDefault(ci =>
                    ci.ThreeLetterISOLanguageName.Equals(cultureName, StringComparison.OrdinalIgnoreCase)
                    || ci.ThreeLetterWindowsLanguageName.Equals(cultureName, StringComparison.OrdinalIgnoreCase)
                ) is CultureInfo threeLetterMatch)
                {
                    return threeLetterMatch;
                }
                break;
        }

        // Full match of any name
        if (flattened.FirstOrDefault(ci =>
            ci.Name.Equals(cultureName, StringComparison.OrdinalIgnoreCase)
            || ci.DisplayName.Equals(cultureName, StringComparison.OrdinalIgnoreCase)
            || ci.EnglishName.Equals(cultureName, StringComparison.OrdinalIgnoreCase)
            || ci.NativeName.Equals(cultureName, StringComparison.OrdinalIgnoreCase)
        ) is CultureInfo fullMatch)
        {
            return fullMatch;
        }

        // StartsWith
        if (flattened.FirstOrDefault(ci =>
            ci.Name.StartsWith(cultureName, StringComparison.OrdinalIgnoreCase)
            || ci.DisplayName.StartsWith(cultureName, StringComparison.OrdinalIgnoreCase)
            || ci.EnglishName.StartsWith(cultureName, StringComparison.OrdinalIgnoreCase)
            || ci.NativeName.StartsWith(cultureName, StringComparison.OrdinalIgnoreCase)
        ) is CultureInfo partialMatch)
        {
            return partialMatch;
        }

        // Contains
        if (flattened.FirstOrDefault(ci =>
            ci.Name.Contains(cultureName, StringComparison.OrdinalIgnoreCase)
            || ci.DisplayName.Contains(cultureName, StringComparison.OrdinalIgnoreCase)
            || ci.EnglishName.Contains(cultureName, StringComparison.OrdinalIgnoreCase)
            || ci.NativeName.Contains(cultureName, StringComparison.OrdinalIgnoreCase)
        ) is CultureInfo containsMatch)
        {
            return containsMatch;
        }

        // Similarity
        if (flattened.FirstOrDefault(ci =>
            _similarityCleanerRegex.Replace(ci.Name, "").GetSimilarity(cultureName) >= SimilarityThreshold
            || _similarityCleanerRegex.Replace(ci.DisplayName, "").GetSimilarity(cultureName) >= SimilarityThreshold
            || _similarityCleanerRegex.Replace(ci.EnglishName, "").GetSimilarity(cultureName) >= SimilarityThreshold
            || _similarityCleanerRegex.Replace(ci.NativeName, "").GetSimilarity(cultureName) >= SimilarityThreshold
        ) is CultureInfo similarityMatch)
        {
            return similarityMatch;
        }

        return null;
    }
}
