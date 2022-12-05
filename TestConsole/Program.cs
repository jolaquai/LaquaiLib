using System.Runtime.InteropServices;

using LaquaiLib;
using LaquaiLib.ScreenCapture;

using static LaquaiLib.Math.Topology;
using static LaquaiLib.Miscellaneous;

namespace TestConsole;

public class Program
{
    enum MouseEvent : uint
    {
        MOUSEEVENTF_LEFTDOWN = 0x02,
        MOUSEEVENTF_LEFTUP = 0x04,
        MOUSEEVENTF_RIGHTDOWN = 0x08,
        MOUSEEVENTF_RIGHTUP = 0x10
    }

    public async static Task Main()
    {


        while (false)
        {
            
        }

        await Task.Run(() =>
        {
            while (true)
            {
                Console.Title = $"{System.Math.Round(GC.GetTotalMemory(false) / System.Math.Pow(1024, 2), 3)} MB";
                Thread.Sleep(100);
                GC.Collect(0);
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