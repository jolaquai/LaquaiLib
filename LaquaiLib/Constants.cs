using System.Unicode;

namespace LaquaiLib;

public static class Constants
{
    public static IEnumerable<int> Numbers => Enumerable.Range(0, 10);

    public static IEnumerable<char> LettersUppercase => Enumerable.Range(65, 26).Select(x => (char)x);
    public static IEnumerable<char> LettersLowercase => Enumerable.Range(97, 26).Select(x => (char)x);

    public static IEnumerable<string> GreekLettersUppercase => Enumerable.Range(0x0391, 25).Where(i => i != 0x03A2).Select(char.ConvertFromUtf32);
    public static IEnumerable<string> GreekLettersLowercase => Enumerable.Range(0x03B1, 25).Where(i => i != 0x03C2).Select(char.ConvertFromUtf32);
}
