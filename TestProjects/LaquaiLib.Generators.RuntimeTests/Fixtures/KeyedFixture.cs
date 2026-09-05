namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

public class KeyedFixture
{
    // Private nested reference type used as a constructor parameter; erased to object in the proxy's Create overload.
    private sealed class Key
    {
        public int Value;
        public Key(int value)
        {
            Value = value;
        }
    }

    public int KeyValue { get; }

    private KeyedFixture(Key key)
    {
        KeyValue = key.Value;
    }

    private static Key MakeKey(int v) => new Key(v);
}
