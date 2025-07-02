namespace LaquaiLib.Generators;

internal static class Helpers
{
    static Helpers()
    {
        try
        {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            File.Delete("C:\\debug.log");
        }
        catch { }
    }

    private static object _lock = new object();
    private static StreamWriter sw;
    public static void WriteDebugDiagnostic(string message)
    {
        lock (_lock)
        {
            sw ??= new StreamWriter("C:\\debug.log", append: true)
            {
                AutoFlush = true
            };
            sw.WriteLine($"""
                [{DateTime.Now:o}] {message}
                """);
        }
    }
}
