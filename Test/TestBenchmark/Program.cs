using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

internal sealed class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<ReusableTaskCompletionSourceBenchmarks>(
            DefaultConfig.Instance.AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance)),
            args
        );
    }
}
