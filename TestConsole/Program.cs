using System.Xml;
using System.Xml.Linq;

using LaquaiLib.Extensions;

namespace TestConsole;

public class Program
{
    [STAThread] // Needed for Clipboard operations
    public static void Main()
    {
        using (var vxw = XmlWriter.Create(@"C:\test.xml"))
        {
            vxw.WriteStartDocument();
            vxw.WriteXNode(new XElement("Root", new XRepetition(new XElement("Test", "Hallo"), 5)));
            vxw.Flush();
        }
        XElement.Load(@"C:\test.xml").Save(@"C:\test.xml");
    }

    public static async Task Asd()
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
                var mem = GC.GetTotalMemory(false);
                var log = Math.Floor(Math.Log(mem, 1024));
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
}
