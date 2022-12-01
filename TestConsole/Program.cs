using LaquaiLib;

using static LaquaiLib.Miscellaneous;
using static LaquaiLib.Math;
using static LaquaiLib.Math.Topology;

namespace TestConsole;

public class Program
{
    public static void Main()
    {
        NodeGrid grid = new(
            new Node("A"),
            new Node("B"),
            new Node("C"),
            new Node("D")
        );

        grid.SetWeight("A", "B", 10);
        grid.SetWeight("A", "C",  5);
        grid.SetWeight("A", "D", 20);

        grid.SetWeight("B", "C", 25);

        grid.SetWeight("C", "D", 30);

        Logger.WriteInfo(
            "A: " + grid.Star("A"),
            "B: " + grid.Star("B"),
            "C: " + grid.Star("C"),
            "D: " + grid.Star("D"),
            "f: " + grid.FullMesh()
        );
    }
}