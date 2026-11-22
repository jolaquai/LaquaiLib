using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

internal sealed class Program
{
    private static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(
            args,
            DefaultConfig.Instance
                .AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance).WithLaunchCount(1).WithWarmupCount(5).WithIterationCount(12))
        );
    }
}
