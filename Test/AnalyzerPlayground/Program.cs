namespace AnalyzerPlayground;

internal static class Program
{
    public static void Main(string[] args)
    {
        var cloneable = new Cloneable();
        _ = (Cloneable)cloneable.Clone();
        _ = cloneable.Clone() as Cloneable;
    }

    public class Cloneable : ICloneable
    {
        public object Clone() => new Cloneable();
    }
}
