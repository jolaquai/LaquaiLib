using LaquaiLib;

using static LaquaiLib.Miscellaneous;

namespace TestConsole;

public class Program
{
    public static void Main()
    {
        Logger log = new();
        log.LogInfo("fuck", "you");
        Console.WriteLine(log);
    }
}