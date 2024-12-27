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
    [Benchmark]
    public void TestTempArray()
    {
        for (var i = 0; i < 244; i++)
        {
            var ints = Enumerable.Range(0, 130000).Select(i => i++);
            using var mat = ints.GetTempArray();
        }
    }
    [Benchmark(Baseline = true)]
    public void TestArray()
    {
        for (var i = 0; i < 244; i++)
        {
            var ints = Enumerable.Range(0, 130000).Select(i => i++);
            var mat = ints.ToArray();
        }
    }
}
