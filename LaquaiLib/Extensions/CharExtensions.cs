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
    /// <returns><see langword="true"/> if <paramref name="character"/> is a vowel, otherwise, <see langword="false"/>.</returns>
    public static bool IsVowel(this char character) => "aeiouy".Contains(char.ToLower(character));

    /// <summary>
    /// Determines if a specified <see cref="char"/> is a consonant.
    /// </summary>
    /// <param name="character">The <see cref="char"/> to check.</param>
    /// <returns><see langword="true"/> if <paramref name="character"/> is a consonant, otherwise, <see langword="false"/>.</returns>
    public static bool IsConsonant(this char character) => !character.IsVowel();
}
