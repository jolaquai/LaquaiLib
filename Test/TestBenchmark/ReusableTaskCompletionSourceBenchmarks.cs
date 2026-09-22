#pragma warning disable CA2007

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using LaquaiLib.Threading;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[GcServer(false), GcConcurrent(false)]
public class ReusableTaskCompletionSourceBenchmarks
{
    private const int N = 16384;

    private const string VoidSync = "Void, completed before await";
    private const string VoidAsync = "Void, ping-pong across threads";
    private const string Int32Sync = "Int32, completed before await";
    private const string Int32Async = "Int32, ping-pong across threads";

    private TaskCompletionSource _ping;
    private TaskCompletionSource _pong;
    private TaskCompletionSource<int> _pingOfInt32;
    private TaskCompletionSource<int> _pongOfInt32;

    private static TaskCompletionSource NewTcs() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static TaskCompletionSource<int> NewTcsOfInt32() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    [BenchmarkCategory(VoidSync), Benchmark(Baseline = true, OperationsPerInvoke = N)]
    public async Task TcsTask()
    {
        for (var i = 0; i < N; i++)
        {
            var tcs = NewTcs();
            tcs.SetResult();
            await tcs.Task;
        }
    }
    [BenchmarkCategory(VoidSync), Benchmark(OperationsPerInvoke = N)]
    public async Task ReusableTask()
    {
        var tcs = new ReusableTaskCompletionSource();
        for (var i = 0; i < N; i++)
        {
            tcs.SetResult();
            await tcs.Task;
            tcs.Reset();
        }
    }
    [BenchmarkCategory(VoidSync), Benchmark(OperationsPerInvoke = N)]
    public async Task ReusableValueTask()
    {
        var tcs = new ReusableTaskCompletionSource();
        for (var i = 0; i < N; i++)
        {
            tcs.SetResult();
            await tcs.ValueTask;
            tcs.Reset();
        }
    }

    [BenchmarkCategory(Int32Sync), Benchmark(Baseline = true, OperationsPerInvoke = N)]
    public async Task<int> TcsTaskOfInt32()
    {
        var sum = 0;
        for (var i = 0; i < N; i++)
        {
            var tcs = NewTcsOfInt32();
            tcs.SetResult(i);
            sum += await tcs.Task;
        }
        return sum;
    }
    [BenchmarkCategory(Int32Sync), Benchmark(OperationsPerInvoke = N)]
    public async Task<int> ReusableTaskOfInt32()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var sum = 0;
        for (var i = 0; i < N; i++)
        {
            tcs.SetResult(i);
            sum += await tcs.Task;
            tcs.Reset();
        }
        return sum;
    }
    [BenchmarkCategory(Int32Sync), Benchmark(OperationsPerInvoke = N)]
    public async Task<int> ReusableValueTaskOfInt32()
    {
        var tcs = new ReusableTaskCompletionSource<int>();
        var sum = 0;
        for (var i = 0; i < N; i++)
        {
            tcs.SetResult(i);
            sum += await tcs.ValueTask;
            tcs.Reset();
        }
        return sum;
    }

    [BenchmarkCategory(VoidAsync), Benchmark(Baseline = true, OperationsPerInvoke = N)]
    public async Task TcsTaskPingPong()
    {
        _ping = NewTcs();
        var echo = EchoTcs();
        for (var i = 0; i < N; i++)
        {
            var pong = _pong = NewTcs();
            _ping.SetResult();
            await pong.Task;
        }
        await echo;
    }
    private async Task EchoTcs()
    {
        for (var i = 0; i < N; i++)
        {
            await _ping.Task;
            _ping = NewTcs();
            _pong.SetResult();
        }
    }
    [BenchmarkCategory(VoidAsync), Benchmark(OperationsPerInvoke = N)]
    public async Task ReusableTaskPingPong()
    {
        var ping = new ReusableTaskCompletionSource();
        var pong = new ReusableTaskCompletionSource();
        var echo = EchoReusableTask(ping, pong);
        for (var i = 0; i < N; i++)
        {
            ping.SetResult();
            await pong.Task;
            pong.Reset();
        }
        await echo;
    }
    private static async Task EchoReusableTask(ReusableTaskCompletionSource ping, ReusableTaskCompletionSource pong)
    {
        for (var i = 0; i < N; i++)
        {
            await ping.Task;
            ping.Reset();
            pong.SetResult();
        }
    }
    [BenchmarkCategory(VoidAsync), Benchmark(OperationsPerInvoke = N)]
    public async Task ReusableValueTaskPingPong()
    {
        var ping = new ReusableTaskCompletionSource();
        var pong = new ReusableTaskCompletionSource();
        var echo = EchoReusableValueTask(ping, pong);
        for (var i = 0; i < N; i++)
        {
            ping.SetResult();
            await pong.ValueTask;
            pong.Reset();
        }
        await echo;
    }
    private static async Task EchoReusableValueTask(ReusableTaskCompletionSource ping, ReusableTaskCompletionSource pong)
    {
        for (var i = 0; i < N; i++)
        {
            await ping.ValueTask;
            ping.Reset();
            pong.SetResult();
        }
    }

    [BenchmarkCategory(Int32Async), Benchmark(Baseline = true, OperationsPerInvoke = N)]
    public async Task<int> TcsTaskOfInt32PingPong()
    {
        _pingOfInt32 = NewTcsOfInt32();
        var echo = EchoTcsOfInt32();
        var sum = 0;
        for (var i = 0; i < N; i++)
        {
            var pong = _pongOfInt32 = NewTcsOfInt32();
            _pingOfInt32.SetResult(i);
            sum += await pong.Task;
        }
        await echo;
        return sum;
    }
    private async Task EchoTcsOfInt32()
    {
        for (var i = 0; i < N; i++)
        {
            var value = await _pingOfInt32.Task;
            _pingOfInt32 = NewTcsOfInt32();
            _pongOfInt32.SetResult(value + 1);
        }
    }
    [BenchmarkCategory(Int32Async), Benchmark(OperationsPerInvoke = N)]
    public async Task<int> ReusableTaskOfInt32PingPong()
    {
        var ping = new ReusableTaskCompletionSource<int>();
        var pong = new ReusableTaskCompletionSource<int>();
        var echo = EchoReusableTaskOfInt32(ping, pong);
        var sum = 0;
        for (var i = 0; i < N; i++)
        {
            ping.SetResult(i);
            sum += await pong.Task;
            pong.Reset();
        }
        await echo;
        return sum;
    }
    private static async Task EchoReusableTaskOfInt32(ReusableTaskCompletionSource<int> ping, ReusableTaskCompletionSource<int> pong)
    {
        for (var i = 0; i < N; i++)
        {
            var value = await ping.Task;
            ping.Reset();
            pong.SetResult(value + 1);
        }
    }
    [BenchmarkCategory(Int32Async), Benchmark(OperationsPerInvoke = N)]
    public async Task<int> ReusableValueTaskOfInt32PingPong()
    {
        var ping = new ReusableTaskCompletionSource<int>();
        var pong = new ReusableTaskCompletionSource<int>();
        var echo = EchoReusableValueTaskOfInt32(ping, pong);
        var sum = 0;
        for (var i = 0; i < N; i++)
        {
            ping.SetResult(i);
            sum += await pong.ValueTask;
            pong.Reset();
        }
        await echo;
        return sum;
    }
    private static async Task EchoReusableValueTaskOfInt32(ReusableTaskCompletionSource<int> ping, ReusableTaskCompletionSource<int> pong)
    {
        for (var i = 0; i < N; i++)
        {
            var value = await ping.ValueTask;
            ping.Reset();
            pong.SetResult(value + 1);
        }
    }
}
