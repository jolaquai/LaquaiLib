namespace LaquaiLib
{
    public static class Constants
    {
        static IEnumerable<int> Numbers => LaquaiLib.Range(0, 9).Cast<int>();
        static IEnumerable<char> LettersLowercase => LaquaiLib.Range(65, 90).Cast<char>();
        static IEnumerable<char> LettersUppercase => LaquaiLib.Range(97, 122).Cast<char>();


    }
}
