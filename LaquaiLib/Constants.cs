namespace LaquaiLib;

public static class Constants
{
    public static IEnumerable<int> Numbers => LaquaiLib.Range(0, 9);
    public static IEnumerable<char> LettersUppercase => LaquaiLib.Range(65, 90).Select(x => (char)x);
    public static IEnumerable<char> LettersLowercase => LaquaiLib.Range(97, 122).Select(x => (char)x);

    
}
