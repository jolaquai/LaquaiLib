using System.Buffers;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

internal sealed class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<Bench>();
    }
}

public class Bench
{
    public static string RepeatFromPooledArray(string source, int count)
    {
        var srcSpan = source.AsSpan();
        var length = srcSpan.Length * count;

        var arr = ArrayPool<char>.Shared.Rent(length);
        var newStr = arr.AsSpan(0, length);
        for (var i = 0; i < count; i++)
        {
            srcSpan.CopyTo(newStr[(i * srcSpan.Length)..]);
        }
        var str = newStr.ToString();
        ArrayPool<char>.Shared.Return(arr);
        return str;
    }
    public static string RepeatFromConcat(string source, int count)
    {
        var srcSpan = source.AsSpan();
        var length = srcSpan.Length * count;

        return string.Concat(Enumerable.Repeat(source, count));
    }
    public static string RepeatFromStringCreate(string source, int count)
    {
        var len = source.Length * count;
        return string.Create(len, (len, source), static (span, state) =>
        {
            var (len, src) = state;
            var srcSpan = src.AsSpan();
            for (var i = 0; i < state.len; i += srcSpan.Length)
                srcSpan.CopyTo(span[i..]);
        });
    }
    public string RepeatFromZeroStringNoBoundsCheck(string source, int count)
    {
        var srcLen = source.Length;
        var len = source.Length * count;
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
    public string RepeatFromStringCreateNoBoundsCheck(string source, int count)
    {
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

    [Params(100, 1000, 10000, 100000, 1000000)]
    public static int Count { get; set; }
    public static string Source { get; set; }

    static Bench()
    {
    }
}