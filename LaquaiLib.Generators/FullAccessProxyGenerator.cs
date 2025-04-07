
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LaquaiLib.Generators;

[Generator(LanguageNames.CSharp)]
public class FullAccessProxyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarationSyntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
            static (context, _) =>
            {
                var classDeclarationSyntax = Unsafe.As<ClassDeclarationSyntax>(context.Node);
                var attribute = classDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes)
                    .FirstOrDefault(x => x.Name.ToString().Contains("FullAccessProxy"));
                if (attribute is not null)
                {
                    return classDeclarationSyntax;
                }
                return null;
            }
        ).Where(static m => m is not null);

        context.RegisterSourceOutput(classDeclarationSyntaxProvider, Execute);

        var structDeclarationSyntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is StructDeclarationSyntax { AttributeLists.Count: > 0 },
            static (context, _) =>
            {
                var structDeclarationSyntax = Unsafe.As<StructDeclarationSyntax>(context.Node);
                var attribute = structDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes)
                    .FirstOrDefault(x => x.Name.ToString().Contains("FullAccessProxy"));
                if (attribute is not null)
                {
                    return structDeclarationSyntax;
                }
                return null;
            }
        ).Where(static m => m is not null);

        context.RegisterSourceOutput(classDeclarationSyntaxProvider, Execute);
    }

    private void Execute(SourceProductionContext context, ClassDeclarationSyntax source)
    {
#warning TODO
        // Get the semantic model
        var semanticModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
        
        // Get the class symbol
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null) return;
        
        // Find the FullAccessProxy attribute and its type argument
        var attribute = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString().Contains("FullAccessProxy") == true);
        
        if (attribute == null) return;
        
        // Get the type argument from the attribute (e.g., MemoryStream)
        var typeArg = attribute.AttributeClass?.TypeArguments.FirstOrDefault();
        if (typeArg == null) return;
        
        var proxyClassName = classSymbol.Name;
        var targetTypeName = typeArg.ToDisplayString();
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

        // Generate the proxy class
        var source = GenerateProxyClass(namespaceName, proxyClassName, targetTypeName);
        
        context.AddSource($"{proxyClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
    }
    private void Execute(SourceProductionContext context, StructDeclarationSyntax source)
    {
        
    }
}
