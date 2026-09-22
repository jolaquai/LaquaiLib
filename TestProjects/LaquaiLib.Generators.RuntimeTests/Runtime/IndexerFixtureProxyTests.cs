namespace LaquaiLib.Generators.RuntimeTests.Runtime;

public class IndexerFixtureProxyTests
{
    [Fact]
    public void StandardIndexerReadWriteRoundTrips()
    {
        var proxy = new IndexerFixtureProxy(new IndexerFixture());
        proxy[3] = 55;
        Assert.Equal(55, proxy[3]);
    }

    [Fact]
    public void RenamedIndexerUsesRealAccessorNameNotHardcodedItem()
    {
        // Both indexers were renamed to "Item2" via [IndexerName]; if the generator hardcoded "get_Item"/"set_Item"
        // this would throw MissingMethodException at first call instead of round-tripping.
        var proxy = new IndexerFixtureProxy(new IndexerFixture());
        proxy["key"] = 88;
        Assert.Equal(88, proxy["key"]);
    }
}
