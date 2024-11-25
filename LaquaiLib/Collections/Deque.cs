using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using LaquaiLib.Extensions;

namespace LaquaiLib.Collections;

/// <summary>
/// Represents a "deque" (or "double-ended queue") data structure.
/// This is essentially just a <see cref="LinkedList{T}"/> with the last element pointing to the first element and vice versa.
/// </summary>
/// <typeparam name="T">The type of elements in the deque.</typeparam>
[DebuggerDisplay("Head = {Head}, Count = {Count}")]
public class Deque<T> : IEnumerable<DequeNode<T>>, IEnumerable<T>
{
    #region Properties
    /// <summary>
    /// Gets the first element of the deque.
    /// </summary>
    public DequeNode<T> Head { get; private set; }
    /// <summary>
    /// Gets the "last" element of the deque.
    /// </summary>
    public DequeNode<T> Tail => Head?.Previous;
    /// <summary>
    /// Gets the number of nodes in the <see cref="Deque{T}"/>.
    /// </summary>
    public int Count
    {
        get
        {
            var i = 0;
            var current = Head;
            if (current is not null)
            {
                do
                {
                    i++;
                    current = current.Next;
                } while (current != Head && current is not null);
            }
            return i;
        }
    }
    /// <summary>
    /// Gets a value indicating whether the <see cref="Deque{T}"/> is empty.
    /// </summary>
    /// <remarks>
    /// Do not use <see cref="Count"/> to perform an empty check under any circumstances. With sufficiently large deques, the performance penalty is significant.
    /// </remarks>
    public bool IsEmpty => Head is null;
    #endregion

    #region .ctors
    /// <summary>
    /// Initializes a new, empty <see cref="Deque{T}"/>.
    /// </summary>
    public Deque()
    {
    }
    /// <summary>
    /// Initializes a new <see cref="Deque{T}"/> with nodes containing the specified values.
    /// </summary>
    /// <param name="values">The values to insert into the deque.</param>
    public Deque(params ReadOnlySpan<T> values)
    {
        foreach (var value in values)
        {
            AddLast(value);
        }
    }
    /// <summary>
    /// Initializes a new <see cref="Deque{T}"/> with the specified number of nodes that contain the default value of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="nodes">The number of nodes to insert into the deque.</param>
    public Deque(int nodes)
    {
        for (var i = 0; i < nodes; i++)
        {
            AddLast(default(T));
        }
    }
    /// <summary>
    /// Initializes a new <see cref="Deque{T}"/> by creating copies of the nodes in the specified <see cref="LinkedList{T}"/>.
    /// This copy operation is shallow; if <typeparamref name="T"/> is a reference type, only the references are copied.
    /// </summary>
    /// <param name="linkedList">The <see cref="LinkedList{T}"/> to copy nodes from.</param>
    public Deque(LinkedList<T> linkedList)
    {
        foreach (var value in linkedList)
        {
            AddLast(value);
        }
    }
    #endregion

    #region Find*
    /// <summary>
    /// Finds the first node in the deque that contains the specified value.
    /// </summary>
    /// <param name="value">The value to find.</param>
    /// <returns>A reference to the first node that contains the specified value, or <see langword="null"/> if no such node was found.</returns>
    public DequeNode<T> FindFirst(T value)
    {
        if (Head != null)
        {
            var node = Head;
            do
            {
                if (node.Value?.Equals(value) is true)
                {
                    return node;
                }
                node = node.Next;
            } while (node != Head && node is not null);
        }
        return null;
    }
    /// <summary>
    /// Finds the last node in the deque that contains the specified value.
    /// </summary>
    /// <param name="value">The value to find.</param>
    /// <returns>A reference to the last node that contains the specified value, or <see langword="null"/> if no such node was found.</returns>
    public DequeNode<T> FindLast(T value)
    {
        if (Tail != null)
        {
            var node = Tail;
            do
            {
                if (node.Value?.Equals(value) is true)
                {
                    return node;
                }
                node = node.Previous;
            } while (node != Tail && node is not null);
        }
        return null;
    }
    /// <summary>
    /// Finds all nodes in the deque that contain the specified value.
    /// </summary>
    /// <param name="value">The value to find.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that enumerates all nodes that contain the specified value.</returns>
    public DequeNode<T>[] FindAll(T value)
    {
        IEnumerable<DequeNode<T>> FindAllImpl()
        {
            if (Head != null)
            {
                var node = Head;
                do
                {
                    if (node.Value?.Equals(value) is true)
                    {
                        yield return node;
                    }
                    node = node.Next;
                } while (node != Head && node is not null);
            }
        }
        return FindAllImpl().ToArray();
    }
    #endregion

    #region Add*
    /// <summary>
    /// Inserts a new <see cref="DequeNode{T}"/> containing the specified value after the specified node.
    /// </summary>
    /// <param name="node">The node to insert the new node after.</param>
    /// <param name="value">The value to insert.</param>
    /// <returns>A reference to the newly inserted node.</returns>
    public DequeNode<T> AddAfter(DequeNode<T> node, T value) => AddAfter(node, new DequeNode<T>(value));
    /// <summary>
    /// Inserts the specified <see cref="DequeNode{T}"/> after the specified node.
    /// </summary>
    /// <param name="node">The node to insert the new node after.</param>
    /// <param name="newNode">The node to insert.</param>
    /// <returns>A reference to the newly inserted node.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="newNode"/> already belongs to a <see cref="Deque{T}"/>.</exception>
    public DequeNode<T> AddAfter(DequeNode<T> node, DequeNode<T> newNode)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(newNode);

        if (node.Deque != this)
        {
            throw new InvalidOperationException("The specified node does not belong to this deque.");
        }
        if (newNode.Deque != null)
        {
            throw new InvalidOperationException("The specified node already belongs to a deque.");
        }

        var oldNext = node.Next!;
        node.Next = newNode;
        newNode.Next = oldNext;

        oldNext.Previous = newNode;
        newNode.Previous = node;

        newNode.Deque = this;
        return newNode;
    }

    /// <summary>
    /// Inserts a new <see cref="DequeNode{T}"/> containing the specified value before the specified node.
    /// </summary>
    /// <param name="node">The node to insert the new node before.</param>
    /// <param name="value">The value to insert.</param>
    /// <returns>A reference to the newly inserted node.</returns>
    public DequeNode<T> AddBefore(DequeNode<T> node, T value) => AddBefore(node, new DequeNode<T>(value));
    /// <summary>
    /// Inserts the specified <see cref="DequeNode{T}"/> before the specified node.
    /// </summary>
    /// <param name="node">The node to insert the new node before.</param>
    /// <param name="newNode">The node to insert.</param>
    /// <returns>A reference to the newly inserted node.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="newNode"/> already belongs to a <see cref="Deque{T}"/>.</exception>
    public DequeNode<T> AddBefore(DequeNode<T> node, DequeNode<T> newNode)
    {
        ArgumentNullException.ThrowIfNull(node);
        return AddAfter(node.Previous ?? throw new InvalidOperationException("The specified node is the head of the deque."), newNode);
    }

    /// <summary>
    /// Adds a new <see cref="DequeNode{T}"/> containing the specified value to the beginning of the deque (that is, the specified node becomes the <see cref="Head"/>).
    /// </summary>
    /// <param name="value">The value to insert.</param>
    /// <returns>A reference to the newly inserted node.</returns>
    public DequeNode<T> AddFirst(T value) => AddFirst(new DequeNode<T>(value));
    /// <summary>
    /// Adds the specified <see cref="DequeNode{T}"/> to the beginning of the deque (that is, the specified node becomes the <see cref="Head"/>).
    /// </summary>
    /// <param name="node">The node to insert.</param>
    /// <returns>A reference to the newly inserted node.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="node"/> already belongs to a <see cref="Deque{T}"/>.</exception>
    public DequeNode<T> AddFirst(DequeNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.Deque != null)
        {
            throw new InvalidOperationException("The specified node already belongs to a deque.");
        }

        if (Head == null)
        {
            // If this is the first node, it is also the last node.
            Head = node;
            Head.Next = Head;
            Head.Previous = Head;
        }
        else
        {
            // Otherwise make this node the new head
            AddBefore(Head, node);
        }

        node.Deque = this;

        return node;
    }

    /// <summary>
    /// Adds a new <see cref="DequeNode{T}"/> containing the specified value to the end of the deque (that is, the specified node becomes the <see cref="Tail"/>).
    /// </summary>
    /// <param name="value">The value to insert.</param>
    /// <returns>A reference to the newly inserted node.</returns>
    public DequeNode<T> AddLast(T value) => AddLast(new DequeNode<T>(value));
    /// <summary>
    /// Adds the specified <see cref="DequeNode{T}"/> to the end of the deque (that is, the specified node becomes the <see cref="Tail"/>).
    /// </summary>
    /// <param name="node">The node to insert.</param>
    /// <returns>A reference to the newly inserted node.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="node"/> already belongs to a <see cref="Deque{T}"/>.</exception>
    public DequeNode<T> AddLast(DequeNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.Deque != null)
        {
            throw new InvalidOperationException("The specified node already belongs to a deque.");
        }

        if (Head == null)
        {
            // If this is the first node, it is also the last node.
            Head = node;
            Head.Next = Head;
            Head.Previous = Head;
        }
        else
        {
            // Otherwise, make this node the new tail
            AddAfter(Tail!, node);
        }

        node.Deque = this;

        return node;
    }
    #endregion

    #region Misc
    /// <summary>
    /// Clears the reference to <see cref="Head"/>, which in turn clears the reference to all other nodes.
    /// </summary>
    public void Clear() => Head = null;
    /// <summary>
    /// Determines whether the <see cref="Deque{T}"/> contains at least one node that contains the specified value.
    /// </summary>
    /// <param name="value">The value to find.</param>
    /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> implementation to use when comparing values.</param>
    /// <returns><see langword="true"/> if the <see cref="Deque{T}"/> contains at least one node that contains the specified value, otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// If the <see cref="Deque{T}"/> is empty (that is, <see cref="Head"/> is <see langword="null"/>), this method always returns <see langword="false"/>.
    /// </remarks>
    public bool Contains(T value, IEqualityComparer<T> comparer = null)
    {
        var current = Head;
        if (current is not null)
        {
            do
            {
                if ((comparer ?? EqualityComparer<T>.Default).Equals(value, current.Value))
                {
                    return true;
                }
                current = current.Next;
            } while (current != Head && current is not null);
        }
        return false;
    }
    /// <summary>
    /// Copies the values of the <see cref="Deque{T}"/> to the specified array, starting at the specified index.
    /// </summary>
    /// <param name="array">The array to copy the values to.</param>
    /// <param name="index">The index in <paramref name="array"/> at which to begin inserting values.</param>
    public void CopyTo(T[] array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);

        var len = array.Length;
        var nodeCount = Count;
        if (index < 0 || index >= len)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"The specified index is out of range. It must be between 0 and {len - 1}.");
        }
        if (index + nodeCount > len)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"The specified index is out of range. It must be between 0 and {len - nodeCount}.");
        }

        var current = Head;
        for (var i = 0; i < len && current is not null; i++)
        {
            array[i] = current.Value;
            current = current.Next;
        }
    }
    /// <summary>
    /// Enumerates the <see cref="Deque{T}"/> and ensures that all contained nodes have references to neighboring nodes and to this <see cref="Deque{T}"/>.
    /// </summary>
    public void EnsureIntegrity()
    {
        var current = Head ?? throw new InvalidOperationException("The deque is empty.");
        do
        {
            if (current.Deque != this)
            {
                throw new DetachedDequeNodeException<T>(current, DetachedDequeNodeException<T>.OffenseKind.NoDeque);
            }
            if (current.Next is null)
            {
                throw new DetachedDequeNodeException<T>(current, DetachedDequeNodeException<T>.OffenseKind.NoNext);
            }
            if (current.Previous is null)
            {
                throw new DetachedDequeNodeException<T>(current, DetachedDequeNodeException<T>.OffenseKind.NoPrevious);
            }
            if (current.Next.Previous != current)
            {
                throw new DetachedDequeNodeException<T>(current.Next, DetachedDequeNodeException<T>.OffenseKind.InconsistentNext);
            }
            if (current.Previous.Next != current)
            {
                throw new DetachedDequeNodeException<T>(current.Previous, DetachedDequeNodeException<T>.OffenseKind.InconsistentPrevious);
            }
            current = current.Next;
        } while (current != Head);
    }

    /// <summary>
    /// Rotates the <see cref="Deque{T}"/> by the specified number of nodes (that is, the node <see cref="Head"/> references is shifted by the specified number of nodes).
    /// </summary>
    /// <param name="n">The number of nodes to rotate by. For example, if <paramref name="n"/> is 1, the node <see cref="Head"/> references becomes <c><see cref="Head"/>.Previous</c>.</param>
    /// <returns>A reference to the new <see cref="Head"/> node.</returns>
    public DequeNode<T> Rotate(int n)
    {
        if (Head is not null)
        {
            if (n == 0)
            {
                return Head;
            }
            else if (n > 0)
            {
                while (n-- != 0)
                {
                    Head = Head!.Previous;
                }
            }
            else
            {
                while (n++ != 0)
                {
                    Head = Head!.Next;
                }
            }
        }
        return Head;
    }
    #endregion

    #region Remove*
    /// <summary>
    /// Detaches the specified node from the <see cref="Deque{T}"/> and its neighbors.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <remarks>
    /// This method ensures that, unless <see cref="Head"/> itself is detached, <see cref="Head"/> will always remain in a valid state (that is, it will remain attached to its <see cref="Deque{T}"/> and its, possibly new, neighbors).
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="node"/> does not belong to this <see cref="Deque{T}"/>.</exception>
    public DequeNode<T> RemoveNode(DequeNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.Deque != this)
        {
            throw new InvalidOperationException("The specified node does not belong to this deque.");
        }

        if (node == Head && node.Next == Head && node.Previous == Head)
        {
            // If this is the only node, clear the deque
            Clear();
            return node;
        }

        var oldNext = node.Next!;
        var oldPrevious = node.Previous!;
        oldNext.Previous = oldPrevious;
        oldPrevious.Next = oldNext;

        if (node == Head)
        {
            Head = oldNext;
        }

        node.Next = null;
        node.Previous = null;
        node.Deque = null;
        return node;
    }
    /// <summary>
    /// Detaches the specified nodes from the <see cref="Deque{T}"/> and their neighbors.
    /// </summary>
    /// <param name="nodes">The nodes to remove.</param>
    /// <returns>The number of nodes that were removed.</returns>
    public int RemoveNodes(params ReadOnlySpan<DequeNode<T>> nodes)
    {
        var removed = 0;
        var current = Head;
        if (current is not null)
        {
            do
            {
                if (nodes.IndexOf(current) > 0)
                {
                    RemoveNode(current);
                    removed++;
                }
                current = current.Next;
            } while (current != Head && current is not null);
        }

        return removed;
    }
    /// <summary>
    /// Detaches all nodes that contain the specified value from the <see cref="Deque{T}"/> and their neighbors.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    /// <returns>The number of nodes that were removed.</returns>
    public int RemoveAll(T value) => RemoveNodes(FindAll(value));
    /// <summary>
    /// Detaches the <see cref="Head"/> from the <see cref="Deque{T}"/> and its neighbors.
    /// </summary>
    /// <returns>The value of the detached node.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Deque{T}"/> is empty.</exception>
    public T PopLeft() => RemoveNode(Head ?? throw new InvalidOperationException("The deque is empty.")).Value;
    /// <summary>
    /// Detaches the <see cref="Tail"/> from the <see cref="Deque{T}"/> and its neighbors.
    /// </summary>
    /// <returns>The value of the detached node.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="Deque{T}"/> is empty.</exception>
    public T PopRight() => RemoveNode(Tail ?? throw new InvalidOperationException("The deque is empty.")).Value;
    #endregion

    #region IEnumerable
    /// <summary>
    /// Returns an <see cref="IEnumerator{T}"/> that iterates through the <see cref="Deque{T}"/>'s nodes.
    /// </summary>
    IEnumerator<DequeNode<T>> IEnumerable<DequeNode<T>>.GetEnumerator()
    {
        if (Head != null)
        {
            var node = Head;
            do
            {
                yield return node;
                node = node.Next;
            } while (node != Head && node is not null);
        }
    }
    /// <summary>
    /// Returns an <see cref="IEnumerator{T}"/> that iterates through the values of the <see cref="Deque{T}"/>'s nodes.
    /// </summary>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        if (Head != null)
        {
            var node = Head;
            do
            {
                yield return node.Value;
                node = node.Next;
            } while (node != Head && node is not null);
        }
    }
    /// <summary>
    /// Returns an <see cref="IEnumerator{T}"/> that iterates over the current instance's <see cref="DequeNode{T}"/>s.
    /// </summary>
    /// <returns>An <see cref="IEnumerator{T}"/> as described.</returns>
    public IEnumerator<DequeNode<T>> EnumerateNodes() => ((IEnumerable<DequeNode<T>>)this).GetEnumerator();
    /// <summary>
    /// Returns an <see cref="IEnumerator{T}"/> that iterates over the current instance's values.
    /// </summary>
    /// <returns>An <see cref="IEnumerator{T}"/> as described.</returns>
    public IEnumerator<T> EnumerateValues() => ((IEnumerable<T>)this).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
    #endregion

    #region Conversions
    /// <summary>
    /// Constructs a new <see cref="LinkedList{T}"/> from the <see cref="Deque{T}"/>.
    /// </summary>
    /// <returns>The newly constructed <see cref="LinkedList{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LinkedList<T> ToLinkedList() => new LinkedList<T>((IEnumerable<T>)this);
    #endregion
}

/// <summary>
/// Represents a node in a <see cref="Deque{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the value this node contains. It must be compatible with the type of the <see cref="Deque{T}"/> this node belongs to.</typeparam>
public class DequeNode<T> : IEquatable<DequeNode<T>>
{
    /// <summary>
    /// Initializes a new <see cref="DequeNode{T}"/> with the specified value that is not attached to a <see cref="Deque{T}"/> and has no neighbors.
    /// </summary>
    /// <param name="value">The value this node contains.</param>
    public DequeNode(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new <see cref="DequeNode{T}"/> with the specified value that is not attached to a <see cref="Deque{T}"/> and has the specified neighbors.
    /// </summary>
    /// <param name="value">The value this node contains.</param>
    /// <param name="next">The next node in the <see cref="Deque{T}"/>.</param>
    /// <param name="previous">The previous node in the <see cref="Deque{T}"/>.</param>
    public DequeNode(T value, DequeNode<T> next, DequeNode<T> previous) : this(value)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(previous);

        Next = next;
        Next.Previous = this;
        Previous = previous;
        Previous.Next = this;
    }
    /// <summary>
    /// Initializes a new <see cref="DequeNode{T}"/> with the specified value that attached to the specified <see cref="Deque{T}"/> and has the specified neighbors.
    /// </summary>
    /// <param name="value">The value this node contains.</param>
    /// <param name="next">The next node in the <see cref="Deque{T}"/>.</param>
    /// <param name="previous">The previous node in the <see cref="Deque{T}"/>.</param>
    /// <param name="deque">The <see cref="Deque{T}"/> this node belongs to.</param>
    public DequeNode(T value, DequeNode<T> next, DequeNode<T> previous, Deque<T> deque) : this(value, next, previous)
    {
        Deque = deque;
    }

    /// <summary>
    /// Returns the <see cref="Deque{T}"/> this node belongs to.
    /// If <see langword="null"/> or set explicitly, the node is not coupled to a <see cref="Deque{T}"/>.
    /// </summary>
    public Deque<T> Deque { get; set; }
    /// <summary>
    /// Returns the next node in the <see cref="Deque{T}"/>.
    /// </summary>
    public DequeNode<T> Next { get; set; }
    /// <summary>
    /// Returns the previous node in the <see cref="Deque{T}"/>.
    /// </summary>
    public DequeNode<T> Previous { get; set; }
    /// <summary>
    /// Returns the value this node contains.
    /// </summary>
    public T Value { get; set; }

    /// <inheritdoc/>
    public bool Equals(DequeNode<T> other) => ReferenceEquals(this, other) || other?.Value?.Equals(Value) is true;
    /// <inheritdoc/>
    public override bool Equals(object obj) => Equals(obj as DequeNode<T>);
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Deque);
        hash.Add(Next);
        hash.Add(Previous);
        hash.Add(Value);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Returns the string representation of the <see cref="Value"/> this node contains.
    /// </summary>
    /// <returns>A <see cref="string"/> as described.</returns>
    public override string ToString() => Value?.ToString() ?? "";
}
