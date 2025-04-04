using LaquaiLib.Collections.LimitedCollections;

namespace LaquaiLib.UnitTests.Collections.LimitedCollections;

public class ConcurrentLimitedQueueTests
{
    #region Constructor Tests
    [Fact]
    public void ConstructorWithCapacityInitializesCorrectly()
    {
        var queue = new ConcurrentLimitedQueue<int>(5);
        Assert.Empty(queue);
        Assert.Equal(5, queue.Capacity);
    }

    [Fact]
    public void ConstructorWithEnumerableInitializesCorrectly()
    {
        var list = new List<int> { 1, 2, 3 };
        var queue = new ConcurrentLimitedQueue<int>(list);
        Assert.Equal(3, queue.Count);
        Assert.Equal(3, queue.Capacity);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void ConstructorWithReadOnlySpanInitializesCorrectly()
    {
        var span = new ReadOnlySpan<int>([1, 2, 3]);
        var queue = new ConcurrentLimitedQueue<int>(span);
        Assert.Equal(3, queue.Count);
        Assert.Equal(3, queue.Capacity);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void ConstructorWithCapacityAndEnumerableInitializesCorrectly()
    {
        var list = new List<int> { 1, 2, 3 };
        var queue = new ConcurrentLimitedQueue<int>(5, list);
        Assert.Equal(3, queue.Count);
        Assert.Equal(5, queue.Capacity);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void ConstructorWithCapacityAndReadOnlySpanInitializesCorrectly()
    {
        var span = new ReadOnlySpan<int>([1, 2, 3]);
        var queue = new ConcurrentLimitedQueue<int>(5, span);
        Assert.Equal(3, queue.Count);
        Assert.Equal(5, queue.Capacity);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void ConstructorWithCapacityAndArrayInitializesCorrectly()
    {
        var array = new[] { 1, 2, 3 };
        var queue = new ConcurrentLimitedQueue<int>(5, array);
        Assert.Equal(3, queue.Count);
        Assert.Equal(5, queue.Capacity);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void ConstructorWithZeroCapacityThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentLimitedQueue<int>(0));
    }

    [Fact]
    public void ConstructorWithNegativeCapacityThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentLimitedQueue<int>(-1));
    }

    [Fact]
    public void ConstructorWithCapacitySmallerThanItemCountThrowsException()
    {
        var list = new List<int> { 1, 2, 3 };
        Assert.Throws<ArgumentException>(() => new ConcurrentLimitedQueue<int>(2, list));
    }

    [Fact]
    public void ConstructorWithEmptyEnumerableThrowsException()
    {
        var emptyList = new List<int>();
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConcurrentLimitedQueue<int>(emptyList));
    }
    #endregion

    #region Queue Operation Tests
    [Fact]
    public void EnqueueWhenBelowCapacityAddsItem()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void EnqueueWhenAtCapacityRemovesOldestItemAndAddsNew()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);
        Assert.Equal(3, queue.Count);
        Assert.Equal(2, queue.Peek());
        Assert.Contains(4, queue);
        Assert.DoesNotContain(1, queue);
    }

    [Fact]
    public void TryEnqueueWhenBelowCapacityReturnsTrueAndAddsItem()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        var result = queue.TryEnqueue(3);
        Assert.True(result);
        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void TryEnqueueWhenAtCapacityReturnsFalseAndDoesNotModifyQueue()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        var result = queue.TryEnqueue(4);
        Assert.False(result);
        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Peek());
        Assert.Contains(1, queue);
        Assert.Contains(2, queue);
        Assert.Contains(3, queue);
        Assert.DoesNotContain(4, queue);
    }

    [Fact]
    public void DequeueWhenNotEmptyRemovesAndReturnsFirstItem()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        var dequeued = queue.Dequeue();
        Assert.Equal(1, dequeued);
        Assert.Equal(2, queue.Count);
        Assert.Equal(2, queue.Peek());
    }

    [Fact]
    public void DequeueWhenEmptyThrowsException()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
    }

    [Fact]
    public void TryDequeueWhenNotEmptyReturnsTrueAndOutputsFirstItem()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        var result = queue.TryDequeue(out var dequeued);
        Assert.True(result);
        Assert.Equal(1, dequeued);
        Assert.Equal(2, queue.Count);
        Assert.Equal(2, queue.Peek());
    }

    [Fact]
    public void TryDequeueWhenEmptyReturnsFalse()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        var result = queue.TryDequeue(out var dequeued);
        Assert.False(result);
        Assert.Equal(0, dequeued); // Default value for int
    }

    [Fact]
    public void PeekWhenNotEmptyReturnsFirstItemWithoutRemoving()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        var peeked = queue.Peek();
        Assert.Equal(1, peeked);
        Assert.Equal(3, queue.Count);
    }

    [Fact]
    public void PeekWhenEmptyThrowsException()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        Assert.Throws<InvalidOperationException>(() => queue.Peek());
    }

    [Fact]
    public void TryPeekWhenNotEmptyReturnsTrueAndOutputsFirstItem()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        var result = queue.TryPeek(out var peeked);
        Assert.True(result);
        Assert.Equal(1, peeked);
        Assert.Equal(3, queue.Count);
    }

    [Fact]
    public void TryPeekWhenEmptyReturnsFalse()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        var result = queue.TryPeek(out var peeked);
        Assert.False(result);
        Assert.Equal(0, peeked); // Default value for int
    }
    #endregion

    #region Collection Interface Tests
    [Fact]
    public void AddShouldBehaveTheSameAsEnqueue()
    {
        // Collection initializers use ICollection<>.Add internally
        var queue = new ConcurrentLimitedQueue<int>(3) { 1, 2 };
        Assert.Equal(2, queue.Count);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void ClearRemovesAllItems()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Clear();
        Assert.Empty(queue);
        Assert.Throws<InvalidOperationException>(() => queue.Peek());
    }

    [Fact]
    public void ContainsWithExistingItemReturnsTrue()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        Assert.True(queue.Contains(2));
    }

    [Fact]
    public void ContainsWithNonExistingItemReturnsFalse()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        Assert.False(queue.Contains(4));
    }

    [Fact]
    public void CopyToCopiesAllItemsToArray()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
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
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        var result = queue.Remove(2);
        Assert.False(result);
        Assert.Equal(3, queue.Count);
        Assert.True(queue.Contains(2));
    }

    [Fact]
    public void CountReflectsNumberOfItems()
    {
        var queue = new ConcurrentLimitedQueue<int>(5);
        Assert.Empty(queue);

        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        Assert.Equal(3, queue.Count);

        queue.Dequeue();
        Assert.Equal(2, queue.Count);

        queue.Clear();
        Assert.Empty(queue);
    }

    [Fact]
    public void GetEnumeratorYieldsAllItemsInOrder()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        var expected = new[] { 1, 2, 3 };
        var actual = new List<int>();
        foreach (var item in queue)
        {
            actual.Add(item);
        }
        Assert.Equal(expected, actual);
    }
    #endregion

    #region Resize Tests
    [Fact]
    public void ResizeIncreasePreservesAllItems()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Resize(5);
        Assert.Equal(5, queue.Capacity);
        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Peek());
        Assert.True(queue.Contains(1));
        Assert.True(queue.Contains(2));
        Assert.True(queue.Contains(3));
    }

    [Fact]
    public void ResizeDecreaseDiscardOldestItems()
    {
        var queue = new ConcurrentLimitedQueue<int>(5);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);
        queue.Enqueue(5);
        queue.Resize(3);
        Assert.Equal(3, queue.Capacity);
        Assert.Equal(3, queue.Count);
        Assert.Equal(3, queue.Peek());
        Assert.False(queue.Contains(1));
        Assert.False(queue.Contains(2));
        Assert.True(queue.Contains(3));
        Assert.True(queue.Contains(4));
        Assert.True(queue.Contains(5));
    }

    [Fact]
    public void ResizeSameSizeHasNoEffect()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Resize(3);
        Assert.Equal(3, queue.Capacity);
        Assert.Equal(3, queue.Count);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    public void ResizeZeroCapacityThrowsException()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.Resize(0));
    }

    [Fact]
    public void ResizeNegativeCapacityThrowsException()
    {
        var queue = new ConcurrentLimitedQueue<int>(3);
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.Resize(-1));
    }
    #endregion

    #region Thread-Safety Tests
    [Fact]
    public async Task ConcurrentEnqueueDequeueMaintainsCorrectCount()
    {
        const int capacity = 100;
        const int operations = 1000;
        var queue = new ConcurrentLimitedQueue<int>(capacity);
        var tasks = new List<Task>();
        var enqueueCount = 0;
        var dequeueCount = 0;
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < operations; j++)
                {
                    queue.Enqueue(j);
                    Interlocked.Increment(ref enqueueCount);
                }
            }, TestContext.Current.CancellationToken));
        }
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < operations; j++)
                {
                    if (queue.TryDequeue(out _))
                    {
                        Interlocked.Increment(ref dequeueCount);
                    }
                }
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks);
        Assert.True(queue.Count <= capacity);
        Assert.Equal(5 * operations, enqueueCount); // All enqueue operations completed
        Assert.True(dequeueCount <= enqueueCount); // Cannot dequeue more than enqueued
    }

    [Fact]
    public async Task ConcurrentResizeDoesNotCorruptQueue()
    {
        const int initialCapacity = 50;
        const int operations = 100;
        var queue = new ConcurrentLimitedQueue<int>(initialCapacity);
        var tasks = new List<Task>();
        for (var i = 0; i < initialCapacity; i++)
        {
            queue.Enqueue(i);
        }
        for (var i = 0; i < 3; i++)
        {
            var taskId = i; // Capture for closure
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < operations; j++)
                {
                    var newCapacity = 20 + (taskId * j % 80);
                    queue.Resize(newCapacity);
                    Thread.Sleep(1); // Small delay to increase chance of thread interleaving
                }
            }, TestContext.Current.CancellationToken));
        }
        for (var i = 0; i < 3; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < operations; j++)
                {
                    queue.Enqueue(j);
                    if (queue.TryDequeue(out _))
                    {
                    }
                    Thread.Sleep(1); // Small delay to increase chance of thread interleaving
                }
            }, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(tasks);
        Assert.True(queue.Count <= queue.Capacity);
        queue.Enqueue(999);
        Assert.True(queue.Contains(999));
    }

    [Fact]
    public async Task ConcurrentOperationsDoNotCorruptQueue()
    {
        const int capacity = 50;
        const int operations = 1000;
        var queue = new ConcurrentLimitedQueue<int>(capacity);
        var tasks = new Task[10];
        var random = new Random(42); // Fixed seed for reproducibility
        for (var i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (var j = 0; j < operations; j++)
                {
                    TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
                    var operation = random.Next(5);
                    switch (operation)
                    {
                        case 0:
                            queue.Enqueue(j);
                            break;
                        case 1:
                            queue.TryEnqueue(j);
                            break;
                        case 2:
                            try { queue.Dequeue(); } catch (InvalidOperationException) { }
                            break;
                        case 3:
                            queue.TryDequeue(out _);
                            break;
                        case 4:
                            try { queue.Peek(); } catch (InvalidOperationException) { }
                            break;
                    }
                }
            }, TestContext.Current.CancellationToken);
        }

        await Task.WhenAll(tasks);
        Assert.True(queue.Count <= capacity);
    }
    #endregion
}
