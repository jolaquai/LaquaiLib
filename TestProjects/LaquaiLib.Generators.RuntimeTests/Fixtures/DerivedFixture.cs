namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

public class DerivedFixture : BaseFixture, IGreeter
{
    private static int s_staticField = 500;
    private int _field = 3;
    private readonly int _readonlyField = 77;

    private int Add(int a, int b) => a + b;
    private int Add(int a, int b, int c) => a + b + c;

    private static int StaticAdd(int a, int b) => a + b;

    private int PropGetSet { get; set; } = 5;
    private int PropGetOnly => 123;
    private int PropInitOnly { get; init; } = 456;

    private event EventHandler<int> Notified;
    // Internal helper for the test harness to raise the event on the real instance; not meant to be called through the proxy.
    internal void RaiseNotified(int value) => Notified?.Invoke(this, value);

    private bool TryProcess(ref int r, out int o, in int inVal)
    {
        r += inVal;
        o = inVal * 2;
        return inVal > 0;
    }

    // UnsafeAccessor binding is non-virtual; the proxy must target THIS override (not BaseFixture's) to observe "derived-greeting".
    protected override string VirtualGreeting() => "derived-greeting";

    string IGreeter.Greet() => "hello-from-derived";

    public DerivedFixture() { }
}
