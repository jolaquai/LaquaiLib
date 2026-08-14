using System.CodeDom.Compiler;
using System.Text;

using LaquaiLib.Analyzers.Shared;
using LaquaiLib.Generators.Extensions;

namespace LaquaiLib.Generators;

[Generator(LanguageNames.CSharp)]
public class EnumExpanderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var enumDeclSyntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is EnumDeclarationSyntax,
            static (context, _) => (Node: Unsafe.As<EnumDeclarationSyntax>(context.Node), context.SemanticModel)
        );
        var contexts = enumDeclSyntaxProvider.Collect();
        context.RegisterSourceOutput(contexts, static (spc, decls) =>
        {
            var sourceText = GenerateEnumExpansions(decls.Select(static d => (d.Node, d.SemanticModel.GetDeclaredSymbol(d.Node))).ToArray());
            spc.AddSource(
                $"{nameof(EnumExpanderGenerator)}.{nameof(GenerateEnumExpansions)}.g.cs",
                SourceText.From(sourceText, Encoding.UTF8)
            );
        });
    }

    private static string GenerateEnumExpansions((EnumDeclarationSyntax, INamedTypeSymbol)[] decls)
    {
        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        using var writer = new IndentedTextWriter(sw);

        var writtenEnumDataStruct = false;
        foreach (var (decl, symbol) in decls)
        {
            // enums can never be generic, so there is no unbound-generic form to construct here
            var fqEnumName = symbol.ToDisplayString(SymbolDisplayFormats.FullyQualified);
            var enumFields = symbol.GetMembers()
                .Where(m => m is IFieldSymbol { IsConst: true, HasConstantValue: true })
                .OrderBy(f => f.DeclaringSyntaxReferences.FirstOrDefault()?.Span.Start ?? int.MaxValue)
                .Select(m => (IFieldSymbol)m).ToArray();
            if (enumFields.Length == 0)
            {
                continue;
            }
            var names = enumFields.Select(f => f.Name).ToArray();
            var fqNames = enumFields.Select(f => fqEnumName + '.' + f.Name).ToArray();
            var valuesUnderlying = enumFields.Select(f => f.ConstantValue).ToArray();
            var fqUnderlying = symbol.EnumUnderlyingType.ToDisplayString(SymbolDisplayFormats.FullyQualified);
            var descs = enumFields.Select(f =>
            {
                var descAttr = f.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "DescriptionAttribute");
                return descAttr?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? null;
            }).ToArray();

            var assemblyRootNamespace = symbol.ContainingAssembly.Name;
            if (!writtenEnumDataStruct)
            {
                writtenEnumDataStruct = true;
                writer.WriteLine($"namespace {assemblyRootNamespace}");
                using (writer.Scope)
                {
                    writer.WriteLines(SourceEmitHelper.Summary(
                        "Encapsulates data of an enum field.",
                        parameters: [
                            ("Name", "The name of the enum member."),
                            ("Value", "The value of the enum member."),
                            ("UnderlyingValue", "The value of the enum member typed as the enum's underlying value."),
                            ("Description", "The value of the <c>[DescriptionAttribute]</c> that adorns the enum member, if any; <see langword=\"null\"/> if not present."),
                            ("DeclaredPosition", "The 0-based index of the enum member at which it was declared in source. For members without explicitly declared values, this is generally equal to the <see cref=\"UnderlyingValue\"/>."),
                        ],
                        typeParameters: [
                            ("TEnum", "The type of the enum."),
                            ("TUnderlying", "The type of the enum's underlying value.")
                        ]
                    ));
                    writer.WriteLine($"public readonly record struct EnumFieldData<TEnum, TUnderlying>(string Name, TEnum Value, TUnderlying UnderlyingValue, string Description, int DeclaredPosition);");
                }
            }

            writer.WriteLine();
            using var _ = writer.Region(decl.Identifier.Text);

            // a top-level enum has no namespace to wrap in; ToDisplayString() would emit the literal "<global namespace>"
            var containingNamespace = symbol.ContainingNamespace;
            var isGlobalNamespace = containingNamespace.IsGlobalNamespace;
            if (!isGlobalNamespace)
            {
                writer.WriteLine($"namespace {containingNamespace.ToDisplayString()}");
            }
            using (isGlobalNamespace ? null : writer.Scope)
            {
                var accessibility = decl.Modifiers.Any(SyntaxKind.PublicKeyword) ? "public" : decl.Modifiers.Any(SyntaxKind.InternalKeyword) ? "internal" : "internal";

                writer.WriteLines(SourceEmitHelper.Summary($"Provides alternative data representations for the enum <c>{fqEnumName}</c>."));
                writer.WriteLines(SourceEmitHelper.GeneratedCodeAttribute(typeof(EnumExpanderGenerator)));
                writer.WriteLine($"{accessibility} static class {decl.Identifier.Text}Data");
                using (writer.Scope)
                {
                    writer.WriteLines(SourceEmitHelper.Summary($"Gets the names of the enum members of <c>{fqEnumName}</c>."));
                    writer.Write($"public static string[] Names {{ get; }} = [");
                    writer.Write(string.Join(", ", names.Select(n => $"\"{n}\"")));
                    writer.WriteLine("];");

                    writer.WriteLines(SourceEmitHelper.Summary($"Gets the values of the enum members of <c>{fqEnumName}</c>."));
                    writer.WriteLine($"public static {fqEnumName}[] Values {{ get; }} = [");
                    writer.Indent++;
                    {
                        for (var i = 0; i < names.Length; i++)
                        {
                            writer.WriteLine($"{fqNames[i]},");
                        }
                    }
                    writer.Indent--;
                    writer.WriteLine("];");

                    writer.WriteLines(SourceEmitHelper.Summary($"Gets the underlying values of the enum members of <c>{fqEnumName}</c>."));
                    writer.Write($"public static {fqUnderlying}[] UnderlyingValues {{ get; }} = [");
                    writer.Write(string.Join(", ", valuesUnderlying));
                    writer.WriteLine("];");

                    writer.WriteLines(SourceEmitHelper.Summary($"Gets the data of the enum members of <c>{fqEnumName}</c>."));
                    writer.WriteLine($"public static global::{assemblyRootNamespace}.EnumFieldData<{fqEnumName}, {fqUnderlying}>[] Data {{ get; }} = [");
                    writer.Indent++;
                    {
                        for (var i = 0; i < names.Length; i++)
                        {
                            var desc = descs[i] is not null ? $"\"{descs[i]}\"" : "null";
                            writer.WriteLine($"new(\"{names[i]}\", {fqNames[i]}, {valuesUnderlying[i]}, {desc}, {i}),");
                        }
                    }
                    writer.Indent--;
                    writer.WriteLine("];");
                }
            }
        }

        writer.Dispose();
        sw.Dispose();
        return sb.ToString();
    }
}
