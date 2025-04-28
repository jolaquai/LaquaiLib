using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IEnumerableExtensionsTests
{
    #region TryGetSpan Tests
    [Fact]
    public void TryGetSpanWithArrayReturnsTrue()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.TryGetSpan(out var span);

        Assert.True(result);
        Assert.Equal(3, span.Length);
        Assert.Equal(1, span[0]);
        Assert.Equal(2, span[1]);
        Assert.Equal(3, span[2]);
    }

    [Fact]
    public void TryGetSpanWithListReturnsTrue()
    {
        var source = new List<int> { 1, 2, 3 };

        var result = source.TryGetSpan(out var span);

        Assert.True(result);
        Assert.Equal(3, span.Length);
        Assert.Equal(1, span[0]);
        Assert.Equal(2, span[1]);
        Assert.Equal(3, span[2]);
    }

    [Fact]
    public void TryGetSpanWithIEnumerableReturnsFalse()
    {
        IEnumerable<int> source = new HashSet<int> { 1, 2, 3 };

        var result = source.TryGetSpan(out var span);

        Assert.False(result);
        Assert.Equal(0, span.Length);
    }
    #endregion

    #region SelectMany Tests
    [Fact]
    public void SelectManyFlattensNestedSequences()
    {
        var source = new List<List<int>>
        {
            new List<int> { 1, 2 },
            new List<int> { 3, 4 },
            new List<int> { 5, 6 }
        };

        var result = source.SelectMany().ToArray();

        Assert.Equal([1, 2, 3, 4, 5, 6], result);
    }

    [Fact]
    public void SelectManyWithEmptySourceReturnsEmptySequence()
    {
        var source = new List<List<int>>();

        var result = source.SelectMany().ToArray();

        Assert.Empty(result);
    }

    [Fact]
    public void SelectManyWithEmptyNestedSequencesReturnsEmptySequence()
    {
        var source = new List<List<int>>
        {
            new List<int>(),
            new List<int>(),
            new List<int>()
        };

        var result = source.SelectMany().ToArray();

        Assert.Empty(result);
    }
    #endregion

    #region Split Tests
    [Fact]
    public void SplitDividesSequenceBasedOnPredicate()
    {
        var source = new[] { 1, 2, 3, 4, 5, 6 };

        var (trueList, falseList) = source.Split(x => x % 2 == 0);

        Assert.Equal(new[] { 2, 4, 6 }, trueList);
        Assert.Equal(new[] { 1, 3, 5 }, falseList);
    }

    [Fact]
    public void SplitWithEmptySourceReturnsEmptyLists()
    {
        var source = Array.Empty<int>();

        var (trueList, falseList) = source.Split(x => x % 2 == 0);

        Assert.Empty(trueList);
        Assert.Empty(falseList);
    }

    [Fact]
    public void SplitWithAllTruePredicateReturnsAllInTrueList()
    {
        var source = new[] { 2, 4, 6, 8 };

        var (trueList, falseList) = source.Split(x => x % 2 == 0);

        Assert.Equal(source, trueList);
        Assert.Empty(falseList);
    }

    [Fact]
    public void SplitWithAllFalsePredicateReturnsAllInFalseList()
    {
        var source = new[] { 1, 3, 5, 7 };

        var (trueList, falseList) = source.Split(x => x % 2 == 0);

        Assert.Empty(trueList);
        Assert.Equal(source, falseList);
    }

    [Fact]
    public void SplitThrowsForNullSource()
    {
        IEnumerable<int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.Split(x => x % 2 == 0));
    }

    [Fact]
    public void SplitThrowsForNullPredicate()
    {
        IEnumerable<int> source = [1, 2, 3];

        Assert.Throws<ArgumentNullException>(() => source.Split(null));
    }
    #endregion

    #region Halve Tests
    [Fact]
    public void HalveDividesSequenceIntoEqualParts()
    {
        var source = new[] { 1, 2, 3, 4 };

        var (first, second) = source.Halve();

        Assert.Equal([1, 2], first);
        Assert.Equal([3, 4], second);
    }

    [Fact]
    public void HalveWithOddLengthDividesSequenceWithFirstPartSmaller()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var (first, second) = source.Halve();

        Assert.Equal([1, 2], first);
        Assert.Equal([3, 4, 5], second);
    }

    [Fact]
    public void HalveThrowsForEmptySequence()
    {
        var source = Array.Empty<int>();

        Assert.Throws<ArgumentException>(() => source.Halve());
    }

    [Fact]
    public void HalveThrowsForSingleElementSequence()
    {
        var source = new[] { 1 };

        Assert.Throws<ArgumentException>(() => source.Halve());
    }
    #endregion

    #region Random Tests
    [Fact]
    public void RandomReturnsElementFromSequence()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.Random();

        Assert.Contains(result, source);
    }

    [Fact]
    public void RandomWithSingleElementReturnsElement()
    {
        var source = new[] { 42 };

        var result = source.Random();

        Assert.Equal(42, result);
    }

    [Fact]
    public void RandomWithCustomRandomInstanceWorks()
    {
        var source = new[] { 1, 2, 3 };
        var random = new Random(42);

        var result = source.Random(random);

        Assert.Contains(result, source);
    }

    [Fact]
    public void RandomThrowsForEmptySequence()
    {
        var source = Array.Empty<int>();

        Assert.Throws<InvalidOperationException>(() => source.Random());
    }

    [Fact]
    public void RandomThrowsForNullSource()
    {
        IEnumerable<int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.Random());
    }

    [Fact]
    public void RandomWorksWithNonIndexedCollection()
    {
        IEnumerable<int> source = new HashSet<int> { 1, 2, 3 };

        var result = source.Random();

        Assert.Contains(result, source);
    }
    #endregion

    #region Interlace Tests
    [Fact]
    public void InterlaceWithEqualSizedSequencesReturnsInterlacedSequence()
    {
        var first = new[] { 1, 3, 5 };
        var second = new[] { 2, 4, 6 };

        var result = first.Interlace(second).ToArray();

        Assert.Equal([1, 2, 3, 4, 5, 6], result);
    }

    [Fact]
    public void InterlaceWithFirstSequenceLargerAppendsRemainingElements()
    {
        var first = new[] { 1, 3, 5, 7 };
        var second = new[] { 2, 4 };

        var result = first.Interlace(second).ToArray();

        Assert.Equal([1, 2, 3, 4, 5, 7], result);
    }

    [Fact]
    public void InterlaceWithSecondSequenceLargerAppendsRemainingElements()
    {
        var first = new[] { 1, 3 };
        var second = new[] { 2, 4, 6, 8 };

        var result = first.Interlace(second).ToArray();

        Assert.Equal([1, 2, 3, 4, 6, 8], result);
    }

    [Fact]
    public void InterlaceWithEmptyFirstSequenceReturnsSecondSequence()
    {
        var first = Array.Empty<int>();
        var second = new[] { 2, 4, 6 };

        var result = first.Interlace(second).ToArray();

        Assert.Equal(second, result);
    }

    [Fact]
    public void InterlaceWithEmptySecondSequenceReturnsFirstSequence()
    {
        var first = new[] { 1, 3, 5 };
        var second = Array.Empty<int>();

        var result = first.Interlace(second).ToArray();

        Assert.Equal(first, result);
    }

    [Fact]
    public void InterlaceWithBothEmptySequencesReturnsEmptySequence()
    {
        var first = Array.Empty<int>();
        var second = Array.Empty<int>();

        var result = first.Interlace(second).ToArray();

        Assert.Empty(result);
    }
    #endregion

    #region ForEach Tests
    [Fact]
    public void ForEachAppliesActionToEachElement()
    {
        var source = new[] { 1, 2, 3 };
        var sum = 0;

        source.ForEach(x => sum += x);

        Assert.Equal(6, sum);
    }

    [Fact]
    public void ForEachWithIndexAppliesActionToEachElementWithIndex()
    {
        var source = new[] { 1, 2, 3 };
        var result = new List<(int Value, int Index)>();

        source.ForEach((x, i) => result.Add((x, i)));

        Assert.Equal(3, result.Count);
        Assert.Equal((1, 0), result[0]);
        Assert.Equal((2, 1), result[1]);
        Assert.Equal((3, 2), result[2]);
    }

    [Fact]
    public void ForEachWithEmptySequenceDoesNothing()
    {
        var source = Array.Empty<int>();
        var wasCalled = false;

        source.ForEach(_ => wasCalled = true);

        Assert.False(wasCalled);
    }

    [Fact]
    public async Task ForEachAsyncAppliesActionToEachElementAsync()
    {
        var source = new[] { 1, 2, 3 };
        var sum = 0;

        await source.ForEachAsync(x =>
        {
            sum += x;
            return Task.CompletedTask;
        });

        Assert.Equal(6, sum);
    }

    [Fact]
    public async Task ForEachAsyncWithIndexAppliesActionToEachElementWithIndexAsync()
    {
        var source = new[] { 1, 2, 3 };
        var result = new List<(int Value, int Index)>();

        await source.ForEachAsync((x, i) =>
        {
            result.Add((x, i));
            return Task.CompletedTask;
        });

        Assert.Equal(3, result.Count);
        Assert.Equal((1, 0), result[0]);
        Assert.Equal((2, 1), result[1]);
        Assert.Equal((3, 2), result[2]);
    }

    [Fact]
    public async Task ForEachAsyncWithEmptySequenceDoesNothing()
    {
        var source = Array.Empty<int>();
        var wasCalled = false;

        await source.ForEachAsync(_ =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(wasCalled);
    }
    #endregion

    #region GetRange Tests
    [Fact]
    public void GetRangeReturnsElementsInRange()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.GetRange(1..4).ToArray();

        Assert.Equal([2, 3, 4], result);
    }

    [Fact]
    public void GetRangeWithEndIndexReturnsElementsToEnd()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.GetRange(2..).ToArray();

        Assert.Equal([3, 4, 5], result);
    }

    [Fact]
    public void GetRangeWithStartIndexReturnsElementsFromStart()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.GetRange(..3).ToArray();

        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public void GetRangeWithEmptyRangeReturnsEmptySequence()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.GetRange(2..2).ToArray();

        Assert.Empty(result);
    }

    [Fact]
    public void GetRangeWithNonIndexableCollectionWorks()
    {
        var source = Enumerable.Range(1, 5);

        var result = source.GetRange(1..4).ToArray();

        Assert.Equal([2, 3, 4], result);
    }
    #endregion

    #region AllEqual Tests
    [Fact]
    public void AllEqualReturnsTrueWhenAllElementsEqual()
    {
        var source = new[] { 42, 42, 42 };

        var result = source.AllEqual();

        Assert.True(result);
    }

    [Fact]
    public void AllEqualReturnsFalseWhenAnyElementDiffers()
    {
        var source = new[] { 42, 42, 43 };

        var result = source.AllEqual();

        Assert.False(result);
    }

    [Fact]
    public void AllEqualReturnsTrueForSingleElementSequence()
    {
        var source = new[] { 42 };

        var result = source.AllEqual();

        Assert.True(result);
    }

    [Fact]
    public void AllEqualThrowsForEmptySequence()
    {
        var source = Array.Empty<int>();

        Assert.Throws<ArgumentException>(() => source.AllEqual());
    }

    [Fact]
    public void AllEqualHandlesNullValuesCorrectly()
    {
        var source = new string[] { null, null, null };

        var result = source.AllEqual();

        Assert.True(result);
    }

    [Fact]
    public void AllEqualReturnsFalseForMixedNullAndNonNull()
    {
        var source = new string[] { "test", null, "test" };

        var result = source.AllEqual();

        Assert.False(result);
    }
    #endregion

    #region Mode and ModeBy Tests
    [Fact]
    public void ModeReturnsValueWithHighestFrequency()
    {
        var source = new[] { 1, 2, 2, 3, 2, 4 };

        var result = source.Mode();

        Assert.Equal(2, result);
    }

    [Fact]
    public void ModeReturnsFirstOccurrenceWithHighestFrequency()
    {
        var source = new[] { 1, 2, 2, 3, 3, 4 };

        var result = source.Mode();

        Assert.Equal(2, result);
    }

    [Fact]
    public void ModeThrowsForEmptySequence()
    {
        var source = Array.Empty<int>();

        Assert.Throws<ArgumentException>(() => source.Mode());
    }

    [Fact]
    public void ModeWithCustomEqualityComparerWorks()
    {
        var source = new[] { "A", "a", "B", "b", "b" };

        var result = source.Mode(StringComparer.OrdinalIgnoreCase);

        Assert.Equal("B", result);
    }

    [Fact]
    public void ModeByReturnsElementWithHighestFrequencyBasedOnSelector()
    {
        var source = new[]
        {
            new { Id = 1, Category = "A" },
            new { Id = 2, Category = "B" },
            new { Id = 3, Category = "A" },
            new { Id = 4, Category = "A" }
        };

        var result = source.ModeBy(x => x.Category);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void ModeByThrowsForEmptySequence()
    {
        var source = Array.Empty<string>();

        Assert.Throws<ArgumentException>(() => source.ModeBy(x => x.Length));
    }

    [Fact]
    public void ModeByWithNullSelectorFallsBackToMode()
    {
        var source = new[] { 1, 2, 2, 3, 2, 4 };

        var result = source.ModeBy<int, int>(null);

        Assert.Equal(2, result);
    }
    #endregion

    #region Sample and OrderedSample Tests
    [Fact]
    public void SampleReturnsRequestedNumberOfElements()
    {
        var source = Enumerable.Range(1, 100).ToArray();

        var result = source.Sample(10);

        Assert.Equal(10, result.Length);
        Assert.All(result, x => Assert.Contains(x, source));
    }

    [Fact]
    public void SampleWithoutItemCountReturnsOnePercentOfElements()
    {
        var source = Enumerable.Range(1, 100).ToArray();

        var result = source.Sample();

        Assert.Equal(1, result.Length);
        Assert.All(result, x => Assert.Contains(x, source));
    }

    [Fact]
    public void OrderedSampleReturnsElementsInSameRelativeOrder()
    {
        var source = Enumerable.Range(1, 100).ToArray();

        var result = source.OrderedSample(10).ToArray();

        Assert.Equal(10, result.Length);

        for (var i = 1; i < result.Length; i++)
        {
            Assert.True(result[i] > result[i - 1]);
        }
    }

    [Fact]
    public void OrderedSampleWithoutItemCountReturnsOnePercentOfElements()
    {
        var source = Enumerable.Range(1, 100).ToArray();

        var result = source.OrderedSample().ToArray();

        Assert.Equal(1, result.Length);
    }
    #endregion

    #region SequenceEquivalent Tests
    [Fact]
    public void SequenceEquivalentReturnsTrueForEquivalentSequences()
    {
        var first = new[] { 1, 2, 3 };
        var second = new[] { 3, 1, 2 };

        var result = first.SequenceEquivalent(second);

        Assert.True(result);
    }

    [Fact]
    public void SequenceEquivalentReturnsFalseForDifferentSequences()
    {
        var first = new[] { 1, 2, 3 };
        var second = new[] { 1, 2, 4 };

        var result = first.SequenceEquivalent(second);

        Assert.False(result);
    }

    [Fact]
    public void SequenceEquivalentReturnsFalseForDifferentLengthSequences()
    {
        var first = new[] { 1, 2, 3 };
        var second = new[] { 1, 2 };

        var result = first.SequenceEquivalent(second);

        Assert.False(result);
    }

    [Fact]
    public void SequenceEquivalentReturnsTrueForSameElementDifferentFrequencies()
    {
        var first = new[] { 1, 1, 2 };
        var second = new[] { 1, 2, 1 };

        var result = first.SequenceEquivalent(second);

        Assert.True(result);
    }

    [Fact]
    public void SequenceEquivalentReturnsFalseForDuplicatedElements()
    {
        var first = new[] { 1, 2, 3 };
        var second = new[] { 1, 2, 2 };

        var result = first.SequenceEquivalent(second);

        Assert.False(result);
    }

    [Fact]
    public void SequenceEquivalentWithCustomComparerWorks()
    {
        var first = new[] { "a", "B", "c" };
        var second = new[] { "A", "b", "C" };

        var result = first.SequenceEquivalent(second, StringComparer.OrdinalIgnoreCase);

        Assert.True(result);
    }

    [Fact]
    public void SequenceEquivalentReturnsTrueForBothNull()
    {
        IEnumerable<int> first = null;
        IEnumerable<int> second = null;

        var result = first.SequenceEquivalent(second);

        Assert.True(result);
    }

    [Fact]
    public void SequenceEquivalentReturnsFalseForOneNull()
    {
        IEnumerable<int> first = null;
        var second = new[] { 1, 2, 3 };

        var result1 = first.SequenceEquivalent(second);
        var result2 = second.SequenceEquivalent(first);

        Assert.False(result1);
        Assert.False(result2);
    }
    #endregion

    #region SelectWhere and SelectOnlyWhere Tests
    [Fact]
    public void SelectWhereTransformsOnlyElementsThatSatisfyPredicate()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.SelectWhere(x => x % 2 == 0, x => x * 10).ToArray();

        Assert.Equal([1, 20, 3, 40, 5], result);
    }

    [Fact]
    public void SelectWhereWithIndexTransformsBasedOnPredicateWithIndex()
    {
        var source = new[] { 10, 20, 30, 40, 50 };

        var result = source.SelectWhere((x, i) => i % 2 == 0, (x, i) => x + i).ToArray();

        Assert.Equal([10, 20, 32, 40, 54], result);
    }

    [Fact]
    public void SelectOnlyWhereFiltersAndTransformsElements()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.SelectOnlyWhere(x => x % 2 == 0, x => x * 10).ToArray();

        Assert.Equal([20, 40], result);
    }

    [Fact]
    public void SelectOnlyWhereWithIndexFiltersAndTransformsWithIndex()
    {
        var source = new[] { "a", "bb", "dddd", "ccc" };

        var result = source.SelectOnlyWhere((s, i) => s.Length > i, (s, i) => $"{s}:{i}").ToArray();
        // -> 1 > 0, 2 > 1, 4 > 2, 3 > 3
        // -> true,  true,  true,  false

        Assert.Equal(["a:0", "bb:1", "dddd:2"], result);
    }
    #endregion

    #region Blitted Tests
    [Fact]
    public void BlittedConvertsElementsToBytes()
    {
        var source = new[] { (byte)1, (byte)2, (byte)3 };

        var result = source.Blitted().ToArray();

        Assert.Equal(new byte[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void BlittedWithIntsConvertsToCorrectBytes()
    {
        var source = new[] { 1 };

        var result = source.Blitted().ToArray();

        Assert.Equal(BitConverter.GetBytes(1), result);
    }

    [Fact]
    public void BlittedWithEmptySequenceReturnsEmptySequence()
    {
        var source = Array.Empty<int>();

        var result = source.Blitted().ToArray();

        Assert.Empty(result);
    }
    #endregion

    #region IfWhere and WhereNot Tests
    [Fact]
    public void IfWhereFiltersWhenExpressionIsTrue()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.IfWhere(true, x => x % 2 == 0).ToArray();

        Assert.Equal([2, 4], result);
    }

    [Fact]
    public void IfWhereReturnsOriginalSequenceWhenExpressionIsFalse()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.IfWhere(false, x => x % 2 == 0).ToArray();

        Assert.Equal(source, result);
    }

    [Fact]
    public void WhereNotFiltersElementsThatDoNotSatisfyPredicate()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.WhereNot(x => x % 2 == 0).ToArray();

        Assert.Equal([1, 3, 5], result);
    }
    #endregion

    #region NotOfType Tests
    [Fact]
    public void NotOfTypeExcludesSpecifiedType()
    {
        var cat = new Cat();
        var dog = new Dog();
        var fish = new Fish();

        var source = new Animal[] { cat, dog, fish };

        var result = source.NotOfType<Animal, Dog>().ToArray();

        Assert.Equal(2, result.Length);
        Assert.Contains(cat, result);
        Assert.Contains(fish, result);
        Assert.DoesNotContain(dog, result);
    }

    [Fact]
    public void NotOfTypeReturnsOriginalSequenceWhenTypeIsNotAssignable()
    {
        var source = new[] { "a", "b", "c" };

        var result = source.NotOfType<string, int>().ToArray();

        Assert.Equal(source, result);
    }

    private class Animal { }
    private class Cat : Animal { }
    private class Dog : Animal { }
    private class Fish : Animal { }
    #endregion

    #region Indexed Tests
    [Fact]
    public void IndexedReturnsElementsWithCounts()
    {
        var source = new[] { "a", "b", "a", "c", "b", "a" };

        var result = source.Indexed().ToArray();

        Assert.Equal(3, result.Length);
        Assert.Contains(result, kv => kv.Key == "a" && kv.Value == 3);
        Assert.Contains(result, kv => kv.Key == "b" && kv.Value == 2);
        Assert.Contains(result, kv => kv.Key == "c" && kv.Value == 1);
    }

    [Fact]
    public void IndexedWithEmptySequenceReturnsEmptyResult()
    {
        var source = Array.Empty<string>();

        var result = source.Indexed().ToArray();

        Assert.Empty(result);
    }
    #endregion

    #region None Tests
    [Fact]
    public void NoneReturnsTrueForEmptySequence()
    {
        var source = Array.Empty<int>();

        var result = source.None();

        Assert.True(result);
    }

    [Fact]
    public void NoneReturnsFalseForNonEmptySequence()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.None();

        Assert.False(result);
    }

    [Fact]
    public void NoneWithPredicateReturnsTrueWhenNoElementSatisfiesPredicate()
    {
        var source = new[] { 1, 3, 5 };

        var result = source.None(x => x % 2 == 0);

        Assert.True(result);
    }

    [Fact]
    public void NoneWithPredicateReturnsFalseWhenAnyElementSatisfiesPredicate()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.None(x => x % 2 == 0);

        Assert.False(result);
    }
    #endregion

    #region OnlyOrDefault Tests
    [Fact]
    public void OnlyOrDefaultReturnsSingleElementInSequence()
    {
        var source = new[] { 42 };

        var result = source.OnlyOrDefault();

        Assert.Equal(42, result);
    }

    [Fact]
    public void OnlyOrDefaultReturnsDefaultForEmptySequence()
    {
        var source = Array.Empty<int>();

        var result = source.OnlyOrDefault();

        Assert.Equal(0, result);
    }

    [Fact]
    public void OnlyOrDefaultReturnsDefaultForMultipleElements()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.OnlyOrDefault();

        Assert.Equal(0, result);
    }

    [Fact]
    public void OnlyOrDefaultReturnsSpecifiedDefaultValue()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.OnlyOrDefault(-1);

        Assert.Equal(-1, result);
    }

    [Fact]
    public void OnlyOrDefaultWithPredicateReturnsSingleMatchingElement()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.OnlyOrDefault(x => x % 2 == 0);

        Assert.Equal(2, result);
    }

    [Fact]
    public void OnlyOrDefaultWithPredicateReturnsDefaultForNoMatches()
    {
        var source = new[] { 1, 3, 5 };

        var result = source.OnlyOrDefault(x => x % 2 == 0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void OnlyOrDefaultWithPredicateReturnsDefaultForMultipleMatches()
    {
        var source = new[] { 1, 2, 3, 4 };

        var result = source.OnlyOrDefault(x => x % 2 == 0);

        Assert.Equal(0, result);
    }
    #endregion

    #region ElementAtOrDefault Tests
    [Fact]
    public void ElementAtOrDefaultReturnsElementAtValidIndex()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.ElementAtOrDefault(1);

        Assert.Equal(2, result);
    }

    [Fact]
    public void ElementAtOrDefaultReturnsDefaultForOutOfRangeIndex()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.ElementAtOrDefault(5);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ElementAtOrDefaultReturnsDefaultForNegativeIndex()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.ElementAtOrDefault(-1);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ElementAtOrDefaultReturnsSpecifiedDefaultValue()
    {
        var source = new[] { 1, 2, 3 };

        var result = source.ElementAtOrDefault(5, -1);

        Assert.Equal(-1, result);
    }

    [Fact]
    public void ElementAtOrDefaultWorksWithNonIndexableCollection()
    {
        IEnumerable<int> source = new HashSet<int> { 1, 2, 3 };

        var result = source.ElementAtOrDefault(1);

        Assert.Equal(2, result);
    }

    [Fact]
    public void ElementAtOrDefaultThrowsForNullSource()
    {
        IEnumerable<int> source = null;

        Assert.Throws<ArgumentNullException>(() => source.ElementAtOrDefault(0));
    }
    #endregion

    #region IndexOf Tests
    [Fact]
    public void IndexOfReturnsPositionOfElement()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.IndexOf(3);

        Assert.Equal(2, result);
    }

    [Fact]
    public void IndexOfReturnsNegativeOneForMissingElement()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.IndexOf(6);

        Assert.Equal(-1, result);
    }

    [Fact]
    public void IndexOfWithCustomComparerWorks()
    {
        var source = new[] { "a", "B", "c" };

        var result = source.IndexOf("b", StringComparer.OrdinalIgnoreCase);

        Assert.Equal(1, result);
    }

    [Fact]
    public void IndexOfReturnsFirstMatchForDuplicates()
    {
        var source = new[] { 1, 2, 3, 2, 1 };

        var result = source.IndexOf(2);

        Assert.Equal(1, result);
    }

    [Fact]
    public void IndexOfSequenceReturnsStartPositionOfSequence()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var sequence = new[] { 3, 4 };

        var result = source.IndexOf(sequence);

        Assert.Equal(2, result);
    }

    [Fact]
    public void IndexOfSequenceReturnsNegativeOneForMissingSequence()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var sequence = new[] { 2, 5 };

        var result = source.IndexOf(sequence);

        Assert.Equal(-1, result);
    }

    [Fact]
    public void IndexOfEmptySequenceReturnsZero()
    {
        var source = new[] { 1, 2, 3 };
        var sequence = Array.Empty<int>();

        var result = source.IndexOf(sequence);

        Assert.Equal(0, result);
    }

    [Fact]
    public void IndexOfSequenceWithCustomComparerWorks()
    {
        var source = new[] { "a", "B", "c", "D" };
        var sequence = new[] { "b", "c" };

        var result = source.IndexOf(sequence, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(1, result);
    }
    #endregion

    #region Majority Tests
    [Fact]
    public void MajorityReturnsTrueWhenMostElementsSatisfyPredicate()
    {
        var source = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        var result = source.Majority(x => x > 4);

        Assert.True(result);
    }

    [Fact]
    public void MajorityReturnsFalseWhenMostElementsDoNotSatisfyPredicate()
    {
        var source = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        var result = source.Majority(x => x > 7);

        Assert.False(result);
    }

    [Fact]
    public void MajorityReturnsFalseWhenExactlyHalfElementsSatisfyPredicate()
    {
        var source = new[] { 1, 2, 3, 4, 5, 6 };

        var result = source.Majority(x => x > 3);

        Assert.False(result);
    }

    [Fact]
    public void MajorityWithEmptySequenceReturnsFalse()
    {
        var source = Array.Empty<int>();

        var result = source.Majority(x => x > 0);

        Assert.False(result);
    }
    #endregion

    #region BuildDictionary Tests
    [Fact]
    public void BuildDictionaryLinearCreatesDictionaryFromSplitSequence()
    {
        var source = new[] { "a", "b", "c", "A", "B", "C" };

        var result = source.BuildDictionaryLinear();

        Assert.Equal(3, result.Count);
        Assert.Equal("A", result["a"]);
        Assert.Equal("B", result["b"]);
        Assert.Equal("C", result["c"]);
    }

    [Fact]
    public void BuildDictionaryLinearThrowsForOddLengthSequence()
    {
        var source = new[] { "a", "b", "c", "A", "B" };

        Assert.Throws<ArgumentException>(() => source.BuildDictionaryLinear());
    }

    [Fact]
    public void BuildDictionaryLinearThrowsForEmptySequence()
    {
        var source = Array.Empty<string>();

        Assert.Throws<ArgumentException>(() => source.BuildDictionaryLinear());
    }

    [Fact]
    public void BuildDictionaryZippedCreatesDictionaryFromPairedElements()
    {
        var source = new[] { "a", "A", "b", "B", "c", "C" };

        var result = source.BuildDictionaryZipped();

        Assert.Equal(3, result.Count);
        Assert.Equal("A", result["a"]);
        Assert.Equal("B", result["b"]);
        Assert.Equal("C", result["c"]);
    }

    [Fact]
    public void BuildDictionaryZippedThrowsForOddLengthSequence()
    {
        var source = new[] { "a", "A", "b", "B", "c" };

        Assert.Throws<ArgumentException>(() => source.BuildDictionaryZipped());
    }

    [Fact]
    public void BuildDictionaryZippedThrowsForEmptySequence()
    {
        var source = Array.Empty<string>();

        Assert.Throws<ArgumentException>(() => source.BuildDictionaryZipped());
    }

    [Fact]
    public void BuildDictionaryFromTwoSequencesCreatesDictionary()
    {
        var keys = new[] { "a", "b", "c" };
        var values = new[] { 1, 2, 3 };

        var result = keys.BuildDictionary(values);

        Assert.Equal(3, result.Count);
        Assert.Equal(1, result["a"]);
        Assert.Equal(2, result["b"]);
        Assert.Equal(3, result["c"]);
    }

    [Fact]
    public void BuildDictionaryFromTwoSequencesThrowsForDifferentLengths()
    {
        var keys = new[] { "a", "b", "c" };
        var values = new[] { 1, 2 };

        Assert.Throws<ArgumentException>(() => keys.BuildDictionary(values));
    }
    #endregion

    #region MapTo Tests
    [Fact]
    public void MapToCreatesDictionaryWithFactoryFunction()
    {
        var source = new[] { "a", "bb", "ccc" };

        var result = source.MapTo(s => s.Length);

        Assert.Equal(3, result.Count);
        Assert.Equal(1, result["a"]);
        Assert.Equal(2, result["bb"]);
        Assert.Equal(3, result["ccc"]);
    }

    [Fact]
    public void MapToWithEmptySequenceReturnsEmptyDictionary()
    {
        var source = Array.Empty<string>();

        var result = source.MapTo(s => s.Length);

        Assert.Empty(result);
    }
    #endregion

    #region Correlate Tests
    [Fact]
    public void CorrelateMatchesElementsBasedOnPredicate()
    {
        var first = new[] { 1, 2, 3 };
        var second = new[] { "one", "two", "three" };

        var result = first.Correlate(second, (n, s) =>
            (n == 1 && s == "one") ||
            (n == 2 && s == "two") ||
            (n == 3 && s == "three")).ToArray();

        Assert.Equal(3, result.Length);
        Assert.Equal((1, "one"), result[0]);
        Assert.Equal((2, "two"), result[1]);
        Assert.Equal((3, "three"), result[2]);
    }

    [Fact]
    public void CorrelateThrowsWhenPredicateMatchesMultipleElements()
    {
        var first = new[] { 1, 2, 3 };
        var second = new[] { "odd", "odd", "even" };

        Assert.Throws<InvalidOperationException>(() =>
            first.Correlate(second, (n, s) =>
                (n % 2 == 1 && s == "odd") ||
                (n % 2 == 0 && s == "even")).ToArray());
    }

    [Fact]
    public void CorrelateThrowsWhenPredicateMatchesNoElements()
    {
        var first = new[] { 1 };
        var second = new[] { "two" };

        Assert.Throws<InvalidOperationException>(() =>
            first.Correlate(second, (n, s) => n.ToString() == s).ToArray());
    }
    #endregion

    #region AsAsynchronous Tests
    [Fact]
    public async Task AsAsynchronousConvertsToAsyncEnumerable()
    {
        var source = new[] { 1, 2, 3 };

        var result = new List<int>();
        await foreach (var item in source.AsAsynchronous())
        {
            result.Add(item);
        }

        Assert.Equal(source, result);
    }

    [Fact]
    public async Task AsAsynchronousWithEmptySequenceCompletesSuccessfully()
    {
        var source = Array.Empty<int>();

        var result = new List<int>();
        await foreach (var item in source.AsAsynchronous())
        {
            result.Add(item);
        }

        Assert.Empty(result);
    }
    #endregion
}