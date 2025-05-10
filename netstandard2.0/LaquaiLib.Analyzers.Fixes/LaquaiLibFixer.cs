using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.Editing;

namespace LaquaiLib.Analyzers.Fixes;

/// <summary>
/// Provides a base class for all code fix providers in the LaquaiLib library with some shared functionality for implementing single and batch fixes in a more streamlined way.
/// </summary>
public abstract class LaquaiLibFixer(params ImmutableArray<string> fixableDiagnosticIds) : CodeFixProvider
{
    #region override
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = fixableDiagnosticIds;
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var compilationUnitSyntax = await document.GetRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostics = context.Diagnostics;
        for (var i = 0; i < context.Diagnostics.Length; i++)
        {
            var diagnostic = diagnostics[i];
            var (title, key, fixAction) = GetFixInfo(compilationUnitSyntax, diagnostic);

            context.RegisterCodeFix(CodeAction.Create(
                title: title,
                createChangedDocument: async c =>
                {
                    var editor = await DocumentEditor.CreateAsync(document, c).ConfigureAwait(false);
                    await fixAction(editor).ConfigureAwait(false);
                    var changed = editor.GetChangedDocument();
                    changed = await ExecutePostFixes(changed).ConfigureAwait(false);
                    // In the single fix instance, we need to reset the PostFixAction to null, otherwise those might be executed multiple times (in the odd case where there this does actually fix multiple diagnostics)
                    PostFixAction = null;
                    return changed;
                },
                equivalenceKey: $"{Prefix}_{key}"
            ), diagnostic);
        }
    }

    public override FixAllProvider GetFixAllProvider() => FixAllProvider.Create(FixAllAsync);
    #endregion

    private string Prefix => field ??= GetType().FullName;
    private async Task<Document> FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<Diagnostic> diagnostics)
    {
        if (diagnostics.IsEmpty)
        {
            return document;
        }

        var root = await document.GetRootAsync(fixAllContext.CancellationToken).ConfigureAwait(false);
        var editor = await DocumentEditor.CreateAsync(document, fixAllContext.CancellationToken).ConfigureAwait(false);

        for (var i = 0; i < diagnostics.Length; i++)
        {
            var diagnostic = diagnostics[i];
            var (title, key, fixAction) = GetFixInfo(root, diagnostic);
            await fixAction(editor).ConfigureAwait(false);
        }

        var changed = editor.GetChangedDocument();
        changed = await ExecutePostFixes(changed).ConfigureAwait(false);

        return changed;
    }

    private async Task<Document> ExecutePostFixes(Document changed)
    {
        if (PostFixAction?.InvocationList is var postFixes and not null)
        {
            for (var i = 0; i < postFixes.Length; i++)
            {
                changed = await postFixes[i](changed).ConfigureAwait(false);
            }
        }

        return changed;
    }

    /// <summary>
    /// When overridden in a derived class, provides the fix information for a specific <see cref="Diagnostic"/>.
    /// When using a more derived base class than <see cref="LaquaiLibFixer"/> itself, override that class's <c>GetFixInfo</c> instead.
    /// Returned <see cref="FixInfo.FixAction"/> must not replace the root node of the <see cref="DocumentEditor"/> they are passed (which is the <paramref name="compilationUnitSyntax"/> passed here). If that is required, use <see cref="PostFixAction"/> instead.
    /// </summary>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="diagnostic">The <see cref="Diagnostic"/> to fix.</param>
    /// <returns>A <see cref="FixInfo"/> containing the fix information.</returns>
    public abstract FixInfo GetFixInfo(CompilationUnitSyntax compilationUnitSyntax, Diagnostic diagnostic);
    /// <summary>
    /// Encapsulates methods to call after the fix has been applied (or all fixes if called from the context of the fix-all provider).
    /// Can be used to perform additional actions on the <see cref="Document"/>, for example, if they are incompatible with the <see cref="DocumentEditor"/>.
    /// They are invoked in order of registration and awaited individually. Their changes are introduced sequentially, each invocation <see langword="await"/>ed and passed the result of the previous invocation (if there are multiple).
    /// </summary>
    public event Func<Document, ValueTask<Document>> PostFixAction;
}

/// <summary>
/// Provides a base class for code fix providers for analyzers that report diagnostics on <see cref="SyntaxToken"/>s.
/// </summary>
/// <param name="fixableDiagnosticIds">An <see cref="ImmutableArray{T}"/> of fixable diagnostic IDs.</param>
public abstract class LaquaiLibTokenFixer(params ImmutableArray<string> fixableDiagnosticIds) : LaquaiLibFixer(fixableDiagnosticIds)
{
    /// <summary>
    /// When overridden in a derived class, provides the fix information for a specific <see cref="Diagnostic"/>.
    /// </summary>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="syntaxToken">The <see cref="SyntaxToken"/> on which <paramref name="diagnostic"/> was reported.</param>
    /// <param name="diagnostic">The <see cref="Diagnostic"/> to fix.</param>
    /// <returns>A <see cref="FixInfo"/> containing the fix information.</returns>
    public abstract FixInfo GetFixInfo(CompilationUnitSyntax compilationUnitSyntax, SyntaxToken syntaxToken, Diagnostic diagnostic);
    /// <summary>
    /// Do not use. Override <see cref="GetFixInfo(CompilationUnitSyntax, SyntaxToken, Diagnostic)"/> instead.
    /// </summary>
    public sealed override FixInfo GetFixInfo(CompilationUnitSyntax compilationUnitSyntax, Diagnostic diagnostic)
        => GetFixInfo(compilationUnitSyntax, compilationUnitSyntax.FindToken(diagnostic.Location.SourceSpan.Start), diagnostic);
}
/// <summary>
/// Provides a base class for code fix providers for analyzers that report diagnostics on <see cref="SyntaxNode"/>s.
/// </summary>
/// <param name="fixableDiagnosticIds">An <see cref="ImmutableArray{T}"/> of fixable diagnostic IDs.</param>
public abstract class LaquaiLibNodeFixer(params ImmutableArray<string> fixableDiagnosticIds) : LaquaiLibFixer(fixableDiagnosticIds)
{
    /// <summary>
    /// When overridden in a derived class, provides the fix information for a specific <see cref="Diagnostic"/>.
    /// </summary>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="syntaxNode">The <see cref="SyntaxNode"/> on which <paramref name="diagnostic"/> was reported.</param>
    /// <param name="diagnostic">The <see cref="Diagnostic"/> to fix.</param>
    /// <returns>A <see cref="FixInfo"/> containing the fix information.</returns>
    public abstract FixInfo GetFixInfo(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, Diagnostic diagnostic);
    /// <summary>
    /// Do not use. Override <see cref="GetFixInfo(CompilationUnitSyntax, SyntaxToken, Diagnostic)"/> instead.
    /// </summary>
    public sealed override FixInfo GetFixInfo(CompilationUnitSyntax compilationUnitSyntax, Diagnostic diagnostic)
        => GetFixInfo(compilationUnitSyntax, compilationUnitSyntax.FindNode(diagnostic.Location.SourceSpan), diagnostic);
}

/// <summary>
/// Encapsulates the information required to construct a <see cref="CodeAction"/>.
/// </summary>
public readonly partial struct FixInfo(string title, Func<DocumentEditor, ValueTask> fixAction, string equivalenceKey = null)
{
    private static readonly Regex _keyNormalizationRegex = new Regex(@"[^A-Za-z]", RegexOptions.Compiled);
    /// <summary>
    /// Gets a <see cref="FixInfo"/> with no fix information. Executing it does nothing.
    /// </summary>
    public static FixInfo Empty { get; } = new FixInfo();
    public FixInfo() : this("", _ => ValueTask.CompletedTask) { }

    public void Deconstruct(out string title, out string equivalenceKey, out Func<DocumentEditor, ValueTask> fixAction)
    {
        title = Title;
        equivalenceKey = EquivalenceKey;
        fixAction = FixAction;
    }
    /// <summary>
    /// The title of the code action.
    /// </summary>
    public string Title { get; } = title;
    /// <summary>
    /// The equivalence key for the code action. Need not be set, in which case it is automatically generated from <see cref="Title"/>.
    /// </summary>
    public string EquivalenceKey { get; } = !string.IsNullOrWhiteSpace(equivalenceKey) ? equivalenceKey : _keyNormalizationRegex.Replace(title.ToTitleCase(), "");
    /// <summary>
    /// A <see langword="delegate"/> that uses a <see cref="DocumentEditor"/> to produce a new <see cref="Document"/> that contains the fix.
    /// </summary>
    public Func<DocumentEditor, ValueTask> FixAction { get; } = fixAction;

}