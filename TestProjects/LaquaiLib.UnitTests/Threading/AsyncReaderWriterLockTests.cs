using System.ComponentModel;

using LaquaiLib.Threading;

using Entry = LaquaiLib.Threading.AsyncReaderWriterLock.LockEntry;
using Policy = LaquaiLib.Threading.AsyncReaderWriterLockFairnessPolicy;

namespace LaquaiLib.UnitTests.Threading;

public class AsyncReaderWriterLockTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PendingProbe = TimeSpan.FromMilliseconds(50);
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<Policy> Policies => [Policy.FirstComeFirstServed, Policy.PreferReaders, Policy.PreferWriters];

    private static async Task<T> Completes<T>(Task<T> task) => await task.WaitAsync(WaitTimeout, Ct);
    private static async Task AssertPending(Task task)
    {
        await Task.Delay(PendingProbe, Ct);
        Assert.False(task.IsCompleted);
    }
    private static Entry Read(AsyncReaderWriterLock l)
    {
        Assert.True(l.TryEnterRead(out var e));
        return e;
    }
    private static Entry Upgradeable(AsyncReaderWriterLock l)
    {
        Assert.True(l.TryEnterUpgradeable(out var e));
        return e;
    }
    private static Entry Write(AsyncReaderWriterLock l)
    {
        Assert.True(l.TryEnterWriter(out var e));
        return e;
    }

    [Fact]
    public void ConstructorRejectsUndefinedPolicy() => Assert.Throws<InvalidEnumArgumentException>(() => new AsyncReaderWriterLock((Policy)42));

    [Theory]
    [MemberData(nameof(Policies))]
    public void EntriesReportTheirKind(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var r = Read(l);
        Assert.False(r.IsUpgradeable);
        Assert.False(r.IsWriter);
        var u = Upgradeable(l);
        Assert.True(u.IsUpgradeable);
        Assert.False(u.IsWriter);
        r.Dispose();
        u.Dispose();
        var w = Write(l);
        Assert.False(w.IsUpgradeable);
        Assert.True(w.IsWriter);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void MultipleReadersCanEnterConcurrently(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var entries = Enumerable.Range(0, 16).Select(_ => Read(l)).ToArray();
        Assert.Equal(16, entries.Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void ReadersBlockWriterButNotUpgradeable(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        Read(l);
        Assert.False(l.TryEnterWriter(out var w));
        Assert.Null(w);
        Assert.True(l.TryEnterUpgradeable(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void WriterExcludesEverything(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        Write(l);
        Assert.False(l.TryEnterRead(out var r));
        Assert.Null(r);
        Assert.False(l.TryEnterUpgradeable(out _));
        Assert.False(l.TryEnterWriter(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void OnlyOneUpgradeableAtATime(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        Assert.False(l.TryEnterUpgradeable(out _));
        Assert.True(l.TryEnterRead(out _));
        u.Dispose();
        Assert.True(l.TryEnterUpgradeable(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void UpgradeableBlocksWriter(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        Assert.False(l.TryEnterWriter(out _));
        u.Dispose();
        Assert.True(l.TryEnterWriter(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void ReleaseFreesTheLock(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        Write(l).Dispose();
        var r1 = Read(l);
        var r2 = Read(l);
        r1.Dispose();
        Assert.False(l.TryEnterWriter(out _));
        r2.Dispose();
        Write(l).Dispose();
        Upgradeable(l).Dispose();
        Write(l);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void DisposingEntryTwiceReleasesOnce(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var r1 = Read(l);
        var r2 = Read(l);
        r1.Dispose();
        r1.Dispose();
        Assert.False(l.TryEnterWriter(out _));
        r2.Dispose();
        Assert.True(l.TryEnterWriter(out _));
    }

    [Fact]
    public void DisposingWriterTwiceDoesNotReleaseNextWriter()
    {
        var l = new AsyncReaderWriterLock();
        var w1 = Write(l);
        w1.Dispose();
        var w2 = Write(l);
        w1.Dispose();
        Assert.False(l.TryEnterRead(out _));
        w2.Dispose();
        Assert.True(l.TryEnterRead(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task EntryCanBeReleasedOnAnotherThread(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var w = Write(l);
        var pending = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        await Task.Run(w.Dispose, Ct);
        Assert.NotNull(await Completes(pending));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task WaitCompletesSynchronouslyWhenUncontended(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var r = l.WaitToEnterReadAsync(cancellationToken: Ct);
        Assert.True(r.IsCompletedSuccessfully);
        (await r).Dispose();
        var u = l.WaitToEnterUpgradeableAsync(cancellationToken: Ct);
        Assert.True(u.IsCompletedSuccessfully);
        (await u).Dispose();
        var w = l.WaitToEnterWriterAsync(cancellationToken: Ct);
        Assert.True(w.IsCompletedSuccessfully);
        Assert.True((await w).IsWriter);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task WaitingWriterIsGrantedWhenReadersRelease(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var r1 = Read(l);
        var r2 = Read(l);
        var pending = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        r1.Dispose();
        await AssertPending(pending);
        r2.Dispose();
        Assert.True((await Completes(pending)).IsWriter);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task WaitingReadersAreGrantedTogetherWhenWriterReleases(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var w = Write(l);
        var pending = Enumerable.Range(0, 8).Select(_ => l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask()).ToArray();
        await AssertPending(Task.WhenAny(pending));
        w.Dispose();
        var entries = await Task.WhenAll(pending).WaitAsync(WaitTimeout, Ct);
        Assert.All(entries, e => Assert.False(e.IsWriter));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task WaitingWritersAreGrantedOneAtATimeInOrder(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var w = Write(l);
        var w1 = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        var w2 = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        w.Dispose();
        var e1 = await Completes(w1);
        await AssertPending(w2);
        e1.Dispose();
        Assert.True((await Completes(w2)).IsWriter);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task WaitingUpgradeableCoexistsWithSubsequentReaders(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var w = Write(l);
        var u = l.WaitToEnterUpgradeableAsync(cancellationToken: Ct).AsTask();
        var r = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        w.Dispose();
        Assert.True((await Completes(u)).IsUpgradeable);
        Assert.False((await Completes(r)).IsWriter);
    }

    [Fact]
    public async Task FirstComeFirstServedQueuesReaderBehindWaitingWriter()
    {
        var l = new AsyncReaderWriterLock(Policy.FirstComeFirstServed);
        var r = Read(l);
        var w = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        Assert.False(l.TryEnterRead(out _));
        var r2 = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        await AssertPending(r2);
        r.Dispose();
        var we = await Completes(w);
        await AssertPending(r2);
        we.Dispose();
        await Completes(r2);
    }

    [Fact]
    public async Task FirstComeFirstServedGrantsEarlierReaderBeforeLaterWriter()
    {
        var l = new AsyncReaderWriterLock(Policy.FirstComeFirstServed);
        var w = Write(l);
        var r1 = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        var w2 = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        var r3 = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        w.Dispose();
        var re1 = await Completes(r1);
        await AssertPending(w2);
        Assert.False(r3.IsCompleted);
        re1.Dispose();
        var we2 = await Completes(w2);
        await AssertPending(r3);
        we2.Dispose();
        await Completes(r3);
    }

    [Fact]
    public async Task PreferReadersLetsReadersBargePastWaitingWriter()
    {
        var l = new AsyncReaderWriterLock(Policy.PreferReaders);
        var r = Read(l);
        var w = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        var r2 = Read(l);
        Assert.True(l.WaitToEnterReadAsync(cancellationToken: Ct).IsCompletedSuccessfully);
        r.Dispose();
        r2.Dispose();
        await AssertPending(w);
    }

    [Fact]
    public async Task PreferReadersGrantsWaitingReadersBeforeEarlierWriter()
    {
        var l = new AsyncReaderWriterLock(Policy.PreferReaders);
        var w = Write(l);
        var w2 = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        var r = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        w.Dispose();
        var re = await Completes(r);
        await AssertPending(w2);
        re.Dispose();
        await Completes(w2);
    }

    [Fact]
    public async Task PreferWritersBlocksNewReadersWhileWriterWaits()
    {
        var l = new AsyncReaderWriterLock(Policy.PreferWriters);
        var r = Read(l);
        var w = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        Assert.False(l.TryEnterRead(out _));
        var r2 = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        r.Dispose();
        var we = await Completes(w);
        await AssertPending(r2);
        we.Dispose();
        await Completes(r2);
    }

    [Fact]
    public async Task PreferWritersGrantsWaitingWritersBeforeEarlierReaders()
    {
        var l = new AsyncReaderWriterLock(Policy.PreferWriters);
        var w = Write(l);
        var r = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        var w2 = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        var w3 = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        w.Dispose();
        var we2 = await Completes(w2);
        await AssertPending(r);
        we2.Dispose();
        var we3 = await Completes(w3);
        await AssertPending(r);
        we3.Dispose();
        await Completes(r);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void TryEnterWriterDoesNotBargePastWaitingWriter(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var r = Read(l);
        _ = l.WaitToEnterWriterAsync(cancellationToken: Ct);
        r.Dispose();
        Assert.False(l.TryEnterWriter(out _));
        Assert.False(l.TryEnterUpgradeable(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void TryUpgradeSucceedsWithoutOtherReaders(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        Assert.True(u.TryUpgrade());
        Assert.True(u.IsWriter);
        Assert.False(u.IsUpgradeable);
        Assert.False(l.TryEnterRead(out _));
        Assert.False(l.TryEnterUpgradeable(out _));
        Assert.True(u.TryUpgrade());
        u.Dispose();
        Assert.True(l.TryEnterWriter(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void TryUpgradeFailsWhileOtherReadersHold(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        var r = Read(l);
        Assert.False(u.TryUpgrade());
        Assert.True(u.IsUpgradeable);
        r.Dispose();
        Assert.True(u.TryUpgrade());
    }

    [Fact]
    public void TryUpgradeOnPlainReaderReturnsFalse()
    {
        var l = new AsyncReaderWriterLock();
        Assert.False(Read(l).TryUpgrade());
    }

    [Fact]
    public void TryUpgradeOnWriterReturnsTrue()
    {
        var l = new AsyncReaderWriterLock();
        Assert.True(Write(l).TryUpgrade());
    }

    [Fact]
    public void TryUpgradeAfterReleaseReturnsFalse()
    {
        var l = new AsyncReaderWriterLock();
        var u = Upgradeable(l);
        u.Dispose();
        Assert.False(u.TryUpgrade());
        Assert.True(l.TryEnterWriter(out _));
    }

    [Fact]
    public void WaitToUpgradeOnPlainReaderThrows()
    {
        var l = new AsyncReaderWriterLock();
        var r = Read(l);
        Assert.Throws<InvalidOperationException>(() => r.WaitToUpgradeAsync(cancellationToken: Ct));
    }

    [Fact]
    public async Task WaitToUpgradeOnWriterCompletesTrueSynchronously()
    {
        var l = new AsyncReaderWriterLock();
        var t = Write(l).WaitToUpgradeAsync(cancellationToken: Ct);
        Assert.True(t.IsCompletedSuccessfully);
        Assert.True(await t);
    }

    [Fact]
    public async Task WaitToUpgradeAfterReleaseReturnsFalse()
    {
        var l = new AsyncReaderWriterLock();
        var u = Upgradeable(l);
        u.Dispose();
        Assert.False(await u.WaitToUpgradeAsync(cancellationToken: Ct));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task WaitToUpgradeCompletesSynchronouslyWithoutOtherReaders(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        var t = u.WaitToUpgradeAsync(cancellationToken: Ct);
        Assert.True(t.IsCompletedSuccessfully);
        Assert.True(await t);
        Assert.True(u.IsWriter);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task WaitToUpgradeWaitsForReadersToDrain(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        var r1 = Read(l);
        var r2 = Read(l);
        var t = u.WaitToUpgradeAsync(cancellationToken: Ct).AsTask();
        r1.Dispose();
        await AssertPending(t);
        r2.Dispose();
        Assert.True(await Completes(t));
        Assert.True(u.IsWriter);
        Assert.False(l.TryEnterRead(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task PendingUpgradeBlocksNewReaders(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        var r = Read(l);
        var t = u.WaitToUpgradeAsync(cancellationToken: Ct).AsTask();
        Assert.False(l.TryEnterRead(out _));
        var r2 = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        r.Dispose();
        Assert.True(await Completes(t));
        await AssertPending(r2);
        u.Dispose();
        await Completes(r2);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task PendingUpgradeTakesPriorityOverWaitingWriter(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        var r = Read(l);
        var w = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        var t = u.WaitToUpgradeAsync(cancellationToken: Ct).AsTask();
        r.Dispose();
        Assert.True(await Completes(t));
        await AssertPending(w);
        u.Dispose();
        Assert.True((await Completes(w)).IsWriter);
    }

    [Fact]
    public async Task ConcurrentUpgradeOnSameEntryThrows()
    {
        var l = new AsyncReaderWriterLock();
        var u = Upgradeable(l);
        var r = Read(l);
        var t = u.WaitToUpgradeAsync(cancellationToken: Ct).AsTask();
        Assert.Throws<InvalidOperationException>(() => u.WaitToUpgradeAsync(cancellationToken: Ct));
        Assert.False(u.TryUpgrade());
        r.Dispose();
        Assert.True(await Completes(t));
    }

    [Fact]
    public async Task ReleasingUpgradeableWithPendingUpgradeCompletesFalse()
    {
        var l = new AsyncReaderWriterLock();
        var u = Upgradeable(l);
        var r = Read(l);
        var t = u.WaitToUpgradeAsync(cancellationToken: Ct).AsTask();
        u.Dispose();
        Assert.False(await Completes(t));
        Assert.True(l.TryEnterRead(out _));
        r.Dispose();
    }

    [Fact]
    public async Task UpgradeTimeoutReturnsFalseAndUnblocksReaders()
    {
        var l = new AsyncReaderWriterLock();
        var u = Upgradeable(l);
        Read(l);
        Assert.False(await u.WaitToUpgradeAsync(TimeSpan.FromMilliseconds(30), Ct).AsTask().WaitAsync(WaitTimeout, Ct));
        Assert.True(u.IsUpgradeable);
        Assert.True(l.TryEnterRead(out _));
    }

    [Fact]
    public async Task UpgradeZeroTimeoutReturnsFalseSynchronously()
    {
        var l = new AsyncReaderWriterLock();
        var u = Upgradeable(l);
        Read(l);
        var t = u.WaitToUpgradeAsync(TimeSpan.Zero, Ct);
        Assert.True(t.IsCompletedSuccessfully);
        Assert.False(await t);
        Assert.True(l.TryEnterRead(out _));
    }

    [Fact]
    public async Task UpgradeCancellationThrowsAndUnblocksReaders()
    {
        var l = new AsyncReaderWriterLock();
        var u = Upgradeable(l);
        Read(l);
        using var cts = new CancellationTokenSource();
        var t = u.WaitToUpgradeAsync(cancellationToken: cts.Token).AsTask();
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => t.WaitAsync(WaitTimeout, Ct));
        Assert.True(u.IsUpgradeable);
        Assert.True(l.TryEnterRead(out _));
    }

    [Fact]
    public void UpgradeWithPrecanceledTokenIsCanceled()
    {
        var l = new AsyncReaderWriterLock();
        var t = Upgradeable(l).WaitToUpgradeAsync(cancellationToken: new CancellationToken(true));
        Assert.True(t.IsCanceled);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void DowngradeAfterUpgradeAdmitsReaders(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        Assert.True(u.TryUpgrade());
        Assert.False(l.TryEnterRead(out _));
        u.Downgrade();
        Assert.True(u.IsUpgradeable);
        Assert.False(u.IsWriter);
        Assert.True(l.TryEnterRead(out _));
        Assert.False(l.TryEnterUpgradeable(out _));
        Assert.False(l.TryEnterWriter(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void DowngradeOfCreatedWriterBecomesUpgradeable(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var w = Write(l);
        w.Downgrade();
        Assert.True(w.IsUpgradeable);
        Assert.False(w.IsWriter);
        Assert.True(l.TryEnterRead(out _));
        Assert.False(l.TryEnterUpgradeable(out _));
        Assert.False(l.TryEnterWriter(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task DowngradedEntryCanUpgradeAgain(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        Assert.True(u.TryUpgrade());
        u.Downgrade();
        var r = Read(l);
        var t = u.WaitToUpgradeAsync(cancellationToken: Ct).AsTask();
        await AssertPending(t);
        r.Dispose();
        Assert.True(await Completes(t));
        Assert.True(u.IsWriter);
        u.Downgrade();
        Assert.True(u.TryUpgrade());
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void ReleaseAfterDowngradeFreesTheLock(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var w = Write(l);
        w.Downgrade();
        w.Dispose();
        Assert.True(l.TryEnterUpgradeable(out var u));
        u.Dispose();
        Assert.True(l.TryEnterWriter(out _));
    }

    [Fact]
    public void DowngradeOnUpgradeableHasNoEffect()
    {
        var l = new AsyncReaderWriterLock();
        var u = Upgradeable(l);
        u.Downgrade();
        Assert.True(u.IsUpgradeable);
        Assert.True(l.TryEnterRead(out _));
        Assert.False(l.TryEnterUpgradeable(out _));
    }

    [Fact]
    public void DowngradeOnPlainReaderThrows()
    {
        var l = new AsyncReaderWriterLock();
        var r = Read(l);
        Assert.Throws<InvalidOperationException>(r.Downgrade);
        Assert.False(l.TryEnterWriter(out _));
    }

    [Fact]
    public void DowngradeAfterReleaseThrowsAndLeavesNextWriterIntact()
    {
        var l = new AsyncReaderWriterLock();
        var w1 = Write(l);
        w1.Dispose();
        Assert.Throws<InvalidOperationException>(w1.Downgrade);
        var w2 = Write(l);
        Assert.Throws<InvalidOperationException>(w1.Downgrade);
        Assert.True(w2.IsWriter);
        Assert.False(l.TryEnterRead(out _));
    }

    [Theory]
    [InlineData(Policy.FirstComeFirstServed)]
    [InlineData(Policy.PreferReaders)]
    public async Task DowngradeGrantsEarlierWaitingReadersButNotWaitingWriter(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var w = Write(l);
        var r1 = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        var r2 = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        var w2 = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        w.Downgrade();
        var e1 = await Completes(r1);
        var e2 = await Completes(r2);
        await AssertPending(w2);
        e1.Dispose();
        e2.Dispose();
        await AssertPending(w2);
        w.Dispose();
        Assert.True((await Completes(w2)).IsWriter);
    }

    [Fact]
    public async Task FirstComeFirstServedDowngradeKeepsReaderBehindEarlierWriter()
    {
        var l = new AsyncReaderWriterLock(Policy.FirstComeFirstServed);
        var w = Write(l);
        var w2 = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        var r = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        w.Downgrade();
        await AssertPending(r);
        w.Dispose();
        var we2 = await Completes(w2);
        await AssertPending(r);
        we2.Dispose();
        await Completes(r);
    }

    [Fact]
    public async Task PreferReadersDowngradeGrantsReaderBehindEarlierWriter()
    {
        var l = new AsyncReaderWriterLock(Policy.PreferReaders);
        var w = Write(l);
        var w2 = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        var r = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        w.Downgrade();
        var re = await Completes(r);
        await AssertPending(w2);
        re.Dispose();
        w.Dispose();
        await Completes(w2);
    }

    [Fact]
    public async Task PreferWritersDowngradeKeepsReadersBehindWaitingWriter()
    {
        var l = new AsyncReaderWriterLock(Policy.PreferWriters);
        var w = Write(l);
        var r = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        var w2 = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        w.Downgrade();
        await AssertPending(r);
        Assert.False(l.TryEnterRead(out _));
        w.Dispose();
        var we2 = await Completes(w2);
        await AssertPending(r);
        we2.Dispose();
        await Completes(r);
    }

    [Fact]
    public async Task DowngradeAfterDisposeIsAllowed()
    {
        var l = new AsyncReaderWriterLock();
        var w = Write(l);
        await l.DisposeAsync();
        w.Downgrade();
        Assert.True(w.IsUpgradeable);
        w.Dispose();
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task ZeroTimeoutReturnsNullSynchronouslyWithoutQueueing(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var r = Read(l);
        var t = l.WaitToEnterWriterAsync(TimeSpan.Zero, Ct);
        Assert.True(t.IsCompletedSuccessfully);
        Assert.Null(await t);
        Assert.True(l.TryEnterRead(out _));
        r.Dispose();
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task TimeoutReturnsNull(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        Write(l);
        Assert.Null(await Completes(l.WaitToEnterReadAsync(TimeSpan.FromMilliseconds(30), Ct).AsTask()));
        Assert.Null(await Completes(l.WaitToEnterUpgradeableAsync(TimeSpan.FromMilliseconds(30), Ct).AsTask()));
        Assert.Null(await Completes(l.WaitToEnterWriterAsync(TimeSpan.FromMilliseconds(30), Ct).AsTask()));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task TimedOutWriterNoLongerBlocksReaders(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var r = Read(l);
        Assert.Null(await Completes(l.WaitToEnterWriterAsync(TimeSpan.FromMilliseconds(30), Ct).AsTask()));
        Assert.True(l.TryEnterRead(out _));
        r.Dispose();
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task TimedOutWriterReleasesQueuedReaders(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var r = Read(l);
        var w = l.WaitToEnterWriterAsync(TimeSpan.FromMilliseconds(100), Ct).AsTask();
        var r2 = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        Assert.Null(await Completes(w));
        Assert.NotNull(await Completes(r2));
        r.Dispose();
    }

    [Fact]
    public async Task InfiniteTimeoutWaitsUntilReleased()
    {
        var l = new AsyncReaderWriterLock();
        var w = Write(l);
        var t = l.WaitToEnterReadAsync(Timeout.InfiniteTimeSpan, Ct).AsTask();
        await AssertPending(t);
        w.Dispose();
        Assert.NotNull(await Completes(t));
    }

    [Fact]
    public async Task GrantedBeforeTimeoutReturnsEntry()
    {
        var l = new AsyncReaderWriterLock();
        var w = Write(l);
        var t = l.WaitToEnterReadAsync(TimeSpan.FromSeconds(30), Ct).AsTask();
        w.Dispose();
        Assert.NotNull(await Completes(t));
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(-1000)]
    public void NegativeTimeoutThrows(int ms)
    {
        var l = new AsyncReaderWriterLock();
        Assert.Throws<ArgumentOutOfRangeException>(() => l.WaitToEnterReadAsync(TimeSpan.FromMilliseconds(ms), Ct));
        Assert.Throws<ArgumentOutOfRangeException>(() => l.WaitToEnterWriterAsync(TimeSpan.FromMilliseconds(ms), Ct));
        Assert.Throws<ArgumentOutOfRangeException>(() => Upgradeable(l).WaitToUpgradeAsync(TimeSpan.FromMilliseconds(ms), Ct));
    }

    [Fact]
    public void OversizedTimeoutThrows()
    {
        var l = new AsyncReaderWriterLock();
        Assert.Throws<ArgumentOutOfRangeException>(() => l.WaitToEnterReadAsync(TimeSpan.FromDays(100), Ct));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public void PrecanceledTokenIsCanceledEvenWhenUncontended(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        Assert.True(l.WaitToEnterReadAsync(cancellationToken: new CancellationToken(true)).IsCanceled);
        Assert.True(l.WaitToEnterUpgradeableAsync(cancellationToken: new CancellationToken(true)).IsCanceled);
        Assert.True(l.WaitToEnterWriterAsync(cancellationToken: new CancellationToken(true)).IsCanceled);
        Assert.True(l.TryEnterWriter(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task CancellationThrowsWithToken(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        Write(l);
        using var cts = new CancellationTokenSource();
        var t = l.WaitToEnterReadAsync(cancellationToken: cts.Token).AsTask();
        await cts.CancelAsync();
        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => t.WaitAsync(WaitTimeout, Ct));
        Assert.Equal(cts.Token, ex.CancellationToken);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task CanceledWriterReleasesQueuedReaders(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var r = Read(l);
        using var cts = new CancellationTokenSource();
        var w = l.WaitToEnterWriterAsync(cancellationToken: cts.Token).AsTask();
        var r2 = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => w.WaitAsync(WaitTimeout, Ct));
        Assert.NotNull(await Completes(r2));
        r.Dispose();
        Assert.False(l.TryEnterWriter(out _));
    }

    [Fact]
    public async Task CancellationAfterGrantIsIgnored()
    {
        var l = new AsyncReaderWriterLock();
        var w = Write(l);
        using var cts = new CancellationTokenSource();
        var t = l.WaitToEnterReadAsync(cancellationToken: cts.Token).AsTask();
        w.Dispose();
        var e = await Completes(t);
        await cts.CancelAsync();
        Assert.False(l.TryEnterWriter(out _));
        e.Dispose();
        Assert.True(l.TryEnterWriter(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task DisposeFaultsAllPendingWaiters(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var u = Upgradeable(l);
        Read(l);
        var w = l.WaitToEnterWriterAsync(cancellationToken: Ct).AsTask();
        var up = u.WaitToUpgradeAsync(cancellationToken: Ct).AsTask();
        var r = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
        await l.DisposeAsync();
        await Assert.ThrowsAsync<ObjectDisposedException>(() => w.WaitAsync(WaitTimeout, Ct));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => up.WaitAsync(WaitTimeout, Ct));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => r.WaitAsync(WaitTimeout, Ct));
    }

    [Fact]
    public async Task AcquisitionAfterDisposeThrows()
    {
        var l = new AsyncReaderWriterLock();
        var u = Upgradeable(l);
        await l.DisposeAsync();
        Assert.Throws<ObjectDisposedException>(() => l.TryEnterRead(out _));
        Assert.Throws<ObjectDisposedException>(() => l.TryEnterUpgradeable(out _));
        Assert.Throws<ObjectDisposedException>(() => l.TryEnterWriter(out _));
        Assert.Throws<ObjectDisposedException>(() => l.WaitToEnterReadAsync(cancellationToken: Ct));
        Assert.Throws<ObjectDisposedException>(() => l.WaitToEnterUpgradeableAsync(cancellationToken: Ct));
        Assert.Throws<ObjectDisposedException>(() => l.WaitToEnterWriterAsync(cancellationToken: Ct));
        Assert.Throws<ObjectDisposedException>(() => u.TryUpgrade());
        Assert.Throws<ObjectDisposedException>(() => u.WaitToUpgradeAsync(cancellationToken: Ct));
    }

    [Fact]
    public async Task HeldEntriesCanBeReleasedAfterDispose()
    {
        var l = new AsyncReaderWriterLock();
        var r = Read(l);
        var u = Upgradeable(l);
        await l.DisposeAsync();
        r.Dispose();
        u.Dispose();
    }

    [Fact]
    public async Task DisposeAsyncIsIdempotent()
    {
        var l = new AsyncReaderWriterLock();
        await l.DisposeAsync();
        await l.DisposeAsync();
    }

    [ThreadStatic] private static bool releasing;

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task ContinuationsDoNotRunInlineOnReleasingThread(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        var w = Write(l);
        var observed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var waiter = Task.Run(async () =>
        {
            var e = await l.WaitToEnterWriterAsync(cancellationToken: Ct);
            observed.SetResult(releasing);
            e.Dispose();
        }, Ct);
        await Task.Delay(PendingProbe, Ct);
        releasing = true;
        w.Dispose();
        releasing = false;
        Assert.False(await Completes(observed.Task));
        await waiter.WaitAsync(WaitTimeout, Ct);
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task RepeatedContendedWaitsReuseWaitersCorrectly(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        for (var i = 0; i < 200; i++)
        {
            var w = Write(l);
            var r = l.WaitToEnterReadAsync(cancellationToken: Ct).AsTask();
            var t = l.WaitToEnterReadAsync(TimeSpan.FromMilliseconds(1), Ct).AsTask();
            Assert.Null(await Completes(t));
            w.Dispose();
            (await Completes(r)).Dispose();
        }
        Assert.True(l.TryEnterWriter(out _));
    }

    [Theory]
    [MemberData(nameof(Policies))]
    public async Task StressPreservesMutualExclusion(Policy policy)
    {
        var l = new AsyncReaderWriterLock(policy);
        int inRead = 0, inWrite = 0, violations = 0, writes = 0, downgrades = 0;
        var tasks = Enumerable.Range(0, Environment.ProcessorCount * 2).Select(i => Task.Run(async () =>
        {
            for (var k = 0; k < 2000; k++)
            {
                var kind = (i + k) % 5;
                var e = kind switch
                {
                    0 => await l.WaitToEnterWriterAsync(cancellationToken: Ct),
                    1 => await l.WaitToEnterUpgradeableAsync(cancellationToken: Ct),
                    _ => await l.WaitToEnterReadAsync(k % 7 == 0 ? TimeSpan.FromMilliseconds(1) : null, Ct),
                };
                if (e is null)
                    continue;
                if (kind == 1 && k % 2 == 0)
                    await e.WaitToUpgradeAsync(cancellationToken: Ct);
                if (e.IsWriter)
                {
                    if (Interlocked.Increment(ref inWrite) != 1 || Volatile.Read(ref inRead) != 0)
                        Interlocked.Increment(ref violations);
                    Interlocked.Increment(ref writes);
                    await Task.Yield();
                    Interlocked.Decrement(ref inWrite);
                    if (k % 3 == 0)
                    {
                        e.Downgrade();
                        Interlocked.Increment(ref downgrades);
                    }
                }
                if (!e.IsWriter)
                {
                    Interlocked.Increment(ref inRead);
                    if (Volatile.Read(ref inWrite) != 0)
                        Interlocked.Increment(ref violations);
                    await Task.Yield();
                    Interlocked.Decrement(ref inRead);
                }
                e.Dispose();
            }
        }, Ct)).ToArray();
        await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(60), Ct);
        Assert.Equal(0, violations);
        Assert.True(writes > 0);
        Assert.True(downgrades > 0);
        Assert.True(l.TryEnterWriter(out _));
    }
}
