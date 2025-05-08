namespace LaquaiLib.Analyzers.Fixes;

internal static class Helpers
{
    extension(Document document)
    {
        public async Task<CompilationUnitSyntax> GetRootAsync(CancellationToken cancellationToken = default)
            => (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
    }
    extension(CompilationUnitSyntax compilationUnitSyntax)
    {
        /// <summary>
        /// Adds the specified using directive to the compilation unit if it does not already exist. If it does, the return value is a reference to the original compilation unit.
        /// <para/>This will most likely break <see cref="WellKnownFixAllProviders.BatchFixer"/>. Usage in conjunction with it is discouraged.
        /// </summary>
        /// <returns></returns>
        public CompilationUnitSyntax AddUsingsIfNotExists(params UsingDirectiveSyntax[] usingDirectiveSyntaxes)
        {
            var existingUsings = new HashSet<string>(compilationUnitSyntax.Usings.Select(static u => u.Name.ToString()));
            var filtered = usingDirectiveSyntaxes.Where(u => !existingUsings.Contains(u.Name.ToString())).ToArray();
            return filtered.Length == 0 ? compilationUnitSyntax : compilationUnitSyntax.AddUsings(filtered);
        }
    }
}
