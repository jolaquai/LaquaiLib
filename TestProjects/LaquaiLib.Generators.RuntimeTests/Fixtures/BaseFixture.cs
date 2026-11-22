namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

public class BaseFixture
{
    private readonly string _baseReadonlyField = "base-readonly";
    private int _baseField = 11;

    // Not redeclared in DerivedFixture; proves the inherited-member walk reaches base-only members.
    private int BaseOnlyMethod() => 99;

    protected virtual string VirtualGreeting() => "base-greeting";

    public BaseFixture() { }
}
