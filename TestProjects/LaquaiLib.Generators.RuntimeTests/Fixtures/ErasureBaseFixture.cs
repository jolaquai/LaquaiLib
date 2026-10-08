namespace LaquaiLib.Generators.RuntimeTests.Fixtures;

public class ErasureBaseFixture
{
    // Declared here, not on ErasureDerivedFixture, to pin that [UnsafeAccessor] targets the member's OWN containing type.
    private sealed class BaseSecret
    {
        public int Value = 7;
    }

    private BaseSecret GetBaseSecret() => new BaseSecret();
    private int ReadBaseSecret(BaseSecret s) => s.Value;
}
