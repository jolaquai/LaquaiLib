using LaquaiLib.Collections.LimitedCollections;

namespace LaquaiLib.UnitTests.Collections.LimitedCollections;

public class LimitedQueueTests
{
    #region Constructor Tests
    [Fact]
    public void ConstructorWithItemsInitializesCorrectly()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        Assert.Equal(3, queue.Count);
        Assert.Equal(3, queue.Capacity);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void ConstructorWithCapacityAndItemsInitializesCorrectly()
    {
        var queue = new LimitedQueue<string>(5, ["a", "b", "c"]);

        Assert.Equal(3, queue.Count);
        Assert.Equal(5, queue.Capacity);
        Assert.Equal("a", queue.Peek());
    }

    [Fact]
    public void ConstructorWithEnumerableInitializesCorrectly()
    {
        List<int> list = [1, 2, 3];

        var queue = new LimitedQueue<int>(list);

        Assert.Equal(3, queue.Count);
        Assert.Equal(3, queue.Capacity);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void ConstructorWithCapacityAndEnumerableInitializesCorrectly()
    {
        List<int> list = [1, 2, 3];

        var queue = new LimitedQueue<int>(5, list);

        Assert.Equal(3, queue.Count);
        Assert.Equal(5, queue.Capacity);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void ConstructorWithZeroCapacityThrowsException() => Assert.Throws<ArgumentOutOfRangeException>(static () => new LimitedQueue<int>(0, []));

    [Fact]
    public void ConstructorWithNegativeCapacityThrowsException() => Assert.Throws<ArgumentOutOfRangeException>(static () => new LimitedQueue<int>(-1, []));

    [Fact]
    public void ConstructorWithCapacitySmallerThanItemCountThrowsException() => Assert.Throws<ArgumentException>(static () => new LimitedQueue<int>(2, [1, 2, 3]));

    [Fact]
    public void ConstructorWithEmptyEnumerableThrowsException()
    {
        List<int> emptyList = [];

        Assert.Throws<ArgumentOutOfRangeException>(() => new LimitedQueue<int>(emptyList));
    }
    #endregion

    #region Queue Operation Tests
    [Fact]
    public void EnqueueWhenBelowCapacityAddsItem()
    {
        var queue = new LimitedQueue<int>(3, [1, 2]);

        queue.Enqueue(3);

        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void EnqueueWhenAtCapacityRemovesOldestItemAndAddsNew()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        queue.Enqueue(4);

        Assert.Equal(3, queue.Count);
        Assert.Equal(2, queue.Peek());
        Assert.Contains(4, queue);
        Assert.DoesNotContain(1, queue);
    }

    [Fact]
    public void TryEnqueueWhenBelowCapacityReturnsTrueAndAddsItem()
    {
        var queue = new LimitedQueue<int>(3, [1, 2]);

        var result = queue.TryEnqueue(3);

        Assert.True(result);
        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void TryEnqueueWhenAtCapacityReturnsFalseAndDoesNotModifyQueue()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);
        var expectedQueue = new LimitedQueue<int>([1, 2, 3]);

        var result = queue.TryEnqueue(4);

        Assert.False(result);
        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Peek());
        Assert.Equal(expectedQueue.Count, queue.Count);
        Assert.Equal(expectedQueue.Peek(), queue.Peek());
    }

    [Fact]
    public void DequeueWhenNotEmptyRemovesAndReturnsFirstItem()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        var dequeued = queue.Dequeue();

        Assert.Equal(1, dequeued);
        Assert.Equal(2, queue.Count);
        Assert.Equal(2, queue.Peek());
    }

    [Fact]
    public void DequeueWhenEmptyThrowsException()
    {
        var queue = new LimitedQueue<int>(3, [1]);
        queue.Dequeue(); // Now empty

        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
    }

    [Fact]
    public void TryDequeueWhenNotEmptyReturnsTrueAndOutputsFirstItem()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        var result = queue.TryDequeue(out var dequeued);

        Assert.True(result);
        Assert.Equal(1, dequeued);
        Assert.Equal(2, queue.Count);
        Assert.Equal(2, queue.Peek());
    }

    [Fact]
    public void TryDequeueWhenEmptyReturnsFalse()
    {
        var queue = new LimitedQueue<int>(3, [1]);
        queue.Dequeue(); // Now empty

        var result = queue.TryDequeue(out var dequeued);

        Assert.False(result);
        Assert.Equal(0, dequeued); // Default value for int
    }

    [Fact]
    public void PeekWhenNotEmptyReturnsFirstItemWithoutRemoving()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        var peeked = queue.Peek();

        Assert.Equal(1, peeked);
        Assert.Equal(3, queue.Count);
    }

    [Fact]
    public void PeekWhenEmptyThrowsException()
    {
        var queue = new LimitedQueue<int>(3, [1]);
        queue.Dequeue(); // Now empty

        Assert.Throws<InvalidOperationException>(() => queue.Peek());
    }

    [Fact]
    public void TryPeekWhenNotEmptyReturnsTrueAndOutputsFirstItem()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        var result = queue.TryPeek(out var peeked);

        Assert.True(result);
        Assert.Equal(1, peeked);
        Assert.Equal(3, queue.Count);
    }

    [Fact]
    public void TryPeekWhenEmptyReturnsFalse()
    {
        var queue = new LimitedQueue<int>(3, [1]);
        queue.Dequeue(); // Now empty

        var result = queue.TryPeek(out var peeked);

        Assert.False(result);
        Assert.Equal(0, peeked); // Default value for int
    }
    #endregion

    #region Collection Interface Tests
    [Fact]
    public void AddShouldBehaveTheSameAsEnqueue()
    {
        var queue1 = new LimitedQueue<int>(3, [1, 2]);
        var queue2 = new LimitedQueue<int>(3, [1, 2]);

        queue1.Enqueue(3);
        queue2.Add(3);

        Assert.Equal(queue1.Count, queue2.Count);
        Assert.Equal(queue1.Peek(), queue2.Peek());
        List<int> list1 = [];
        List<int> list2 = [];
        foreach (var item in queue1)
        {
            list1.Add(item);
        }
        foreach (var item in queue2)
        {
            list2.Add(item);
        }

        Assert.Equal(list1, list2);
    }

    [Fact]
    public void ClearRemovesAllItems()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        queue.Clear();

        Assert.Empty(queue);
        Assert.Throws<InvalidOperationException>(() => queue.Peek());
    }

    [Fact]
    public void ContainsWithExistingItemReturnsTrue()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        Assert.Contains(2, queue);
    }

    [Fact]
    public void ContainsWithNonExistingItemReturnsFalse()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        Assert.DoesNotContain(4, queue);
    }

    [Fact]
    public void CopyToCopiesAllItemsToArray()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);
        var array = new int[5];

        queue.CopyTo(array, 1);

        Assert.Equal(0, array[0]);
        Assert.Equal(1, array[1]);
        Assert.Equal(2, array[2]);
        Assert.Equal(3, array[3]);
        Assert.Equal(0, array[4]);
    }

    [Fact]
    public void RemoveAlwaysReturnsFalse()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        var result = queue.Remove(2);

        Assert.False(result);
        Assert.Equal(3, queue.Count);
        Assert.Contains(2, queue);
    }

    [Fact]
    public void CountReflectsNumberOfItems()
    {
        var queue = new LimitedQueue<int>(5, [1, 2, 3]);

        Assert.Equal(3, queue.Count);

        queue.Enqueue(4);
        Assert.Equal(4, queue.Count);

        queue.Dequeue();
        Assert.Equal(3, queue.Count);

        queue.Clear();
        Assert.Empty(queue);
    }

    [Fact]
    public void GetEnumeratorYieldsAllItemsInOrder()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);
        var expected = new[] { 1, 2, 3 };
        List<int> actual = [.. queue];

        Assert.Equal(expected, actual);
    }
    #endregion

    #region Resize Tests
    [Fact]
    public void ResizeIncreasePreservesAllItems()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);

        queue.Resize(5);

        Assert.Equal(5, queue.Capacity);
        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Peek());
        Assert.Contains(1, queue);
        Assert.Contains(2, queue);
        Assert.Contains(3, queue);
    }

    [Fact]
    public void ResizeDecreaseDiscardOldestItems()
    {
        var queue = new LimitedQueue<int>([1, 2, 3, 4, 5]);

        queue.Resize(3);

        Assert.Equal(3, queue.Capacity);
        Assert.Equal(3, queue.Count);
        Assert.Equal(3, queue.Peek());
        Assert.DoesNotContain(1, queue);
        Assert.DoesNotContain(2, queue);
        Assert.Contains(3, queue);
        Assert.Contains(4, queue);
        Assert.Contains(5, queue);
    }

    [Fact]
    public void ResizeSameSizeHasNoEffect()
    {
        var queue = new LimitedQueue<int>([1, 2, 3]);
        var expected = new[] { 1, 2, 3 };
        List<int> actual = [];

        queue.Resize(3);
        foreach (var item in queue)
        {
            actual.Add(item);
        }

        Assert.Equal(3, queue.Capacity);
        Assert.Equal(3, queue.Count);
        Assert.Equal(expected, actual);
    }
    #endregion
}
