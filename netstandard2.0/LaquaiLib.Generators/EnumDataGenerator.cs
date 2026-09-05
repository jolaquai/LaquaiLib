using System.CodeDom.Compiler;
using System.Text;

using LaquaiLib.Analyzers.Shared;
using LaquaiLib.Generators.Extensions;

namespace LaquaiLib.Generators;

/// <summary>
/// One enum member, resolved to strings.
/// </summary>
internal sealed record EnumMemberModel(string Name, string UnderlyingValue, string Description);

/// <summary>
/// Everything the emitter needs about one enum, fully resolved so the pipeline never pins a Roslyn object.
/// </summary>
internal sealed record EnumModel(
    string AssemblyRootNamespace,
    string Namespace,
    string Identifier,
    string Accessibility,
    string FullyQualifiedName,
    string UnderlyingTypeName,
    EquatableArray<EnumMemberModel> Members);

[Generator(LanguageNames.CSharp)]
public class EnumExpanderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is EnumDeclarationSyntax,
            static (context, _) => CreateModel(context)
        ).WithTrackingName(GeneratorStepNames.EnumExpanderModels)
        .Where(static model => model is not null)
        .WithTrackingName(GeneratorStepNames.EnumExpanderFiltered);

        var collected = models.Collect().WithTrackingName(GeneratorStepNames.EnumExpanderCollected);
        context.RegisterSourceOutput(collected, static (spc, models) =>
        {
            var sourceText = GenerateEnumExpansions(models);
            spc.AddSource(
                $"{nameof(EnumExpanderGenerator)}.{nameof(GenerateEnumExpansions)}.g.cs",
                SourceText.From(sourceText, Encoding.UTF8)
            );
        });
    }

    private static EnumModel CreateModel(GeneratorSyntaxContext context)
    {
        var decl = Unsafe.As<EnumDeclarationSyntax>(context.Node);
        if (context.SemanticModel.GetDeclaredSymbol(decl) is not INamedTypeSymbol symbol)
            return null;

        // the Data class is emitted at namespace scope, so it must be able to name the enum from there
        var accessibility = symbol.DeclaredAccessibility;
        for (var containing = symbol.ContainingType; containing is not null; containing = containing.ContainingType)
        {
            // a nested enum's fully qualified name carries the containing type's type arguments, which are unbindable at namespace scope
            if (containing.IsGenericType)
                return null;
            if (containing.DeclaredAccessibility < accessibility)
                accessibility = containing.DeclaredAccessibility;
        }
        // anything less visible than internal cannot be referenced by a namespace-scope class at all
        if (accessibility is not (Accessibility.Public or Accessibility.Internal))
            return null;

        var enumFields = symbol.GetMembers()
            .Where(static m => m is IFieldSymbol { IsConst: true, HasConstantValue: true })
            .OrderBy(static f => f.DeclaringSyntaxReferences.FirstOrDefault()?.Span.Start ?? int.MaxValue)
            .Select(static m => (IFieldSymbol)m).ToArray();
        if (enumFields.Length == 0)
            return null;

        var members = ImmutableArray.CreateBuilder<EnumMemberModel>(enumFields.Length);
        foreach (var field in enumFields)
        {
            var descAttr = field.GetAttributes().FirstOrDefault(static a => a.AttributeClass?.Name == "DescriptionAttribute");
            members.Add(new EnumMemberModel(
                field.Name,
                field.ConstantValue?.ToString(),
                descAttr?.ConstructorArguments.FirstOrDefault().Value?.ToString()
            ));
        }

        var containingNamespace = symbol.ContainingNamespace;
        return new EnumModel(
            symbol.ContainingAssembly.Name,
            // a top-level enum has no namespace to wrap in; ToDisplayString() would emit the literal "<global namespace>"
            containingNamespace.IsGlobalNamespace ? null : containingNamespace.ToDisplayString(),
            decl.Identifier.Text,
            accessibility == Accessibility.Public ? "public" : "internal",
            // enums can never be generic, so there is no unbound-generic form to construct here
            symbol.ToDisplayString(SymbolDisplayFormats.FullyQualified),
            symbol.EnumUnderlyingType.ToDisplayString(SymbolDisplayFormats.FullyQualified),
            members.MoveToImmutable()
        );
    }

    private static string GenerateEnumExpansions(ImmutableArray<EnumModel> models)
    {
        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        using var writer = new IndentedTextWriter(sw);

        var writtenEnumDataStruct = false;
        foreach (var model in models)
        {
            var fqEnumName = model.FullyQualifiedName;
            var members = model.Members;
            var fqUnderlying = model.UnderlyingTypeName;

            var assemblyRootNamespace = model.AssemblyRootNamespace;
            if (!writtenEnumDataStruct)
            {
                writtenEnumDataStruct = true;
                writer.WriteLines(SourceEmitHelper.GeneratedFileHeader);
                // the arrays materialize every member, including obsolete ones, and that is the whole point of this generator
                writer.WriteLine("#pragma warning disable CS0612, CS0618 // Type or member is obsolete");
                writer.WriteLine();
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
            using var _ = writer.Region(model.Identifier);

            // a top-level enum has no namespace to wrap in
            var isGlobalNamespace = model.Namespace is null;
            if (!isGlobalNamespace)
                writer.WriteLine($"namespace {model.Namespace}");
            using (isGlobalNamespace ? null : writer.Scope)
            {
                writer.WriteLines(SourceEmitHelper.Summary($"Provides alternative data representations for the enum <c>{fqEnumName}</c>."));
                writer.WriteLines(SourceEmitHelper.GeneratedCodeAttribute(typeof(EnumExpanderGenerator)));
                writer.WriteLine($"{model.Accessibility} static class {model.Identifier}Data");
                using (writer.Scope)
                {
                    writer.WriteLines(SourceEmitHelper.Summary($"Gets the names of the enum members of <c>{fqEnumName}</c>."));
                    writer.Write($"public static string[] Names {{ get; }} = [");
                    writer.Write(string.Join(", ", members.Select(static m => $"\"{m.Name}\"")));
                    writer.WriteLine("];");

                    writer.WriteLines(SourceEmitHelper.Summary($"Gets the values of the enum members of <c>{fqEnumName}</c>."));
                    writer.WriteLine($"public static {fqEnumName}[] Values {{ get; }} = [");
                    writer.Indent++;
                    {
                        for (var i = 0; i < members.Length; i++)
                            writer.WriteLine($"{fqEnumName}.{members[i].Name},");
                    }
                    writer.Indent--;
                    writer.WriteLine("];");

                    writer.WriteLines(SourceEmitHelper.Summary($"Gets the underlying values of the enum members of <c>{fqEnumName}</c>."));
                    writer.Write($"public static {fqUnderlying}[] UnderlyingValues {{ get; }} = [");
                    writer.Write(string.Join(", ", members.Select(static m => m.UnderlyingValue)));
                    writer.WriteLine("];");

                    writer.WriteLines(SourceEmitHelper.Summary($"Gets the data of the enum members of <c>{fqEnumName}</c>."));
                    writer.WriteLine($"public static global::{assemblyRootNamespace}.EnumFieldData<{fqEnumName}, {fqUnderlying}>[] Data {{ get; }} = [");
                    writer.Indent++;
                    {
                        for (var i = 0; i < members.Length; i++)
                        {
                            var member = members[i];
                            var desc = member.Description is not null ? $"\"{member.Description}\"" : "null";
                            writer.WriteLine($"new(\"{member.Name}\", {fqEnumName}.{member.Name}, {member.UnderlyingValue}, {desc}, {i}),");
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
