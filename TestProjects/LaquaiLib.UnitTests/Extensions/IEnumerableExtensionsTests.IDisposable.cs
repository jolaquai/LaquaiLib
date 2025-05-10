using LaquaiLib.Extensions;

namespace LaquaiLib.UnitTests.Extensions;

public class IEnumerableIDisposableExtensionsTests
{
    [Fact]
    public void DisposeCallsDisposeOnAllItems()
    {
        var mock1 = new MockDisposable();
        var mock2 = new MockDisposable();
        var mock3 = new MockDisposable();

        var disposables = new[] { mock1, mock2, mock3 };
        disposables.Dispose();

        Assert.True(mock1.IsDisposed);
        Assert.True(mock2.IsDisposed);
        Assert.True(mock3.IsDisposed);
    }

    [Fact]
    public void DisposeHandlesEmptyCollection()
    {
        var emptyCollection = Array.Empty<IDisposable>();
        emptyCollection.Dispose();

        Assert.True(true); // No exception thrown
    }

    [Fact]
    public void DisposeContinuesAfterIndividualExceptions()
    {
        var mock1 = new MockDisposable();
        var throwing = new ThrowingDisposable("Test exception");
        var mock2 = new MockDisposable();

        var disposables = new IDisposable[] { mock1, throwing, mock2 };

        var exception = Assert.Throws<AggregateException>(disposables.Dispose);

        Assert.Single(exception.InnerExceptions);
        Assert.Equal("Test exception", exception.InnerExceptions[0].Message);
        Assert.True(mock1.IsDisposed);
        Assert.True(mock2.IsDisposed);
    }

    [Fact]
    public void DisposeCombinesMultipleExceptions()
    {
        var throwing1 = new ThrowingDisposable("First exception");
        var throwing2 = new ThrowingDisposable("Second exception");
        var throwing3 = new ThrowingDisposable("Third exception");

        var disposables = new IDisposable[] { throwing1, throwing2, throwing3 };

        var exception = Assert.Throws<AggregateException>(disposables.Dispose);

        Assert.Equal(3, exception.InnerExceptions.Count);
        Assert.Equal("First exception", exception.InnerExceptions[0].Message);
        Assert.Equal("Second exception", exception.InnerExceptions[1].Message);
        Assert.Equal("Third exception", exception.InnerExceptions[2].Message);
    }

    [Fact]
    public void DisposeWorksWithCustomCollection()
    {
        var mock1 = new MockDisposable();
        var mock2 = new MockDisposable();

        var disposables = new CustomDisposableCollection([mock1, mock2]);
        IEnumerableExtensions.Dispose(disposables);

        Assert.True(mock1.IsDisposed);
        Assert.True(mock2.IsDisposed);
        Assert.False(disposables.IsDisposed); // The collection itself shouldn't be disposed
    }

    [Fact]
    public void DisposeDoesNotDisposeTheCollectionItself()
    {
        var disposableCollection = new DisposableList<IDisposable>
        {
            new MockDisposable(),
            new MockDisposable()
        };

        IEnumerableExtensions.Dispose(disposableCollection);

        // Verify all items are disposed
        Assert.True(disposableCollection.All(static d => ((MockDisposable)d).IsDisposed));

        // The collection itself should not be disposed by the extension method
        Assert.False(disposableCollection.IsCollectionDisposed);
    }

    private class MockDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }

    private class ThrowingDisposable(string message) : IDisposable
    {
        private readonly string _message = message;

        public void Dispose() => throw new InvalidOperationException(_message);
    }

    private class CustomDisposableCollection(IDisposable[] disposables) : IEnumerable<IDisposable>, IDisposable
    {
        private readonly IDisposable[] _disposables = disposables;

        public bool IsDisposed { get; private set; }

        public IEnumerator<IDisposable> GetEnumerator() => ((IEnumerable<IDisposable>)_disposables).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _disposables.GetEnumerator();

        public void Dispose() => IsDisposed = true;
    }

    private class DisposableList<T> : List<T>, IDisposable
    {
        public bool IsCollectionDisposed { get; private set; }

        public void Dispose() => IsCollectionDisposed = true;
    }
}
