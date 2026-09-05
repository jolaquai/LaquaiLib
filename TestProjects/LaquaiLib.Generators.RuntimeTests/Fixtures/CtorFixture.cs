namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

public class CtorFixture
{
    public int Value { get; }
    public string Tag { get; }

    private CtorFixture(int value)
    {
        Value = value;
        Tag = "private";
    }

    public CtorFixture(string tag)
    {
        Value = -1;
        Tag = tag;
    }
}
