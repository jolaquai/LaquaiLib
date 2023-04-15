namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="char"/> Type.
/// </summary>
public static class CharExtensions
{
    /// <summary>
    /// Determines if a specified <see cref="char"/> is a vowel (including 'y').
    /// </summary>
    /// <param name="character">The <see cref="char"/> to check.</param>
    /// <returns><c>true</c> if <paramref name="character"/> is a vowel, otherwise, <c>false</c>.</returns>
    public static bool IsVowel(this char character) => "aeiouy".Contains(char.ToLower(character));

    /// <summary>
    /// Determines if a specified <see cref="char"/> is a consonant.
    /// </summary>
    /// <param name="character">The <see cref="char"/> to check.</param>
    /// <returns><c>true</c> if <paramref name="character"/> is a consonant, otherwise, <c>false</c>.</returns>
    public static bool IsConsonant(this char character) => !character.IsVowel();
}
