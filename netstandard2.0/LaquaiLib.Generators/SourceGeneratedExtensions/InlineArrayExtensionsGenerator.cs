using System.CodeDom.Compiler;
using System.Text;

using LaquaiLib.Analyzers.Shared;
using LaquaiLib.Generators.Extensions;

namespace LaquaiLib.Generators.SourceGeneratedExtensions;

/// <summary>
/// Everything the emitter needs about one <c>[InlineArray]</c> struct, fully resolved to strings so the pipeline never pins a Roslyn object.
/// </summary>
internal sealed record InlineArrayModel(
    string TypeName,
    string SimpleName,
    string Namespace,
    string ElementTypeName,
    string FieldName,
    int Length,
    string TypeParameterList);

/// <summary>
/// Generates extensions directly into <see langword="struct"/>s marked with the <c>[InlineArray]</c> attribute.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class InlineArrayExtensionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // For user-declared structs in the current compilation
        var models = context.SyntaxProvider.ForAttributeWithMetadataNameOn<StructDeclarationSyntax, InlineArrayModel>(
            "System.Runtime.CompilerServices.InlineArrayAttribute",
            static (context, _) => CreateModel(context)
        ).WithTrackingName(GeneratorStepNames.InlineArrayModels)
        .Where(static model => model is not null)
        .WithTrackingName(GeneratorStepNames.InlineArrayFiltered);

        var collected = models.Collect().WithTrackingName(GeneratorStepNames.InlineArrayCollected);
        context.RegisterSourceOutput(collected, static (spc, source) =>
        {
            if (source.Length == 0)
                return;

            var declaredInlineArrayClassesSource = GenerateExtensionClasses(source);
            spc.AddSource($"InlineArraySpanExtensions.g.cs", SourceText.From(declaredInlineArrayClassesSource, Encoding.UTF8));
        });
    }

    private static InlineArrayModel CreateModel(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol type)
            return null;

        // the singular field declared in the struct (it very literally has to have exactly one field)
        var field = type.GetMembers().OfType<IFieldSymbol>().FirstOrDefault();
        if (field is null)
            return null;

        var inlineArrayAttribute = type.GetAttributes().FirstOrDefault(static attr => attr.AttributeClass.ToDisplayString(SymbolDisplayFormats.FullyQualified) == "global::System.Runtime.CompilerServices.InlineArrayAttribute");
        if (inlineArrayAttribute is null || inlineArrayAttribute.ConstructorArguments.Length == 0 || inlineArrayAttribute.ConstructorArguments[0].Value is not int length)
            return null;

        // If the field is a struct and explicitly declared nullable, we'll have to make it nullable in our returns as well
        var elementTypeName = field.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified);
        if (field.Type.NullableAnnotation == NullableAnnotation.Annotated)
            elementTypeName += '?';

        return new InlineArrayModel(
            type.ToDisplayString(SymbolDisplayFormats.FullyQualified),
            type.Name,
            type.ContainingNamespace.ToDisplayString(),
            elementTypeName,
            field.Name,
            length,
            type.TypeParameters.Length > 0 ? $"<{string.Join(", ", type.TypeParameters.Select(static p => p.Name))}>" : ""
        );
    }

    private static string GenerateExtensionClasses(ImmutableArray<InlineArrayModel> results)
    {
        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        using var writer = new IndentedTextWriter(sw);

        writer.WriteLine(SourceEmitHelper.GeneratedFileHeader);

        for (var i = 0; i < results.Length; i++)
        {
            WriteExtensionsForClass(writer, results[i]);
            writer.WriteLine();
        }

        writer.Dispose();
        sw.Dispose();
        return sb.ToString();
    }

    private static void WriteExtensionsForClass(IndentedTextWriter writer, InlineArrayModel model)
    {
        var typeName = model.TypeName;
        var typeNameNullable = typeName + '?';
        var useFieldTypeName = model.ElementTypeName;
        var typeParams = model.TypeParameterList;
        var length = model.Length;
        var fieldName = model.FieldName;

        writer.WriteLine($"namespace {model.Namespace}");
        using (writer.Scope)
        {
            writer.WriteLines(SourceEmitHelper.GeneratedCodeAttribute(typeof(InlineArrayExtensionsGenerator)));

            writer.WriteLines(SourceEmitHelper.Summary($"Provides <c>AsSpan</c> extension methods for <see cref=\"{typeName}\"/>."));
            writer.WriteLine($"public static class {model.SimpleName}Extensions");
            using (writer.Scope)
            {
                var typeNameForDoc = typeName.Replace('<', '{').Replace('>', '}');

                // Start with the nullable struct overloads
                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over the entirety of the specified nullable <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/> or <see langword=\"default\"/>(<see cref=\"global::System.Span{{T}}\"/>) if it is <see langword=\"null\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified."));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static global::System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeNameNullable} instance) => instance.HasValue ? AsSpan(instance.Value, 0, {length}) : default;");

                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over a portion of the specified nullable <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/>, beginning at <paramref name=\"start\"/>, or <see langword=\"default\"/>(<see cref=\"global::System.Span{{T}}\"/>) if it is <see langword=\"null\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified.", [("start", "The index to start the span at.")]));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static global::System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeNameNullable} instance, int start) => instance.HasValue ? AsSpan(instance.Value, start, {length} - start) : default;");

                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over a portion of the specified nullable <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/>, beginning at <paramref name=\"start\"/>, or <see langword=\"default\"/>(<see cref=\"global::System.Span{{T}}\"/>) if it is <see langword=\"null\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified.", [("start", "The index to start the span at."), ("length", "The length of the span.")]));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static global::System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeNameNullable} instance, int start, int length) => instance.HasValue ? AsSpan(instance.Value, start, length) : default;");

                // For reference types, we only use the non-annotated version
                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over the entirety of the specified <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified."));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static global::System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeName} instance) => AsSpan(instance, 0, {length});");

                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over the entirety of the specified <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/>, beginning at <paramref name=\"start\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified.", [("start", "The index to start the span at.")]));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static global::System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeName} instance, int start) => AsSpan(instance, start, {length} - start);");

                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over the entirety of the specified <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/>, beginning at <paramref name=\"start\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified.", [("start", "The index to start the span at."), ("length", "The length of the span.")]));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static global::System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeName} instance, int start, int length)");
                using (writer.Scope)
                {
                    writer.WriteLines($$"""
                        if (length < 0 || start < 0 || start + length > {{length}})
                        {
                            throw new global::System.IndexOutOfRangeException($"The start index {start} and length {length} must be within the bounds of the inline array length {{length}}.");
                        }
                        """);

                    writer.WriteLines($"""
                        return global::System.Runtime.InteropServices.MemoryMarshal.CreateSpan(
                            ref global::System.Runtime.CompilerServices.Unsafe.Add(ref instance.{fieldName}, start), length
                        );
                        """);
                }
            }
        }
    }
}
