namespace LaquaiLib.Generators.Tests;

internal static class CapturingLambdaAssertions
{
    public static void AssertNoCapturingLambdas<TGenerator>() => AssertNoCapturingLambdas(typeof(TGenerator));

    /// <summary>
    /// Fails if <c>Initialize</c> contains a capturing lambda.
    /// </summary>
    /// <remarks>
    /// A non-capturing lambda is cached into the singleton <c>&lt;&gt;c</c>; a capturing one allocates a display
    /// class per call and can smuggle a <see cref="Compilation"/> or <see cref="ISymbol"/> into the pipeline graph.
    /// </remarks>
    public static void AssertNoCapturingLambdas(Type generatorType)
    {
        var offenders = new List<string>();

        foreach (var nested in generatorType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            var name = nested.Name;
            if (!name.Contains("c__DisplayClass") || name == "<>c")
            {
                continue;
            }

            if (name.Contains("Initialize"))
            {
                offenders.Add(name);
                continue;
            }

            // display class name carries no method name: inspect its methods instead
            var hasInitializeMethod = nested
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(m => m.Name.Contains("<Initialize>b__"));

            if (hasInitializeMethod)
            {
                offenders.Add(name);
            }
        }

        if (offenders.Count == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"{generatorType.FullName} has capturing lambda(s) in Initialize:");
        foreach (var offender in offenders)
        {
            sb.AppendLine(offender);
        }
        sb.AppendLine("A capturing lambda in Initialize allocates a display class per invocation and can pin Roslyn objects into the incremental pipeline.");

        Assert.Fail(sb.ToString());
    }
}
