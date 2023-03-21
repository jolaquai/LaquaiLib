using System.IO;
using System.Runtime.InteropServices;

using LaquaiLib.Extensions;

using LaquaiLib.Util;

using static LaquaiLib.Miscellaneous;
using static LaquaiLib.RandomMath.Topology;

namespace TestConsole;

public class Program
{
    [STAThread] // Needed for Clipboard operations
    public static void Main()
    {
        
    }

    public async static Task asd()
    {
        Dictionary<string, Dictionary<string, object>> groups = new()
        {
            {
                "Predator",
                new()
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
                new()
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
                "Series",
                new()
                {
                    { "Jojo", "S2E7" },
                    { "Initial D", "S5E8" }
                }
            }
        };
        Console.WriteLine(string.Join("\r\n\r\n", groups.Select(kv => $"""
            [{kv.Key} ({kv.Value.Count})]
            {string.Join("\r\n", kv.Value.Select((movie, i) => $"{i + 1:D2} {movie.Key.PadRight(kv.Value.Select(movie => movie.Key.Length).Max() + 4)}{(movie.Value is bool boolean ? (boolean ? "✓" : "X") : movie.Value)}"))}
            """
        )));

        return;

        while (false)
        {

        }

        await Task.Run(() =>
        {
            while (true)
            {
                long mem = GC.GetTotalMemory(false);
                double log = Math.Floor(Math.Log(mem, 1024));
                Console.Title = $"{Math.Round((double)(mem / Math.Pow(1024, log)), 3)} {log switch
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
    }

    public static void NodeGridTest()
    {
        NodeGrid grid = new(
            new("A"),
            new("B"),
            new("C"),
            new("D")
        );

        grid.SetWeight("A", "B", 10);
        grid.SetWeight("A", "C", 5);
        grid.SetWeight("A", "D", 20);

        grid.SetWeight("B", "C", 25);

        grid.SetWeight("C", "D", 30);

        (double Total, List<int> Path) = grid.Ring();
        Logger.WriteInfo(
            "Ring: " + Total,
            "Path: " + string.Join(", ", Path.Select(n => grid.Nodes[n].Name))
        );
    }
}
