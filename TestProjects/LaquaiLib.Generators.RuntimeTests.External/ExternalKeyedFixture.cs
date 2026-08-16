namespace LaquaiLib.Generators.RuntimeTests.External;

// Internal top-level type with no InternalsVisibleTo grant: pins whole-type erasure through a constructor
// parameter that is itself an effectively-inaccessible nested type (same mechanism as ExternalErasureFixture.Hidden).
internal sealed class ExternalKeyedFixture
{
    public sealed class Key
    {
        public int Value;
        public Key(int value) => Value = value;
    }

    public int KeyValue { get; }

    public ExternalKeyedFixture(Key key) => KeyValue = key.Value;

    public static Key MakeKey(int v) => new Key(v);
}
