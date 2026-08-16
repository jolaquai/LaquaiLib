namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

public class ErasureFixture
{
    // Private nested reference type; can't be named from the proxy assembly, only erased to object.
    private sealed class Secret
    {
        public int Value;
        public Secret(int value) => Value = value;
    }

    private Secret _secretProp = new Secret(-1);

    private Secret MakeSecret(int value) => new Secret(value);
    private int ReadSecret(Secret s) => s.Value;

    private Secret SecretProperty
    {
        get => _secretProp;
        set => _secretProp = value;
    }

    private static Secret CreateSecret(int v) => new Secret(v);
}
