namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

public class SkippedMembersFixture
{
    // None of these three can be proxied even with IncludeInaccessible: field/event accessors are 'ref T' returns,
    // and GetHiddenStruct's result is an inaccessible value type - the runtime refuses [UnsafeAccessorType] on both.
    private sealed class Hidden { public int X; }
    private struct HiddenStruct { public int X; }
    private delegate void HiddenDelegate();

    private Hidden _hiddenField = new Hidden();
    private event HiddenDelegate HiddenEvent;

    private HiddenStruct GetHiddenStruct() => new HiddenStruct();

    internal void RaiseHiddenEvent() => HiddenEvent?.Invoke();

    private int Marker() => 123;
}
