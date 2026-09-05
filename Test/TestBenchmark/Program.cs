using System.Buffers;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

internal sealed class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<Bench>(
            DefaultConfig.Instance.AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance)),
            args
        );
        Console.ReadLine();
    }
}

[MemoryDiagnoser]
public class Bench
{
    [Params("ab", "Hello, World!")]
    public string Source { get; set; }

    [Params(100, 1_000, 10_000, 100_000, 1_000_000)]
    public int Count { get; set; }

    public string RepeatFromPooledArray()
    {
        var srcSpan = Source.AsSpan();
        var length = srcSpan.Length * Count;

        var arr = ArrayPool<char>.Shared.Rent(length);
        var newStr = arr.AsSpan(0, length);
        for (var i = 0; i < Count; i++)
            srcSpan.CopyTo(newStr[(i * srcSpan.Length)..]);
        var str = newStr.ToString();
        ArrayPool<char>.Shared.Return(arr);
        return str;
    }

    [Benchmark]
    public string RepeatFromStringCreateCopyFromSelf()
    {
        var source = Source;
        var len = source.Length * Count;
        return string.Create(len, source, static (span, src) =>
        {
            src.AsSpan().CopyTo(span);
            var filled = src.Length;
            // Every iteration here is a full doubling - no Math.Min needed
            while (filled <= span.Length >> 1)
            {
                span[..filled].CopyTo(span[filled..]);
                filled <<= 1;
            }
            // Single remainder copy, no conditional in the hot path
            if (filled < span.Length)
                span[..(span.Length - filled)].CopyTo(span[filled..]);
        });
    }

    public string RepeatFromConcat()
    {
        return string.Concat(Enumerable.Repeat(Source, Count));
    }

    public string RepeatFromStringCreate()
    {
        var source = Source;
        var count = Count;
        var len = source.Length * count;
        return string.Create(len, (len, source), static (span, state) =>
        {
            var (len, src) = state;
            var srcSpan = src.AsSpan();
            for (var i = 0; i < state.len; i += srcSpan.Length)
                srcSpan.CopyTo(span[i..]);
        });
    }

    public string RepeatFromZeroStringNoBoundsCheck()
    {
        var source = Source;
        var srcLen = source.Length;
        var len = srcLen * Count;
        var str = new string('\0', len);
        unsafe
        {
            fixed (void* srcPtr = source)
            fixed (void* destPtr = str)
            {
                var endPtr = (void*)((nint)destPtr + (len * sizeof(char)));
                for (var ptr = destPtr; ptr < endPtr; ptr = (void*)((nint)ptr + (srcLen * sizeof(char))))
                    Buffer.MemoryCopy(srcPtr, ptr, srcLen * sizeof(char), srcLen * sizeof(char));
            }
        }
        return str;
    }

    public string RepeatFromStringCreateNoBoundsCheck()
    {
        var source = Source;
        var count = Count;
        var len = source.Length * count;
        return string.Create(len, (len, source), static (span, state) =>
        {
            var (len, src) = state;
            var srcSpan = src.AsSpan();
            var srcLen = srcSpan.Length;
            unsafe
            {
                fixed (void* srcPtr = srcSpan)
                fixed (void* destPtr = span)
                {
                    var endPtr = (void*)((nint)destPtr + (len * sizeof(char)));
                    for (var ptr = destPtr; ptr < endPtr; ptr = (void*)((nint)ptr + (srcLen * sizeof(char))))
                        Buffer.MemoryCopy(srcPtr, ptr, srcLen * sizeof(char), srcLen * sizeof(char));
                }
            }
        });
    }
}
