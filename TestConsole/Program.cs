using LaquaiLib;

namespace TestConsole
{
    public class Program
    {
        public static void Main()
        {
            Write(
                Constants.LettersUppercase.Shuffle().Join(", ")
            );
        }

        public static void Write(params object[] lines)
        {
            foreach (object line in lines)
            {
                Console.WriteLine(line);
            }
        }
    }
}