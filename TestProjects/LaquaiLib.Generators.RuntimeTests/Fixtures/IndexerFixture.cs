namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

public class IndexerFixture
{
    private readonly int[] _ints = new int[8];
    private readonly Dictionary<string, int> _strings = new();

    // C# requires every indexer in a type to share the same real (IndexerName) name, so both are
    // renamed here to "Item2" - that alone is enough to prove the generator uses the real accessor
    // name (get_Item2/set_Item2) rather than hardcoding "get_Item"/"set_Item".
    [System.Runtime.CompilerServices.IndexerName("Item2")]
    private int this[int i]
    {
        get => _ints[i];
        set => _ints[i] = value;
    }

    [System.Runtime.CompilerServices.IndexerName("Item2")]
    private int this[string key]
    {
        get => _strings.TryGetValue(key, out var v) ? v : 0;
        set => _strings[key] = value;
    }
}
