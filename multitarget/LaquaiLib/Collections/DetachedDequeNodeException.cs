namespace LaquaiLib.Collections;

/// <summary>
/// The exception that is thrown when <see cref="Deque{T}.EnsureIntegrity"/> encounters a <see cref="DequeNode{T}"/> that is not attached to a <see cref="Deque{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the value the detached <see cref="DequeNode{T}"/> contains.</typeparam>
public sealed class DetachedDequeNodeException<T> : Exception
{
    /// <inheritdoc />
    public override string Message
    {
        get;
    }
    /// <inheritdoc cref="Exception.InnerException"/>
    public new Exception InnerException
    {
        get;
    }
    /// <summary>
    /// The <see cref="DequeNode{T}"/> that is not attached to a <see cref="Deque{T}"/>.
    /// </summary>
    public DequeNode<T> Node
    {
        get;
    }
    /// <summary>
    /// The kind of offense that <see cref="Node"/> committed which caused this exception to be thrown.
    /// </summary>
    public OffenseKind Offense
    {
        get;
    }

    /// <summary>
    /// Initializes a new <see cref="DetachedDequeNodeException{T}"/> with the offending <paramref name="node"/> and the kind of <paramref name="offense"/> that caused this exception to be thrown.
    /// </summary>
    /// <param name="node">The detached <see cref="DequeNode{T}"/>.</param>
    /// <param name="offense">The kind of offense that <paramref name="node"/> committed which caused this exception to be thrown.</param>
    public DetachedDequeNodeException(DequeNode<T> node, OffenseKind offense)
    {
        Message = GetOffenseString(offense);
        Node = node;
        Offense = offense;
    }
    /// <summary>
    /// Initializes a new <see cref="DetachedDequeNodeException{T}"/> with the offending <paramref name="node"/>, the kind of <paramref name="offense"/> that caused this exception to be thrown, and an <paramref name="innerException"/>.
    /// </summary>
    /// <param name="node">The detached <see cref="DequeNode{T}"/>.</param>
    /// <param name="offense">The kind of offense that <paramref name="node"/> committed which caused this exception to be thrown.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DetachedDequeNodeException(DequeNode<T> node, OffenseKind offense, Exception innerException) : this(node, offense) => InnerException = innerException;

    private static string GetOffenseString(OffenseKind offense) => offense switch
    {
        OffenseKind.NoDeque => $"The specified {nameof(DequeNode<>)} is not attached to a {nameof(Deque<>)}.",
        OffenseKind.NoNext => $"The specified {nameof(DequeNode<>)} is missing a reference to its next node.",
        OffenseKind.NoPrevious => $"The specified {nameof(DequeNode<>)} is missing a reference to its previous node.",
        OffenseKind.InconsistentNext => $"The specified {nameof(DequeNode<>)}'s next node does not have a reference to the specified {nameof(DequeNode<>)} as its previous node.",
        OffenseKind.InconsistentPrevious => $"The specified {nameof(DequeNode<>)}'s previous node does not have a reference to the specified {nameof(DequeNode<>)} as its next node.",
        _ => throw new ArgumentOutOfRangeException(nameof(offense), offense, $"The specified {nameof(OffenseKind)} is not supported.")
    };

    /// <summary>
    /// Identifies the kind of offense that <see cref="Node"/> committed which caused this exception to be thrown.
    /// </summary>
    [Flags]
    public enum OffenseKind
    {
        /// <summary>
        /// Indicates that the kind of offense is undefined.
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// Indicates that <see cref="Node"/> is not attached to a <see cref="Deque{T}"/>.
        /// </summary>
        NoDeque = 1 << 0,
        /// <summary>
        /// Indicates that <see cref="Node"/> is missing a reference to its next node.
        /// </summary>
        NoNext = 1 << 1,
        /// <summary>
        /// Indicates that <see cref="Node"/> is missing a reference to its previous node.
        /// </summary>
        NoPrevious = 1 << 2,
        /// <summary>
        /// Indicates that <see cref="Node"/>'s next node does not have a reference to <see cref="Node"/> as its previous node.
        /// </summary>
        InconsistentNext = 1 << 3,
        /// <summary>
        /// Indicates that <see cref="Node"/>'s previous node does not have a reference to <see cref="Node"/> as its next node.
        /// </summary>
        InconsistentPrevious = 1 << 4
    }
}
