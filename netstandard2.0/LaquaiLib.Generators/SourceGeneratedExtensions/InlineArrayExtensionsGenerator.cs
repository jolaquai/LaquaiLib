using System.CodeDom.Compiler;
using System.Text;

using LaquaiLib.Generators.Extensions;

namespace LaquaiLib.Generators.SourceGeneratedExtensions;

/// <summary>
/// Generates extensions directly into <see langword="struct"/>s marked with the <c>[InlineArray]</c> attribute.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class InlineArrayExtensionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // For user-declared structs in the current compilation
        var structDeclSyntaxProvider = context.SyntaxProvider.ForAttributeWithMetadataNameOn<StructDeclarationSyntax>("System.Runtime.CompilerServices.InlineArrayAttribute");

        var contexts = structDeclSyntaxProvider.Collect();
        context.RegisterSourceOutput(contexts, static (spc, source) =>
        {
            var typeSymbols = source.Select(static c => c.TargetSymbol as INamedTypeSymbol).ToList();
            if (typeSymbols.Count == 0)
            {
                return;
            }

            var declaredInlineArrayClassesSource = GenerateExtensionClasses(typeSymbols);
            spc.AddSource($"InlineArraySpanExtensions.g.cs", SourceText.From(declaredInlineArrayClassesSource, Encoding.UTF8));
        });
    }
    private static string GenerateExtensionClasses(List<INamedTypeSymbol> results)
    {
        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        using var writer = new IndentedTextWriter(sw);

        writer.WriteLine(SourceEmitHelper.GeneratedFileHeader);

        for (var i = 0; i < results.Count; i++)
        {
            var type = results[i];
            var typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var typeNameNullable = typeName + '?';

            // Find the type of the singular field declared in the struct (it very literally has to have exactly one field)
            var field = type.GetMembers().OfType<IFieldSymbol>().First();
            var fieldTypeName = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var fieldName = field.Name;

            // Get the AttributeData for the [InlineArray] attribute on this symbol, we know it's there
            var inlineArrayAttribute = type.GetAttributes().First(attr => attr.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "System.Runtime.CompilerServices.InlineArrayAttribute");
            // ...and get its constructor argument
            var length = (int)inlineArrayAttribute.ConstructorArguments[0].Value;
            WriteExtensionsForClass(writer, type, typeName, typeNameNullable, field, fieldTypeName, fieldName, length);
            writer.WriteLine();
        }

        writer.Dispose();
        sw.Dispose();
        return sb.ToString();
    }

    private static void WriteExtensionsForClass(IndentedTextWriter writer, INamedTypeSymbol type, string typeName, string typeNameNullable, IFieldSymbol field, string fieldTypeName, string fieldName, int length)
    {
        writer.WriteLine($"namespace {type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");
        using (writer.Scope)
        {
            writer.WriteLines(SourceEmitHelper.GeneratedCodeAttribute(typeof(InlineArrayExtensionsGenerator)));

            writer.WriteLines(SourceEmitHelper.Summary($"Provides <c>AsSpan</c> extension methods for <see cref=\"{typeName}\"/>."));
            writer.WriteLine($"public static class {type.Name}Extensions");
            using (writer.Scope)
            {
                // If the field is a struct and explicitly declared nullable, we'll have to make it nullable in our returns as well
                var useFieldTypeName = field.Type.NullableAnnotation == NullableAnnotation.Annotated ? fieldTypeName + '?' : fieldTypeName;

                var typeParams = type.TypeParameters.Length > 0
                    ? $"<{string.Join(", ", type.TypeParameters.Select(p => p.Name))}>"
                    : "";
                var typeNameForDoc = typeName.Replace('<', '{').Replace('>', '}');

                // Start with the nullable struct overloads
                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over the entirety of the specified nullable <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/> or <see langword=\"default\"/>(<see cref=\"global::System.Span{{T}}\"/>) if it is <see langword=\"null\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified."));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeNameNullable} instance) => instance.HasValue ? AsSpan(instance.Value, 0, {length}) : default;");

                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over a portion of the specified nullable <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/>, beginning at <paramref name=\"start\"/>, or <see langword=\"default\"/>(<see cref=\"global::System.Span{{T}}\"/>) if it is <see langword=\"null\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified.", [("start", "The index to start the span at.")]));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeNameNullable} instance, int start) => instance.HasValue ? AsSpan(instance.Value, start, {length} - start) : default;");

                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over a portion of the specified nullable <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/>, beginning at <paramref name=\"start\"/>, or <see langword=\"default\"/>(<see cref=\"global::System.Span{{T}}\"/>) if it is <see langword=\"null\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified.", [("start", "The index to start the span at."), ("length", "The length of the span.")]));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeNameNullable} instance, int start, int length) => instance.HasValue ? AsSpan(instance.Value, start, length) : default;");

                // For reference types, we only use the non-annotated version
                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over the entirety of the specified <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified."));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeName} instance) => AsSpan(instance, 0, {length});");

                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over the entirety of the specified <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/>, beginning at <paramref name=\"start\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified.", [("start", "The index to start the span at.")]));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeName} instance, int start) => AsSpan(instance, start, {length} - start);");

                writer.WriteLines(SourceEmitHelper.Summary($"Gets a <see cref=\"global::System.Span{{T}}\"/> over the entirety of the specified <see cref=\"{typeNameForDoc}\"/> <paramref name=\"instance\"/>, beginning at <paramref name=\"start\"/>.", "The <see cref=\"global::System.Span{T}\"/> as specified.", [("start", "The index to start the span at."), ("length", "The length of the span.")]));
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"public static System.Span<{useFieldTypeName}> AsSpan{typeParams}(this {typeName} instance, int start, int length)");
                using (writer.Scope)
                {
                    writer.WriteLines($$"""
                        if (length < 0 || start < 0 || start + length > {{length}})
                        {
                            throw new System.IndexOutOfRangeException($"The start index {start} and length {length} must be within the bounds of the inline array length {{length}}.");
                        }
                        """);

                    writer.WriteLines($"""
                        return System.Runtime.InteropServices.MemoryMarshal.CreateSpan(
                            ref System.Runtime.CompilerServices.Unsafe.Add(ref instance.{fieldName}, start), length
                        );
                        """);
                }
            }
        }
    }
}
