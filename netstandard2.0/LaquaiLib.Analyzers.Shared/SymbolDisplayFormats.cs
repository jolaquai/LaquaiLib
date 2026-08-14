using Microsoft.CodeAnalysis;

namespace LaquaiLib.Analyzers.Shared;

/// <summary>
/// Provides <see cref="SymbolDisplayFormat"/> instances used to emit fully <c>global::</c>-qualified source text.
/// </summary>
public static class SymbolDisplayFormats
{
    /// <summary>
    /// Fully qualifies types with a <c>global::</c> prefix, for use where no parameter list is involved.
    /// </summary>
    public static readonly SymbolDisplayFormat FullyQualified = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
            | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );

    /// <summary>
    /// Fully qualifies types with a <c>global::</c> prefix, for use on <see cref="IParameterSymbol"/>s so full parameter lists are emitted correctly.
    /// </summary>
    public static readonly SymbolDisplayFormat FullyQualifiedParameter = new SymbolDisplayFormat(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType
            | SymbolDisplayParameterOptions.IncludeName
            | SymbolDisplayParameterOptions.IncludeParamsRefOut
            | SymbolDisplayParameterOptions.IncludeDefaultValue,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
            | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
    );
}
