using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LaquaiLib.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Test : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _descriptor = new(
        id: "LAQ0001",
        title: "Test",
        messageFormat: "Message",
        category: "Category",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [_descriptor];

    public override void Initialize(AnalysisContext context)
    {

    }
}
