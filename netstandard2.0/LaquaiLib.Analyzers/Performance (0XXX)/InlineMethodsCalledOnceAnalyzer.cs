using System.Collections.Concurrent;

namespace LaquaiLib.Analyzers.Performance__0XXX_;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InlineMethodsCalledOnceAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Descriptor { get; } = new(
        id: "LAQ0004",
        title: "Inline methods called only once",
        messageFormat: "Inline to methods called only once to improve performance",
        description: "Methods called only once are usually not worth the method call overhead, no matter their size, so they should be inlined.",
        category: AnalyzerCategories.Performance,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        // Track method declarations and their references
        var methodDeclarations = new ConcurrentDictionary<IMethodSymbol, Location>(SymbolEqualityComparer.Default);
        var methodReferences = new ConcurrentDictionary<IMethodSymbol, ConcurrentDictionary<Location, bool>>(SymbolEqualityComparer.Default);

        // Register for method declarations
        context.RegisterSyntaxNodeAction(syntaxContext =>
        {
            if (syntaxContext.Node is MethodDeclarationSyntax methodDeclaration)
            {
                var methodSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(methodDeclaration);
                if (methodSymbol != null && !ShouldSkipMethod(methodSymbol))
                {
                    methodDeclarations.TryAdd(methodSymbol, methodDeclaration.Identifier.GetLocation());
                }
            }
        }, SyntaxKind.MethodDeclaration);

        // Register for method invocations
        context.RegisterSyntaxNodeAction(syntaxContext =>
        {
            if (syntaxContext.Node is InvocationExpressionSyntax invocation)
            {
                if (syntaxContext.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol)
                {
                    methodReferences.GetOrAdd(methodSymbol, new ConcurrentDictionary<Location, bool>()).TryAdd(invocation.GetLocation(), true);
                }
            }
        }, SyntaxKind.InvocationExpression);

        // Register for completion of compilation to analyze results
        context.RegisterCompilationEndAction(compilationContext =>
        {
            foreach (var methodDecl in methodDeclarations)
            {
                if (methodReferences.TryGetValue(methodDecl.Key, out var references))
                {
                    if (references.Count == 1)
                    {
                        var hasAttribute = InlineArrowExpressionMethodsAnalyzer.HasAggressiveInliningAttribute(methodDecl.Key);
                        if (hasAttribute)
                        {
                            continue;
                        }

                        var diagnostic = Diagnostic.Create(Descriptor, methodDecl.Value);
                        compilationContext.ReportDiagnostic(diagnostic);
                    }
                }
            }
        });
    }

    private static Location GetMethodNameLocation(InvocationExpressionSyntax invocationExpressionSyntax)
    {
        if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            // For expressions like obj.Method()
            return memberAccess.Name.GetLocation();
        }
        else if (invocationExpressionSyntax.Expression is MemberBindingExpressionSyntax memberBinding)
        {
            // For expressions like obj?.Method()
            return memberBinding.Name.GetLocation();
        }
        else if (invocationExpressionSyntax.Expression is IdentifierNameSyntax identifier)
        {
            // For simple method calls like Method()
            return identifier.GetLocation();
        }
        else
        {
            // Fallback to the entire expression location if we can't identify the method name specifically
            return invocationExpressionSyntax.Expression.GetLocation();
        }
    }

    private bool ShouldSkipMethod(IMethodSymbol methodSymbol)
    {
        // Skip special methods
        if (methodSymbol.MethodKind is not MethodKind.Ordinary)
        {
            return true;
        }

        // Skip public/protected methods (might be used externally)
        if (methodSymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedAndInternal)
        {
            return true;
        }

        // Skip test methods
        foreach (var attribute in methodSymbol.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass != null && (attributeClass.Name.Contains("Test") || attributeClass.Name.Contains("Fact")))
            {
                return true;
            }
        }

        return false;
    }
}