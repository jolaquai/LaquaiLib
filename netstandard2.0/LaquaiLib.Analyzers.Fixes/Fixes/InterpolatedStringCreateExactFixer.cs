using System.Text;

namespace LaquaiLib.Analyzers.Fixes.Fixes;

/// <summary>
/// Rewrites the interpolated string LAQ0009 reports on into a <c>string.Create&lt;TState&gt;</c> call that writes every part straight into the exactly-sized result.
/// Only registered if the final length and every part's output are known with certainty.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InterpolatedStringCreateExactFixer)), Shared]
public sealed class InterpolatedStringCreateExactFixer() : LaquaiLibFixer(["LAQ0009"])
{
    public override async ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        if (!diagnostic.Properties.ContainsKey(StringCreateHelper.ExactLengthKey)
            || compilationUnitSyntax.FindNode(diagnostic.Location.SourceSpan) is not InterpolatedStringExpressionSyntax interpolated)
            return [];

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (!StringCreateHelper.TryGetExactSegments(interpolated, semanticModel, cancellationToken, out var segments, out var length))
            return [];

        var replacement = Build(interpolated, segments, length, semanticModel);
        return [new CodeActionInfo($"Change to 'string.Create' over an exact {length}-char string", editor =>
        {
            editor.ReplaceNode(interpolated, replacement);
            return ValueTask.CompletedTask;
        }, "ChangeToExactStringCreate")];
    }

    private static ExpressionSyntax Build(InterpolatedStringExpressionSyntax interpolated, ImmutableArray<ExactSegment> segments, int length, SemanticModel semanticModel)
    {
        var position = interpolated.SpanStart;
        var spanName = GetFreeName(semanticModel, position, "span");
        var stateName = GetFreeName(semanticModel, position, "state");

        var states = new List<string>();
        for (var i = 0; i < segments.Length; i++)
            if (segments[i].Expression is { } expression)
                states.Add(expression.WithoutTrivia().ToString());

        var body = new StringBuilder();
        var offset = 0;
        var holeIndex = 0;
        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            if (segment.Kind == ExactSegmentKind.Text)
            {
                AppendText(body, spanName, offset, segment.Text);
                offset += segment.Length;
                continue;
            }

            var state = states.Count == 1 ? stateName : $"{stateName}.Item{++holeIndex}";
            AppendFill(body, spanName, offset, segment.PadLeft);
            offset += segment.PadLeft;
            if (segment.Kind == ExactSegmentKind.Char)
                body.Append(spanName).Append('[').Append(offset).Append("] = ").Append(state).Append(";\r\n");
            else
            {
                body.Append(state).Append(".TryFormat(").Append(spanName).Append(".Slice(").Append(offset).Append(", ").Append(segment.ContentLength).Append("), out _");
                if (segment.Format is not null)
                    body.Append(", ").Append(SymbolDisplay.FormatLiteral(segment.Format, true));
                body.Append(");\r\n");
            }
            offset += segment.ContentLength;
            AppendFill(body, spanName, offset, segment.PadRight);
            offset += segment.PadRight;
        }

        var stateArgument = states.Count == 1 ? states[0] : $"({string.Join(", ", states)})";
        var line = interpolated.SyntaxTree.GetText().Lines.GetLineFromPosition(interpolated.SpanStart).ToString();
        var indent = line.Substring(0, line.Length - line.TrimStart().Length);
        var indented = new StringBuilder();
        foreach (var statement in body.ToString().Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries))
            indented.Append(indent).Append("    ").Append(statement).Append("\r\n");
        var source = $"string.Create({length}, {stateArgument}, static ({spanName}, {stateName}) =>\r\n{indent}{{\r\n{indented}{indent}}})";
        return SyntaxFactory.ParseExpression(source).WithTriviaFrom(interpolated);
    }

    private static void AppendText(StringBuilder body, string spanName, int offset, string text)
    {
        if (text.Length == 1)
        {
            body.Append(spanName).Append('[').Append(offset).Append("] = ").Append(SymbolDisplay.FormatLiteral(text[0], true)).Append(";\r\n");
            return;
        }

        body.Append(SymbolDisplay.FormatLiteral(text, true)).Append(".CopyTo(").Append(spanName);
        if (offset != 0)
            body.Append(".Slice(").Append(offset).Append(')');
        body.Append(");\r\n");
    }

    private static void AppendFill(StringBuilder body, string spanName, int offset, int count)
    {
        if (count == 0)
            return;
        if (count == 1)
            body.Append(spanName).Append('[').Append(offset).Append("] = ' ';\r\n");
        else
            body.Append(spanName).Append(".Slice(").Append(offset).Append(", ").Append(count).Append(").Fill(' ');\r\n");
    }

    private static string GetFreeName(SemanticModel semanticModel, int position, string baseName)
    {
        var taken = new HashSet<string>();
        foreach (var symbol in semanticModel.LookupSymbols(position))
            taken.Add(symbol.Name);
        var name = baseName;
        for (var i = 2; taken.Contains(name); i++)
            name = baseName + i;
        return name;
    }
}
