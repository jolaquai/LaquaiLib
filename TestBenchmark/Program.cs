using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using LaquaiLib.Extensions;

namespace TestBenchmark;

internal class Program
{
    static void Main()
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAllJoined();
        Console.ReadLine();
    }
}

[MemoryDiagnoser(false)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
public class BenchmarkTarget
{
}
