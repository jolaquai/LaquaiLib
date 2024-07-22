using System.Runtime.CompilerServices;

using LaquaiLib.Collections;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="LinkedListNode{T}"/> Type.
/// </summary>
public static class LinkedListNodeExtensions
{
    /// <summary>
    /// Returns the next node in the <see cref="LinkedList{T}"/> to which <paramref name="node"/> belongs, wrapping around to the first node if <paramref name="node"/> is the last node (that is, the <see cref="LinkedList{T}"/> is treated like a "deque" data structure).
    /// </summary>
    /// <typeparam name="T">The type of the elements in the <see cref="LinkedList{T}"/>.</typeparam>
    /// <param name="node">The node to get the next node of.</param>
    /// <returns>The next node as described, or whatever <see cref="LinkedListNode{T}.Next"/> returns if <paramref name="node"/> is not the last node. This can be <see langword="null"/>.</returns>
    public static LinkedListNode<T>? DequeNext<T>(this LinkedListNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return node.List is not null
            && node == node.List.Last
            ? node.List.First
            : node.Next;
    }
    /// <summary>
    /// Returns the previous node in the <see cref="LinkedList{T}"/> to which <paramref name="node"/> belongs, wrapping around to the last node if <paramref name="node"/> is the first node (that is, the <see cref="LinkedList{T}"/> is treated like a "deque" data structure).
    /// </summary>
    /// <typeparam name="T">The type of the elements in the <see cref="LinkedList{T}"/>.</typeparam>
    /// <param name="node">The node to get the previous node of.</param>
    /// <returns>The previous node as described, or whatever <see cref="LinkedListNode{T}.Previous"/> returns if <paramref name="node"/> is not the first node. This can be <see langword="null"/>.</returns>
    public static LinkedListNode<T>? DequePrevious<T>(this LinkedListNode<T> node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return node.List is not null
               && node == node.List.First
            ? node.List.Last
            : node.Previous;
    }
    /// <summary>
    /// Constructs a <see cref="Deque{T}"/> from the specified <see cref="LinkedList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the <see cref="LinkedList{T}"/>.</typeparam>
    /// <param name="linkedList">The <see cref="LinkedList{T}"/> to construct the <see cref="Deque{T}"/> from.</param>
    /// <returns>The newly constructed <see cref="Deque{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Deque<T> ToDeque<T>(this LinkedList<T> linkedList) => new Deque<T>(linkedList);
}
