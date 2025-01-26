using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace TestBenchmark;

internal class Program
{
    private static void Main()
    {
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAllJoined();
        _ = Console.ReadLine();
    }
}

[MemoryDiagnoser(false)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
public class BenchmarkTarget
{
}
