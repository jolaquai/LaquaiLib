using System.IO;
using System.Reflection;

using LaquaiLib.Classes.Streams;
using LaquaiLib.Extensions;

namespace TestConsole;

public class Program
{
    [STAThread] // Needed for Clipboard operations
    public static void Main()
    {
        using (var ms = new MultiStream(typeof(MemoryStream), 5)
        {

        }
    }

    public static void Asd()
    {
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                var mem = GC.GetTotalMemory(false);
                var log = (int)Math.Log(mem, 1024);
                Console.Title = $"{Math.Round(mem / Math.Pow(1024, log), 3)} {log switch
                {
                    0 => "B",
                    1 => "KB",
                    2 => "MB",
                    3 => "GB",
                    _ => "??"
                }}";
                Thread.Sleep(100);
            }
        });

        var groups = new Dictionary<string, Dictionary<string, object>>()
        {
            {
                "Predator",
                new Dictionary<string, object>()
                {
                    { "Predator", false },
                    { "Predator 2", false },
                    { "Predators", false },
                    { "The Predator", false },
                    { "Prey", false }
                }
            },
            {
                "Alien",
                new Dictionary<string, object>()
                {
                    { "Prometheus", false },
                    { "Alien: Covenant", false },
                    { "Alien", false },
                    { "Aliens", false },
                    { "Alien 3", false },
                    { "Alien Resurrection", false }
                }
            },
            {
                "Other movies",
                new Dictionary<string, object>()
                {
                    { "The Banker", false },
                    { "Manchester by the Sea", false },
                    { "You People", false }
                }
            },
            {
                "Series",
                new Dictionary<string, object>()
                {
                    { "Jojo", "S2E7" },
                    { "Initial D", "S5E8" }
                }
            }
        };
        Console.WriteLine(string.Join("\r\n\r\n", groups.Select(kv => $"""
            [{kv.Key} ({kv.Value.Count(kv => kv.Value is not false)}/{kv.Value.Count})]
            {string.Join("\r\n", kv.Value.Select((movie, i) => $"{i + 1:D2} {movie.Key.PadRight(kv.Value.Select(movie => movie.Key.Length).Max() + 4)}{(movie.Value is bool b ? (b ? "✓" : "X") : movie.Value)}"))}
            """
        )));
    }
}
