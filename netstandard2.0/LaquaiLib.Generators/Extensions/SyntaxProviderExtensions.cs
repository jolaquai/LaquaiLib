namespace LaquaiLib.Generators.Extensions;

internal static class SyntaxProviderExtensions
{
    extension(SyntaxValueProvider svp)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IncrementalValuesProvider<GeneratorAttributeSyntaxContext> ForAttributeWithMetadataNameOn<T>(string fullyQualifiedMetadataName) where T : SyntaxNode
            => svp.ForAttributeWithMetadataName(fullyQualifiedMetadataName, Delegates.TypeCheckPredicate<T>,
                static (context, _) => context);
    }
}
