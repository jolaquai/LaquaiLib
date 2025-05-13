namespace AnalyzerPlayground;

internal static class Program
{
    public static void Main(string[] args)
    {
    }
    public class TestClass
    {
        public static int TestMethod(int x) => x + 1;
        public int this[int x] => x + 1;
        public int TestProperty => 1;
        public int TestProperty2
        {
            get => 1;
            set => throw new NotSupportedException();
        }

        public static implicit operator int(TestClass testClass) => 1;
        public static explicit operator TestClass(int x) => new TestClass { TestProperty2 = x };
        public static bool operator <(TestClass t, int x) => t.TestProperty < x;
        public static bool operator >(TestClass t, int x) => t.TestProperty > x;

        public static void Test()
        {
            static int LocalFunc(int x) => x + 1;
        }
        private static void InlinableMethod()
        {
            TestMethod(5);
        }
        private static void InlinableMethod2()
        {
            InlinableMethod();
        }
    }
}
