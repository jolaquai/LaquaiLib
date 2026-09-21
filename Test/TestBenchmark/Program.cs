#pragma warning disable CA2007

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

using LaquaiLib.Threading;

internal sealed class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<Bench>(
            DefaultConfig.Instance.AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance)),
            args
        );
    }
}

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[GcServer(false), GcConcurrent(false)]
public class Bench
{
    private const int N = 65535;

    private const string Void = "TCS";
    private const string Int32 = "TCS<int>";

    [BenchmarkCategory(Void), Benchmark(Baseline = true, OperationsPerInvoke = N)]
    public async ValueTask BuiltIn()
    {
        var tcs = new TaskCompletionSource();
        for (var i = 0; i < N; i++)
        {
            tcs.SetResult();
            await tcs.Task;
            tcs = new();
        }
    }
    [BenchmarkCategory(Void), Benchmark(OperationsPerInvoke = N)]
    public async ValueTask ReusableTask()
    {
        var tcs = new ReusableTaskCompletionSource();
        for (var i = 0; i < N; i++)
        {
            tcs.SetResult();
            await tcs.Task;
            tcs.Reset();
        }
    }
    [BenchmarkCategory(Void), Benchmark(OperationsPerInvoke = N)]
    public async ValueTask ReusableValueTask()
    {
        var tcs = new ReusableTaskCompletionSource();
        for (var i = 0; i < N; i++)
        {
            tcs.SetResult();
            await tcs.ValueTask;
            tcs.Reset();
        }
    }

    [BenchmarkCategory(Int32), Benchmark(Baseline = true, OperationsPerInvoke = N)]
    public async ValueTask BuiltInOfInt32()
    {
        var tcs = new TaskCompletionSource<int>();
        for (var i = 0; i < N; i++)
        {
            tcs.SetResult(i);
            await tcs.Task;
            tcs = new();
        }
    }
    [BenchmarkCategory(Int32), Benchmark(OperationsPerInvoke = N)]
    public async ValueTask ReusableTaskOfInt32()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        for (var i = 0; i < N; i++)
        {
            tcs.SetResult(i);
            await tcs.Task;
            tcs.Reset();
        }
    }
    [BenchmarkCategory(Int32), Benchmark(OperationsPerInvoke = N)]
    public async ValueTask ReusableValueTaskOfInt32()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        for (var i = 0; i < N; i++)
        {
            tcs.SetResult(i);
            await tcs.ValueTask;
            tcs.Reset();
        }
    }
}
