namespace LaquaiLib.Generators.RuntimeTests.External;

// Internal top-level type with no InternalsVisibleTo grant: the class itself is erased to object cross-assembly
// (a different erasure path than a nested type, since IsAccessible checks the type's OWN declared accessibility here
// rather than walking a container chain). typeof() can't name this type from the proxy's compilation (CS0122), so
// its proxy uses the assembly-qualified-name string overload of [FullAccessProxy] instead.
internal sealed class ExternalErasureFixture
{
    // Declared public: a public member exposing this (below) must satisfy CS0050/CS0051, which only checks the
    // type's OWN declared accessibility, not its effective one. Hidden's EFFECTIVE accessibility is still bounded
    // by ExternalErasureFixture being internal, so it is just as unnameable from another assembly.
    public sealed class Hidden
    {
        public int Value;
        public Hidden(int value)
        {
            Value = value;
        }
    }

    private Hidden _hiddenProp = new Hidden(-1);

    public Hidden MakeHidden(int value) => new Hidden(value);
    public int ReadHidden(Hidden h) => h.Value;

    public Hidden HiddenProperty
    {
        get => _hiddenProp;
        set => _hiddenProp = value;
    }

    public static Hidden CreateHidden(int v) => new Hidden(v);

    public int RoundTrip(int v) => ReadHidden(MakeHidden(v));
}
