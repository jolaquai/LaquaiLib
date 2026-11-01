using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace LaquaiLib.Threading;

/// <summary>
/// Manages exclusive access to a resource for any number of concurrent readers and a single writer.
/// Additionally, a single reader may be in upgradeable mode, which initially acts the same as a reader, but allows upgrade to a writer lock without releasing the lock in between.
/// Any writer lock may likewise be downgraded to upgradeable mode without releasing the lock in between.
/// </summary>
/// <remarks>
/// For the purposes of fairness and entry ordering, upgradeable locks are treated the same as writers. A pending upgrade takes precedence over all other waiters, regardless of the fairness policy.
/// <para/>Waits that time out complete with <see langword="null"/> (or <see langword="false"/> for upgrades); waits that are canceled throw an <see cref="OperationCanceledException"/>.
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
        private int released;
        /// <summary>
        /// Gets whether this entry is currently an upgradeable read lock, either from creation or through a downgrade.
        /// </summary>
        public bool IsUpgradeable { get; internal set; }
        /// <summary>
        /// Gets whether this entry is a writer lock, either from creation or through an upgrade.
        /// </summary>
        public bool IsWriter { get; internal set; }
        internal LockEntry(AsyncReaderWriterLock parent, bool isUpgradeable, bool isWriter)
        {
            _lock = parent;
            IsUpgradeable = isUpgradeable;
            IsWriter = isWriter;
        }

        /// <summary>
        /// Attempts to upgrade the lock entry to a writer lock without waiting.
        /// If the entry is not upgradeable or has been released, this will return <see langword="false"/>.
        /// If the entry was created as a writer lock or has already been upgraded, this will return <see langword="true"/>.
        /// If other readers are currently holding the lock, this will return <see langword="false"/>.
        /// </summary>
        public bool TryUpgrade() => IsWriter || (IsUpgradeable && _lock.TryUpgrade(this));
        /// <summary>
        /// Waits to upgrade the lock entry to a writer lock.
        /// If the entry was created as a writer lock or has already been upgraded, this will return <see langword="true"/>.
        /// If the entry has been released or the wait times out, this will return <see langword="false"/>.
        /// </summary>
        /// <param name="timeout">The maximum time to wait. <see langword="null"/> or <see cref="Timeout.InfiniteTimeSpan"/> waits indefinitely.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the wait.</param>
        public ValueTask<bool> WaitToUpgradeAsync(TimeSpan? timeout = default, CancellationToken cancellationToken = default)
        {
            if (IsWriter)
                return new ValueTask<bool>(true);
            if (!IsUpgradeable)
                ThrowLockEntryNotUpgradeable();
            return _lock.WaitToUpgradeAsync(this, timeout, cancellationToken);
        }
        /// <summary>
        /// Downgrades a writer lock entry to an upgradeable read lock without releasing the lock in between, admitting waiting readers according to the fairness policy.
        /// This never waits. The entry may be upgraded again afterwards.
        /// If the entry is already upgradeable, this has no effect.
        /// </summary>
        /// <exception cref="InvalidOperationException">The entry is a plain read lock or has been released.</exception>
        public void Downgrade()
        {
            if (IsUpgradeable)
                return;
            if (!IsWriter)
                ThrowLockEntryNotDowngradeable();
            _lock.Downgrade(this);
        }

        /// <summary>
        /// Releases the lock entry. Subsequent calls have no effect.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref released, 1) == 0)
                _lock.Release(this);
        }

        #region Throw helpers
        [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] private static void ThrowLockEntryNotUpgradeable() => throw new InvalidOperationException("Lock entry is not upgradeable.");
        [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] private static void ThrowLockEntryNotDowngradeable() => throw new InvalidOperationException("Lock entry is not a writer lock.");
        #endregion
    }

    private enum WaiterKind : byte { Reader, Upgradeable, Writer, Upgrade }
    private sealed class Waiter
    {
        public readonly AsyncReaderWriterLock Owner;
        public readonly LinkedListNode<Waiter> Node;
        public readonly ReusableTaskCompletionSource<LockEntry> Source = new();
        public WaiterKind Kind;
        public long Sequence;
        public LockEntry Entry;
        public Waiter(AsyncReaderWriterLock owner)
        {
            Owner = owner;
            Node = new LinkedListNode<Waiter>(this);
        }
    }

    private readonly Lock _sync = new();
    private readonly AsyncReaderWriterLockFairnessPolicy _fairness;
    private readonly LinkedList<Waiter> _readers = new();
    private readonly LinkedList<Waiter> _writers = new();
    private readonly Stack<Waiter> _pool = new();
    private Waiter upgradeWaiter;
    private LockEntry upgrader;
    private LockEntry writer;
    private int readers;
    private long sequence;
    private bool disposed;

    /// <summary>
    /// Initializes a new <see cref="AsyncReaderWriterLock"/>.
    /// </summary>
    /// <param name="fairnessPolicy">The <see cref="AsyncReaderWriterLockFairnessPolicy"/> to employ for this lock.</param>
    public AsyncReaderWriterLock(AsyncReaderWriterLockFairnessPolicy fairnessPolicy = AsyncReaderWriterLockFairnessPolicy.FirstComeFirstServed)
    {
        if (!Enum.IsDefined(fairnessPolicy))
            throw new InvalidEnumArgumentException(nameof(fairnessPolicy), (int)fairnessPolicy, typeof(AsyncReaderWriterLockFairnessPolicy));
        _fairness = fairnessPolicy;
    }

    /// <summary>
    /// Attempts to enter the lock in read mode without waiting.
    /// </summary>
    public bool TryEnterRead(out LockEntry entry) => TryEnter(WaiterKind.Reader, out entry);
    /// <summary>
    /// Attempts to enter the lock in upgradeable read mode without waiting.
    /// </summary>
    public bool TryEnterUpgradeable(out LockEntry entry) => TryEnter(WaiterKind.Upgradeable, out entry);
    /// <summary>
    /// Attempts to enter the lock in write mode without waiting.
    /// </summary>
    public bool TryEnterWriter(out LockEntry entry) => TryEnter(WaiterKind.Writer, out entry);

    /// <summary>
    /// Waits to enter the lock in read mode.
    /// </summary>
    /// <param name="timeout">The maximum time to wait. <see langword="null"/> or <see cref="Timeout.InfiniteTimeSpan"/> waits indefinitely.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the wait.</param>
    /// <returns>The acquired <see cref="LockEntry"/>, or <see langword="null"/> if the wait timed out.</returns>
    public ValueTask<LockEntry> WaitToEnterReadAsync(TimeSpan? timeout = default, CancellationToken cancellationToken = default) => WaitToEnterAsync(WaiterKind.Reader, timeout, cancellationToken);
    /// <summary>
    /// Waits to enter the lock in upgradeable read mode.
    /// </summary>
    /// <inheritdoc cref="WaitToEnterReadAsync(TimeSpan?, CancellationToken)"/>
    public ValueTask<LockEntry> WaitToEnterUpgradeableAsync(TimeSpan? timeout = default, CancellationToken cancellationToken = default) => WaitToEnterAsync(WaiterKind.Upgradeable, timeout, cancellationToken);
    /// <summary>
    /// Waits to enter the lock in write mode.
    /// </summary>
    /// <inheritdoc cref="WaitToEnterReadAsync(TimeSpan?, CancellationToken)"/>
    public ValueTask<LockEntry> WaitToEnterWriterAsync(TimeSpan? timeout = default, CancellationToken cancellationToken = default) => WaitToEnterAsync(WaiterKind.Writer, timeout, cancellationToken);

    private bool TryEnter(WaiterKind kind, out LockEntry entry)
    {
        lock (_sync)
        {
            EnsureNotDisposed();
            entry = CanAcquire(kind) ? Grant(kind, null) : null;
        }
        return entry != null;
    }
    private ValueTask<LockEntry> WaitToEnterAsync(WaiterKind kind, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        var ms = ToMilliseconds(timeout);
        if (cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled<LockEntry>(cancellationToken);

        Waiter waiter;
        lock (_sync)
        {
            EnsureNotDisposed();
            if (CanAcquire(kind))
                return new ValueTask<LockEntry>(Grant(kind, null));
            if (ms == 0)
                return new ValueTask<LockEntry>((LockEntry)null);
            waiter = Enqueue(kind, null);
        }
        return WaitCoreAsync(waiter, ms, cancellationToken);
    }

    private bool TryUpgrade(LockEntry entry)
    {
        lock (_sync)
        {
            EnsureNotDisposed();
            if (upgrader != entry || readers != 0 || upgradeWaiter != null)
                return false;
            Grant(WaiterKind.Upgrade, entry);
            return true;
        }
    }
    private ValueTask<bool> WaitToUpgradeAsync(LockEntry entry, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        var ms = ToMilliseconds(timeout);
        if (cancellationToken.IsCancellationRequested)
            return ValueTask.FromCanceled<bool>(cancellationToken);

        Waiter waiter;
        lock (_sync)
        {
            EnsureNotDisposed();
            if (upgrader != entry)
                return new ValueTask<bool>(false);
            if (upgradeWaiter != null)
                ThrowUpgradePending();
            if (readers == 0)
            {
                Grant(WaiterKind.Upgrade, entry);
                return new ValueTask<bool>(true);
            }
            if (ms == 0)
                return new ValueTask<bool>(false);
            waiter = Enqueue(WaiterKind.Upgrade, entry);
        }
        return WaitUpgradeCoreAsync(waiter, ms, cancellationToken);
    }

    private void Downgrade(LockEntry entry)
    {
        lock (_sync)
        {
            if (writer != entry)
                ThrowEntryReleased();
            writer = null;
            upgrader = entry;
            entry.IsWriter = false;
            entry.IsUpgradeable = true;
            Pump();
        }
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    private async ValueTask<bool> WaitUpgradeCoreAsync(Waiter waiter, int millisecondsTimeout, CancellationToken cancellationToken) => await WaitCoreAsync(waiter, millisecondsTimeout, cancellationToken).ConfigureAwait(false) != null;
    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    private async ValueTask<LockEntry> WaitCoreAsync(Waiter waiter, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        CancellationTokenSource timeoutCts = null;
        CancellationTokenRegistration cancelReg = default, timeoutReg = default;
        try
        {
            if (cancellationToken.CanBeCanceled)
                cancelReg = cancellationToken.UnsafeRegister(static (s, ct) => ((Waiter)s).Owner.Abandon((Waiter)s, ct, false), waiter);
            if (millisecondsTimeout > 0)
            {
                timeoutCts = new CancellationTokenSource(millisecondsTimeout);
                timeoutReg = timeoutCts.Token.UnsafeRegister(static (s, ct) => ((Waiter)s).Owner.Abandon((Waiter)s, ct, true), waiter);
            }
            return await waiter.Source.ValueTask.ConfigureAwait(false);
        }
        finally
        {
            cancelReg.Dispose();
            timeoutReg.Dispose();
            timeoutCts?.Dispose();
            waiter.Source.Reset();
            lock (_sync)
            {
                waiter.Entry = null;
                _pool.Push(waiter);
            }
        }
    }

    private bool CanAcquire(WaiterKind kind) => kind == WaiterKind.Reader
        ? writer == null && upgradeWaiter == null && (_fairness == AsyncReaderWriterLockFairnessPolicy.PreferReaders || _writers.Count == 0)
        : writer == null && upgrader == null && (kind == WaiterKind.Upgradeable || readers == 0) && _writers.Count == 0 && (_fairness == AsyncReaderWriterLockFairnessPolicy.PreferWriters || _readers.Count == 0);
    private LockEntry Grant(WaiterKind kind, LockEntry entry)
    {
        switch (kind)
        {
            case WaiterKind.Reader:
                readers++;
                return new LockEntry(this, false, false);
            case WaiterKind.Upgradeable:
                return upgrader = new LockEntry(this, true, false);
            case WaiterKind.Writer:
                return writer = new LockEntry(this, false, true);
            default:
                upgrader = null;
                entry.IsUpgradeable = false;
                entry.IsWriter = true;
                return writer = entry;
        }
    }
    private Waiter Enqueue(WaiterKind kind, LockEntry entry)
    {
        if (!_pool.TryPop(out var waiter))
            waiter = new Waiter(this);
        waiter.Kind = kind;
        waiter.Entry = entry;
        waiter.Sequence = sequence++;
        if (kind == WaiterKind.Upgrade)
            upgradeWaiter = waiter;
        else
            (kind == WaiterKind.Reader ? _readers : _writers).AddLast(waiter.Node);
        return waiter;
    }
    private void Abandon(Waiter waiter, CancellationToken cancellationToken, bool timedOut)
    {
        lock (_sync)
        {
            if (waiter == upgradeWaiter)
                upgradeWaiter = null;
            else if (waiter.Node.List is { } list)
                list.Remove(waiter.Node);
            else
                return;

            if (timedOut)
                waiter.Source.SetResult(null);
            else
                waiter.Source.SetCanceled(cancellationToken);
            Pump();
        }
    }
    private void Release(LockEntry entry)
    {
        lock (_sync)
        {
            if (entry.IsWriter)
            {
                Debug.Assert(writer == entry);
                writer = null;
            }
            else if (entry.IsUpgradeable)
            {
                Debug.Assert(upgrader == entry);
                upgrader = null;
                if (upgradeWaiter is { } pending)
                {
                    upgradeWaiter = null;
                    pending.Source.SetResult(null);
                }
            }
            else
            {
                readers--;
            }
            Pump();
        }
    }
    private void Pump()
    {
        if (upgradeWaiter is { } pending)
        {
            if (readers == 0)
            {
                upgradeWaiter = null;
                pending.Source.SetResult(Grant(WaiterKind.Upgrade, pending.Entry));
            }
            return;
        }

        while (writer == null)
        {
            var r = _readers.First;
            var w = _writers.First;
            if (w != null && (r == null || _fairness == AsyncReaderWriterLockFairnessPolicy.PreferWriters || (_fairness == AsyncReaderWriterLockFairnessPolicy.FirstComeFirstServed && w.Value.Sequence < r.Value.Sequence)))
            {
                var waiter = w.Value;
                if (upgrader != null || (waiter.Kind == WaiterKind.Writer && readers != 0))
                    return;
                _writers.RemoveFirst();
                waiter.Source.SetResult(Grant(waiter.Kind, null));
            }
            else if (r != null)
            {
                _readers.RemoveFirst();
                r.Value.Source.SetResult(Grant(WaiterKind.Reader, null));
            }
            else
            {
                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ToMilliseconds(TimeSpan? timeout)
    {
        if (timeout is not { } t)
            return Timeout.Infinite;
        var ms = (long)t.TotalMilliseconds;
        ArgumentOutOfRangeException.ThrowIfLessThan(ms, -1, nameof(timeout));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(ms, int.MaxValue, nameof(timeout));
        return (int)ms;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureNotDisposed()
    {
        if (disposed)
            ThrowDisposed();
    }

    /// <summary>
    /// Disposes the lock, faulting all pending waiters with an <see cref="ObjectDisposedException"/>.
    /// Entries that are currently held may still be released.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        lock (_sync)
        {
            if (disposed)
                return default;
            disposed = true;

            if (upgradeWaiter is { } pending)
            {
                upgradeWaiter = null;
                pending.Source.SetException(new ObjectDisposedException(nameof(AsyncReaderWriterLock)));
            }
            FailAll(_readers);
            FailAll(_writers);
        }
        return default;

        static void FailAll(LinkedList<Waiter> list)
        {
            while (list.First is { } node)
            {
                list.RemoveFirst();
                node.Value.Source.SetException(new ObjectDisposedException(nameof(AsyncReaderWriterLock)));
            }
        }
    }

    #region Throw helpers
    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] private static void ThrowDisposed() => throw new ObjectDisposedException(nameof(AsyncReaderWriterLock));
    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] private static void ThrowUpgradePending() => throw new InvalidOperationException("An upgrade is already pending for this lock entry.");
    [MethodImpl(MethodImplOptions.NoInlining), DoesNotReturn] private static void ThrowEntryReleased() => throw new InvalidOperationException("Lock entry has been released.");
    #endregion
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
