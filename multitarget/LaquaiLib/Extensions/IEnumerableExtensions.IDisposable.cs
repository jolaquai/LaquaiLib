namespace LaquaiLib.Extensions;

public static partial class IEnumerableExtensions
{
    extension(IEnumerable<IDisposable> disposables)
    {
        /// <summary>
        /// Disposes all elements in this sequence of <see cref="IDisposable"/> objects.
        /// </summary>
        /// <param name="disposables">The sequence of <see cref="IDisposable"/> objects to dispose.</param>
        /// <remarks>
        /// If any of the <see cref="IDisposable.Dispose"/> calls throw an exception, the exceptions are collected and rethrown as an <see cref="AggregateException"/> after all <see cref="IDisposable.Dispose"/> calls have been made.
        /// <para/>If <paramref name="disposables"/> is of a type that implements <see cref="IDisposable"/> itself, the collection itself will <b>not</b> be disposed because binding prioritizes instance methods over extension methods. This scenario would require qualifying the identifier with the type name and calling it like a regular <see langword="static"/> method.
        /// </remarks>
        public void Dispose()
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
}