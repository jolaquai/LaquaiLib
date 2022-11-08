namespace LaquaiLib
{
    public static class Constants
    {
        public static IEnumerable<int> Numbers => Miscellaneous.Range(0, 9);
        public static IEnumerable<char> LettersUppercase => Miscellaneous.Range(65, 90).Select(x => (char)x);
        public static IEnumerable<char> LettersLowercase => Miscellaneous.Range(97, 122).Select(x => (char)x);


    }
}
