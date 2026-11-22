namespace LaquaiLib.Interfaces;

/// <summary>
/// Defines the duck-typed shape for an awaitable object.
/// Note: awaitables/needn't be <see langword="struct"/>s to qualify for the compiler's awaitable pattern, but these markers enforce that they are since that is the idiomatic shape.
/// </summary>
internal interface IAwaitable<TAwaitable, out TAwaiter>
    where TAwaitable : struct, IAwaitable<TAwaitable, TAwaiter>
    where TAwaiter : struct, ICriticalNotifyCompletion
{
    /// <summary>
    /// Gets a <typeparamref name="TAwaiter"/> instance that implements the awaiter side of this awaitable.
    /// </summary>
    public TAwaiter GetAwaiter();
}
/// <summary>
/// Defines the duck-typed shape for the awaiter side of an awaitable object that does not return a result.
/// </summary>
/// <typeparam name="TAwaiter">The type of the awaiter implementing this interface. Allows the type constraint to <see langword="struct"/> and awaiter infrastructure to be specified.</typeparam>
internal interface IAwaiter<TAwaiter> : ICriticalNotifyCompletion
    where TAwaiter : struct, ICriticalNotifyCompletion
{
    /// <summary>
    /// Gets whether the asynchronous wait has completed.
    /// </summary>
    public bool IsCompleted { get; }
    /// <summary>
    /// Ends the await on this awaiter.
    /// </summary>
    public void GetResult();
}
/// <summary>
/// Defines the duck-typed shape for the awaiter side of an awaitable object that returns a result.
/// </summary>
/// <typeparam name="TAwaiter">The type of the awaiter implementing this interface. Allows the type constraint to <see langword="struct"/> and awaiter infrastructure to be specified.</typeparam>
/// <typeparam name="TResult">The type of the result an asynchronous wait on this awaiter produces.</typeparam>
internal interface IAwaiter<TAwaiter, out TResult> : IAwaiter<TAwaiter>
    where TAwaiter : struct, IAwaiter<TAwaiter, TResult>
{
    void IAwaiter<TAwaiter>.GetResult() => GetResult();
    /// <summary>
    /// Ends the await on this awaiter and gets the result of the asynchronous operation.
    /// </summary>
    /// <returns>The result of the asynchronous operation.</returns>
    public new TResult GetResult();
}