using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        for (var i = 0; i < diagnostics.Length; i++)
        {
            var diagnostic = diagnostics[i];
            var infos = await GetCodeActionInfosAsync(document, compilationUnitSyntax, diagnostic, context.CancellationToken).ConfigureAwait(false);
            for (var j = 0; j < infos.Length; j++)
            {
                var (title, key, action) = infos[j];
                var postFixActions = infos[j].PostFixActions;

                context.RegisterCodeFix(CodeAction.Create(
                    title: title,
                    createChangedDocument: async c =>
                    {
                        var editor = await DocumentEditor.CreateAsync(document, c).ConfigureAwait(false);
                        await action(editor).ConfigureAwait(false);
                        var changed = editor.GetChangedDocument();
                        return await Helpers.ApplyPostFixesAsync(changed, postFixActions, c).ConfigureAwait(false);
                    },
                    equivalenceKey: $"{Prefix}_{key}"
                ), diagnostic);
            }
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

        List<PostFixAction> postFixes = null;
        for (var i = 0; i < diagnostics.Length; i++)
        {
            var diagnostic = diagnostics[i];
            var infos = await GetCodeActionInfosAsync(document, root, diagnostic, fixAllContext.CancellationToken).ConfigureAwait(false);
            for (var j = 0; j < infos.Length; j++)
            {
                var info = infos[j];
                // A diagnostic may offer several alternative actions; apply only the one the user invoked the fix-all from.
                if ($"{Prefix}_{info.EquivalenceKey}" != fixAllContext.CodeActionEquivalenceKey)
                {
                    continue;
                }
                await info.Action(editor).ConfigureAwait(false);
                if (!info.PostFixActions.IsDefaultOrEmpty)
                {
                    (postFixes ??= []).AddRange(info.PostFixActions);
                }
            }
        }

        var changed = editor.GetChangedDocument();
        if (postFixes is not null)
        {
            for (var i = 0; i < postFixes.Count; i++)
            {
                changed = await postFixes[i](changed, fixAllContext.CancellationToken).ConfigureAwait(false);
            }
        }

        return changed;
    }

    /// <summary>
    /// When overridden in a derived class, provides zero or more fixes for a specific <see cref="Diagnostic"/>.
    /// Most fixers should derive from <see cref="LaquaiLibTokenFixer"/>, <see cref="LaquaiLibNodeFixer"/> or <see cref="LaquaiLibOperationFixer"/> and override that class's <c>GetCodeActionInfos</c> instead of this method directly.
    /// Returned <see cref="CodeActionInfo.Action"/>s must not replace the root node of the <see cref="DocumentEditor"/> they are passed (which edits the <paramref name="compilationUnitSyntax"/> passed here). If that is required, declare the operation as a <see cref="CodeActionInfo.PostFixActions"/> instead.
    /// </summary>
    /// <param name="document">The <see cref="Document"/> being fixed.</param>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="diagnostic">The <see cref="Diagnostic"/> to fix.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="CodeActionInfo"/> instances. Return an empty array to offer no fix.</returns>
    public abstract ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, Diagnostic diagnostic, CancellationToken cancellationToken);
}

public abstract class LaquaiLibRefactoring : CodeRefactoringProvider
{
    #region override
    public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        var document = context.Document;
        var compilationUnitSyntax = await document.GetRootAsync(context.CancellationToken).ConfigureAwait(false);

        var infos = await GetCodeActionInfosAsync(document, compilationUnitSyntax, context.Span, context.CancellationToken).ConfigureAwait(false);
        for (var i = 0; i < infos.Length; i++)
        {
            var (title, key, action) = infos[i];
            var postFixActions = infos[i].PostFixActions;
            context.RegisterRefactoring(CodeAction.Create(
                title: title,
                createChangedDocument: async c =>
                {
                    var editor = await DocumentEditor.CreateAsync(document, c).ConfigureAwait(false);
                    await action(editor).ConfigureAwait(false);
                    var changed = editor.GetChangedDocument();
                    return await Helpers.ApplyPostFixesAsync(changed, postFixActions, c).ConfigureAwait(false);
                },
                equivalenceKey: $"{Prefix}_{key}"
            ));
        }
    }
    #endregion

    private string Prefix => field ??= GetType().FullName;

    /// <summary>
    /// When overridden in a derived class, provides zero or more refactorings for the current selection span.
    /// Most refactorings should derive from <see cref="LaquaiLibTokenRefactoring"/>, <see cref="LaquaiLibNodeRefactoring"/> or <see cref="LaquaiLibOperationRefactoring"/> and override that class's <c>GetCodeActionInfos</c> instead of this method directly.
    /// Returned <see cref="CodeActionInfo.Action"/>s must not replace the root node of the <see cref="DocumentEditor"/>. If that is required, declare the operation as a <see cref="CodeActionInfo.PostFixActions"/> instead.
    /// </summary>
    /// <param name="document">The <see cref="Document"/> being refactored.</param>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="span">The <see cref="TextSpan"/> of the current selection.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="CodeActionInfo"/> instances. Return an empty array to offer no refactoring.</returns>
    public abstract ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, TextSpan span, CancellationToken cancellationToken);
}

/// <summary>
/// Provides a base class for code fix providers for analyzers that report diagnostics on <see cref="SyntaxToken"/>s.
/// </summary>
/// <param name="fixableDiagnosticIds">An <see cref="ImmutableArray{T}"/> of fixable diagnostic IDs.</param>
public abstract class LaquaiLibTokenFixer(params ImmutableArray<string> fixableDiagnosticIds) : LaquaiLibFixer(fixableDiagnosticIds)
{
    /// <summary>
    /// When overridden in a derived class, provides zero or more fixes for a specific <see cref="Diagnostic"/>.
    /// </summary>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="syntaxToken">The <see cref="SyntaxToken"/> on which <paramref name="diagnostic"/> was reported.</param>
    /// <param name="diagnostic">The <see cref="Diagnostic"/> to fix.</param>
    /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="CodeActionInfo"/> instances.</returns>
    public abstract ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxToken syntaxToken, Diagnostic diagnostic);
    /// <summary>
    /// Do not use. Override <see cref="GetCodeActionInfos(CompilationUnitSyntax, SyntaxToken, Diagnostic)"/> instead.
    /// </summary>
    public sealed override ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, Diagnostic diagnostic, CancellationToken cancellationToken)
        => new(GetCodeActionInfos(compilationUnitSyntax, compilationUnitSyntax.FindToken(diagnostic.Location.SourceSpan.Start), diagnostic));
}
/// <summary>
/// Provides a base class for code fix providers for analyzers that report diagnostics on <see cref="SyntaxNode"/>s.
/// </summary>
/// <param name="fixableDiagnosticIds">An <see cref="ImmutableArray{T}"/> of fixable diagnostic IDs.</param>
public abstract class LaquaiLibNodeFixer(params ImmutableArray<string> fixableDiagnosticIds) : LaquaiLibFixer(fixableDiagnosticIds)
{
    /// <summary>
    /// When overridden in a derived class, provides zero or more fixes for a specific <see cref="Diagnostic"/>.
    /// </summary>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="syntaxNode">The <see cref="SyntaxNode"/> on which <paramref name="diagnostic"/> was reported.</param>
    /// <param name="diagnostic">The <see cref="Diagnostic"/> to fix.</param>
    /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="CodeActionInfo"/> instances.</returns>
    public abstract ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, Diagnostic diagnostic);
    /// <summary>
    /// Do not use. Override <see cref="GetCodeActionInfos(CompilationUnitSyntax, SyntaxNode, Diagnostic)"/> instead.
    /// </summary>
    public sealed override ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, Diagnostic diagnostic, CancellationToken cancellationToken)
        => new(GetCodeActionInfos(compilationUnitSyntax, compilationUnitSyntax.FindNode(diagnostic.Location.SourceSpan), diagnostic));
}
/// <summary>
/// Provides a base class for code fix providers for analyzers that report diagnostics on nodes that map to an <see cref="IOperation"/>.
/// </summary>
/// <param name="fixableDiagnosticIds">An <see cref="ImmutableArray{T}"/> of fixable diagnostic IDs.</param>
public abstract class LaquaiLibOperationFixer(params ImmutableArray<string> fixableDiagnosticIds) : LaquaiLibFixer(fixableDiagnosticIds)
{
    /// <summary>
    /// When overridden in a derived class, provides zero or more fixes for a specific <see cref="Diagnostic"/>.
    /// </summary>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="operation">The <see cref="IOperation"/> the diagnostic was reported on, or <see langword="null"/> if the node at the diagnostic location does not map to an operation.</param>
    /// <param name="diagnostic">The <see cref="Diagnostic"/> to fix.</param>
    /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="CodeActionInfo"/> instances.</returns>
    public abstract ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, IOperation operation, Diagnostic diagnostic);
    /// <summary>
    /// Do not use. Override <see cref="GetCodeActionInfos(CompilationUnitSyntax, IOperation, Diagnostic)"/> instead.
    /// </summary>
    public sealed override async ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var operation = semanticModel.GetOperation(compilationUnitSyntax.FindNode(diagnostic.Location.SourceSpan), cancellationToken);
        return GetCodeActionInfos(compilationUnitSyntax, operation, diagnostic);
    }
}

/// <summary>
/// Provides a base class for refactoring providers that operate on the <see cref="SyntaxToken"/> at the start of the selection span.
/// </summary>
public abstract class LaquaiLibTokenRefactoring : LaquaiLibRefactoring
{
    /// <summary>
    /// When overridden in a derived class, provides zero or more refactorings for the <see cref="SyntaxToken"/> at the start of the selection.
    /// </summary>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="syntaxToken">The <see cref="SyntaxToken"/> at the start of <paramref name="span"/>.</param>
    /// <param name="span">The <see cref="TextSpan"/> of the current selection.</param>
    /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="CodeActionInfo"/> instances.</returns>
    public abstract ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxToken syntaxToken, TextSpan span);
    /// <summary>
    /// Do not use. Override <see cref="GetCodeActionInfos(CompilationUnitSyntax, SyntaxToken, TextSpan)"/> instead.
    /// </summary>
    public sealed override ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, TextSpan span, CancellationToken cancellationToken)
        => new(GetCodeActionInfos(compilationUnitSyntax, compilationUnitSyntax.FindToken(span.Start), span));
}
/// <summary>
/// Provides a base class for refactoring providers that operate on the <see cref="SyntaxNode"/> encompassing the selection span.
/// </summary>
public abstract class LaquaiLibNodeRefactoring : LaquaiLibRefactoring
{
    /// <summary>
    /// When overridden in a derived class, provides zero or more refactorings for the <see cref="SyntaxNode"/> encompassing the selection.
    /// </summary>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="syntaxNode">The <see cref="SyntaxNode"/> encompassing <paramref name="span"/>.</param>
    /// <param name="span">The <see cref="TextSpan"/> of the current selection.</param>
    /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="CodeActionInfo"/> instances.</returns>
    public abstract ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, SyntaxNode syntaxNode, TextSpan span);
    /// <summary>
    /// Do not use. Override <see cref="GetCodeActionInfos(CompilationUnitSyntax, SyntaxNode, TextSpan)"/> instead.
    /// </summary>
    public sealed override ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, TextSpan span, CancellationToken cancellationToken)
        => new(GetCodeActionInfos(compilationUnitSyntax, compilationUnitSyntax.FindNode(span), span));
}
/// <summary>
/// Provides a base class for refactoring providers that operate on the <see cref="IOperation"/> encompassing the selection span.
/// </summary>
public abstract class LaquaiLibOperationRefactoring : LaquaiLibRefactoring
{
    /// <summary>
    /// When overridden in a derived class, provides zero or more refactorings for the <see cref="IOperation"/> encompassing the selection.
    /// </summary>
    /// <param name="compilationUnitSyntax">The <see cref="CompilationUnitSyntax"/> of the document.</param>
    /// <param name="operation">The <see cref="IOperation"/> encompassing <paramref name="span"/>, or <see langword="null"/> if the node encompassing the selection does not map to an operation.</param>
    /// <param name="span">The <see cref="TextSpan"/> of the current selection.</param>
    /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="CodeActionInfo"/> instances.</returns>
    public abstract ImmutableArray<CodeActionInfo> GetCodeActionInfos(CompilationUnitSyntax compilationUnitSyntax, IOperation operation, TextSpan span);
    /// <summary>
    /// Do not use. Override <see cref="GetCodeActionInfos(CompilationUnitSyntax, IOperation, TextSpan)"/> instead.
    /// </summary>
    public sealed override async ValueTask<ImmutableArray<CodeActionInfo>> GetCodeActionInfosAsync(Document document, CompilationUnitSyntax compilationUnitSyntax, TextSpan span, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var operation = semanticModel.GetOperation(compilationUnitSyntax.FindNode(span), cancellationToken);
        return GetCodeActionInfos(compilationUnitSyntax, operation, span);
    }
}

/// <summary>
/// Encapsulates the information required to construct a <see cref="CodeAction"/> for a code fix or refactoring.
/// </summary>
public readonly partial struct CodeActionInfo
{
    public CodeActionInfo(string title, Func<DocumentEditor, ValueTask> action, string equivalenceKey = null, params ImmutableArray<PostFixAction> postFixActions)
    {
        Title = title;
        EquivalenceKey = !string.IsNullOrWhiteSpace(equivalenceKey) ? equivalenceKey : Helpers.NormalizeKey(title);
        Action = action;
        PostFixActions = postFixActions;
    }

    public void Deconstruct(out string title, out string equivalenceKey, out Func<DocumentEditor, ValueTask> action)
    {
        title = Title;
        equivalenceKey = EquivalenceKey;
        action = Action;
    }
    /// <summary>
    /// The title of the code action.
    /// </summary>
    public string Title { get; }
    /// <summary>
    /// The equivalence key for the code action. Need not be set, in which case it is automatically generated from <see cref="Title"/>.
    /// </summary>
    public string EquivalenceKey { get; }
    /// <summary>
    /// A <see langword="delegate"/> that uses a <see cref="DocumentEditor"/> to produce a new <see cref="Document"/> that contains the fix or refactoring.
    /// </summary>
    public Func<DocumentEditor, ValueTask> Action { get; }
    /// <summary>
    /// A collection of <see cref="PostFixAction"/>s to run, in order, after <see cref="Action"/> has been applied. Use these for changes incompatible with the <see cref="DocumentEditor"/>, for example replacing the document's root node. Each is <see langword="await"/>ed and passed the result of the previous one.
    /// </summary>
    public ImmutableArray<PostFixAction> PostFixActions { get; }
}

public delegate ValueTask<Document> PostFixAction(Document document, CancellationToken cancellationToken);
/// <summary>
/// Contains factory methods that produce <see cref="PostFixAction"/>s for common operations.
/// </summary>
public static class WellKnownPostFixActions
{
    /// <summary>
    /// Creates a <see cref="PostFixAction"/> that adds the specified <paramref name="usings"/> to the document, skipping any that are already present.
    /// </summary>
    public static PostFixAction AddUsings(params string[] usings) => async (document, cancellationToken) =>
    {
        var compilationUnitSyntax = await document.GetRootAsync(cancellationToken).ConfigureAwait(false);
        var newUsings = new HashSet<string>(usings);
        newUsings.ExceptWith(compilationUnitSyntax.Usings.Select(static u => u.Name.ToString()));
        if (newUsings.Count == 0)
        {
            return document;
        }
        var directives = newUsings.Select(static u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u))).ToArray();
        return document.WithSyntaxRoot(compilationUnitSyntax.AddUsings(directives));
    };
}
