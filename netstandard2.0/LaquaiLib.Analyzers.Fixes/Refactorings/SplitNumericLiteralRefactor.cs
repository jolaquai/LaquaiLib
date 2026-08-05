namespace LaquaiLib.Analyzers.Fixes.Refactorings;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SplitNumericLiteralRefactor)), Shared]
public sealed class SplitNumericLiteralRefactor : LaquaiLibRefactoring
{
    public override async ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, TextSpan span, CancellationToken cancellationToken)
    {
        var token = compilationUnitSyntax.FindToken(span.Start);
        if (!token.IsKind(SyntaxKind.NumericLiteralToken) || token.Parent is not LiteralExpressionSyntax literalExpr || !TryGetIntegralValue(token, out var value) || PopCount(value) < 2)
            return [];

        var format = ParseFormat(token.Text);

        if (token.Parent.FirstAncestorOrSelf<EnumMemberDeclarationSyntax>() is { EqualsValue: { Value.Span: var valueSpan } equalsValue } enumMember
            && valueSpan.Contains(token.Span))
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel.GetDeclaredSymbol(enumMember, cancellationToken) is IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } enumMemberSymbol)
            {
                var flagParts = GetEnumFlagParts(enumMemberSymbol.ContainingType, value, format);
                return [new CodeActionInfo("Split into constituent flags", editor => ReplaceWithChainAsync(editor, equalsValue.Value, flagParts), "SplitEnumFlags")];
            }
        }

        var powerParts = GetPowerOfTwoParts(value, format);
        return [new CodeActionInfo("Split into powers of 2", editor => ReplaceWithChainAsync(editor, literalExpr, powerParts), "SplitNumericLiteralPowersOfTwo")];
    }

    private static ValueTask ReplaceWithChainAsync(DocumentEditor editor, ExpressionSyntax target, List<string> parts)
    {
        var newExpression = SyntaxFactory.ParseExpression(string.Join(" | ", parts)).Formatted;
        editor.ReplaceNode(target, newExpression);
        return ValueTask.CompletedTask;
    }

    private static List<string> GetPowerOfTwoParts(ulong value, (string Prefix, string Suffix, int Base) format)
    {
        var parts = new List<string>();
        for (var bit = 63; bit >= 0; bit--)
        {
            var mask = 1UL << bit;
            if ((value & mask) != 0)
            {
                parts.Add(FormatPart(mask, format));
            }
        }
        return parts;
    }

    private static List<string> GetEnumFlagParts(INamedTypeSymbol enumType, ulong value, (string Prefix, string Suffix, int Base) format)
    {
        var bitToName = new Dictionary<ulong, string>();
        foreach (var member in enumType.GetMembers().OfType<IFieldSymbol>())
        {
            if (!member.HasConstantValue)
            {
                continue;
            }
            var memberValue = ToUInt64(member.ConstantValue);
            // Only single-bit members can serve as unambiguous constituent names
            if (memberValue != 0 && (memberValue & (memberValue - 1)) == 0)
            {
                if (!bitToName.ContainsKey(memberValue))
                {
                    bitToName[memberValue] = member.Name;
                }
            }
        }

        var parts = new List<string>();
        for (var bit = 63; bit >= 0; bit--)
        {
            var mask = 1UL << bit;
            if ((value & mask) == 0)
            {
                continue;
            }
            parts.Add(bitToName.TryGetValue(mask, out var name) ? name : FormatPart(mask, format));
        }
        return parts;
    }

    private static bool TryGetIntegralValue(SyntaxToken token, out ulong value)
    {
        switch (token.Value)
        {
            case sbyte or byte or short or ushort or int or uint or long or ulong:
                value = ToUInt64(token.Value);
                return true;
            default:
                value = 0;
                return false;
        }
    }
    private static ulong ToUInt64(object value) => value switch
    {
        sbyte sb => unchecked((ulong)(byte)sb),
        byte b => b,
        short s => unchecked((ulong)(ushort)s),
        ushort us => us,
        int i => unchecked((ulong)(uint)i),
        uint ui => ui,
        long l => unchecked((ulong)l),
        ulong ul => ul,
        _ => Convert.ToUInt64(value),
    };
    private static int PopCount(ulong value)
    {
        var count = 0;
        while (value != 0)
        {
            value &= value - 1;
            count++;
        }
        return count;
    }

    private static (string Prefix, string Suffix, int Base) ParseFormat(string text)
    {
        var suffixEnd = text.Length;
        while (suffixEnd > 0 && text[suffixEnd - 1] is 'u' or 'U' or 'l' or 'L')
        {
            suffixEnd--;
        }
        var suffix = text.Substring(suffixEnd);
        var body = text.Substring(0, suffixEnd);

        if (body.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return (body.Substring(0, 2), suffix, 16);
        }
        if (body.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            return (body.Substring(0, 2), suffix, 2);
        }
        return ("", suffix, 10);
    }
    private static string FormatPart(ulong part, (string Prefix, string Suffix, int Base) format) => format.Base switch
    {
        16 => format.Prefix + part.ToString("X") + format.Suffix,
        2 => format.Prefix + Convert.ToString(unchecked((long)part), 2) + format.Suffix,
        _ => part.ToString() + format.Suffix,
    };
}
