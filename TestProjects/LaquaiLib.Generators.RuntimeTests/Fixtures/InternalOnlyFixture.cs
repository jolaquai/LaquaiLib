namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

// Same-assembly internal type; proxyable at all only since IsSymbolAccessibleWithin honours internal/InternalsVisibleTo.
internal sealed class InternalOnlyFixture
{
    private int _value = 55;

    private int GetValue() => _value;
}
