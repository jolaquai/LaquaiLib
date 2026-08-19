using LaquaiLib.Collections.Enumeration;
using LaquaiLib.Interfaces;

namespace LaquaiLib.UnitTests.Collections.Enumeration;

public class MultiDimArrayEnumeratorTests
{
    [Fact]
    public void OneDimArray()
    {
        int[] array = [1, 2, 3];
        var enumerable = new MultiDimArrayEnumerable<int>(array);
        Assert.Collection(enumerable,
            static i => Assert.Equal(1, i),
            static i => Assert.Equal(2, i),
            static i => Assert.Equal(3, i)
        );
    }
    [Fact]
    public void TwoDimArray()
    {
        int[,] array =
        {
            {1, 2, 3},
            {4, 5, 6}
        };
        var enumerable = new MultiDimArrayEnumerable<int>(array);
        Assert.Collection(enumerable,
            static i => Assert.Equal(1, i),
            static i => Assert.Equal(2, i),
            static i => Assert.Equal(3, i),
            static i => Assert.Equal(4, i),
            static i => Assert.Equal(5, i),
            static i => Assert.Equal(6, i)
        );
    }
    [Fact]
    public void ThreeDimArray()
    {
        int[,,] array =
        {
            {
                {1, 2, 3},
                {4, 5, 6}
            },
            {
                {7, 8, 9},
                {10, 11, 12}
            }
        };
        var enumerable = new MultiDimArrayEnumerable<int>(array);
        Assert.Collection(enumerable,
            static i => Assert.Equal(1, i),
            static i => Assert.Equal(2, i),
            static i => Assert.Equal(3, i),
            static i => Assert.Equal(4, i),
            static i => Assert.Equal(5, i),
            static i => Assert.Equal(6, i),
            static i => Assert.Equal(7, i),
            static i => Assert.Equal(8, i),
            static i => Assert.Equal(9, i),
            static i => Assert.Equal(10, i),
            static i => Assert.Equal(11, i),
            static i => Assert.Equal(12, i)
        );
    }

    [Fact]
    public void NullArrayThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(static () => _ = new MultiDimArrayEnumerable<int>(null));
    }
    [Fact]
    public void EmptyArrayEnumeratesNothing()
    {
        var array = new int[0, 3];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        var enumerator = enumerable.GetEnumerator();

        Assert.False(enumerator.MoveNext());
    }
    [Fact]
    public void EnumerationReflectsArrayMutations()
    {
        var array = new int[,] { { 1, 2 }, { 3, 4 } };
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        array[1, 1] = 40;

        Assert.Collection(enumerable,
            static i => Assert.Equal(1, i),
            static i => Assert.Equal(2, i),
            static i => Assert.Equal(3, i),
            static i => Assert.Equal(40, i)
        );
    }

    [Fact]
    public void CurrentBeforeMoveNextThrows()
    {
        int[] array = [1, 2, 3];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        var enumerator = enumerable.GetEnumerator();

        Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
    }
    [Fact]
    public void CurrentAfterEnumerationThrows()
    {
        int[] array = [1, 2];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext())
        {
        }

        Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
    }
    [Fact]
    public void CurrentOnEmptyArrayThrows()
    {
        var array = new int[0];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        var enumerator = enumerable.GetEnumerator();
        Assert.False(enumerator.MoveNext());

        Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
    }
    [Fact]
    public void MoveNextPastEndRemainsFalse()
    {
        int[] array = [1];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        var enumerator = enumerable.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
    }
    [Fact]
    public void ResetRestartsEnumeration()
    {
        int[] array = [1, 2, 3];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        var enumerator = enumerable.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.True(enumerator.MoveNext());
        Assert.Equal(2, enumerator.Current);
        enumerator.Reset();
        Assert.True(enumerator.MoveNext());
        Assert.Equal(1, enumerator.Current);
    }
    [Fact]
    public void ResetAfterExhaustionRestartsEnumeration()
    {
        int[] array = [1, 2];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext())
        {
        }

        enumerator.Reset();
        Assert.True(enumerator.MoveNext());
        Assert.Equal(1, enumerator.Current);
    }
    [Fact]
    public void MultipleEnumeratorsAreIndependent()
    {
        int[] array = [1, 2, 3];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        var first = enumerable.GetEnumerator();
        var second = enumerable.GetEnumerator();

        Assert.True(first.MoveNext());
        Assert.True(first.MoveNext());
        Assert.Equal(2, first.Current);
        Assert.True(second.MoveNext());
        Assert.Equal(1, second.Current);
    }
    [Fact]
    public void GetEnumeratorReturnsValueTypeEnumerator()
    {
        int[] array = [1];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);

        Assert.IsType<MultiDimArrayEnumerable<int>.Enumerator>((object)enumerable.GetEnumerator());
    }
    [Fact]
    public void NonGenericCurrentMatchesGenericCurrent()
    {
        int[] array = [7, 8];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        IEnumerator enumerator = enumerable.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal(7, enumerator.Current);
    }

    [Fact]
    public void NonGenericEnumerableEnumeratesAllElements()
    {
        var array = new int[,] { { 1, 2 }, { 3, 4 } };
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        IEnumerable sequence = enumerable;

        var seen = new List<int>();
        foreach (int item in sequence)
            seen.Add(item);

        Assert.Equal([1, 2, 3, 4], seen);
    }

    [Fact]
    public void SpanLengthMatchesTotalElementCount()
    {
        var array = new int[2, 3, 4];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);

        Assert.Equal(24, enumerable.Span.Length);
    }
    [Fact]
    public void SpanWritesArePropagatedToArray()
    {
        var array = new int[2, 2];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);

        enumerable.Span[0] = 11;
        enumerable.Span[3] = 44;

        Assert.Equal(11, array[0, 0]);
        Assert.Equal(44, array[1, 1]);
    }
    [Fact]
    public void ReadOnlySpanMatchesArrayContents()
    {
        var array = new int[,] { { 1, 2 }, { 3, 4 } };
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        IReadOnlySpanProvider<int> provider = enumerable;

        Assert.True(provider.ReadOnlySpan.SequenceEqual([1, 2, 3, 4]));
    }
    [Fact]
    public void GetPinnableReferenceExposesFirstElement()
    {
        var array = new int[,] { { 5, 6 }, { 7, 8 } };
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        ISpanProvider<int> provider = enumerable;

        ref var first = ref provider.GetPinnableReference();
        Assert.Equal(5, first);
        first = 99;
        Assert.Equal(99, array[0, 0]);
    }

    [Fact]
    public void MismatchedElementSizeThrowsArgumentException()
    {
        var array = new int[2, 3];

        Assert.Throws<ArgumentException>(() => _ = new MultiDimArrayEnumerable<long>(array));
    }
    [Fact]
    public void SameSizeElementTypeIsAllowed()
    {
        DayOfWeek[] array = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);

        Assert.Equal([1, 3, 5], enumerable.Span.ToArray());
    }
    [Fact]
    public void ReferenceTypeParameterThrowsArgumentException()
    {
        var array = new long[4];

        Assert.Throws<ArgumentException>(() => _ = new MultiDimArrayEnumerable<string>(array));
    }
    [Fact]
    public void ReferenceElementArrayThrowsArgumentException()
    {
        var array = new string[3];

        Assert.Throws<ArgumentException>(() => _ = new MultiDimArrayEnumerable<int>(array));
    }

    [Fact]
    public void ReinterpretWideningIncreasesElementCount()
    {
        var array = new int[2, 3];
        using var enumerable = MultiDimArrayEnumerable<byte>.Reinterpret(array);

        Assert.Equal(24, enumerable.Span.Length);
    }
    [Fact]
    public void ReinterpretNarrowingDecreasesElementCount()
    {
        var array = new int[2, 3];
        using var enumerable = MultiDimArrayEnumerable<long>.Reinterpret(array);

        Assert.Equal(3, enumerable.Span.Length);
    }
    [Fact]
    public void ReinterpretTruncatesTrailingPartialElement()
    {
        var array = new int[3];
        using var enumerable = MultiDimArrayEnumerable<long>.Reinterpret(array);

        Assert.Equal(1, enumerable.Span.Length);
    }
    [Fact]
    public void ReinterpretObservesUnderlyingBytes()
    {
        int[] array = [0x04030201, 0x08070605];
        using var enumerable = MultiDimArrayEnumerable<byte>.Reinterpret(array);

        Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8], enumerable.Span.ToArray());
    }
    [Fact]
    public void ReinterpretWritesArePropagatedToArray()
    {
        var array = new int[2];
        using var enumerable = MultiDimArrayEnumerable<byte>.Reinterpret(array);

        enumerable.Span[4] = 1;

        Assert.Equal(0, array[0]);
        Assert.Equal(1, array[1]);
    }
    [Fact]
    public void ReinterpretRejectsReferenceTypeParameter()
    {
        var array = new long[4];

        Assert.Throws<ArgumentException>(() => _ = MultiDimArrayEnumerable<string>.Reinterpret(array));
    }
    [Fact]
    public void ReinterpretEnumeratesReinterpretedElements()
    {
        int[] array = [0x02010000, 0x04030000];
        using var enumerable = MultiDimArrayEnumerable<short>.Reinterpret(array);

        Assert.Equal([0, 0x0201, 0, 0x0403], enumerable.Span.ToArray());
    }

    private struct ReferenceHolder
    {
        public string Value;
    }

    [Fact]
    public void ReferenceArrayWithSameElementTypeIsAllowed()
    {
        var first = new MemoryStream();
        var second = new MemoryStream();
        Stream[,] array = { { first, second } };
        using var enumerable = new MultiDimArrayEnumerable<Stream>(array);

        Assert.Equal([first, second], enumerable.Span.ToArray());
    }
    [Fact]
    public void ReferenceArrayCrossTypeThrowsFromConstructor()
    {
        var array = new Stream[2, 2];

        Assert.Throws<ArgumentException>(() => _ = new MultiDimArrayEnumerable<MemoryStream>(array));
    }
    [Fact]
    public void ReinterpretRejectsReferenceArrayCrossType()
    {
        var array = new Stream[2, 2];

        Assert.Throws<ArgumentException>(() => _ = MultiDimArrayEnumerable<MemoryStream>.Reinterpret(array));
    }
    [Fact]
    public void ReinterpretUnsafeAllowsReferenceDowncast()
    {
        var first = new MemoryStream();
        var second = new MemoryStream();
        Stream[,] array = { { first, second } };
        using var enumerable = MultiDimArrayEnumerable<MemoryStream>.ReinterpretUnsafe(array);

        Assert.Equal([first, second], enumerable.Span.ToArray());
        Assert.Collection(enumerable,
            item => Assert.Same(first, item),
            item => Assert.Same(second, item)
        );
    }
    [Fact]
    public void ReinterpretUnsafeAllowsReferenceUpcast()
    {
        var stream = new MemoryStream();
        MemoryStream[] array = [stream];
        using var enumerable = MultiDimArrayEnumerable<Stream>.ReinterpretUnsafe(array);

        Assert.Same(stream, enumerable.Span[0]);
    }
    [Fact]
    public void ReinterpretUnsafeStillRejectsMixedTrackedness()
    {
        var array = new long[4];

        Assert.Throws<ArgumentException>(() => _ = MultiDimArrayEnumerable<string>.ReinterpretUnsafe(array));
    }
    [Fact]
    public void ReinterpretUnsafeStillRejectsReferenceHoldingStructs()
    {
        var array = new ReferenceHolder[4];

        Assert.Throws<ArgumentException>(() => _ = MultiDimArrayEnumerable<string>.ReinterpretUnsafe(array));
    }
    [Fact]
    public void ReinterpretRejectsReferenceHoldingStructArray()
    {
        var array = new ReferenceHolder[4];

        Assert.Throws<ArgumentException>(() => _ = MultiDimArrayEnumerable<long>.Reinterpret(array));
    }
    [Fact]
    public void ReferenceHoldingStructArrayWithSameElementTypeIsAllowed()
    {
        ReferenceHolder[] array = [new ReferenceHolder { Value = "a" }, new ReferenceHolder { Value = "b" }];
        using var enumerable = new MultiDimArrayEnumerable<ReferenceHolder>(array);

        Assert.Equal(["a", "b"], enumerable.Span.ToArray().Select(static h => h.Value));
    }
    [Fact]
    public void ReinterpretUnsafeStillRejectsReferenceHoldingStructTarget()
    {
        var array = new string[2];

        Assert.Throws<ArgumentException>(() => _ = MultiDimArrayEnumerable<ReferenceHolder>.ReinterpretUnsafe(array));
    }
    [Fact]
    public unsafe void PointerElementArrayIsViewableAsNativeInt()
    {
        var array = new int*[2];
        using var enumerable = new MultiDimArrayEnumerable<nint>(array);

        Assert.Equal(2, enumerable.Span.Length);
    }

    [Fact]
    public void SpanRemainsValidAcrossGarbageCollection()
    {
        var array = new int[2, 3];
        using var enumerable = new MultiDimArrayEnumerable<int>(array);
        enumerable.Span.Fill(7);

        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();

        Assert.Equal([7, 7, 7, 7, 7, 7], enumerable.Span.ToArray());
        Assert.Equal(7, array[1, 2]);
    }

    [Fact]
    public void SpanAfterDisposeThrowsObjectDisposedException()
    {
        int[] array = [1, 2, 3];
        var enumerable = new MultiDimArrayEnumerable<int>(array);
        enumerable.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = enumerable.Span.Length);
    }
    [Fact]
    public void CurrentAfterDisposeThrowsObjectDisposedException()
    {
        int[] array = [1, 2, 3];
        var enumerable = new MultiDimArrayEnumerable<int>(array);
        var enumerator = enumerable.GetEnumerator();
        Assert.True(enumerator.MoveNext());
        enumerable.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = enumerator.Current);
    }
    [Fact]
    public void DisposeIsIdempotent()
    {
        int[] array = [1, 2, 3];
        var enumerable = new MultiDimArrayEnumerable<int>(array);

        enumerable.Dispose();
        enumerable.Dispose();
        enumerable.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _ = enumerable.Span.Length);
    }
    [Fact]
    public void MoveNextAfterDisposeStillReportsPositionWithoutReading()
    {
        int[] array = [1, 2, 3];
        var enumerable = new MultiDimArrayEnumerable<int>(array);
        var enumerator = enumerable.GetEnumerator();
        enumerable.Dispose();

        Assert.True(enumerator.MoveNext());
        Assert.Throws<ObjectDisposedException>(() => _ = enumerator.Current);
    }
}
