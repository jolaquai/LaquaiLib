using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace LaquaiLib.UnitTests.UnsafeUtils.Accessors;

/// <summary>
/// Reflects over every <see cref="UnsafeAccessorAttribute"/>-decorated member in <c>LaquaiLib</c> and invokes it against
/// a real receiver, so a runtime that renames or removes a private BCL member fails a test instead of failing silently
/// at whatever call site happens to touch the accessor first. Deliberately hardcodes nothing about the accessor types
/// or member names themselves - only the target BCL types need factories - so a newly added accessor is picked up
/// automatically and either passes or fails loudly here without this file needing an edit.
/// </summary>
public sealed class AccessorCanaryTests
{
    private static readonly Dictionary<Type, Func<object>> ReceiverFactories = new()
    {
        [typeof(List<int>)] = static () => new List<int> { 1, 2, 3 },
        [typeof(Queue<int>)] = static () => new Queue<int>([1, 2, 3]),
        [typeof(Stack<int>)] = static () => new Stack<int>([1, 2, 3]),
        [typeof(MemoryStream)] = static () => new MemoryStream(16),
        [typeof(CompositeFormat)] = static () => CompositeFormat.Parse("{0} literal"),
        [typeof(Match)] = static () => Regex.Match("abc", "b"),
        [typeof(Capture)] = static () => (Capture)Regex.Match("abc", "b"),
    };

    private static Assembly LaquaiLibAssembly => typeof(LaquaiLib.Extensions.ArrayExtensions).Assembly;

    private static IEnumerable<MethodInfo> EnumerateAccessors()
    {
        foreach (var type in LaquaiLibAssembly.GetTypes())
        {
            if (type.Namespace != "LaquaiLib.UnsafeUtils.Accessors")
            {
                continue;
            }
            var closed = type.IsGenericTypeDefinition ? type.MakeGenericType(typeof(int)) : type;
            foreach (var method in closed.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.IsDefined(typeof(UnsafeAccessorAttribute), false))
                {
                    yield return method;
                }
            }
        }
    }

    // Identifies rows by (declaring type name, member name) rather than by MethodInfo since xUnit needs the
    // theory data to be serializable/displayable; the test method re-resolves the MethodInfo from these two strings.
    public static TheoryData<string, string> Accessors()
    {
        var data = new TheoryData<string, string>();
        foreach (var method in EnumerateAccessors())
        {
            data.Add(method.DeclaringType.Name, method.Name);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(Accessors))]
    public void AccessorBindsAndInvokesAgainstRealReceiver(string declaringTypeName, string memberName)
    {
        var method = EnumerateAccessors().Single(m => m.DeclaringType.Name == declaringTypeName && m.Name == memberName);

        var kind = method.GetCustomAttribute<UnsafeAccessorAttribute>().Kind;
        Assert.True(kind is UnsafeAccessorKind.Field or UnsafeAccessorKind.Method,
            $"{declaringTypeName}.{memberName} uses {kind}, which this canary does not yet know how to drive - add support instead of ignoring it.");

        var targetType = method.GetParameters()[0].ParameterType;
        Assert.True(ReceiverFactories.TryGetValue(targetType, out var factory),
            $"No receiver factory registered for target type '{targetType}' (required by {declaringTypeName}.{memberName}). Register one in {nameof(AccessorCanaryTests)}.{nameof(ReceiverFactories)}.");

        var receiver = factory();
        var thrown = Record.Exception(() => method.Invoke(null, [receiver]));
        if (thrown is TargetInvocationException tie)
        {
            thrown = tie.InnerException;
        }
        Assert.True(thrown is null, $"{declaringTypeName}.{memberName} threw against a real {targetType.Name}: {thrown}");
    }
}
