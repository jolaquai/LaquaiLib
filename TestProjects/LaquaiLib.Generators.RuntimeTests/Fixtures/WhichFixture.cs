namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

public class WhichFixture
{
    // A and B both erase to 'object', so the two Which overloads collapse; the generator must keep only the first.
    private sealed class A;
    private sealed class B;

    private string Which(A a) => "A";
    private string Which(B b) => "B";

    private static A MakeA() => new A();
}
