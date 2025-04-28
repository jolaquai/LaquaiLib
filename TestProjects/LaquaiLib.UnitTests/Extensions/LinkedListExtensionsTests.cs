using LaquaiLib.Collections;
using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class LinkedListNodeExtensionsTests
{
    [Fact]
    public void DequeNextReturnsNextNodeForMiddleNode()
    {
        var list = new LinkedList<int>();
        var firstNode = list.AddFirst(1);
        var middleNode = list.AddAfter(firstNode, 2);
        var lastNode = list.AddAfter(middleNode, 3);

        var result = middleNode.DequeNext();

        Assert.Same(lastNode, result);
    }

    [Fact]
    public void DequeNextReturnsFirstNodeForLastNode()
    {
        var list = new LinkedList<int>();
        var firstNode = list.AddFirst(1);
        var middleNode = list.AddAfter(firstNode, 2);
        var lastNode = list.AddAfter(middleNode, 3);

        var result = lastNode.DequeNext();

        Assert.Same(firstNode, result);
    }

    [Fact]
    public void DequeNextThrowsForNullNode()
    {
        LinkedListNode<int> node = null;

        Assert.Throws<ArgumentNullException>(() => node.DequeNext());
    }

    [Fact]
    public void DequeNextReturnsNullForNodeWithoutList()
    {
        var node = new LinkedListNode<int>(42);

        var result = node.DequeNext();

        Assert.Null(result);
    }

    [Fact]
    public void DequeNextReturnsSelfForSingleElementList()
    {
        var list = new LinkedList<int>();
        var node = list.AddFirst(42);

        var result = node.DequeNext();

        Assert.Same(node, result);
    }

    [Fact]
    public void DequePreviousReturnsPreviousNodeForMiddleNode()
    {
        var list = new LinkedList<int>();
        var firstNode = list.AddFirst(1);
        var middleNode = list.AddAfter(firstNode, 2);
        var lastNode = list.AddAfter(middleNode, 3);

        var result = middleNode.DequePrevious();

        Assert.Same(firstNode, result);
    }

    [Fact]
    public void DequePreviousReturnsLastNodeForFirstNode()
    {
        var list = new LinkedList<int>();
        var firstNode = list.AddFirst(1);
        var middleNode = list.AddAfter(firstNode, 2);
        var lastNode = list.AddAfter(middleNode, 3);

        var result = firstNode.DequePrevious();

        Assert.Same(lastNode, result);
    }

    [Fact]
    public void DequePreviousThrowsForNullNode()
    {
        LinkedListNode<int> node = null;

        Assert.Throws<ArgumentNullException>(() => node.DequePrevious());
    }

    [Fact]
    public void DequePreviousReturnsNullForNodeWithoutList()
    {
        var node = new LinkedListNode<int>(42);

        var result = node.DequePrevious();

        Assert.Null(result);
    }

    [Fact]
    public void DequePreviousReturnsSelfForSingleElementList()
    {
        var list = new LinkedList<int>();
        var node = list.AddFirst(42);

        var result = node.DequePrevious();

        Assert.Same(node, result);
    }

    [Fact]
    public void ToDequeCreatesDequeFromLinkedList()
    {
        var list = new LinkedList<int>(new[] { 1, 2, 3 });

        var deque = list.ToDeque();

        Assert.NotNull(deque);
        Assert.IsType<Deque<int>>(deque);
    }
}
