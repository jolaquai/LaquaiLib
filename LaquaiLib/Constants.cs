namespace LaquaiLib;

/// <summary>
/// Provides a number of constants.
/// </summary>
public static class Constants
{
    /// <summary>
    /// An <see cref="IEnumerable{T}"/> of <see cref="int"/> containing the numbers <c>0</c> through <c>9</c>.
    /// </summary>
    public static readonly IEnumerable<int> Numbers = Enumerable.Range(0, 10);
    /// <summary>
    /// An <see cref="IEnumerable{T}"/> of <see cref="char"/> containing the uppercase letters <c>A</c> through <c>Z</c>.
    /// </summary>
    public static readonly IEnumerable<char> LettersUppercase = Enumerable.Range(65, 26).Select(x => (char)x);
    /// <summary>
    /// An <see cref="IEnumerable{T}"/> of <see cref="char"/> containing the lowercase letters <c>a</c> through <c>z</c>.
    /// </summary>
    public static readonly IEnumerable<char> LettersLowercase = Enumerable.Range(97, 26).Select(x => (char)x);
    /// <summary>
    /// An <see cref="IEnumerable{T}"/> of <see cref="char"/> containing the uppercase letters <c>Α</c> through <c>Ω</c>.
    /// </summary>
    public static readonly IEnumerable<string> GreekLettersUppercase = Enumerable.Range(0x0391, 25).Where(i => i != 0x03A2).Select(char.ConvertFromUtf32);
    /// <summary>
    /// An <see cref="IEnumerable{T}"/> of <see cref="char"/> containing the lowercase letters <c>α</c> through <c>ω</c>.
    /// </summary>
    public static readonly IEnumerable<string> GreekLettersLowercase = Enumerable.Range(0x03B1, 25).Where(i => i != 0x03C2).Select(char.ConvertFromUtf32);
}
