namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Semaphore"/> and <see cref="SemaphoreSlim"/> Types.
/// </summary>
public static class SemaphoreExtensions
{

    extension(Semaphore semaphore)
    {
        /// <summary>
        /// Blocks the current thread until it can enter the <paramref name="semaphore"/> once.
        /// </summary>
        /// <param name="semaphore">The <see cref="Semaphore"/> to enter.</param>
        /// <returns>A <see cref="SemaphoreEntry"/> that represents the entry into the <paramref name="semaphore"/>.</returns>
        public SemaphoreEntry WaitOne()
        {
            _ = semaphore.WaitOne();
            return new SemaphoreEntry(semaphore);
        }
        /// <summary>
        /// Blocks the current thread until it can enter the <paramref name="semaphore"/> <paramref name="count"/> times.
        /// </summary>
        /// <param name="semaphore">The <see cref="Semaphore"/> to enter.</param>
        /// <param name="count">The number of times to enter the <paramref name="semaphore"/>.</param>
        /// <returns>A <see cref="SemaphoreEntry"/> that represents the entries into the <paramref name="semaphore"/>.</returns>
        public SemaphoreEntry Wait(int count)
        {
            for (var i = count - 1; i >= 0; i--)
            {
                _ = semaphore.WaitOne();
            }
            return new SemaphoreEntry(semaphore, count);
        }
        /// <summary>
        /// Blocks the current thread until it can enter the <paramref name="semaphore"/> once.
        /// If a signal is not received within the specified <paramref name="timeout"/>, a <see cref="TimeoutException"/> is thrown.
        /// </summary>
        /// <param name="semaphore">The <see cref="Semaphore"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <returns>A <see cref="SemaphoreEntry"/> that represents the entry into the <paramref name="semaphore"/>.</returns>
        public SemaphoreEntry WaitOne(TimeSpan timeout)
        {
            if (!semaphore.WaitOne(timeout))
            {
                throw new TimeoutException("The operation has timed out.");
            }
            return new SemaphoreEntry(semaphore);
        }
        /// <summary>
        /// Blocks the current thread until it can enter the <paramref name="semaphore"/> once, optionally leaving the current synchronization domain before beginning the wait.
        /// If a signal is not received within the specified <paramref name="timeout"/>, a <see cref="TimeoutException"/> is thrown.
        /// </summary>
        /// <param name="semaphore">The <see cref="Semaphore"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <param name="exitContext">Whether to exit the current synchronization domain before the wait.</param>
        /// <returns>A <see cref="SemaphoreEntry"/> that represents the entry into the <paramref name="semaphore"/>.</returns>
        public SemaphoreEntry WaitOne(TimeSpan timeout, bool exitContext)
        {
            if (!semaphore.WaitOne(timeout, exitContext))
            {
                throw new TimeoutException("The operation has timed out.");
            }
            return new SemaphoreEntry(semaphore);
        }
        /// <summary>
        /// Attempts to enter specified <paramref name="semaphore"/> once. If the entry is not granted within the specified <paramref name="timeout"/>, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="semaphore">The <see cref="Semaphore"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <returns>A <see cref="SemaphoreEntry"/> that represents the entry into the <paramref name="semaphore"/> or <see langword="null"/> if the timeout expired.</returns>
        public SemaphoreEntry TryWaitOne(TimeSpan timeout)
        {
            if (!semaphore.WaitOne(timeout))
            {
                return null;
            }
            return new SemaphoreEntry(semaphore);
        }
        /// <summary>
        /// Attempts to enter specified <paramref name="semaphore"/> once. If the entry is not granted within the specified <paramref name="timeout"/>, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="semaphore">The <see cref="Semaphore"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <param name="exitContext">Whether to exit the current synchronization domain before the wait.</param>
        /// <returns>A <see cref="SemaphoreEntry"/> that represents the entry into the <paramref name="semaphore"/> or <see langword="null"/> if the timeout expired.</returns>
        public SemaphoreEntry TryWaitOne(TimeSpan timeout, bool exitContext)
        {
            if (!semaphore.WaitOne(timeout, exitContext))
            {
                return null;
            }
            return new SemaphoreEntry(semaphore);
        }
    }

    extension(SemaphoreSlim semaphore)
    {
        /// <summary>
        /// Blocks the current thread until it can enter the <paramref name="semaphore"/> once.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <returns>A <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/>.</returns>
        public SemaphoreSlimEntry WaitOne()
        {
            semaphore.Wait();
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Blocks the current thread until it can enter the <paramref name="semaphore"/> <paramref name="count"/> times.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="count">The number of times to enter the <paramref name="semaphore"/>.</param>
        /// <returns>A <see cref="SemaphoreSlimEntry"/> that represents the entries into the <paramref name="semaphore"/>.</returns>
        public SemaphoreSlimEntry Wait(int count)
        {
            for (var i = count - 1; i >= 0; i--)
            {
                semaphore.Wait();
            }
            return new SemaphoreSlimEntry(semaphore, count);
        }
        /// <summary>
        /// Blocks the current thread until it can enter the <paramref name="semaphore"/> once.
        /// If a signal is not received within the specified <paramref name="timeout"/>, a <see cref="TimeoutException"/> is thrown.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <returns>A <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/> or <see langword="null"/> if the timeout expired.</returns>
        public SemaphoreSlimEntry WaitOne(TimeSpan timeout)
        {
            if (!semaphore.Wait(timeout))
            {
                throw new TimeoutException("The operation has timed out.");
            }
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Blocks the current thread until it can enter the <paramref name="semaphore"/> once while observing the specified <paramref name="cancellationToken"/>.
        /// If entry is not granted within the specified <paramref name="timeout"/>, a <see cref="TimeoutException"/> is thrown.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/>.</returns>
        public SemaphoreSlimEntry WaitOne(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (!semaphore.Wait(timeout, cancellationToken))
            {
                throw new TimeoutException("The operation has timed out.");
            }
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Blocks the current thread until it can enter the <paramref name="semaphore"/> once while observing the specified <paramref name="cancellationToken"/>.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/>.</returns>
        public SemaphoreSlimEntry WaitOne(CancellationToken cancellationToken)
        {
            semaphore.Wait(cancellationToken);
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Attempts to enter specified <paramref name="semaphore"/> once. If the entry is not granted within the specified <paramref name="timeout"/>, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <returns>A <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/> or <see langword="null"/> if the timeout expired.</returns>
        public SemaphoreSlimEntry TryWaitOne(TimeSpan timeout)
        {
            if (!semaphore.Wait(timeout))
            {
                return null;
            }
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Attempts to enter specified <paramref name="semaphore"/> once. If the entry is not granted within the specified <paramref name="timeout"/>, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/> or <see langword="null"/> if the timeout expired.</returns>
        public SemaphoreSlimEntry TryWaitOne(TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                if (!semaphore.Wait(timeout, cancellationToken))
                {
                    return null;
                }
            }
            catch // Have to do this for the cancellation token, making this the most expensive overload
            {
                return null;
            }
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Attempts to enter specified <paramref name="semaphore"/> once while observing the specified <paramref name="cancellationToken"/>.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/>.</returns>
        public SemaphoreSlimEntry TryWaitOne(CancellationToken cancellationToken)
        {
            try
            {
                semaphore.Wait(cancellationToken);
            }
            catch // Have to do this for the cancellation token, making this the most expensive overload
            {
                return null;
            }
            return new SemaphoreSlimEntry(semaphore);
        }

        /// <summary>
        /// Asynchronously waits until entry is granted into the specified <paramref name="semaphore"/> once.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to a <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/>.</returns>
        public async ValueTask<SemaphoreSlimEntry> WaitOneAsync()
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Asynchronously waits until entry is granted into the specified <paramref name="semaphore"/> <paramref name="count"/> times.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="count">The number of times to enter the <paramref name="semaphore"/>.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to a <see cref="SemaphoreSlimEntry"/> that represents the entries into the <paramref name="semaphore"/>.</returns>
        public async ValueTask<SemaphoreSlimEntry> WaitAsync(int count)
        {
            for (var i = count - 1; i >= 0; i--)
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
            }
            return new SemaphoreSlimEntry(semaphore, count);
        }
        /// <summary>
        /// Asynchronously waits until entry is granted into the specified <paramref name="semaphore"/> once.
        /// If a signal is not received within the specified <paramref name="timeout"/>, a <see cref="TimeoutException"/> is thrown.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to a <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/> or <see langword="null"/> if the timeout expired.</returns>
        public async ValueTask<SemaphoreSlimEntry> WaitOneAsync(TimeSpan timeout)
        {
            if (!await semaphore.WaitAsync(timeout).ConfigureAwait(false))
            {
                throw new TimeoutException("The operation has timed out.");
            }
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Asynchronously waits until entry is granted into the specified <paramref name="semaphore"/> once while observing the specified <paramref name="cancellationToken"/>.
        /// If entry is not granted within the specified <paramref name="timeout"/>, a <see cref="TimeoutException"/> is thrown.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to a <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/>.</returns>
        public async ValueTask<SemaphoreSlimEntry> WaitOneAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (!await semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false))
            {
                throw new TimeoutException("The operation has timed out.");
            }
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Asynchronously waits until entry is granted into the specified <paramref name="semaphore"/> once while observing the specified <paramref name="cancellationToken"/>.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to a <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/>.</returns>
        public async ValueTask<SemaphoreSlimEntry> WaitOneAsync(CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Asynchronously attempts to enter specified <paramref name="semaphore"/> once.
        /// If the entry is not granted within the specified <paramref name="timeout"/>, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to a <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/> or <see langword="null"/> if the timeout expired.</returns>
        public async ValueTask<SemaphoreSlimEntry> TryWaitOneAsync(TimeSpan timeout)
        {
            if (!await semaphore.WaitAsync(timeout).ConfigureAwait(false))
            {
                return null;
            }
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Asynchronously attempts to enter specified <paramref name="semaphore"/> once.
        /// If the entry is not granted within the specified <paramref name="timeout"/>, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="timeout">The <see cref="TimeSpan"/> to wait before exiting.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to a <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/> or <see langword="null"/> if the timeout expired.</returns>
        public async ValueTask<SemaphoreSlimEntry> TryWaitOneAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                if (!await semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false))
                {
                    return null;
                }
            }
            catch // Have to do this for the cancellation token, making this the most expensive overload
            {
                return null;
            }
            return new SemaphoreSlimEntry(semaphore);
        }
        /// <summary>
        /// Asynchronously attempts to enter specified <paramref name="semaphore"/> once w
        /// ile observing the specified <paramref name="cancellationToken"/>.
        /// </summary>
        /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to enter.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to a <see cref="SemaphoreSlimEntry"/> that represents the entry into the <paramref name="semaphore"/> or <see langword="null"/> if the timeout expired.</returns>
        public async ValueTask<SemaphoreSlimEntry> TryWaitOneAsync(CancellationToken cancellationToken)
        {
            try
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch // Have to do this for the cancellation token, making this the most expensive overload
            {
                return null;
            }
            return new SemaphoreSlimEntry(semaphore);
        }
    }
}

#region Semaphore entry handles
/// <summary>
/// Represents <paramref name="count"/> entries into the specified <paramref name="semaphore"/>.
/// </summary>
public class SemaphoreEntry : IDisposable
{
    /// <summary>
    /// Gets the <see cref="Semaphore"/> that was entered.
    /// </summary>
    public Semaphore Semaphore { get; }
    /// <summary>
    /// Gets the number of entries into <see cref="Semaphore"/> this instance represents.
    /// </summary>
    public int Count { get; }

    internal SemaphoreEntry(Semaphore semaphore, int count = 1)
    {
        Semaphore = semaphore;
        Count = count;
    }

    private bool disposed;
    /// <summary>
    /// Releases the <see cref="SemaphoreSlim"/> entry.
    /// </summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;
        _ = Semaphore.Release(Count);
    }
}
/// <summary>
/// Represents <paramref name="count"/> entries into the specified <paramref name="semaphore"/>.
/// </summary>
public class SemaphoreSlimEntry : IDisposable
{
    /// <summary>
    /// Gets the <see cref="SemaphoreSlim"/> that was entered.
    /// </summary>
    public SemaphoreSlim SemaphoreSlim { get; }
    /// <summary>
    /// Gets the number of entries into <see cref="SemaphoreSlim"/> this instance represents.
    /// </summary>
    public int Count { get; }

    private bool disposed;

    internal SemaphoreSlimEntry(SemaphoreSlim semaphore, int count = 1)
    {
        SemaphoreSlim = semaphore;
        Count = count;
    }

    /// <summary>
    /// Releases the <see cref="System.Threading.SemaphoreSlim"/> entries.
    /// </summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;
        _ = SemaphoreSlim.Release(Count);
    }
}
#endregion