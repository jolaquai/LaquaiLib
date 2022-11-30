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
        grid.SetWeight("A", "D", 20);

        grid.SetWeight("B", "C", 20);

        grid.SetWeight("C", "D", 10);

        var path = grid.GetPath("A", "D");
        Logger.WriteInfo(
            string.Join(", ", path.Path),
            path.Total
        );
    }
}