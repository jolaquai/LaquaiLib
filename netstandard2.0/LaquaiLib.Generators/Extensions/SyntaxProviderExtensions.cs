namespace LaquaiLib.Generators.Extensions;

internal static class SyntaxProviderExtensions
{
    extension(in SyntaxValueProvider svp)
    {
        // resolve to an equatable model inside the transform; anything downstream of it pins Roslyn objects and defeats caching
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IncrementalValuesProvider<TModel> ForAttributeWithMetadataNameOn<TNode, TModel>(string fullyQualifiedMetadataName, Func<GeneratorAttributeSyntaxContext, CancellationToken, TModel> transform) where TNode : SyntaxNode
            => svp.ForAttributeWithMetadataName(fullyQualifiedMetadataName, Delegates.TypeCheckPredicate<TNode>, transform);
    }
}
