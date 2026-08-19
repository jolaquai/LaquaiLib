namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extensions for the <see cref="char"/> type.
/// </summary>
public static class CharExtensions
{
    private static ReadOnlySpan<char> Vowels => ['a', 'e', 'i', 'o', 'u', 'y', 'A', 'E', 'I', 'O', 'U', 'Y'];

    extension(char character)
    {
        /// <summary>
        /// Determines if a specified <see cref="char"/> is a vowel (including 'y').
        /// </summary>
        /// <returns><see langword="true"/> if <paramref name="character"/> is a vowel, otherwise, <see langword="false"/>.</returns>
        public bool IsVowel
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => char.IsLetter(character) && Vowels.Contains(character);
        }

        /// <summary>
        /// Determines if a specified <see cref="char"/> is a consonant.
        /// </summary>
        /// <returns><see langword="true"/> if <paramref name="character"/> is a consonant, otherwise, <see langword="false"/>.</returns>
        public bool IsConsonant
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => char.IsLetter(character) && !character.IsVowel;
        }
    }
}
