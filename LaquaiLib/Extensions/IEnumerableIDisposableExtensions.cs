namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IEnumerable{T}"/> of <see cref="IDisposable"/> Type.
/// </summary>
public static class IEnumerableIDisposableExtensions
{
    /// <summary>
    /// Disposes all elements in this sequence of <see cref="IDisposable"/> objects.
    /// </summary>
    /// <param name="disposables">The sequence of <see cref="IDisposable"/> objects to dispose.</param>
    /// <remarks>
    /// If any of the <see cref="IDisposable.Dispose"/> calls throw an exception, the exceptions are collected and rethrown as an <see cref="AggregateException"/> after all <see cref="IDisposable.Dispose"/> calls have been made.
    /// </remarks>
    public static void Dispose(this IEnumerable<IDisposable> disposables)
    {
        var innerExceptions = new List<Exception>();
        foreach (var disposable in disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                innerExceptions.Add(ex);
            }
        }
        if (innerExceptions.Count > 0)
        {
            throw new AggregateException(innerExceptions);
        }
    }
}