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
    /// Fails if any Roslyn compiler object is reachable from the models flowing into <c>SourceOutput</c>.
    /// </summary>
    /// <remarks>
    /// Only <c>SourceOutput</c> inputs are walked. Roslyn's own built-in steps legitimately hold a
    /// <see cref="Compilation"/> and would be guaranteed false positives.
    /// </remarks>
    public static void AssertNoRoslynObjectsInModel(GeneratorRunResult result)
    {
        if (!result.TrackedOutputSteps.TryGetValue(GeneratorTestHost.SourceOutputStepName, out var steps))
        {
            return;
        }

        var offenders = new List<string>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        foreach (var step in steps)
        {
            foreach (var (source, outputIndex) in step.Inputs)
            {
                var value = source.Outputs[outputIndex].Value;
                if (value is not null)
                {
                    Walk(value, value.GetType().Name, visited, offenders, 0);
                }
            }
        }

        if (offenders.Count == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Roslyn compiler objects reachable from model(s) flowing into SourceOutput:");
        foreach (var offender in offenders.Take(MaxReported))
        {
            sb.Append("  ").AppendLine(offender);
        }
        if (offenders.Count > MaxReported)
        {
            sb.Append("  ... and ").Append(offenders.Count - MaxReported).AppendLine(" more");
        }

        Assert.Fail(sb.ToString());
    }

    private static void Walk(object instance, string path, HashSet<object> visited, List<string> offenders, int depth)
    {
        if (instance is null || depth > MaxDepth)
        {
            return;
        }

        var type = instance.GetType();
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
            || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(Guid))
        {
            return;
        }

        // boxing makes reference identity meaningless for value types, so only dedupe reference types
        if (!type.IsValueType && !visited.Add(instance))
        {
            return;
        }

        foreach (var forbidden in _forbiddenTypes)
        {
            if (forbidden.IsInstanceOfType(instance))
            {
                offenders.Add($"{path} ({type.FullName}) is assignable to {forbidden.FullName}");
                // do not descend: a single ISymbol reaches the entire compilation graph
                return;
            }
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
        {
            try
            {
                var i = 0;
                foreach (var element in enumerable)
                {
                    Walk(element, $"{path}[{i++}]", visited, offenders, depth + 1);
                }
            }
            catch
            {
            }
        }
    }
}
