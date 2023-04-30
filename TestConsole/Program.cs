using System.Text;

using LaquaiLib.Util;

namespace TestConsole;

public class Program
{
    [STAThread] // Needed for Clipboard operations
    public static void Main()
    {
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                Console.Title = $"{GC.GetTotalMemory(false) / 1024d / 1024d:0.000} MB";
                await Task.Delay(100);
                GC.Collect(0);
            }
        });

        var test = "testing my string replacements";
        using (var alloc = TempAlloc.Create<byte>(count: test.Length, true))
        {
            // start init: copy the string's bytes into alloc
            var data = alloc.Data;
            var stringBytes = Encoding.Default.GetBytes(test);
            for (var i = 0; i < stringBytes.Length; i++)
            {
                data[i] = stringBytes[i];
            }
            // end init

            var bytes = new ReadOnlySpan<byte>(new char[] { 's' }.Select(c => (byte)c).ToArray());
            Console.WriteLine($"Replacements made: {alloc.ReplaceAll(bytes, ReadOnlySpan<byte>.Empty, true)}");
            Console.WriteLine(alloc.As<string>());
            Console.WriteLine(alloc.Size);
        }
    }

    public static void Asd()
    {
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                Console.Title = $"{GC.GetTotalMemory(false) / 1024d / 1024d:0.000} MB";
                await Task.Delay(100);
                GC.Collect(0);
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
