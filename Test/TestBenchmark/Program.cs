using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

internal class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<Bench>();
    }
}

public class Bench
{
    public static int DistinctWithIndexOf<TSource>(ReadOnlySpan<TSource> source, Span<TSource> destination, IEqualityComparer<TSource> comparer = null)
    {
        var destIndex = 0;
        // This handles usage of IEquatable<>
        comparer = EqualityComparer<TSource>.Default;
        for (var i = 0; i < source.Length; i++)
        {
            if (destIndex == 0 || destination[..destIndex].IndexOf(source[i], comparer) == -1)
            {
                destination[destIndex++] = source[i];
            }
        }
        return destIndex;
    }
    public static int DistinctWithHashSet<TSource>(ReadOnlySpan<TSource> source, Span<TSource> destination, IEqualityComparer<TSource> comparer = null)
    {
        var hs = new HashSet<TSource>(source.Length);
        for (var i = 0; i < source.Length; i++)
        {
            if (hs.Add(source[i]))
            {
                destination[hs.Count - 1] = source[i];
            }
        }
        return hs.Count;
    }

    [Params(1_000_000, 10_000_000, 100_000_000)]
    public static int ArrLen { get; set; }

    public static byte[] bytes;
    static Bench()
    {
        bytes = new byte[ArrLen];
        Random.Shared.NextBytes(bytes);
    }

    [Benchmark]
    public void DistinctWithIndexOfBenchmark()
    {
        var destination = new Span<byte>(new byte[ArrLen]);
        var count = DistinctWithIndexOf(bytes, destination);
    }
    [Benchmark]
    public void DistinctWithHashSetBenchmark()
    {
        var destination = new Span<byte>(new byte[ArrLen]);
        var count = DistinctWithHashSet(bytes, destination);
    }
}