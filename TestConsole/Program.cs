using System.IO;

using LaquaiLib;

using static LaquaiLib.Math.Topology;
using static LaquaiLib.Miscellaneous;

namespace TestConsole;

public class Program
{
    public static void Main()
    {
        
    }

    public async static Task asd()
    {
        Dictionary<string, List<string>> groups = new()
        {
            {
                "Predator",
                new()
                {
                    "Predator",
                    "Predator 2",
                    "Predators",
                    "The Predator",
                    "Prey"
                }
            },
            {
                "Alien",
                new()
                {
                    "Prometheus",
                    "Alien: Covenant",
                    "Alien",
                    "Aliens",
                    "Alien 3",
                    "Alien Resurrection"
                }
            },
            {
                "Alien vs. Predator",
                new()
                {
                    "Alien vs. Predator",
                    "Alien vs. Predator - Requiem"
                }
            }
        };
        Console.WriteLine(string.Join("\r\n\r\n", groups.Select(kv =>
        {
            return $"""
                   [{kv.Key.ToUpper()}]
                   {string.Join("\r\n", kv.Value.Select((movie, i) => $"{i + 1:D2} {movie.PadRight(kv.Value.Select(movie => movie.Length).Max() + 4)}S"))}
                   """;
        })));

        return;

        while (false)
        {

        }

        await Task.Run(() =>
        {
            while (true)
            {
                long mem = GC.GetTotalMemory(false);
                double log = System.Math.Floor(System.Math.Log(mem, 1024));
                Console.Title = $"{System.Math.Round((double)(mem / System.Math.Pow(1024, log)), 3)} {log switch
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
            new Node("A"),
            new Node("B"),
            new Node("C"),
            new Node("D")
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