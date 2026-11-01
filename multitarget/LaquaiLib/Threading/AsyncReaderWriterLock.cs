using System.ComponentModel;

namespace LaquaiLib.Threading;

/// <summary>
/// Manages exclusive access to a resource for any number of concurrent readers and a single writer.
/// Additionally, a single reader may be in upgradeable mode, which initially acts the same as a reader, but allows upgrade to a writer lock without releasing the lock in between.
/// </summary>
/// <remarks>
/// For the purposes of fairness and entry ordering, upgradeable locks are treated the same as writers.
/// <para/>Lock entries are not thread-affine and may be disposed on a thread different from the one that acquired them.
/// <para/>This type is thread-safe.
/// </remarks>
public sealed class AsyncReaderWriterLock : IAsyncDisposable
{
    /// <summary>
    /// Represents a single entry in an <see cref="AsyncReaderWriterLock"/> instance.
    /// </summary>
    public sealed class LockEntry : IDisposable
    {
        private readonly AsyncReaderWriterLock _lock;
        public bool IsUpgradeable { get; private set; }
        public bool IsWriter { get; private set; }
        internal LockEntry(AsyncReaderWriterLock parent, bool isUpgradeable, bool isWriter)
        {
            _lock = parent;
            IsUpgradeable = isUpgradeable;
            IsWriter = isWriter;
        }

        /// <summary>
        /// Attempts to upgrade the lock entry to a writer lock without waiting.
        /// If the entry is not upgradeable, this will return <see langword="false"/>.
        /// If the entry was created as a writer lock or has already been upgraded, this will return <see langword="true"/>.
        /// If another thread is currently holding the writer lock, this will return <see langword="false"/>.
        /// </summary>
        public bool TryUpgrade()
        {
            if (IsWriter)
                return true;
            if (!IsUpgradeable)
                return false;

            var result = _lock.TryUpgradeToWriter(this);
            if (result)
                Upgrade();
            return result;
        }
        public async ValueTask<bool> WaitToUpgradeAsync(TimeSpan? timeout = default, CancellationToken cancellationToken = default)
        {
            if (IsWriter)
                return true;
            if (!IsUpgradeable)
                ThrowLockEntryNotUpgradeable();

            var result = await _lock.WaitToUpgradeToWriterAsync(this, timeout, cancellationToken).ConfigureAwait(false);
            if (result)
                Upgrade();
            return result;
        }

        private void Upgrade()
        {
            IsUpgradeable = false;
            IsWriter = true;
        }

        /// <summary>
        /// Releases the lock entry.
        /// </summary>
        public void Dispose() => _lock.Release(this);

        #region Throw helpers
        [MethodImpl(MethodImplOptions.NoInlining)] private static void ThrowLockEntryNotUpgradeable() => throw new InvalidOperationException("Lock entry is not upgradeable.");
        #endregion
    }

    private record class State(int Readers, LockEntry UpgradeableEntry, LockEntry WriterEntry);

    private readonly AsyncReaderWriterLockFairnessPolicy _fairness;
    private readonly int? _writerThread;
    private PriorityQueue<long, int> waiting;
    private State state;
    private int disposed;

    /// <summary>
    /// Initializes a new <see cref="AsyncReaderWriterLock"/>.
    /// </summary>
    /// <param name="fairnessPolicy">The <see cref="AsyncReaderWriterLockFairnessPolicy"/> to employ for this lock.</param>
    /// <param name="singleWriterThead">The managed thread ID of the single writer thread that will be using this lock. If this is not specified, the lock will assume that multiple threads may attempt to acquire the writer lock simultaneously. Note that any thread is allowed to take a read lock in upgradeable mode, but attempting to upgrade it when not on the specified thread will result in an exception.</param>
    public AsyncReaderWriterLock(AsyncReaderWriterLockFairnessPolicy fairnessPolicy = AsyncReaderWriterLockFairnessPolicy.FirstComeFirstServed, int? singleWriterThead = null)
    {
        if (!Enum.IsDefined(fairnessPolicy))
            throw new InvalidEnumArgumentException(nameof(fairnessPolicy), (int)fairnessPolicy, typeof(AsyncReaderWriterLockFairnessPolicy));

        _fairness = fairnessPolicy;
        _writerThread = singleWriterThead;

        throw new NotImplementedException();
    }

    public bool TryEnterRead(out LockEntry entry)
    {
        EnsureInitialized();
        entry = null;

        if (state.UpgradeableEntry != null || state.WriterEntry != null)
            return false;

        Volatile.Write(ref state, state with { Readers = state.Readers + 1 });
    }
    public bool TryEnterUpgradeable(out LockEntry entry)
    {
        EnsureInitialized();
        entry = null;

        if (Interlocked.CompareExchange(ref state.UpgradeableEntry, new LockEntry(this, true, false), null) != null)
            return false;

        Volatile.Write(ref state, state with { Readers = state.Readers + 1 });
    }
    public bool TryEnterWriter(out LockEntry entry)
    {
        EnsureInitialized();
        entry = null;

        if (Interlocked.CompareExchange(ref state.WriterEntry, new LockEntry(this, false, true), null) != null)
            return false;

        Volatile.Write(ref state, state with { Readers = state.Readers + 1 });
    }
    private bool TryUpgradeToWriter(LockEntry lockEntry)
    {
        EnsureInitialized();
        Debug.Assert(lockEntry.IsUpgradeable, "Should not reach this method with a non-upgradeable lock entry");

        if (state.WriterEntry != null)
            return false;
        if (IsKnownUpgradeableEntry(lockEntry))

    }

    public ValueTask<LockEntry> WaitToEnterReadAsync(TimeSpan? timeout = default, CancellationToken cancellationToken = default)
    {
        if (TryEnterRead(out var entry))
            return new ValueTask<LockEntry>(entry);
    }
    public ValueTask<LockEntry> WaitToEnterUpgradeableAsync(TimeSpan? timeout = default, CancellationToken cancellationToken = default)
    {
        if (TryEnterUpgradeable(out var entry))
            return new ValueTask<LockEntry>(entry);
    }
    public ValueTask<LockEntry> WaitToEnterWriterAsync(TimeSpan? timeout = default, CancellationToken cancellationToken = default)
    {
        if (TryEnterWriter(out var entry))
            return new ValueTask<LockEntry>(entry);
    }
    private ValueTask<bool> WaitToUpgradeToWriterAsync(LockEntry lockEntry, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        if (TryUpgradeToWriter(lockEntry))
            return new ValueTask<bool>(true);
        Debug.Assert(lockEntry.IsUpgradeable, "Should not reach this method with a non-upgradeable lock entry");

    }

    private void Release(LockEntry lockEntry)
    {
        Interlocked.Decrement(ref state.Readers);
        Interlocked.CompareExchange(ref state.UpgradeableEntry, null, lockEntry);
        Interlocked.CompareExchange(ref state.WriterEntry, null, lockEntry);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] private bool IsKnownUpgradeableEntry(LockEntry lockEntry) => ReferenceEquals(state.UpgradeableEntry, lockEntry);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private bool IsKnownWriterEntry(LockEntry lockEntry) => ReferenceEquals(state.WriterEntry, lockEntry);

    private void EnsureInitialized()
    {
        EnsureNotDisposed();
        waiting ??= new();
    }
    private void EnsureNotDisposed() => ObjectDisposedException.ThrowIf(disposed == 1, nameof(AsyncReaderWriterLock));
    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 1)
            return default;
    }
}

/// <summary>
/// Specifies the fairness policy for an <see cref="AsyncReaderWriterLock"/> instance.
/// </summary>
public enum AsyncReaderWriterLockFairnessPolicy
{
    /// <summary>
    /// Specifies that the lock approximately follows a first-come-first-served policy, with no preference for readers or writers
    /// This is the default policy. Note that this differs from <see cref="ReaderWriterLockSlim"/>'s default policy, which is equivalent to <see cref="PreferWriters"/>.
    /// </summary>
    FirstComeFirstServed,
    /// <summary>
    /// Specifies that the lock prefers readers; that is, if both readers and writers are waiting, readers will be granted access before writers and waiting writers will be delayed until all waiting readers have released the lock.
    /// </summary>
    PreferReaders,
    /// <summary>
    /// Specifies that the lock prefers writers; that is, if both readers and writers are waiting, writers will be granted access before readers and waiting readers will be delayed until all waiting writers have released the lock.
    /// </summary>
    PreferWriters,
}