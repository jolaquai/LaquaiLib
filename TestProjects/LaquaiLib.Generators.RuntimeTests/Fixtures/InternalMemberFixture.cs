namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

// Non-nested, same-assembly internal type appearing in a public fixture's private member signature.
internal class InternalResultType
{
    public int Payload;
    public InternalResultType(int payload) => Payload = payload;
}

public class InternalMemberFixture
{
    private InternalResultType GetInternal() => new InternalResultType(88);
}
