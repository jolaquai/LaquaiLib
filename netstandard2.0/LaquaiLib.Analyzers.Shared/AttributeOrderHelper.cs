namespace LaquaiLib.Analyzers.Shared;

/// <summary>
/// Shared logic for detecting and rewriting attribute lists that are not ordered alphabetically by attribute type name (namespace and the optional "Attribute" suffix ignored).
/// Used by both <c>AttributeOrderAnalyzer</c> and <c>AttributeOrderFixer</c> so the two stay in lockstep.
/// </summary>
public static class AttributeOrderHelper
{
    /// <summary>
    /// Gets the alphabetization key for <paramref name="attribute"/>: its simple type name, namespace and "Attribute" suffix stripped.
    /// </summary>
    public static string GetKey(AttributeSyntax attribute) => Normalize(GetSimpleName(attribute.Name));

    private static string GetSimpleName(NameSyntax name) => name switch
    {
        SimpleNameSyntax simpleName => simpleName.Identifier.ValueText,
        QualifiedNameSyntax qualifiedName => GetSimpleName(qualifiedName.Right),
        AliasQualifiedNameSyntax aliasQualifiedName => GetSimpleName(aliasQualifiedName.Name),
        _ => name.ToString()
    };
    private static string Normalize(string name)
        => name.Length > 9 && name.EndsWith("Attribute", StringComparison.Ordinal) ? name.Substring(0, name.Length - 9) : name;

    /// <summary>
    /// Splits the attribute lists directly parented by <paramref name="declaration"/> into maximal runs that share the same explicit target (<c>return:</c>, <c>field:</c>, ...; no target counts as its own group).
    /// Only lists within the same run are ever reordered or merged relative to each other.
    /// </summary>
    public static ImmutableArray<ImmutableArray<AttributeListSyntax>> GetTargetGroups(SyntaxNode declaration)
    {
        var groups = ImmutableArray.CreateBuilder<ImmutableArray<AttributeListSyntax>>();
        ImmutableArray<AttributeListSyntax>.Builder current = null;
        string currentTarget = null;

        foreach (var child in declaration.ChildNodes())
        {
            if (child is not AttributeListSyntax list)
                continue;

            var target = list.Target?.Identifier.ValueText ?? "";
            if (current is null || target != currentTarget)
            {
                if (current is { Count: > 0 })
                    groups.Add(current.ToImmutable());
                current = ImmutableArray.CreateBuilder<AttributeListSyntax>();
                currentTarget = target;
            }
            current.Add(list);
        }
        if (current is { Count: > 0 })
            groups.Add(current.ToImmutable());

        return groups.ToImmutable();
    }

    /// <summary>
    /// Gets whether every attribute in <paramref name="group"/>, read in document order across all its lists, is already non-decreasing by <see cref="GetKey"/>.
    /// </summary>
    public static bool IsOrdered(ImmutableArray<AttributeListSyntax> group)
    {
        string previous = null;
        for (var i = 0; i < group.Length; i++)
        {
            var attributes = group[i].Attributes;
            for (var j = 0; j < attributes.Count; j++)
            {
                var key = GetKey(attributes[j]);
                if (previous is not null && string.CompareOrdinal(previous, key) > 0)
                    return false;
                previous = key;
            }
        }
        return true;
    }

    /// <summary>
    /// Gets the span from the start of <paramref name="group"/>'s first list to the end of its last, i.e. the location an unordered group is reported and fixed at.
    /// </summary>
    public static TextSpan GetSpan(ImmutableArray<AttributeListSyntax> group) => TextSpan.FromBounds(group[0].SpanStart, group[group.Length - 1].Span.End);

    /// <summary>
    /// Builds a replacement for <paramref name="group"/> with every attribute ordered alphabetically.
    /// Prefers reordering the existing lists as whole blocks (and, within a block, its own attributes) so that lists which don't need to interleave with another list's attributes keep their own brackets.
    /// Only when no such block permutation reproduces the fully sorted sequence - i.e. two lists' attribute ranges interleave - does this fall back to splitting every attribute into its own single-attribute list.
    /// </summary>
    public static ImmutableArray<AttributeListSyntax> BuildOrdered(ImmutableArray<AttributeListSyntax> group)
    {
        var locallySorted = group.Select(SortWithinList).ToImmutableArray();
        var blockOrdered = locallySorted.OrderBy(static list => GetKey(list.Attributes[0]), StringComparer.Ordinal).ToImmutableArray();

        var candidateKeys = blockOrdered.SelectMany(static list => list.Attributes.Select(GetKey));
        var targetKeys = group.SelectMany(static list => list.Attributes.Select(GetKey)).OrderBy(static key => key, StringComparer.Ordinal);

        var replacement = candidateKeys.SequenceEqual(targetKeys, StringComparer.Ordinal) ? blockOrdered : Split(group);
        return TransplantEdgeTrivia(group, replacement);
    }

    private static AttributeListSyntax SortWithinList(AttributeListSyntax list)
    {
        if (list.Attributes.Count <= 1)
            return list;

        var sortedIndices = Enumerable.Range(0, list.Attributes.Count).OrderBy(i => GetKey(list.Attributes[i]), StringComparer.Ordinal);
        var sortedNodes = sortedIndices.Select(i => list.Attributes[i]);
        return list.WithAttributes(SyntaxFactory.SeparatedList(sortedNodes, list.Attributes.GetSeparators()));
    }

    private static ImmutableArray<AttributeListSyntax> Split(ImmutableArray<AttributeListSyntax> group)
    {
        var target = group[0].Target;
        var sortedAttributes = group.SelectMany(static list => list.Attributes).OrderBy(GetKey, StringComparer.Ordinal);

        var builder = ImmutableArray.CreateBuilder<AttributeListSyntax>();
        foreach (var attribute in sortedAttributes)
            builder.Add(SyntaxFactory.AttributeList(target?.WithoutTrivia(), SyntaxFactory.SingletonSeparatedList(attribute.WithoutTrivia())));
        return builder.ToImmutable();
    }

    /// <summary>
    /// Strips every replacement list's own trivia and reattaches only the original group's outer edges (leading trivia of its first list, trailing trivia of its last) to the new first/last list.
    /// Reordered lists otherwise sit bracket-adjacent - reusing whichever interior trivia they happened to carry (e.g. from a one-attribute-per-line layout) would misplace it once the lists move.
    /// </summary>
    private static ImmutableArray<AttributeListSyntax> TransplantEdgeTrivia(ImmutableArray<AttributeListSyntax> original, ImmutableArray<AttributeListSyntax> replacement)
    {
        if (replacement.IsEmpty)
            return replacement;

        var leading = original[0].GetLeadingTrivia();
        var trailing = original[original.Length - 1].GetTrailingTrivia();

        var builder = ImmutableArray.CreateBuilder<AttributeListSyntax>(replacement.Length);
        for (var i = 0; i < replacement.Length; i++)
        {
            var list = replacement[i].WithoutTrivia();
            if (i == 0)
                list = list.WithLeadingTrivia(leading);
            if (i == replacement.Length - 1)
                list = list.WithTrailingTrivia(trailing);
            builder.Add(list);
        }
        return builder.MoveToImmutable();
    }
}
