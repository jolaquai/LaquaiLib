using System.Collections;

namespace LaquaiLib.Generators.Tests;

internal static class ModelPurityAssertions
{
    private static readonly Type[] _forbiddenTypes =
    [
        typeof(ISymbol),
        typeof(Compilation),
        typeof(SyntaxNode),
        typeof(SemanticModel),
        typeof(SyntaxTree),
        typeof(SyntaxReference),
        typeof(Location),
        typeof(AttributeData),
        typeof(IOperation),
        typeof(GeneratorAttributeSyntaxContext),
    ];

    private const int MaxDepth = 32;
    private const int MaxReported = 25;

    /// <summary>
    /// Fails if any Roslyn compiler object is reachable from the models flowing into <c>SourceOutput</c> or out of any of <paramref name="namedSteps"/>.
    /// </summary>
    /// <remarks>
    /// Only <c>SourceOutput</c> inputs and explicitly named steps are walked. Roslyn's own built-in steps legitimately hold a
    /// <see cref="Compilation"/> and would be guaranteed false positives.
    /// </remarks>
    public static void AssertNoRoslynObjectsInModel(GeneratorRunResult result, params string[] namedSteps)
    {
        Assert.Null(result.Exception);

        var offenders = new List<string>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var walked = false;

        if (result.TrackedOutputSteps.TryGetValue(GeneratorTestHost.SourceOutputStepName, out var outputSteps))
            foreach (var step in outputSteps)
                foreach (var (source, outputIndex) in step.Inputs)
                {
                    var value = source.Outputs[outputIndex].Value;
                    if (value is not null)
                    {
                        walked = true;
                        Walk(value, value.GetType().Name, visited, offenders, 0);
                    }
                }

        foreach (var name in namedSteps)
        {
            // a rename would otherwise silently turn this whole assertion into a no-op
            Assert.True(result.TrackedSteps.ContainsKey(name), $"Tracked step '{name}' was not found. Known steps: {string.Join(", ", result.TrackedSteps.Keys)}");

            foreach (var step in result.TrackedSteps[name])
                foreach (var (value, _) in step.Outputs)
                    if (value is not null)
                    {
                        walked = true;
                        Walk(value, $"{name}:{value.GetType().Name}", visited, offenders, 0);
                    }
        }

        Assert.True(walked, "No model values were walked at all, so this assertion proved nothing. Check that the generator actually produced output.");

        if (offenders.Count == 0)
            return;

        var sb = new StringBuilder();
        sb.AppendLine("Roslyn compiler objects reachable from generator model(s):");
        foreach (var offender in offenders.Take(MaxReported))
            sb.Append("  ").AppendLine(offender);
        if (offenders.Count > MaxReported)
            sb.Append("  ... and ").Append(offenders.Count - MaxReported).AppendLine(" more");

        Assert.Fail(sb.ToString());
    }

    private static void Walk(object instance, string path, HashSet<object> visited, List<string> offenders, int depth)
    {
        if (instance is null || depth > MaxDepth)
            return;

        var type = instance.GetType();
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
            || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(Guid))
            return;

        // boxing makes reference identity meaningless for value types, so only dedupe reference types
        if (!type.IsValueType && !visited.Add(instance))
            return;

        foreach (var forbidden in _forbiddenTypes)
            if (forbidden.IsInstanceOfType(instance))
            {
                offenders.Add($"{path} ({type.FullName}) is assignable to {forbidden.FullName}");
                // do not descend: a single ISymbol reaches the entire compilation graph
                return;
            }

        // fields, not properties: properties can throw or lazily materialise Roslyn objects
        for (var t = type; t is not null && t != typeof(object); t = t.BaseType)
        {
            FieldInfo[] fields;
            try
            {
                fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            }
            catch
            {
                continue;
            }

            foreach (var field in fields)
            {
                object fieldValue;
                try
                {
                    fieldValue = field.GetValue(instance);
                }
                catch
                {
                    continue;
                }
                Walk(fieldValue, $"{path}.{field.Name}", visited, offenders, depth + 1);
            }
        }

        if (instance is IEnumerable enumerable)
            try
            {
                var i = 0;
                foreach (var element in enumerable)
                    Walk(element, $"{path}[{i++}]", visited, offenders, depth + 1);
            }
            catch
            {
            }
    }
}
