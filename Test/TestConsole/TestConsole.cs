namespace TestConsole;

/// <summary>
/// [Entry point] Represents a test console application for <see cref="LaquaiLib"/>.
/// </summary>
public static partial class TestConsole
{
    [STAThread]
    private static void Main()
    {
        // FirstChanceExceptionHandlers.RegisterAll();

        Thread.CurrentThread.Name = "[MAIN]";
        using var scope = TestCore.TestCore.GetScope().GetAwaiter().GetResult();
        ActualMain(scope.ServiceProvider).GetAwaiter().GetResult();
        // Debugger.Break();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] private static void cw(this object obj) => Console.WriteLine(obj);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void cw<T>(this IEnumerable<T> enumerable)
        => Console.WriteLine($"<{typeof(T).Namespace + '.' + typeof(T).Name}>[{string.Join(", ", enumerable)}]");
    public static async Task ActualMain(IServiceProvider serviceProvider)
    {
        for (var i = 0; i < 10; i++)
        {
            Console.WriteLine($"Hello World {i}");
        }
        for (var i = 0; i < 10; i++)
        {
            Console.WriteLine($"Hello World {i}");
            await Task.Delay(1000);
        }
    }
}

