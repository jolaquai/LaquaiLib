using System.Collections.Concurrent;

namespace LaquaiLib.Analyzers.Performance__0XXX_;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseAllocateUninitializedArrayAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0006",
        title: "Avoid zeroing for large arrays",
        messageFormat: "Use GC.AllocateUninitializedArray for large arrays to avoid zeroing",
        description: "Arrays of unmanaged element types can skip zeroing once their length reaches 2048 / sizeof(T). If initial contents of the array are irrelevant (such as for use as a scratch/copy buffer), use GC.AllocateUninitializedArray to improve performance.",
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
            // Layout walks are shared across every array creation in the compilation; concurrent execution is on, so the map has to tolerate it
            var sizeCache = new ConcurrentDictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
            compilationStartContext.RegisterSyntaxNodeAction(nodeContext => AnalyzeNode(nodeContext, sizeCache), SyntaxKind.ArrayCreationExpression);
        });
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context, ConcurrentDictionary<ITypeSymbol, int> sizeCache)
    {
        var arrayCreationExpressionSyntax = Unsafe.As<ArrayCreationExpressionSyntax>(context.Node);
        var semanticModel = context.SemanticModel;

        var rankSpecifiers = arrayCreationExpressionSyntax.Type.RankSpecifiers;
        // >1 rank specifier is jagged (reference element type), >1 size in one is multi-dimensional; GC.AUA has no overload for either
        if (rankSpecifiers.Count != 1 || rankSpecifiers[0].Sizes.Count != 1)
        {
            return;
        }

        // GC.AUA only takes a length, so an explicit initializer has nothing to rewrite to
        if (arrayCreationExpressionSyntax.Initializer is not null)
        {
            return;
        }

        var elementType = semanticModel.GetTypeInfo(arrayCreationExpressionSyntax.Type.ElementType).Type;
        // GC.AUA hands anything IsReferenceOrContainsReferences<T>() accepts straight back to 'new T[length]', so only unmanaged types gain anything
        if (elementType is not { IsValueType: true, IsUnmanagedType: true }
            // unmanaged, but pointers and function pointers can't be type arguments in the first place
            || elementType.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer
            // an array of a ref struct is CS0611, so there is nothing here to rewrite
            || elementType.IsRefLikeType)
        {
            return;
        }

        var sizeOf = elementType.SizeOf(context.Compilation, sizeCache);
        if (sizeOf <= 0)
        {
            return;
        }

        // Mirrors the runtime's own 'if (length < 2048 / sizeof(T)) return new T[length];', integer division included
        var minimumLength = 2048 / sizeOf;
        if (arrayCreationExpressionSyntax.GetArraySize(semanticModel) is int length)
        {
            if (length <= 0 || length < minimumLength)
            {
                return;
            }
        }
        else if (minimumLength > 0)
        {
            // Nothing to compare a non-constant length against unless the threshold has collapsed to zero
            return;
        }

        // Report the diagnostic
        var diagnostic = Diagnostic.Create(Descriptor, arrayCreationExpressionSyntax.NewKeyword.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
