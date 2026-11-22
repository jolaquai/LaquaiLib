using LaquaiLib.UnsafeUtils.Accessors;

namespace LaquaiLib.UnitTests.UnsafeUtils.Accessors;

public class QueueAccessorsTests
{
    [Fact]
    public void ArrayLengthAccommodatesCount()
    {
        var queue = new Queue<int>([1, 2, 3]);

        ref var array = ref QueueAccessors<int>._array(queue);

        Assert.True(array.Length >= queue.Count);
    }

    [Fact]
    public void SizeMatchesCount()
    {
        var queue = new Queue<int>([1, 2, 3]);
        queue.Dequeue();
        queue.Enqueue(4);

        Assert.Equal(queue.Count, QueueAccessors<int>._size(queue));
    }

    [Fact]
    public void HeadAdvancesByOneOnDequeue()
    {
        var queue = new Queue<int>([1, 2, 3]);
        var capacity = QueueAccessors<int>._array(queue).Length;

        var before = QueueAccessors<int>._head(queue);
        queue.Dequeue();
        var after = QueueAccessors<int>._head(queue);

        Assert.Equal((before + 1) % capacity, after);
    }

    [Fact]
    public void HeadTailSizeInvariantHoldsAfterWraparound()
    {
        var queue = new Queue<int>(4);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);
        queue.Dequeue();
        queue.Dequeue();
        queue.Enqueue(5);
        queue.Enqueue(6);

        var capacity = QueueAccessors<int>._array(queue).Length;
        var head = QueueAccessors<int>._head(queue);
        var tail = QueueAccessors<int>._tail(queue);
        var size = QueueAccessors<int>._size(queue);

        Assert.Equal(tail, (head + size) % capacity);
    }
}
