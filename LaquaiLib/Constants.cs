using System.Unicode;

namespace LaquaiLib;

public static class Constants
{
    public static IEnumerable<int> Numbers => LaquaiLib.Range(0, 9);

    public static IEnumerable<char> LettersUppercase => LaquaiLib.Range(65, 90).Select(x => (char)x);
    public static IEnumerable<char> LettersLowercase => LaquaiLib.Range(97, 122).Select(x => (char)x);

    public static IEnumerable<string> GreekLettersUppercase => LaquaiLib.Range(0x0391, 0x03A9).Where(i => i != 0x03A2).Select(char.ConvertFromUtf32);
    public static IEnumerable<string> GreekLettersLowercase => LaquaiLib.Range(0x03B1, 0x03C9).Where(i => i != 0x03C2).Select(char.ConvertFromUtf32);
}
