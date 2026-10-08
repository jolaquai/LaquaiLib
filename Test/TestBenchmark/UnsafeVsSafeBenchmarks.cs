using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using LaquaiLib.Collections;
using LaquaiLib.Collections.Enumeration;
using LaquaiLib.Extensions;
using LaquaiLib.Numerics;
using LaquaiLib.Util;

// Baseline = what the library does today (unsafe); the other rows are the idiomatic safe equivalent.
// Ratio > 1 on a safe row means the safe version is slower.

[MemoryDiagnoser]
public class HasValueBenchmarks
{
    private enum E8 : byte { A, B }
    private enum E32 { A, B }
    private enum E64 : long { A, B }

    private E8 _e8 = E8.B;
    private E32 _e32 = E32.B;
    private E64 _e64 = E64.B;

    [Benchmark(Baseline = true)] public bool Library_Int32() => _e32.HasValue();
    [Benchmark] public bool Safe_EqualityComparer_Int32() => !EqualityComparer<E32>.Default.Equals(_e32, default);
    [Benchmark] public bool Safe_BitCast_Int32() => SafeBitCast(_e32);

    [Benchmark] public bool Library_Byte() => _e8.HasValue();
    [Benchmark] public bool Safe_EqualityComparer_Byte() => !EqualityComparer<E8>.Default.Equals(_e8, default);
    [Benchmark] public bool Safe_BitCast_Byte() => SafeBitCast(_e8);

    [Benchmark] public bool Library_Int64() => _e64.HasValue();
    [Benchmark] public bool Safe_EqualityComparer_Int64() => !EqualityComparer<E64>.Default.Equals(_e64, default);
    [Benchmark] public bool Safe_BitCast_Int64() => SafeBitCast(_e64);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SafeBitCast<T>(T v) where T : struct, Enum
        => Unsafe.SizeOf<T>() switch
        {
            1 => Unsafe.BitCast<T, byte>(v) != 0,
            2 => Unsafe.BitCast<T, ushort>(v) != 0,
            4 => Unsafe.BitCast<T, uint>(v) != 0,
            _ => Unsafe.BitCast<T, ulong>(v) != 0,
        };
}

[MemoryDiagnoser]
public class CountsBenchmarks
{
    [Params(1_000, 100_000)] public int N;
    [Params(16, 4096)] public int Distinct;

    private int[] _data;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _data = new int[N];
        for (var i = 0; i < _data.Length; i++)
            _data[i] = rng.Next(Distinct);
    }

    [Benchmark(Baseline = true)] public Dictionary<int, int> Library_CollectionsMarshal() => _data.Counts();

    [Benchmark]
    public Dictionary<int, int> Safe_TryGetValue_Indexer()
    {
        var counts = new Dictionary<int, int>(EqualityComparer<int>.Default);
        foreach (var item in _data)
        {
            counts.TryGetValue(item, out var c);
            counts[item] = c + 1;
        }
        return counts;
    }

    [Benchmark]
    public Dictionary<int, int> Safe_ContainsKey_Indexer()
    {
        var counts = new Dictionary<int, int>(EqualityComparer<int>.Default);
        foreach (var item in _data)
        {
            if (counts.ContainsKey(item))
                counts[item]++;
            else
                counts[item] = 1;
        }
        return counts;
    }
}

[MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class CastBenchmarks
{
    [Params(16, 1024, 65536)] public int N;

    private int[] _ints;
    private uint[] _uints;
    private object[] _objs;
    private string[] _strs;

    [GlobalSetup]
    public void Setup()
    {
        _ints = new int[N];
        _uints = new uint[N];
        _objs = new object[N];
        _strs = new string[N];
        for (var i = 0; i < N; i++)
        {
            _ints[i] = i;
            _objs[i] = i.ToString();
        }
    }

    [Benchmark(Baseline = true), BenchmarkCategory("Struct")]
    public int Library_Convert() { ReadOnlySpan<int> src = _ints; return LinqMemoryExtensions.Convert<int, uint>(src, _uints.AsSpan()); }

    [Benchmark, BenchmarkCategory("Struct")]
    public int Safe_UnsafeBitCast_Loop()
    {
        ReadOnlySpan<int> src = _ints;
        var dst = _uints.AsSpan();
        if (dst.Length < src.Length)
            throw new ArgumentException();
        for (var i = 0; i < src.Length; i++)
            dst[i] = Unsafe.BitCast<int, uint>(src[i]);
        return src.Length;
    }

    [Benchmark, BenchmarkCategory("Struct")]
    public int Safe_Foreach_Cast()
    {
        ReadOnlySpan<int> src = _ints;
        var dst = _uints.AsSpan();
        if (dst.Length < src.Length)
            throw new ArgumentException();
        var i = 0;
        foreach (var v in src)
            dst[i++] = unchecked((uint)v);
        return src.Length;
    }

    [Benchmark(Baseline = true), BenchmarkCategory("Class")]
    public int Library_ReinterpretCast() { ReadOnlySpan<object> src = _objs; return LinqMemoryExtensions.ReinterpretCast<object, string>(src, _strs.AsSpan()); }

    [Benchmark, BenchmarkCategory("Class")]
    public int Safe_CastClass()
    {
        ReadOnlySpan<object> src = _objs;
        var dst = _strs.AsSpan();
        if (dst.Length < src.Length)
            throw new ArgumentException();
        for (var i = 0; i < src.Length; i++)
            dst[i] = (string)src[i];
        return src.Length;
    }
}

[MemoryDiagnoser]
public class BitArrayAsBenchmarks
{
    private BitArray _ba;
    private ulong[] _data;

    [GlobalSetup]
    public void Setup()
    {
        _data = [0x0123456789ABCDEFUL, 0xFEDCBA9876543210UL];
        _ba = new BitArray(_data);
    }

    [Benchmark(Baseline = true)] public int Library_Int32() => _ba.As<int>();
    [Benchmark] public int Safe_Int32() => SafeAs<int>(_data);

    [Benchmark] public ulong Library_UInt64() => _ba.As<ulong>();
    [Benchmark] public ulong Safe_UInt64() => SafeAs<ulong>(_data);

    [Benchmark] public double Library_Double() => _ba.As<double>();
    [Benchmark] public double Safe_Double() => SafeAs<double>(_data);

    [Benchmark] public Guid Library_Guid() => _ba.As<Guid>();
    [Benchmark] public Guid Safe_Guid() => SafeAs<Guid>(_data);

    // Same ladder as BitArray.As<T>, but Unsafe.As<X, T>(ref v) -> Unsafe.BitCast<X, T>(v) and the generic fallback via
    // BitConverter-free span copy (MemoryMarshal.Read stays for Guid, there is no safe generic way to materialize an arbitrary unmanaged T)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T SafeAs<T>(ulong[] data) where T : unmanaged
    {
        unchecked
        {
            var lo = data[0];
            if (typeof(T) == typeof(int)) return Unsafe.BitCast<int, T>((int)lo);
            if (typeof(T) == typeof(uint)) return Unsafe.BitCast<uint, T>((uint)lo);
            if (typeof(T) == typeof(long)) return Unsafe.BitCast<long, T>((long)lo);
            if (typeof(T) == typeof(ulong)) return Unsafe.BitCast<ulong, T>(lo);
            if (typeof(T) == typeof(double)) return Unsafe.BitCast<double, T>(BitConverter.UInt64BitsToDouble(lo));
            // no safe generic equivalent: same as the library's fallback
            var bytes = MemoryMarshal.AsBytes(data.AsSpan());
            return MemoryMarshal.Read<T>(bytes);
        }
    }
}

[MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class MemoryBackingBenchmarks
{
    [Params(16, 4096)] public int N;

    private Memory<byte> _mem;
    private ReadOnlyMemory<char> _strMem;

    [GlobalSetup]
    public void Setup()
    {
        _mem = new byte[N + 8].AsMemory(4, N);
        _strMem = new string('x', N + 8).AsMemory(4, N);
    }

    [Benchmark(Baseline = true), BenchmarkCategory("Array")] public int Library_UnsafeArraySpan() => _mem.UnsafeArraySpan().Length;
    [Benchmark, BenchmarkCategory("Array")] public int Safe_MemorySpan() => _mem.Span.Length;

    [Benchmark(Baseline = true), BenchmarkCategory("ArrayRead")] public byte Library_UnsafeArraySpan_Read() => _mem.UnsafeArraySpan()[N - 1];
    [Benchmark, BenchmarkCategory("ArrayRead")] public byte Safe_MemorySpan_Read() => _mem.Span[N - 1];

    [Benchmark(Baseline = true), BenchmarkCategory("String")] public int Library_UnsafeStringSpan() => _strMem.UnsafeStringSpan().Length;
    [Benchmark, BenchmarkCategory("String")] public int Safe_MemorySpan_String() => _strMem.Span.Length;
}

[MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class SequenceEqualBenchmarks
{
    [Params(8, 1024, 1_000_000)] public int N;

    private int[] _a, _b;
    private List<int> _la, _lb;
    private IEnumerable<int> _ea, _eb;
    private long[] _la64, _lb64;

    [GlobalSetup]
    public void Setup()
    {
        _a = new int[N];
        for (var i = 0; i < N; i++)
            _a[i] = i;
        _b = (int[])_a.Clone();
        _la = [.. _a];
        _lb = [.. _b];
        _ea = _a;
        _eb = _b;
        _la64 = [.. _a.Select(static i => (long)i)];
        _lb64 = (long[])_la64.Clone();
    }

    [Benchmark(Baseline = true), BenchmarkCategory("Array")] public bool Library_Array() => SequenceEqualityComparer<int>.Default.Equals(_a, _b);
    [Benchmark, BenchmarkCategory("Array")] public bool Safe_SpanSequenceEqual() => _a.AsSpan().SequenceEqual(_b);

    [Benchmark(Baseline = true), BenchmarkCategory("List")] public bool Library_List() => SequenceEqualityComparer<int>.Default.Equals(_la, _lb);
    [Benchmark, BenchmarkCategory("List")] public bool Safe_List_SequenceEqual() => _la.SequenceEqual(_lb);

    [Benchmark(Baseline = true), BenchmarkCategory("IEnumerable")] public bool Library_IEnumerable() => SequenceEqualityComparer<int>.Default.Equals(_ea, _eb);
    [Benchmark, BenchmarkCategory("IEnumerable")] public bool Safe_Linq_SequenceEqual() => _ea.SequenceEqual(_eb);

    [Benchmark(Baseline = true), BenchmarkCategory("Int64")] public bool Library_Int64() => SequenceEqualityComparer<long>.Default.Equals(_la64, _lb64);
    [Benchmark, BenchmarkCategory("Int64")] public bool Safe_Int64() => _la64.AsSpan().SequenceEqual(_lb64);
}

[MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class TryGetSpanBenchmarks
{
    private IEnumerable<int> _arr = new int[256];
    private IEnumerable<int> _list = new List<int>(new int[256]);

    [Benchmark(Baseline = true), BenchmarkCategory("Array")]
    public int Library_Array() => _arr.TryGetSpan(out var s) ? s.Length : -1;
    [Benchmark, BenchmarkCategory("Array")]
    public int Safe_Array() => _arr is int[] a ? a.Length : -1;

    [Benchmark(Baseline = true), BenchmarkCategory("List")]
    public int Library_List() => _list.TryGetSpan(out var s) ? s.Length : -1;
    // no safe way to get a Span<T> over a List<T>; Count is the closest thing (and not equivalent)
    [Benchmark, BenchmarkCategory("List")]
    public int Safe_List_Count() => _list is List<int> l ? l.Count : -1;
}

[MemoryDiagnoser]
public class MultiDimBenchmarks
{
    [Params(16, 512)] public int N;

    private int[,] _arr;
    private MultiDimArrayEnumerable<int> _wrap;

    [GlobalSetup]
    public void Setup()
    {
        _arr = new int[N, N];
        for (var i = 0; i < N; i++)
            for (var j = 0; j < N; j++)
                _arr[i, j] = i ^ j;
        _wrap = new MultiDimArrayEnumerable<int>(_arr);
    }

    [Benchmark(Baseline = true)]
    public long Library_Enumerator()
    {
        long sum = 0;
        foreach (var v in _wrap)
            sum += v;
        return sum;
    }

    [Benchmark]
    public long Library_Span()
    {
        long sum = 0;
        foreach (var v in _wrap.Span)
            sum += v;
        return sum;
    }

    [Benchmark]
    public long Safe_NestedFor()
    {
        long sum = 0;
        var a = _arr;
        for (var i = 0; i < a.GetLength(0); i++)
            for (var j = 0; j < a.GetLength(1); j++)
                sum += a[i, j];
        return sum;
    }

    [Benchmark]
    public long Safe_ForeachMultiDim()
    {
        long sum = 0;
        foreach (var v in _arr)
            sum += v;
        return sum;
    }
}

[MemoryDiagnoser]
public class BinaryReadBenchmarks
{
    private byte[] _buf;

    [GlobalSetup]
    public void Setup()
    {
        _buf = new byte[4096];
        new Random(1).NextBytes(_buf);
    }

    // Mirrors FileSystemHelper's BackupRead header parse (int, uint, long, uint at 0/4/8/16)
    [Benchmark(Baseline = true)]
    public long MemoryMarshal_Read()
    {
        Span<byte> buffer = _buf;
        long acc = 0;
        for (var offset = 0; offset + 20 <= buffer.Length; offset += 20)
        {
            acc += MemoryMarshal.Read<int>(buffer[offset..]);
            acc += MemoryMarshal.Read<uint>(buffer[(offset + 4)..]);
            acc += MemoryMarshal.Read<long>(buffer[(offset + 8)..]);
            acc += MemoryMarshal.Read<uint>(buffer[(offset + 16)..]);
        }
        return acc;
    }

    [Benchmark]
    public long BinaryPrimitives_LittleEndian()
    {
        Span<byte> buffer = _buf;
        long acc = 0;
        for (var offset = 0; offset + 20 <= buffer.Length; offset += 20)
        {
            acc += BinaryPrimitives.ReadInt32LittleEndian(buffer[offset..]);
            acc += BinaryPrimitives.ReadUInt32LittleEndian(buffer[(offset + 4)..]);
            acc += BinaryPrimitives.ReadInt64LittleEndian(buffer[(offset + 8)..]);
            acc += BinaryPrimitives.ReadUInt32LittleEndian(buffer[(offset + 16)..]);
        }
        return acc;
    }
}

[MemoryDiagnoser]
public class ArrayHelperSortBenchmarks
{
    [Params(16, 4096)] public int N;

    private int[] _keysSrc;
    private string[] _itemsSrc;
    private int[] _keys;
    private string[] _items;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(7);
        _keysSrc = new int[N];
        _itemsSrc = new string[N];
        for (var i = 0; i < N; i++)
        {
            _keysSrc[i] = rng.Next();
            _itemsSrc[i] = i.ToString();
        }
        _keys = new int[N];
        _items = new string[N];
    }

    [IterationSetup]
    public void Reset()
    {
        Array.Copy(_keysSrc, _keys, N);
        Array.Copy(_itemsSrc, _items, N);
    }

    // function-pointer based (Reverse via delegate*)
    [Benchmark(Baseline = true)]
    public void Library_SortDescending() => ArrayHelper.SortDescending(_keys, _items);

    // same algorithm with the function pointer replaced by a bool
    [Benchmark]
    public void Safe_SortDescending_Flag()
    {
        var keys = _keys;
        var itemsArray = _items;
        var indices = GC.AllocateUninitializedArray<int>(keys.Length);
        for (var i = 0; i < indices.Length; i++)
            indices[i] = i;
        Array.Sort(keys, indices, Comparer<int>.Default);
        Array.Reverse(indices);
        Array.Reverse(keys);
        var changed = false;
        for (var i = 0; i < indices.Length; i++)
            if (indices[i] != i)
            {
                changed = true;
                break;
            }
        if (!changed)
            return;
        var temp = GC.AllocateUninitializedArray<string>(keys.Length);
        Array.Copy(itemsArray, temp, keys.Length);
        for (var j = 0; j < keys.Length; j++)
            itemsArray[j] = temp[indices[j]];
    }
}
