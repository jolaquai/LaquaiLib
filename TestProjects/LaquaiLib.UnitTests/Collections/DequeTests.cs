using LaquaiLib.Collections;
using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Collections;

public class DequeTests
{
    #region Constructor Tests
    [Fact]
    public void DefaultConstructorShouldCreateEmptyDeque()
    {
        var deque = new Deque<int>();
        Assert.Null(deque.Head);
        Assert.Null(deque.Tail);
        Assert.True(deque.IsEmpty);
        Assert.Equal(0, deque.Count);
    }

    [Fact]
    public void ValuesConstructorShouldCreateDequeWithValues()
    {
        ReadOnlySpan<int> values = [1, 2, 3];
        var deque = new Deque<int>(values);
        Assert.Equal(3, deque.Count);
        Assert.False(deque.IsEmpty);
        Assert.Equal(1, deque.Head.Value);
        Assert.Equal(3, deque.Tail.Value);
    }

    [Fact]
    public void NodeCountConstructorShouldCreateDequeWithSpecifiedNumberOfDefaultNodes()
    {
        var deque = new Deque<int>(5);
        Assert.Equal(5, deque.Count);
        Assert.False(deque.IsEmpty);
        Assert.Equal(0, deque.Head.Value); // Default int value
        Assert.Equal(0, deque.Tail.Value); // Default int value
    }

    [Fact]
    public void LinkedListConstructorShouldCreateDequeFromLinkedList()
    {
        var linkedList = new LinkedList<string>(new[] { "a", "b", "c" });
        var deque = new Deque<string>(linkedList);
        Assert.Equal(3, deque.Count);
        Assert.False(deque.IsEmpty);
        Assert.Equal("a", deque.Head.Value);
        Assert.Equal("c", deque.Tail.Value);
    }
    #endregion

    #region Properties Tests
    [Fact]
    public void HeadShouldPointToFirstNode()
    {
        var deque = new Deque<int>([1, 2, 3]);
        Assert.Equal(1, deque.Head.Value);
    }

    [Fact]
    public void TailShouldPointToLastNode()
    {
        var deque = new Deque<int>([1, 2, 3]);
        Assert.Equal(3, deque.Tail.Value);
    }

    [Fact]
    public void CountShouldReturnCorrectNumberOfNodes()
    {
        var deque = new Deque<int>([1, 2, 3]);
        Assert.Equal(3, deque.Count);
        deque.AddLast(4);
        Assert.Equal(4, deque.Count);
        deque.PopLeft();
        Assert.Equal(3, deque.Count);
    }

    [Fact]
    public void IsEmptyShouldReturnTrueWhenDequeIsEmpty()
    {
        var deque = new Deque<int>();
        Assert.True(deque.IsEmpty);
        deque.AddFirst(1);
        Assert.False(deque.IsEmpty);
        deque.Clear();
        Assert.True(deque.IsEmpty);
    }
    #endregion

    #region Find Methods Tests
    [Fact]
    public void FindFirstShouldReturnFirstNodeWithValue()
    {
        var deque = new Deque<int>([1, 2, 3, 2, 1]);
        var node = deque.FindFirst(2);
        Assert.NotNull(node);
        Assert.Equal(2, node.Value);
        Assert.Equal(deque.Head.Next, node);
    }

    [Fact]
    public void FindFirstShouldReturnNullWhenValueNotFound()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var node = deque.FindFirst(4);
        Assert.Null(node);
    }

    [Fact]
    public void FindLastShouldReturnLastNodeWithValue()
    {
        var deque = new Deque<int>([1, 2, 3, 2, 1]);
        var node = deque.FindLast(2);
        Assert.NotNull(node);
        Assert.Equal(2, node.Value);
        Assert.Equal(deque.Tail.Previous, node);
    }

    [Fact]
    public void FindLastShouldReturnNullWhenValueNotFound()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var node = deque.FindLast(4);
        Assert.Null(node);
    }

    [Fact]
    public void FindAllShouldReturnAllNodesWithValue()
    {
        var deque = new Deque<int>([1, 2, 3, 2, 1]);
        var nodes = deque.FindAll(2);
        Assert.Equal(2, nodes.Length);
        Assert.All(nodes, node => Assert.Equal(2, node.Value));
    }

    [Fact]
    public void FindAllShouldReturnEmptyArrayWhenValueNotFound()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var nodes = deque.FindAll(4);
        Assert.Empty(nodes);
    }
    #endregion

    #region Add Methods Tests
    [Fact]
    public void AddAfterShouldInsertNodeAfterSpecifiedNode()
    {
        var deque = new Deque<int>([1, 3]);
        var node = deque.Head;
        var newNode = deque.AddAfter(node, 2);
        Assert.Equal(3, deque.Count);
        Assert.Equal(2, newNode.Value);
        Assert.Equal(node.Next, newNode);
        Assert.Equal(deque.Head.Next, newNode);
    }

    [Fact]
    public void AddAfterShouldThrowExceptionWhenNodeFromDifferentDeque()
    {
        var deque1 = new Deque<int>([1, 2]);
        var deque2 = new Deque<int>([3, 4]);
        Assert.Throws<InvalidOperationException>(() => deque1.AddAfter(deque2.Head, 5));
    }

    [Fact]
    public void AddBeforeShouldInsertNodeBeforeSpecifiedNode()
    {
        var deque = new Deque<int>([1, 3]);
        var node = deque.Tail;
        var newNode = deque.AddBefore(node, 2);
        Assert.Equal(3, deque.Count);
        Assert.Equal(2, newNode.Value);
        Assert.Equal(node.Previous, newNode);
        Assert.Equal(deque.Head.Next, newNode);
    }

    [Fact]
    public void AddFirstShouldInsertNodeAtBeginning()
    {
        var deque = new Deque<int>([2, 3]);
        var newNode = deque.AddFirst(1);
        Assert.Equal(3, deque.Count);
        Assert.Equal(1, newNode.Value);
        Assert.Equal(deque.Head, newNode);
        Assert.Equal(3, deque.Tail.Value);
    }

    [Fact]
    public void AddFirstWhenEmptyShouldSetBothHeadAndTail()
    {
        var deque = new Deque<int>();
        var newNode = deque.AddFirst(1);
        Assert.Equal(1, deque.Count);
        Assert.Equal(1, newNode.Value);
        Assert.Equal(deque.Head, newNode);
        Assert.Equal(deque.Tail, newNode);
        Assert.Equal(deque.Head, deque.Head.Next);
        Assert.Equal(deque.Head, deque.Head.Previous);
    }

    [Fact]
    public void AddLastShouldInsertNodeAtEnd()
    {
        var deque = new Deque<int>([1, 2]);
        var newNode = deque.AddLast(3);
        Assert.Equal(3, deque.Count);
        Assert.Equal(3, newNode.Value);
        Assert.Equal(deque.Tail, newNode);
        Assert.Equal(1, deque.Head.Value);
    }

    [Fact]
    public void AddLastWhenEmptyShouldSetBothHeadAndTail()
    {
        var deque = new Deque<int>();
        var newNode = deque.AddLast(1);
        Assert.Equal(1, deque.Count);
        Assert.Equal(1, newNode.Value);
        Assert.Equal(deque.Head, newNode);
        Assert.Equal(deque.Tail, newNode);
        Assert.Equal(deque.Head, deque.Head.Next);
        Assert.Equal(deque.Head, deque.Head.Previous);
    }
    #endregion

    #region Remove Methods Tests
    [Fact]
    public void RemoveNodeShouldRemoveSpecifiedNode()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var nodeToRemove = deque.Head.Next;
        var removedNode = deque.RemoveNode(nodeToRemove);
        Assert.Equal(2, deque.Count);
        Assert.Equal(2, removedNode.Value);
        Assert.Equal(1, deque.Head.Value);
        Assert.Equal(3, deque.Tail.Value);
        Assert.Equal(deque.Head.Next, deque.Tail);
        Assert.Null(removedNode.Next);
        Assert.Null(removedNode.Previous);
        Assert.Null(removedNode.Deque);
    }

    [Fact]
    public void RemoveNodeWhenRemovingHeadShouldUpdateHeadReference()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var oldHead = deque.Head;
        var removedNode = deque.RemoveNode(oldHead);
        Assert.Equal(2, deque.Count);
        Assert.Equal(1, removedNode.Value);
        Assert.Equal(2, deque.Head.Value);
        Assert.Equal(3, deque.Tail.Value);
        Assert.NotEqual(oldHead, deque.Head);
    }

    [Fact]
    public void RemoveNodeWhenRemovingLastNodeShouldClearDeque()
    {
        var deque = new Deque<int>([1]);
        var removedNode = deque.RemoveNode(deque.Head);
        Assert.Equal(0, deque.Count);
        Assert.True(deque.IsEmpty);
        Assert.Null(deque.Head);
        Assert.Null(deque.Tail);
    }

    [Fact]
    public void RemoveAllShouldRemoveAllNodesWithSpecifiedValue()
    {
        var deque = new Deque<int>([1, 2, 3, 2, 1]);
        var removedCount = deque.RemoveAll(2);
        Assert.Equal(2, removedCount);
        Assert.Equal(3, deque.Count);
        Assert.Equal(1, deque.Head.Value);
        Assert.Equal(1, deque.Tail.Value);
        Assert.Equal(3, deque.Head.Next.Value);
    }

    [Fact]
    public void PopLeftShouldRemoveAndReturnHeadValue()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var value = deque.PopLeft();
        Assert.Equal(1, value);
        Assert.Equal(2, deque.Count);
        Assert.Equal(2, deque.Head.Value);
    }

    [Fact]
    public void PopLeftWhenEmptyShouldThrowInvalidOperationException()
    {
        var deque = new Deque<int>();
        Assert.Throws<InvalidOperationException>(() => deque.PopLeft());
    }

    [Fact]
    public void PopRightShouldRemoveAndReturnTailValue()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var value = deque.PopRight();
        Assert.Equal(3, value);
        Assert.Equal(2, deque.Count);
        Assert.Equal(2, deque.Tail.Value);
    }

    [Fact]
    public void PopRightWhenEmptyShouldThrowInvalidOperationException()
    {
        var deque = new Deque<int>();
        Assert.Throws<InvalidOperationException>(() => deque.PopRight());
    }
    #endregion

    #region Miscellaneous Methods Tests
    [Fact]
    public void ClearShouldRemoveAllNodesFromDeque()
    {
        var deque = new Deque<int>([1, 2, 3]);
        deque.Clear();
        Assert.True(deque.IsEmpty);
        Assert.Equal(0, deque.Count);
        Assert.Null(deque.Head);
        Assert.Null(deque.Tail);
    }

    [Fact]
    public void ContainsShouldReturnTrueWhenValueExists()
    {
        var deque = new Deque<int>([1, 2, 3]);
        Assert.True(deque.Contains(2));
        Assert.False(deque.Contains(4));
    }

    [Fact]
    public void ContainsWithCustomComparerShouldUseComparer()
    {
        var deque = new Deque<string>(["a", "B", "c"]);
        var comparer = StringComparer.OrdinalIgnoreCase;
        Assert.True(deque.Contains("b", comparer));
        Assert.False(deque.Contains("d", comparer));
    }

    [Fact]
    public void CopyToShouldCopyValuesToArray()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var array = new int[5];
        deque.CopyTo(array, 1);
        Assert.Equal([0, 1, 2, 3, 0], array);
    }

    [Fact]
    public void CopyToWithInvalidIndexShouldThrowArgumentOutOfRangeException()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var array = new int[3];
        Assert.Throws<ArgumentOutOfRangeException>(() => deque.CopyTo(array, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => deque.CopyTo(array, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => deque.CopyTo(array, 1)); // Not enough space
    }

    [Fact]
    public void EnsureIntegrityShouldNotThrowForValidDeque()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var exception = Record.Exception(() => deque.EnsureIntegrity());
        Assert.Null(exception);
    }

    [Fact]
    public void RotateShouldShiftHeadReferenceBySpecifiedAmount()
    {
        var deque = new Deque<int>([1, 2, 3, 4, 5]);
        deque.Rotate(2);
        Assert.Equal(4, deque.Head.Value);
        Assert.Equal(3, deque.Tail.Value);
        deque.Rotate(-3);
        Assert.Equal(1, deque.Head.Value);
        Assert.Equal(5, deque.Tail.Value);
    }

    [Fact]
    public void ToLinkedListShouldReturnLinkedListWithSameValues()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var linkedList = deque.ToLinkedList();
        Assert.Equal(deque.Count, linkedList.Count);
        Assert.Equal(1, linkedList.First.Value);
        Assert.Equal(3, linkedList.Last.Value);
    }
    #endregion

    #region Enumeration Tests
    [Fact]
    public void EnumerateNodesShouldEnumerateAllNodes()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var nodes = ((IEnumerable<DequeNode<int>>)deque).ToArray();
        Assert.Equal(3, nodes.Length);
        Assert.Equal(deque.Head, nodes[0]);
        Assert.Equal(deque.Head.Next, nodes[1]);
        Assert.Equal(deque.Tail, nodes[2]);
    }

    [Fact]
    public void EnumerateValuesShouldEnumerateAllValues()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var values = ((IEnumerable<int>)deque).ToArray();
        Assert.Equal([1, 2, 3], values);
    }

    [Fact]
    public void IEnumerableTGetEnumeratorShouldEnumerateAllValues()
    {
        var deque = new Deque<int>([1, 2, 3]);
        var values = ((IEnumerable<int>)deque).ToArray();
        Assert.Equal([1, 2, 3], values);
    }
    #endregion

    #region DequeNode Tests
    [Fact]
    public void DequeNodeConstructorShouldInitializeValue()
    {
        var node = new DequeNode<int>(42);
        Assert.Equal(42, node.Value);
        Assert.Null(node.Next);
        Assert.Null(node.Previous);
        Assert.Null(node.Deque);
    }

    [Fact]
    public void DequeNodeEqualsShouldCompareByValue()
    {
        var node1 = new DequeNode<int>(42);
        var node2 = new DequeNode<int>(42);
        var node3 = new DequeNode<int>(43);
        Assert.True(node1.Equals(node2));
        Assert.False(node1.Equals(node3));
    }

    [Fact]
    public void DequeNodeToStringShouldReturnValueAsString()
    {
        var node = new DequeNode<int>(42);
        Assert.Equal("42", node.ToString());
    }

    [Fact]
    public void DequeNodeToStringWithNullValueShouldReturnEmptyString()
    {
        var node = new DequeNode<string>(null);
        Assert.Equal("", node.ToString());
    }
    #endregion
}
