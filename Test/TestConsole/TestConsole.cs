using System.Numerics;
using System.Reflection;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;

using LaquaiLib.Extensions;
using LaquaiLib.IO;
using LaquaiLib.IO.Streams;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)] private static string FormatExpr(object obj, string expr) => $"{expr}: {FormatSingleObj(obj)}";
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private static string FormatExpr<T>(IEnumerable<T> obj, string expr) => $"{expr}: {FormatEnumerable(obj)}";
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private static string FormatSingleObj<T>(T obj) => $"({typeof(T).GetFriendlyName(false)}){obj}";
    [MethodImpl(MethodImplOptions.AggressiveInlining)] private static string FormatSingleObj<T>(IEnumerable<T> obj) => $"({typeof(T).GetFriendlyName(false)}){FormatEnumerable(obj)}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string FormatEnumerable<T>(this IEnumerable<T> enumerable)
    {
        var typeofParameter = enumerable.GetType();
        return $"({typeofParameter.GetFriendlyName(false)})[{string.Join(", ", enumerable)}]";
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void cw(this object obj, [CallerArgumentExpression(nameof(obj))] string expr = null) => Console.WriteLine(FormatExpr(obj, expr));
    private static void cw<T>(this IEnumerable<T> obj, [CallerArgumentExpression(nameof(obj))] string expr = null) => Console.WriteLine(FormatExpr(obj, expr));

    public static async Task ActualMain(IServiceProvider serviceProvider)
    {
    }
    public static byte[] RevolverBytes(int length)
    {
        var arr = new byte[length];
        for (var i = 0; i < length; i++)
            arr[i] = (byte)(i % 256);
        return arr;
    }

    private static void PrintCpuVectorCapabilities()
    {
        // touch the ISA namespaces so their types are eagerly considered
        _ = typeof(Sse).Assembly;
        _ = typeof(AdvSimd).Assembly;
        _ = typeof(PackedSimd).Assembly;

        const BindingFlags F = BindingFlags.Public | BindingFlags.Static;

        var isaTypes = typeof(object).Assembly.GetTypes()
            .Where(t => t.Namespace is "System.Runtime.Intrinsics.X86" or "System.Runtime.Intrinsics.Arm" or "System.Runtime.Intrinsics.Wasm")
            .Where(t => t.GetProperty("IsSupported", F)?.PropertyType == typeof(bool))
            .OrderBy(t => t.Namespace, StringComparer.Ordinal)
            .ThenBy(t => t.FullName, StringComparer.Ordinal)
            .ToArray();

        Console.WriteLine($"Runtime: {Environment.Version}  OS: {Environment.OSVersion}  Arch: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");

        string ns = null;
        foreach (var t in isaTypes)
        {
            if (t.Namespace != ns)
            { ns = t.Namespace; Console.WriteLine($"\n== {ns} =="); }
            var sup = (bool)t.GetProperty("IsSupported", F).GetValue(null);
            var name = t.FullName[(ns.Length + 1)..].Replace('+', '.');
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = sup ? ConsoleColor.Green : ConsoleColor.DarkGray;
            Console.WriteLine($"  [{(sup ? 'x' : ' ')}] {name}");
            Console.ForegroundColor = prev;
        }

        Console.WriteLine("\n== Cross-platform vector acceleration ==");
        void V(string n, bool s, string extra = "")
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = s ? ConsoleColor.Green : ConsoleColor.DarkGray;
            Console.WriteLine($"  [{(s ? 'x' : ' ')}] {n}{extra}");
            Console.ForegroundColor = prev;
        }
        V("System.Numerics.Vector<T>", Vector.IsHardwareAccelerated, $"  (width = {Vector<byte>.Count * 8} bits)");
        V("Vector64", Vector64.IsHardwareAccelerated);
        V("Vector128", Vector128.IsHardwareAccelerated);
        V("Vector256", Vector256.IsHardwareAccelerated);
        V("Vector512", Vector512.IsHardwareAccelerated);
    }
}
