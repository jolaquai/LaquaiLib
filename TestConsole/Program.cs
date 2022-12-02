using LaquaiLib;

using static LaquaiLib.Miscellaneous;
using static LaquaiLib.Math.Topology;
using LaquaiLib.ScreenCapture;

namespace TestConsole;

public class Program
{
    public static void Main()
    {
        ScreenCapture capture = new();
        capture.Captured += (sender, e) =>
        {
            string formatString = @"HH\:mm\:ss.ffffff";
            Logger.WriteInfo(
                "Capture created at: " + e.CaptureTime.ToString(formatString),
                "Event received at: " + DateTime.Now.ToString(formatString),
                "Bitmap size: " + e.Bitmap.Size.ToString()
            );
            Console.WriteLine();
        };

        DateTime start = DateTime.Now;
        capture.Start();

        SpinWait.SpinUntil(() => (DateTime.Now - start).TotalSeconds > 5000);
        capture.Stop();
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
        grid.SetWeight("A", "C",  5);
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