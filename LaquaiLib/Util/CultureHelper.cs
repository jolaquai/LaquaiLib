using System.Globalization;
using System.Text.RegularExpressions;

using LaquaiLib.Extensions;

namespace LaquaiLib.Util;

/// <summary>
/// Provides helper methods for <see cref="CultureInfo"/>, <see cref="TimeZoneInfo"/> and related types.
/// </summary>
public static partial class CultureHelper
{
    /// <summary>
    /// The similarity threshold for <see cref="CultureInfo"/> names (used for <see cref="ResolveToCultureInfo(string?)"/> and <see cref="ResolveToTimeZoneInfo(string?)"/>). If the similarity of a <see cref="CultureInfo"/> name to the given data is greater than or equal to this value, the <see cref="CultureInfo"/> is considered a match.
    /// </summary>
    /// <remarks>
    /// The default value for this property is <c>0.7</c>.
    /// </remarks>
    public static double SimilarityThreshold { get; set; } = 0.7;

    private static readonly Dictionary<string, CultureInfo[]> _cultureMap;

    private static readonly Regex _similarityCleanerRegex = GetSimilarityCleanerRegex();

    /// <summary>
    /// A <see cref="Dictionary{TKey, TValue}"/> of <see cref="CultureInfo"/> mapping overrides. When a <see cref="CultureInfo"/> is resolved using <see cref="ResolveToCultureInfo(string?)"/>, before any other matching is done, the input to the method is checked against this <see cref="Dictionary{TKey, TValue}"/>. If a match is found, the <see cref="CultureInfo"/> from the <see cref="KeyValuePair{TKey, TValue}.Value"/> is returned.
    /// </summary>
    /// <remarks>
    /// Contrary to all other matching, this matching is case-sensitive. If an override is added for, for example, <c>"united states"</c> with a value of <c>"en-US"</c>, the inputs <c>"United states"</c>, <c>"united States"</c>, <c>"United States"</c> or <c>"UNITED STATES"</c> will not match the override.
    /// </remarks>
    public static Dictionary<string, CultureInfo> CultureInfoOverrides { get; } = [];

    private static readonly Dictionary<string[], TimeZoneInfo> _timeZoneMap;

    static CultureHelper()
    {
        _cultureMap = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .GroupBy(ci => ci.Name.Split('-')[0])
            .ToArrayDictionary();

        _timeZoneMap = TimeZoneInfo.GetSystemTimeZones().ToDictionary(
            tz => tz.DisplayName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(split => split.Trim('(', ')', ',', ' '))
                .ToArray()
        );
    }

    /// <summary>
    /// Resolves a <see cref="CultureInfo"/> from data in a <see cref="string"/>. See the parameter documentation for more information.
    /// </summary>
    /// <param name="data">
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
    /// <item/>"Natural" preferences (such as the expectation that <c>United States</c> should return the <see cref="CultureInfo"/> for <c>en-US</c> and not <c>chr-US</c>) cannot be taken into account. As such, you may use <see cref="CultureInfoOverrides"/> to manually cater to preferences within your application.
    /// </list>
    /// </remarks>
    public static CultureInfo? ResolveToCultureInfo(string? data)
    {
        if (data is not null && CultureInfoOverrides.TryGetValue(data, out var overrideCulture))
        {
            return overrideCulture;
        }

        if (string.IsNullOrWhiteSpace(data) || data.Equals("Invariant", StringComparison.OrdinalIgnoreCase))
        {
            return CultureInfo.InvariantCulture;
        }
        if (data.Equals("Current", StringComparison.OrdinalIgnoreCase))
        {
            return CultureInfo.CurrentCulture;
        }
        if (data.Equals("UI", StringComparison.OrdinalIgnoreCase))
        {
            return CultureInfo.CurrentUICulture;
        }

        // Try some shortcuts first before doing the heavy lifting of iterating through all cultures
        if (data.Length == 2 && _cultureMap.TryGetValue(data, out var twoLetterCultures))
        {
            return Array.Find(twoLetterCultures, ci => ci.TwoLetterISOLanguageName.Equals(data, StringComparison.OrdinalIgnoreCase));
        }
        if (data.Length == 5 && data[2] == '-' && _cultureMap.TryGetValue(data[..2], out var threeLetterCultures))
        {
            if (Array.Find(threeLetterCultures, ci => ci.Name.Equals(data, StringComparison.OrdinalIgnoreCase)) is CultureInfo exactMatch)
            {
                return exactMatch;
            }
            // If there is no exact match, return the first culture that matches the language code and ignore the region code
            return Array.Find(threeLetterCultures, ci => ci.TwoLetterISOLanguageName.Equals(data[..2], StringComparison.OrdinalIgnoreCase));
        }

        // If we didn't find a match yet, iterate through all cultures
        var flattened = _cultureMap.Values.SelectMany().AsParallel();
        // Specific culture matches
        switch (data.Length)
        {
            case 2:
                // CultureInfo.TwoLetterISOLanguageName match
                if (flattened.FirstOrDefault(ci => ci.TwoLetterISOLanguageName.Equals(data, StringComparison.OrdinalIgnoreCase)) is CultureInfo twoLetterMatch)
                {
                    return twoLetterMatch;
                }
                break;
            case 3:
                // CultureInfo.ThreeLetterISOLanguageName or CultureInfo.ThreeLetterWindowsLanguageName match
                // LastOrDefault because we want to prioritize more specific matches
                if (flattened.LastOrDefault(ci =>
                    ci.ThreeLetterISOLanguageName.Equals(data, StringComparison.OrdinalIgnoreCase)
                    || ci.ThreeLetterWindowsLanguageName.Equals(data, StringComparison.OrdinalIgnoreCase)
                ) is CultureInfo threeLetterMatch)
                {
                    return threeLetterMatch;
                }
                break;
        }

        // Full match of any name
        if (flattened.FirstOrDefault(ci =>
            ci.Name.Equals(data, StringComparison.OrdinalIgnoreCase)
            || ci.DisplayName.Equals(data, StringComparison.OrdinalIgnoreCase)
            || ci.EnglishName.Equals(data, StringComparison.OrdinalIgnoreCase)
            || ci.NativeName.Equals(data, StringComparison.OrdinalIgnoreCase)
        ) is CultureInfo fullMatch)
        {
            return fullMatch;
        }

        // StartsWith
        if (flattened.FirstOrDefault(ci =>
            ci.Name.StartsWith(data, StringComparison.OrdinalIgnoreCase)
            || ci.DisplayName.StartsWith(data, StringComparison.OrdinalIgnoreCase)
            || ci.EnglishName.StartsWith(data, StringComparison.OrdinalIgnoreCase)
            || ci.NativeName.StartsWith(data, StringComparison.OrdinalIgnoreCase)
        ) is CultureInfo partialMatch)
        {
            return partialMatch;
        }

        // Contains
        if (flattened.FirstOrDefault(ci =>
            ci.Name.Contains(data, StringComparison.OrdinalIgnoreCase)
            || ci.DisplayName.Contains(data, StringComparison.OrdinalIgnoreCase)
            || ci.EnglishName.Contains(data, StringComparison.OrdinalIgnoreCase)
            || ci.NativeName.Contains(data, StringComparison.OrdinalIgnoreCase)
        ) is CultureInfo containsMatch)
        {
            return containsMatch;
        }

        // Similarity
        if (flattened.FirstOrDefault(ci =>
            _similarityCleanerRegex.Replace(ci.Name, "").GetSimilarity(data) >= SimilarityThreshold
            || _similarityCleanerRegex.Replace(ci.DisplayName, "").GetSimilarity(data) >= SimilarityThreshold
            || _similarityCleanerRegex.Replace(ci.EnglishName, "").GetSimilarity(data) >= SimilarityThreshold
            || _similarityCleanerRegex.Replace(ci.NativeName, "").GetSimilarity(data) >= SimilarityThreshold
        ) is CultureInfo similarityMatch)
        {
            return similarityMatch;
        }

        return null;
    }

    /// <summary>
    /// Resolves a <see cref="TimeZoneInfo"/> from data in a <see cref="string"/>. See the parameter documentation for more information.
    /// </summary>
    /// <param name="data">
    /// The data to analyze and resolve to a <see cref="TimeZoneInfo"/>. The following formats are supported:
    /// <list type="bullet">
    /// <item/>A <see cref="TimeZoneInfo"/>'s <see cref="TimeZoneInfo.Id"/>, <see cref="TimeZoneInfo.DisplayName"/>, <see cref="TimeZoneInfo.StandardName"/> or <see cref="TimeZoneInfo.DaylightName"/>, or a partial match of one of these (however, exact matches are always higher priority and, as such, evaluated first).
    /// <item/>One of the following special values: <see langword="null"/>, <see cref="string.Empty"/>, whitespace-only, <c>Utc</c> (which all resolve to <see cref="TimeZoneInfo.Utc"/>), <c>Current</c> or <c>Local</c> (which resolve to <see cref="TimeZoneInfo.Local"/>).
    /// </list>
    /// </param>
    /// <returns>A <see cref="TimeZoneInfo"/> instance that matches the given data, or <see langword="null"/> if no match was found.</returns>
    public static TimeZoneInfo? ResolveToTimeZoneInfo(string? data)
    {
        if (string.IsNullOrWhiteSpace(data) || data.Equals("Utc", StringComparison.OrdinalIgnoreCase))
        {
            return TimeZoneInfo.Utc;
        }
        if (data.Equals("Current", StringComparison.OrdinalIgnoreCase) || data.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            return TimeZoneInfo.Local;
        }

        if (TimeZoneInfo.TryFindSystemTimeZoneById(data, out var idMatch))
        {
            return idMatch;
        }
        try
        {
            return TimeZoneInfo.FromSerializedString(data);
        }
        catch { }

        Task<TimeZoneInfo?>[] tasks =
        [
            Task.Run(() =>
            {
                var timeZones = TimeZoneInfo.GetSystemTimeZones().AsParallel();

                // Exact match
                if (timeZones.FirstOrDefault(tz =>
                    tz.Id.Equals(data, StringComparison.OrdinalIgnoreCase)
                    || tz.DisplayName.Equals(data, StringComparison.OrdinalIgnoreCase)
                    || tz.StandardName.Equals(data, StringComparison.OrdinalIgnoreCase)
                    || tz.DaylightName.Equals(data, StringComparison.OrdinalIgnoreCase)
                ) is TimeZoneInfo exactMatch)
                {
                    return exactMatch;
                }

                // StartsWith
                if (timeZones.FirstOrDefault(tz =>
                    tz.Id.StartsWith(data, StringComparison.OrdinalIgnoreCase)
                    || tz.DisplayName.StartsWith(data, StringComparison.OrdinalIgnoreCase)
                    || tz.StandardName.StartsWith(data, StringComparison.OrdinalIgnoreCase)
                    || tz.DaylightName.StartsWith(data, StringComparison.OrdinalIgnoreCase)
                ) is TimeZoneInfo partialMatch)
                {
                    return partialMatch;
                }

                // Contains
                if (timeZones.FirstOrDefault(tz =>
                    tz.Id.Contains(data, StringComparison.OrdinalIgnoreCase)
                    || tz.DisplayName.Contains(data, StringComparison.OrdinalIgnoreCase)
                    || tz.StandardName.Contains(data, StringComparison.OrdinalIgnoreCase)
                    || tz.DaylightName.Contains(data, StringComparison.OrdinalIgnoreCase)
                ) is TimeZoneInfo containsMatch)
                {
                    return containsMatch;
                }

                // Similarity
                if (timeZones.FirstOrDefault(tz =>
                    _similarityCleanerRegex.Replace(tz.Id, "").GetSimilarity(data) >= SimilarityThreshold
                    || _similarityCleanerRegex.Replace(tz.DisplayName, "").GetSimilarity(data) >= SimilarityThreshold
                    || _similarityCleanerRegex.Replace(tz.StandardName, "").GetSimilarity(data) >= SimilarityThreshold
                    || _similarityCleanerRegex.Replace(tz.DaylightName, "").GetSimilarity(data) >= SimilarityThreshold
                ) is TimeZoneInfo similarityMatch)
                {
                    return similarityMatch;
                }

                return null;
            }),
            Task.Run(() =>
            {
                // Same in _timeZoneMap
                var keys = _timeZoneMap.Keys.AsParallel();

                // Exact match
                if (keys.FirstOrDefault(key => key.Any(k => k.Equals(data, StringComparison.OrdinalIgnoreCase))) is string[] exactMatchKey)
                {
                    return _timeZoneMap[exactMatchKey];
                }

                // StartsWith
                if (keys.FirstOrDefault(key => key.Any(k => k.StartsWith(data, StringComparison.OrdinalIgnoreCase))) is string[] partialMatchKey)
                {
                    return _timeZoneMap[partialMatchKey];
                }

                // Contains
                if (keys.FirstOrDefault(key => key.Any(k => k.Contains(data, StringComparison.OrdinalIgnoreCase))) is string[] containsMatchKey)
                {
                    return _timeZoneMap[containsMatchKey];
                }

                // Similarity
                if (keys.FirstOrDefault(key => key.Any(k => _similarityCleanerRegex.Replace(k, "").GetSimilarity(data) >= SimilarityThreshold)) is string[] similarityMatchKey)
                {
                    return _timeZoneMap[similarityMatchKey];
                }

                return null;
            })
        ];

        Task.WaitAll(tasks);

        if (tasks[0].Result is not null)
        {
            if (tasks[1].Result is not null)
            {
                if (tasks[0].Result != tasks[1].Result)
                {
                    // Ambiguous matches
                    return null;
                }
                else
                {
                    return tasks[0].Result;
                }
            }
            else
            {
                return tasks[0].Result;
            }
        }
        else if (tasks[1].Result is not null)
        {
            return tasks[1].Result;
        }

        return null;
    }

    [GeneratedRegex("[^a-zà-ÿ ]", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex GetSimilarityCleanerRegex();
}
