using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<BenchmarkTarget>();

[MemoryDiagnoser(false)]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
public class BenchmarkTarget
{
    [Params(10, 20, 30, 40, 50, 100, 150, 200, 250, 300)] public ulong N;
    [Params(3, 5, 7, 10)] public ulong Shift;

    [Benchmark]
    public void Fibonacci() => LaquaiLib.Numerics.Fibonacci.GetNth(N);
    [Benchmark]
    public void FibonacciSeq()
    {
        foreach (var _ in LaquaiLib.Numerics.Fibonacci.GenerateSequence(0, N))
        {
        }
    }
    [Benchmark]
    public void FibonacciSeqShifting()
    {
        foreach (var _ in LaquaiLib.Numerics.Fibonacci.GenerateSequence(N, N + Shift))
        {
        }
    }
}
