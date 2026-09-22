namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

public class RefFixture
{
    private int _value = 5;

    // Ref-returning property; the proxy's generated getter must forward the real reference, not a copy.
    private ref int ValueRef => ref _value;
}
