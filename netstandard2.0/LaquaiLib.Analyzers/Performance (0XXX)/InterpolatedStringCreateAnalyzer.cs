using LaquaiLib.Analyzers.Shared;

namespace LaquaiLib.Analyzers.Performance__0XXX_;

/// <summary>
/// Flags a non-constant interpolated string converted to <see cref="string"/> that could format into a stack buffer through <c>string.Create(IFormatProvider, Span&lt;char&gt;, ref DefaultInterpolatedStringHandler)</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InterpolatedStringCreateAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0009",
        title: "Format interpolated strings into a stack buffer",
        messageFormat: "Use string.Create with a pre-sized buffer for this interpolated string",
        category: AnalyzerCategories.Performance,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static compilationStartContext =>
        {
            var compilation = compilationStartContext.Compilation;
            if (StringCreateHelper.GetStringCreate(compilation) is null)
                return;

            var overloads = ConcatOverloads.Create(compilation);
            compilationStartContext.RegisterSyntaxNodeAction(nodeContext => AnalyzeNode(nodeContext, overloads), SyntaxKind.InterpolatedStringExpression);
        });
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context, ConcatOverloads overloads)
    {
        var interpolated = Unsafe.As<InterpolatedStringExpressionSyntax>(context.Node);
        var semanticModel = context.SemanticModel;
        var cancellationToken = context.CancellationToken;

        if (StringConcatenationHelper.IsNestedInConcatenation(interpolated, semanticModel, cancellationToken))
            return;
        // LAQ0007 already reports it, and its rewrite is cheaper
        if (StringConcatenationHelper.Classify(interpolated, semanticModel, overloads, cancellationToken, out _) is not ConcatRewrite.None)
            return;

        var plan = StringCreateHelper.GetPlan(interpolated, semanticModel, cancellationToken);
        if (plan.IsEmpty)
            return;

        var properties = ImmutableDictionary<string, string>.Empty;
        if (plan.StackLength != 0)
            properties = properties.Add(StringCreateHelper.BufferLengthKey, plan.StackLength.ToString());
        if (plan.ExactLength != 0)
            properties = properties.Add(StringCreateHelper.ExactLengthKey, plan.ExactLength.ToString());
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, interpolated.GetLocation(), properties));
    }
}
