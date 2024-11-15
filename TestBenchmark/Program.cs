using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using LaquaiLib.Util;

namespace TestBenchmark;

internal class Program
{
    static void Main()
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAllJoined();
        Console.ReadLine();
    }
}

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
public class BenchmarkTarget
{
    [Benchmark]
    public void PathGetInvalidFileNameChars()
    {
    }
}
