namespace LaquaiLib.Classes.Collections;

/// <summary>
/// Represents a binary tree, that is, a tree data structure in which each node has at most two children.
/// </summary>
public class BinaryTree<T>
{
    /// <summary>
    /// Represents a node in a binary tree.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// The left child of this <see cref="Node"/>. May be <c>null</c> if this <see cref="Node"/> has no left child.
        /// </summary>
        public Node? Left { get; set; }
        /// <summary>
        /// The right child of this <see cref="Node"/>. May be <c>null</c> if this <see cref="Node"/> has no right child.
        /// </summary>
        public Node? Right { get; set; }
        public T Value { get; set; }

        public Node(T value)
        {
            Value = value;
        }
    }
}
