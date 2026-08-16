using LaquaiLib.Analyzers.Shared;

using System.Reflection;

namespace LaquaiLib.Analyzers.Validity__9XXX_;

/// <summary>
/// Validates <see langword="[UnsafeAccessor]"/> declarations.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnsafeAccessorValidators : DiagnosticAnalyzer
{
    /// <summary>
    /// "The target type is missing this member"
    /// </summary>
    public static DiagnosticDescriptor MissingMemberDescriptor { get; } = new(
        id: "LAQ9001",
        title: "The target type is missing this member",
        messageFormat: "The {1} '{2} {3}' does not exist in type '{0}'",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "The target type is missing this method"
    /// </summary>
    public static DiagnosticDescriptor MissingMethodDescriptor { get; } = new(
        id: "LAQ9001",
        title: "The target type is missing this method",
        messageFormat: "The type '{0}' does not have a method that matches the signature '{1} {2}{3}({4})'",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "The target type is missing this constructor"
    /// </summary>
    public static DiagnosticDescriptor MissingCtorDescriptor { get; } = new(
        id: "LAQ9001",
        title: "The target type is missing this constructor",
        messageFormat: "The type '{0}' does not have a constructor that matches the signature '.ctor({1})'",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "Missing modifier in accessor declaration"
    /// </summary>
    public static DiagnosticDescriptor InvalidDeclarationDescriptor { get; } = new(
        id: "LAQ9002",
        title: "Missing modifier in [UnsafeAccessor] method declaration",
        messageFormat: "[UnsafeAccessor] methods must be static extern",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "Incorrect return type for accessor"
    /// </summary>
    public static DiagnosticDescriptor IncorrectReturnTypeDescriptor { get; } = new(
        id: "LAQ9003",
        title: "Incorrect return type for [UnsafeAccessor] method",
        messageFormat: "The return type of this '{0}' [UnsafeAccessor] method must be exactly '{1}'",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "Accessing any instance member on a value type requires passing it as 'ref'"
    /// </summary>
    public static DiagnosticDescriptor InstanceMemberOnStructRequiresRefDescriptor { get; } = new(
        id: "LAQ9004",
        title: "Accessing any instance member on a value type requires passing it by-ref",
        messageFormat: "Accessing any instance member on a value type requires passing it by-ref",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "The name of a property [UnsafeAccessor] must have an accessor prefix"
    /// </summary>
    public static DiagnosticDescriptor InvalidPropertyAccessorNameDescriptor { get; } = new(
        id: "LAQ9005",
        title: "The name of a property [UnsafeAccessor] must have an accessor prefix",
        messageFormat: "The name of a property [UnsafeAccessor] must have an accessor prefix",
        description: "This [UnsafeAccessor] method attempted to match an accessor method of a property, but did not specify a property accessor prefix in its target's name (like 'get_').",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "Invalid [UnsafeAccessor] declaration for property target"
    /// </summary>
    public static DiagnosticDescriptor InvalidPropertyAccessorDescriptor { get; } = new(
        id: "LAQ9005",
        title: "Invalid [UnsafeAccessor] declaration for property target",
        messageFormat: "An [UnsafeAccessor] that targets a property must specify UnsafeAccessorKind.Method and have an accessor prefix",
        description: "This [UnsafeAccessor] method did not exactly match the declared target, but would have matched a property of the same name. To resolve this, specify UnsafeAccessorKind.Method and prefix the name of the method with the accessor method to target, or change the name to an existing member.",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "Missing mandatory parameter for accessor method"
    /// </summary>
    public static DiagnosticDescriptor MissingTargetTypeDescriptor { get; } = new(
        id: "LAQ9006",
        title: "Missing mandatory parameter for [UnsafeAccessor] method",
        messageFormat: "An [UnsafeAccessor] method requires at least one parameter to define its target type",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "Missing mandatory parameter for accessor method"
    /// </summary>
    public static DiagnosticDescriptor MissingTargetTypeForCtorDescriptor { get; } = new(
        id: "LAQ9007",
        title: "Missing target type for constructor [UnsafeAccessor] method",
        messageFormat: "A constructor [UnsafeAccessor] method requires a non-void return type",
        description: "To invoke instance constructors of the target type as methods on existing objects, specify the attribute as '[UnsafeAccessor(UnsafeAccessorKind.Method, Name = \".ctor\")]', make the first parameter on the adorned method of the target type and specify any contructor parameters after. Note that this will most likely require obtaining uninitialized instances.",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "Type parameter mismatch between the target type and the type containing this [UnsafeAccessor] method"
    /// </summary>
    public static DiagnosticDescriptor ContainingTypeTypeParameterMismatchDescriptor { get; } = new(
        id: "LAQ9008",
        title: "Type parameter mismatch between the target type and the type containing this [UnsafeAccessor] method",
        messageFormat: "The type parameters of the type containing this [UnsafeAccessor] method ({1}) must match the target type's ({0}) in arity, order and constraints",
        description: "The type parameters of the [UnsafeAccessor] method and its containing type respectively must match the type parameters on the target type and the target method exactly, in arity, order and constraints. Typically, this diagnostic indicates that the mismatched type parameter(s) in question should have been placed on the [UnsafeAccessor] method, not the containing type.",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    /// <summary>
    /// "Type parameter mismatch between the target method and the [UnsafeAccessor] method"
    /// </summary>
    public static DiagnosticDescriptor MethodTypeParameterMismatchDescriptor { get; } = new(
        id: "LAQ9008",
        title: "Type parameter mismatch between the target method and the [UnsafeAccessor] method",
        messageFormat: "The type parameters of the [UnsafeAccessor] method ({1}) must match the target method's ({0}) in arity, order and constraints",
        description: "The type parameters of the [UnsafeAccessor] method and its containing type respectively must match the type parameters on the target type and the target method exactly, in arity, order and constraints. Typically, this diagnostic indicates that the mismatched type parameter(s) in question should have been placed on the containing type, not the [UnsafeAccessor] method.",
        category: AnalyzerCategories.Validity,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    [
        MissingMemberDescriptor,
        MissingMethodDescriptor,
        MissingCtorDescriptor,
        InvalidDeclarationDescriptor,
        IncorrectReturnTypeDescriptor,
        InstanceMemberOnStructRequiresRefDescriptor,
        InvalidPropertyAccessorNameDescriptor,
        InvalidPropertyAccessorDescriptor,
        MissingTargetTypeDescriptor,
        MissingTargetTypeForCtorDescriptor,
        ContainingTypeTypeParameterMismatchDescriptor,
        MethodTypeParameterMismatchDescriptor,
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private const string UnsafeAccessorTypeAttributeName = "System.Runtime.CompilerServices.UnsafeAccessorTypeAttribute";
    /// <summary>
    /// Determines whether any position of <paramref name="method"/> carries <c>[UnsafeAccessorType]</c>, which decouples the declared type from the one the runtime binds against.
    /// </summary>
    private static bool HasErasedTypes(IMethodSymbol method)
    {
        var returnAttributes = method.GetReturnTypeAttributes();
        for (var i = 0; i < returnAttributes.Length; i++)
        {
            if (returnAttributes[i].AttributeClass?.ToDisplayString() == UnsafeAccessorTypeAttributeName)
            {
                return true;
            }
        }
        var parameters = method.Parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            var attributes = parameters[i].GetAttributes();
            for (var k = 0; k < attributes.Length; k++)
            {
                if (attributes[k].AttributeClass?.ToDisplayString() == UnsafeAccessorTypeAttributeName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var uaaMethodDeclarationSyntax = Unsafe.As<MethodDeclarationSyntax>(context.Node);
        var semanticModel = context.SemanticModel;
        var compilation = context.Compilation;

        var reportLocation = uaaMethodDeclarationSyntax.Identifier.GetLocation();

        // Quick syntax check before expensive semantic model lookup
        if (!uaaMethodDeclarationSyntax.AttributeLists.Any())
        {
            return;
        }

        var uaaMethodSymbol = context.SemanticModel.GetDeclaredSymbol(uaaMethodDeclarationSyntax);
        if (uaaMethodSymbol == null)
        {
            return;
        }

        if (uaaMethodSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.ToDisplayString() == "System.Runtime.CompilerServices.UnsafeAccessorAttribute") is not { } uaaData)
        {
            return;
        }

        // [UnsafeAccessorType] replaces a declared type with a reflection name resolved at runtime, so the declared signature deliberately no longer matches the target's
        if (HasErasedTypes(uaaMethodSymbol))
        {
            return;
        }

        var unsafeAccessorKind = (UnsafeAccessorKind)(int)uaaData.ConstructorArguments[0].Value!;
        var description = typeof(UnsafeAccessorKind).GetField(unsafeAccessorKind.ToString())!.GetCustomAttribute<DescriptionAttribute>()!.Description;

        var targetMemberName = uaaMethodDeclarationSyntax.Identifier.ToString();
        // Explicit name overrides method name
        if (uaaData.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value is string actualName)
        {
            targetMemberName = actualName;
        }

        var uaaParameters = uaaMethodSymbol.Parameters;
        var uaaTypeParameters = uaaMethodSymbol.TypeParameters;

        var uaaThisParam = uaaParameters.FirstOrDefault();

        // Constructors have no mandatory "this" parameter; the target type comes from the return type instead
        if (unsafeAccessorKind is not UnsafeAccessorKind.Constructor && uaaThisParam is null)
        {
            var missingTargetDiag = Diagnostic.Create(MissingTargetTypeDescriptor, reportLocation);
            context.ReportDiagnostic(missingTargetDiag);
            return;
        }

        var targetTypeSymbol = uaaThisParam?.Type;
        var uaaReturnTypeSymbol = uaaMethodSymbol.ReturnType;

        var uaaRestParams = uaaParameters.Skip(1).ToImmutableArray();
        var signatureString = string.Join(", ", uaaRestParams.Select(p => p.Type.ToDisplayString()));

        // On constructors, the target type is actually the return type
        if (unsafeAccessorKind is not UnsafeAccessorKind.Constructor)
        {
            if (!uaaMethodSymbol.IsStatic || !uaaMethodSymbol.IsExtern)
            {
                var diag = Diagnostic.Create(InvalidDeclarationDescriptor, reportLocation, description);
                context.ReportDiagnostic(diag);
            }
        }
        else
        {
            // else, enfore 'static extern ref'
            targetTypeSymbol = uaaMethodSymbol.ReturnType;
            uaaRestParams = uaaParameters;
            signatureString = string.Join(", ", uaaRestParams.Select(p => p.Type.ToDisplayString()));

            if (uaaMethodSymbol.ReturnsVoid)
            {
                if (uaaThisParam is null)
                {
                    var diag = Diagnostic.Create(MissingTargetTypeForCtorDescriptor, reportLocation);
                    context.ReportDiagnostic(diag);
                }
                else
                {
                    var diag = Diagnostic.Create(IncorrectReturnTypeDescriptor, reportLocation, "constructor", uaaThisParam.Type.ToDisplayString());
                    context.ReportDiagnostic(diag);
                }
                return;
            }

            // Check for existence of the member
            var constructors = (targetTypeSymbol as INamedTypeSymbol)?.Constructors;
            if (constructors is null)
            {
                var diag = Diagnostic.Create(MissingCtorDescriptor, reportLocation, targetTypeSymbol.ToDisplayString(), signatureString);
                context.ReportDiagnostic(diag);
                return;
            }

            if (!constructors.Value.Any(ctor => ctor.Parameters.Select(p => p.Type).SequenceEqual(uaaRestParams.Select(p => p.Type), SymbolEqualityComparer.Default)))
            {
                var diag = Diagnostic.Create(MissingCtorDescriptor, reportLocation, targetTypeSymbol.ToDisplayString(), signatureString);
                context.ReportDiagnostic(diag);
                return;
            }
        }

        if (!SymbolEqualityComparer.Default.Equals(compilation.Assembly, targetTypeSymbol.ContainingAssembly))
        {
            // Can't analyze types that aren't in Compilation's assembly, so we switch over to the Reflection API instead
            // All further checks happen there
            // Bail though if we can't even resolve the type
            if (targetTypeSymbol.RuntimeType is not { } typeInstance)
            {
                return;
            }

            if (typeInstance is not null)
            {
                CheckReflection(context, uaaMethodDeclarationSyntax, reportLocation, uaaMethodSymbol, unsafeAccessorKind, description, targetMemberName, uaaParameters, uaaTypeParameters, uaaThisParam, typeInstance, uaaReturnTypeSymbol, uaaRestParams, signatureString);
            }
            return;
        }

        CheckRoslyn(context, uaaMethodDeclarationSyntax, reportLocation, uaaMethodSymbol, unsafeAccessorKind, description, targetMemberName, uaaParameters, uaaTypeParameters, uaaThisParam, targetTypeSymbol, uaaReturnTypeSymbol, uaaRestParams, signatureString);
    }

    private static void CheckRoslyn(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax uaaMethodDeclarationSyntax, Location reportLocation, IMethodSymbol uaaMethodSymbol, UnsafeAccessorKind unsafeAccessorKind, string description, string targetMemberName, ImmutableArray<IParameterSymbol> uaaParameters, ImmutableArray<ITypeParameterSymbol> uaaTypeParameters, IParameterSymbol uaaThisParam, ITypeSymbol targetTypeSymbol, ITypeSymbol uaaReturnTypeSymbol, ImmutableArray<IParameterSymbol> uaaRestParams, string signatureString)
    {
        // Constructors have no mandatory "this" parameter, so an empty parameter list is legitimate there
        if (unsafeAccessorKind is not UnsafeAccessorKind.Constructor && uaaParameters.Length == 0)
        {
            var diag = Diagnostic.Create(MissingTargetTypeDescriptor, reportLocation);
            context.ReportDiagnostic(diag);
            return;
        }

        switch (unsafeAccessorKind)
        {
            case UnsafeAccessorKind.Method when targetMemberName == ".ctor":
            {
                // Check for existence of the member
                var ctors = (targetTypeSymbol as INamedTypeSymbol)?.Constructors;
                if (ctors is null)
                {
                    var diag = Diagnostic.Create(MissingCtorDescriptor, reportLocation, targetTypeSymbol.ToDisplayString(), signatureString);
                    context.ReportDiagnostic(diag);
                    return;
                }

                if (!ctors.Value.Any(ctor => ctor.Parameters.Select(p => p.Type).SequenceEqual(uaaRestParams.Select(p => p.Type), SymbolEqualityComparer.Default)))
                {
                    var diag = Diagnostic.Create(MissingCtorDescriptor, reportLocation, targetTypeSymbol.ToDisplayString(), signatureString);
                    context.ReportDiagnostic(diag);
                    return;
                }

                // Check for correct return type
                if (!uaaMethodSymbol.ReturnsVoid)
                {
                    var diag = Diagnostic.Create(IncorrectReturnTypeDescriptor, reportLocation, "constructor", "void");
                    context.ReportDiagnostic(diag);
                }
                break;
            }
            case UnsafeAccessorKind.Method:
            {
                // Check for existence of the member
                var methods = targetTypeSymbol!.GetMembers(targetMemberName).OfType<IMethodSymbol>().Where(m => !m.IsStatic).ToArray();
                var flowControl = CheckMethodsRoslyn(context, reportLocation, targetMemberName, uaaTypeParameters, uaaThisParam, targetTypeSymbol, uaaReturnTypeSymbol, uaaRestParams, signatureString, methods);
                if (!flowControl)
                {
                    return;
                }
                break;
            }
            case UnsafeAccessorKind.StaticMethod:
            {
                // Check for existence of the member
                var methods = targetTypeSymbol!.GetMembers(targetMemberName).OfType<IMethodSymbol>().Where(m => m.IsStatic).ToArray();
                var flowControl = CheckMethodsRoslyn(context, reportLocation, targetMemberName, uaaTypeParameters, uaaThisParam, targetTypeSymbol, uaaReturnTypeSymbol, uaaRestParams, signatureString, methods);
                if (!flowControl)
                {
                    return;
                }
                break;
            }
            case UnsafeAccessorKind.Field:
            {
                // Check for existence of the member
                var fields = targetTypeSymbol!.GetMembers(targetMemberName).OfType<IFieldSymbol>().Where(f => !f.IsStatic).ToArray();
                var flowControl = CheckFieldsRoslyn(context, reportLocation, description, targetMemberName, uaaThisParam, targetTypeSymbol, uaaReturnTypeSymbol, fields);
                if (!flowControl)
                {
                    return;
                }

                break;
            }
            case UnsafeAccessorKind.StaticField:
            {
                // Check for existence of the member
                var fields = targetTypeSymbol!.GetMembers(targetMemberName).OfType<IFieldSymbol>().Where(f => f.IsStatic).ToArray();
                var flowControl = CheckFieldsRoslyn(context, reportLocation, description, targetMemberName, uaaThisParam, targetTypeSymbol, uaaReturnTypeSymbol, fields);
                if (!flowControl)
                {
                    return;
                }

                break;
            }
        }

        // Beyond all other checks, the accessed type's type parameters must match the type parameters of the type containing the [UnsafeAccessor] method
        var requiredTypeParams = targetTypeSymbol is INamedTypeSymbol nts ? nts.TypeParameters : [];
        var containingTypeTypeParams = uaaMethodSymbol.ContainingType.TypeParameters;
        if (!TypeParametersEqual(requiredTypeParams, containingTypeTypeParams))
        {
            var reqNames = requiredTypeParams.Length == 0 ? "none" : $"<{string.Join(", ", requiredTypeParams.Select(tp => tp.ToDisplayString()))}>";
            var actualNames = containingTypeTypeParams.Length == 0 ? "none" : $"<{string.Join(", ", containingTypeTypeParams.Select(tp => tp.ToDisplayString()))}>";

            var containingTypeDecl = uaaMethodDeclarationSyntax.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            IEnumerable<Location> moreLocs = [];
            if (containingTypeDecl is not null)
            {
                moreLocs = [containingTypeDecl.Identifier.GetLocation()];
            }
            var diag = Diagnostic.Create(ContainingTypeTypeParameterMismatchDescriptor, reportLocation, moreLocs, reqNames, actualNames);
            context.ReportDiagnostic(diag);
            return;
        }
    }
    private static bool CheckFieldsRoslyn(SyntaxNodeAnalysisContext context, Location reportLocation, string description, string memberName, IParameterSymbol uaaThisParam, ITypeSymbol targetType, ITypeSymbol uaaReturnTypeSymbol, IFieldSymbol[] fieldSymbols)
    {
        if (fieldSymbols.FirstOrDefault(f => SymbolEqualityComparer.Default.Equals(f.Type, uaaReturnTypeSymbol)) is not { } targetFieldSymbol)
        {
            // The field may be missing, but there may be a property accessor with a prefix that could match
            var properties = targetType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic).ToArray();
            var events = targetType.GetMembers().OfType<IEventSymbol>().Where(e => !e.IsStatic).ToArray();
            // Bit of a disgusting check since only a get_ could ever match a field, but whatever
            if (properties.Any(p => p.GetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true || p.SetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true))
            {
                var diag = Diagnostic.Create(InvalidPropertyAccessorDescriptor, reportLocation);
                context.ReportDiagnostic(diag);
                return false;
            }
            else if (events.Any(e => e.AddMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true || e.RemoveMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true))
            {
                var diag = Diagnostic.Create(InvalidPropertyAccessorDescriptor, reportLocation);
                context.ReportDiagnostic(diag);
                return false;
            }
            else
            {
                var diag = Diagnostic.Create(MissingMemberDescriptor, reportLocation, targetType.ToDisplayString(), description, uaaReturnTypeSymbol.ToDisplayString(), memberName);
                context.ReportDiagnostic(diag);
                return false;
            }
        }

        if (!fieldSymbols.Any(f => SymbolEqualityComparer.Default.Equals(f.Type, uaaReturnTypeSymbol)))
        {
            var diag = Diagnostic.Create(MissingMemberDescriptor, reportLocation, targetType.ToDisplayString(), description, uaaReturnTypeSymbol.ToDisplayString(), memberName);
            context.ReportDiagnostic(diag);
            return false;
        }

        // If struct, thisParam must be ref
        if (targetType.IsValueType && !uaaThisParam.RefKind.HasFlag(RefKind.Ref))
        {
            var diag = Diagnostic.Create(InstanceMemberOnStructRequiresRefDescriptor, reportLocation);
            context.ReportDiagnostic(diag);
            return false;
        }

        return true;
    }
    /// <summary>
    /// Constructs <paramref name="method"/> over <paramref name="typeParameters"/> when arities match, so its own type parameters become symbol-comparable against the accessor's.
    /// </summary>
    private static IMethodSymbol SubstituteTypeParameters(IMethodSymbol method, ImmutableArray<ITypeParameterSymbol> typeParameters)
        => method.TypeParameters.Length > 0 && method.TypeParameters.Length == typeParameters.Length
            ? method.Construct([.. typeParameters.CastArray<ITypeSymbol>()])
            : method;
    private static bool CheckMethodsRoslyn(SyntaxNodeAnalysisContext context, Location reportLocation, string memberName, ImmutableArray<ITypeParameterSymbol> uaaTypeParameters, IParameterSymbol uaaThisParam, ITypeSymbol targetTypeSymbol, ITypeSymbol uaaReturnTypeSymbol, ImmutableArray<IParameterSymbol> uaaRestParams, string signatureString, IMethodSymbol[] methodSymbols)
    {
        // Intentionally omitting type parameter check here so we can later differentiate between missing method and type parameter mismatch
        // Construct same-arity generic candidates over the accessor's own type parameters first, so symbol comparison below is meaningful
        // (an ITypeParameterSymbol from the target's declaration is never SymbolEqualityComparer-equal to the accessor's own)
        var substitutedMethods = methodSymbols.Select(m => SubstituteTypeParameters(m, uaaTypeParameters)).ToArray();
        var targetMethodSymbol = substitutedMethods.FirstOrDefault(m =>
            m.Parameters.Select(p => p.Type).SequenceEqual(uaaRestParams.Select(p => p.Type), SymbolEqualityComparer.Default)
            && SymbolEqualityComparer.Default.Equals(uaaReturnTypeSymbol, m.ReturnType)
        );
        targetMethodSymbol ??= substitutedMethods.FirstOrDefault(m =>
            m.Parameters.Select(p => p.Type).SequenceEqual(uaaRestParams.Select(p => p.Type), SymbolEqualityComparer.Default)
        );

        if (targetMethodSymbol is not null)
        {
            // Check for mismatched type parameters - read off the original definition since targetMethodSymbol may be a constructed substitution
            var requiredTypeParams = targetMethodSymbol.OriginalDefinition.TypeParameters;
            if (!TypeParametersEqual(requiredTypeParams, uaaTypeParameters))
            {
                var reqNames = requiredTypeParams.Length == 0 ? "none" : $"<{string.Join(", ", requiredTypeParams.Select(tp => tp.ToDisplayString()))}>";
                var actualNames = uaaTypeParameters.Length == 0 ? "none" : $"<{string.Join(", ", uaaTypeParameters.Select(tp => tp.ToDisplayString()))}>";

                var diag = Diagnostic.Create(MethodTypeParameterMismatchDescriptor, reportLocation, reqNames, actualNames);
                context.ReportDiagnostic(diag);
                return false;
            }

            // Check for correct return type
            if (!SymbolEqualityComparer.Default.Equals(uaaReturnTypeSymbol, targetMethodSymbol.ReturnType))
            {
                var diag = Diagnostic.Create(IncorrectReturnTypeDescriptor, reportLocation, "method", targetMethodSymbol.ReturnType.ToDisplayString());
                context.ReportDiagnostic(diag);
                return false;
            }
        }
        else
        {
            // The method may be missing, but there may be a property accessor with a prefix that could match
            var properties = targetTypeSymbol.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic).ToArray();
            var events = targetTypeSymbol.GetMembers().OfType<IEventSymbol>().Where(e => !e.IsStatic).ToArray();
            if (properties.Any(p => (p.GetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true || p.SetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true)
                && p.Parameters.Select(p => p.Type).SequenceEqual(uaaRestParams.Select(p => p.Type), SymbolEqualityComparer.Default))
                || events.Any(e => (e.AddMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true || e.RemoveMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true)
                && uaaRestParams.Length == 1 && SymbolEqualityComparer.Default.Equals(uaaRestParams[0], e.Type))
            )
            {
                var diag = Diagnostic.Create(InvalidPropertyAccessorNameDescriptor, reportLocation);
                context.ReportDiagnostic(diag);
                return false;
            }
            else
            {
                var diag = Diagnostic.Create(MissingMethodDescriptor, reportLocation, targetTypeSymbol.ToDisplayString(),
                    uaaReturnTypeSymbol.ToDisplayString(),
                    memberName,
                    uaaTypeParameters.Length > 0 ? $"<{string.Join(", ", uaaTypeParameters.Select(t => t.ToDisplayString()))}>" : "",
                    signatureString);
                context.ReportDiagnostic(diag);
                return false;
            }
        }

        // If struct, thisParam must be ref
        if (targetTypeSymbol.IsValueType && !uaaThisParam.RefKind.HasFlag(RefKind.Ref))
        {
            var diag = Diagnostic.Create(InstanceMemberOnStructRequiresRefDescriptor, reportLocation);
            context.ReportDiagnostic(diag);
            return false;
        }

        return true;
    }

    private static void CheckReflection(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax uaaMethodDeclarationSyntax, Location reportLocation, IMethodSymbol uaaMethodSymbol, UnsafeAccessorKind unsafeAccessorKind, string description, string targetMemberName, ImmutableArray<IParameterSymbol> uaaParameters, ImmutableArray<ITypeParameterSymbol> uaaTypeParameters, IParameterSymbol uaaThisParam, Type targetType, ITypeSymbol uaaReturnTypeSymbol, ImmutableArray<IParameterSymbol> uaaRestParams, string signatureString)
    {
        // Constructors have no mandatory "this" parameter, so an empty parameter list is legitimate there
        if (unsafeAccessorKind is not UnsafeAccessorKind.Constructor && uaaParameters.Length == 0)
        {
            var diag = Diagnostic.Create(MissingTargetTypeDescriptor, reportLocation);
            context.ReportDiagnostic(diag);
            return;
        }

        switch (unsafeAccessorKind)
        {
            case UnsafeAccessorKind.Method when targetMemberName == ".ctor":
            {
                // Check for existence of the member
                var ctors = targetType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (ctors is null)
                {
                    var diag = Diagnostic.Create(MissingCtorDescriptor, reportLocation, targetType.ToDisplayString(), signatureString);
                    context.ReportDiagnostic(diag);
                    return;
                }

                if (!ctors.Any(ctor => ParametersEqual(ctor.GetParameters(), uaaRestParams, context.SemanticModel)))
                {
                    var diag = Diagnostic.Create(MissingCtorDescriptor, reportLocation, targetType.ToDisplayString(), signatureString);
                    context.ReportDiagnostic(diag);
                    return;
                }

                // Check for correct return type
                if (uaaMethodSymbol.ReturnType.SpecialType == SpecialType.System_Void)
                {
                    var diag = Diagnostic.Create(IncorrectReturnTypeDescriptor, reportLocation, "constructor", "void");
                    context.ReportDiagnostic(diag);
                }
                break;
            }
            case UnsafeAccessorKind.Method:
            {
                // Check for existence of the member
                var methods = targetType!.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.Name.Equals(targetMemberName, StringComparison.Ordinal))
                    .ToArray();
                var flowControl = CheckMethodsReflection(context, reportLocation, uaaMethodSymbol, targetMemberName, uaaTypeParameters, uaaThisParam, targetType, uaaReturnTypeSymbol, uaaRestParams, signatureString, methods);
                if (!flowControl)
                {
                    return;
                }
                break;
            }
            case UnsafeAccessorKind.StaticMethod:
            {
                // Check for existence of the member
                var methods = targetType!.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.Name.Equals(targetMemberName, StringComparison.Ordinal))
                    .ToArray();
                var flowControl = CheckMethodsReflection(context, reportLocation, uaaMethodSymbol, targetMemberName, uaaTypeParameters, uaaThisParam, targetType, uaaReturnTypeSymbol, uaaRestParams, signatureString, methods);
                if (!flowControl)
                {
                    return;
                }
                break;
            }
            case UnsafeAccessorKind.Field:
            {
                // Check for existence of the member
                var fields = targetType!.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.Name.Equals(targetMemberName, StringComparison.Ordinal))
                    .ToArray();
                var flowControl = CheckFieldsReflection(context, reportLocation, description, targetMemberName, uaaThisParam, targetType, uaaReturnTypeSymbol, fields);
                if (!flowControl)
                {
                    return;
                }

                break;
            }
            case UnsafeAccessorKind.StaticField:
            {
                // Check for existence of the member
                var fields = targetType!.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.Name.Equals(targetMemberName, StringComparison.Ordinal))
                    .ToArray();
                var flowControl = CheckFieldsReflection(context, reportLocation, description, targetMemberName, uaaThisParam, targetType, uaaReturnTypeSymbol, fields);
                if (!flowControl)
                {
                    return;
                }

                break;
            }
        }

        // Beyond all other checks, the accessed type's type parameters must match the type parameters of the type containing the [UnsafeAccessor] method

        // We want type parameters, not arguments
        var requiredTypeParams = (targetType.IsConstructedGenericType ? targetType.GetGenericTypeDefinition() : targetType).GetGenericArguments();
        var containingTypeTypeParams = uaaMethodSymbol.ContainingType.TypeParameters;

        if (!TypeParametersEqual(requiredTypeParams, containingTypeTypeParams))
        {
            var reqNames = requiredTypeParams.Length == 0 ? "none" : $"<{string.Join(", ", requiredTypeParams.Select(tp => tp.ToDisplayString()))}>";
            var actualNames = containingTypeTypeParams.Length == 0 ? "none" : $"<{string.Join(", ", containingTypeTypeParams.Select(tp => tp.ToDisplayString()))}>";

            var containingTypeDecl = uaaMethodDeclarationSyntax.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            IEnumerable<Location> moreLocs = [];
            if (containingTypeDecl is not null)
            {
                moreLocs = [containingTypeDecl.Identifier.GetLocation()];
            }
            var diag = Diagnostic.Create(ContainingTypeTypeParameterMismatchDescriptor, reportLocation, moreLocs, reqNames, actualNames);
            context.ReportDiagnostic(diag);
            return;
        }
    }
    private static bool CheckFieldsReflection(SyntaxNodeAnalysisContext context, Location reportLocation, string description, string memberName, IParameterSymbol uaaThisParam, Type targetType, ITypeSymbol uaaReturnTypeSymbol, FieldInfo[] fieldInfos)
    {
        if (fieldInfos.FirstOrDefault(f => TypesEqual(f.FieldType, uaaReturnTypeSymbol, context.SemanticModel)) is not { } targetFieldSymbol)
        {
            // The field may be missing, but there may be a property accessor with a prefix that could match
            var properties = targetType.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToArray();
            var events = targetType.GetEvents(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToArray();
            // Bit of a disgusting check since only a get_ could ever match a field, but whatever
            if (properties.Any(p => p.GetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true || p.SetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true))
            {
                var diag = Diagnostic.Create(InvalidPropertyAccessorDescriptor, reportLocation);
                context.ReportDiagnostic(diag);
                return false;
            }
            else if (events.Any(e => e.AddMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true || e.RemoveMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true))
            {
                var diag = Diagnostic.Create(InvalidPropertyAccessorDescriptor, reportLocation);
                context.ReportDiagnostic(diag);
                return false;
            }
            else
            {
                var diag = Diagnostic.Create(MissingMemberDescriptor, reportLocation, targetType.ToDisplayString(), description, uaaReturnTypeSymbol.ToDisplayString(), memberName);
                context.ReportDiagnostic(diag);
                return false;
            }
        }

        if (!fieldInfos.Any(f => TypesEqual(f.FieldType, uaaReturnTypeSymbol, context.SemanticModel)))
        {
            var diag = Diagnostic.Create(MissingMemberDescriptor, reportLocation, targetType.ToDisplayString(), description, uaaReturnTypeSymbol.ToDisplayString(), memberName);
            context.ReportDiagnostic(diag);
            return false;
        }

        // If struct, thisParam must be ref
        if (targetType.IsValueType && !uaaThisParam.RefKind.HasFlag(RefKind.Ref))
        {
            var diag = Diagnostic.Create(InstanceMemberOnStructRequiresRefDescriptor, reportLocation);
            context.ReportDiagnostic(diag);
            return false;
        }

        return true;
    }
    private static bool CheckMethodsReflection(SyntaxNodeAnalysisContext context, Location reportLocation, IMethodSymbol uaaMethodSymbol, string memberName, ImmutableArray<ITypeParameterSymbol> typeParameters, IParameterSymbol thisParam, Type targetType, ITypeSymbol uaaReturnType, ImmutableArray<IParameterSymbol> restParams, string signatureString, MethodInfo[] methodInfos)
    {
        // Intentionally omitting type parameter check here so we can later differentiate between missing method and type parameter mismatch
        // Restrict candidates to matching generic arity up front, otherwise a same-named non-matching-arity overload could win by accident
        var candidateMethodInfos = methodInfos.Where(m => (m.IsGenericMethod ? m.GetGenericArguments().Length : 0) == typeParameters.Length).ToArray();
        var targetMethodInfo = candidateMethodInfos.FirstOrDefault(m =>
            ParametersEqual(m.GetParameters(), restParams, context.SemanticModel)
            && TypesEqual(m.ReturnType, uaaReturnType, context.SemanticModel)
        );
        targetMethodInfo ??= candidateMethodInfos.FirstOrDefault(m =>
            ParametersEqual(m.GetParameters(), restParams, context.SemanticModel)
        );
        // No arbitrary same-named-overload fallback here: falling through to the 'else' branch below
        // correctly reports a missing-method diagnostic instead of a bogus return-type mismatch against an unrelated overload

        if (targetMethodInfo is not null)
        {
            // Check for mismatched type parameters
            var requiredTypeParams = targetMethodInfo.IsGenericMethod ? targetMethodInfo.GetGenericMethodDefinition().GetGenericArguments() : [];
            if (!TypeParametersEqual(requiredTypeParams, typeParameters))
            {
                var reqNames = requiredTypeParams.Length == 0 ? "none" : $"<{string.Join(", ", requiredTypeParams.Select(tp => tp.ToDisplayString()))}>";
                var actualNames = typeParameters.Length == 0 ? "none" : $"<{string.Join(", ", typeParameters.Select(tp => tp.ToDisplayString()))}>";

                var diag = Diagnostic.Create(MethodTypeParameterMismatchDescriptor, reportLocation, reqNames, actualNames);
                context.ReportDiagnostic(diag);
                return false;
            }

            // Check for matching ref return
            if ((uaaMethodSymbol.RefKind != RefKind.None && !targetMethodInfo.ReturnType.IsByRef) || (uaaMethodSymbol.RefKind == RefKind.None && targetMethodInfo.ReturnType.IsByRef)
                || !TypesEqual(targetMethodInfo.ReturnType, uaaReturnType, context.SemanticModel))
            {
                var displayString = (targetMethodInfo.ReturnType.IsByRef ? "ref" : "") + targetMethodInfo.ReturnType.ToDisplayString();
                var diag = Diagnostic.Create(IncorrectReturnTypeDescriptor, reportLocation, targetType.ToDisplayString(), displayString);
                context.ReportDiagnostic(diag);
                return false;
            }
        }
        else
        {
            // The method may be missing, but there may be a property accessor with a prefix that could match
            var properties = targetType.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToArray();
            var events = targetType.GetEvents(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToArray();
            if (properties.Any(p => ((p.GetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true || p.SetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true)
                && ParametersEqual(p.GetIndexParameters(), restParams, context.SemanticModel))
                || (events.Any(e => (e.AddMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true || e.RemoveMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true)
                && restParams.Length == 1 && TypesEqual(e.EventHandlerType, restParams[0].Type, context.SemanticModel)))
            ))
            {
                var diag = Diagnostic.Create(InvalidPropertyAccessorNameDescriptor, reportLocation);
                context.ReportDiagnostic(diag);
                return false;
            }
            else
            {
                var diag = Diagnostic.Create(MissingMethodDescriptor, reportLocation,
                    targetType.ToDisplayString(),
                    $"{uaaReturnType.IsRefLikeType} {uaaReturnType.ToDisplayString()}",
                    memberName,
                    typeParameters.Length > 0 ? $"<{string.Join(", ", typeParameters.Select(t => t.ToDisplayString()))}>" : "",
                    signatureString);
                context.ReportDiagnostic(diag);
                return false;
            }
        }

        // If struct, thisParam must be ref
        if (targetType.IsValueType && !thisParam.RefKind.HasFlag(RefKind.Ref))
        {
            var diag = Diagnostic.Create(InstanceMemberOnStructRequiresRefDescriptor, reportLocation);
            context.ReportDiagnostic(diag);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Structurally compares two sets of Roslyn type parameters, which may come from unrelated declarations
    /// (so <see cref="SymbolEqualityComparer"/> never matches them), by arity, variance and constraints.
    /// </summary>
    private static bool TypeParametersEqual(ImmutableArray<ITypeParameterSymbol> expected, ImmutableArray<ITypeParameterSymbol> actualSymbols)
    {
        if (expected.Length != actualSymbols.Length)
        {
            return false;
        }

        for (var i = 0; i < expected.Length; i++)
        {
            var expectedParam = expected[i];
            var actualParam = actualSymbols[i];

            if (expectedParam.Ordinal != actualParam.Ordinal
                || expectedParam.Variance != actualParam.Variance
                || expectedParam.HasReferenceTypeConstraint != actualParam.HasReferenceTypeConstraint
                || expectedParam.HasValueTypeConstraint != actualParam.HasValueTypeConstraint
                || expectedParam.HasUnmanagedTypeConstraint != actualParam.HasUnmanagedTypeConstraint
                || expectedParam.HasNotNullConstraint != actualParam.HasNotNullConstraint
                || expectedParam.HasConstructorConstraint != actualParam.HasConstructorConstraint
                || expectedParam.AllowsRefLikeType != actualParam.AllowsRefLikeType)
            {
                return false;
            }

            if (!ConstraintTypesEqual(expectedParam.ConstraintTypes, actualParam.ConstraintTypes))
            {
                return false;
            }
        }

        return true;
    }
    // Constraint types come from unrelated declarations too; type parameter constraints compare by ordinal+kind, everything else by display string
    private static bool ConstraintTypesEqual(ImmutableArray<ITypeSymbol> expected, ImmutableArray<ITypeSymbol> actual)
    {
        if (expected.Length != actual.Length)
        {
            return false;
        }

        static string Key(ITypeSymbol t) => t is ITypeParameterSymbol tp ? $"#{tp.Ordinal}:{tp.TypeParameterKind}" : t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var expectedKeys = expected.Select(Key).OrderBy(x => x, StringComparer.Ordinal);
        var actualKeys = actual.Select(Key).OrderBy(x => x, StringComparer.Ordinal);
        return expectedKeys.SequenceEqual(actualKeys, StringComparer.Ordinal);
    }
    /// <summary>
    /// Compares two sets of type parameters for equality, including count, type matches, variance and constraints.
    /// </summary>
    private static bool TypeParametersEqual(Type[] expected, ImmutableArray<ITypeParameterSymbol> actualSymbols)
    {
        Debug.Assert(expected.All(t => t.IsGenericParameter));

        if (expected.Length != actualSymbols.Length)
        {
            return false;
        }

        var asRtTypes = actualSymbols.Select(s => s.RuntimeType).ToArray();
        var allTypesResolved = asRtTypes.All(t => t is not null);

        // Handle generic type parameters when the symbols cannot be resolved to runtime Types
        // This unfortunately requires us to do all the checks [UnsafeAccessor] does at runtime right here
        // This means:
        //  - Count (arity) must match (ensured above)
        //  - Order must match (order is ensured by equal types, which comes implicitly below)
        //  - Variance specifiers must match
        //  - Constraints must match
        for (var i = 0; i < expected.Length; i++)
        {
            var expectedParam = expected[i];
            var actualParam = actualSymbols[i];

            // Variance
            var expectedVariance = expectedParam.GenericParameterAttributes & GenericParameterAttributes.VarianceMask;
            var actualVariance = actualParam.Variance switch
            {
                VarianceKind.In => GenericParameterAttributes.Contravariant,
                VarianceKind.Out => GenericParameterAttributes.Covariant,
                _ => GenericParameterAttributes.None
            };
            if (expectedVariance != actualVariance)
            {
                return false;
            }

            // Constraints
            var expectedConstraints = expectedParam.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;
            var actualConstraints = GetConstraintFlags(actualParam);
            if (expectedConstraints != actualConstraints)
            {
                return false;
            }

            if (allTypesResolved)
            {
                var constraintTypes = actualParam.ConstraintTypes.Select(t => t.RuntimeType).ToArray();
                if (!constraintTypes.Any(t => t is null))
                {
                    var expectedTypes = expectedParam.GetGenericParameterConstraints();
                    if (!expectedTypes.SequenceEqual(constraintTypes))
                    {
                        return false;
                    }
                }
            }
            else
            {
                // Type constraints - compare by name since no runtime types
                var expectedTypes = expectedParam.GetGenericParameterConstraints().Select(t => t.FullName).OrderBy(x => x);
                var actualTypes = actualParam.ConstraintTypes.Select(t => t.ToDisplayString()).OrderBy(x => x);
                if (!expectedTypes.SequenceEqual(actualTypes))
                {
                    return false;
                }
            }
        }

        return true;
    }
    private static GenericParameterAttributes GetConstraintFlags(ITypeParameterSymbol symbol)
    {
        var flags = GenericParameterAttributes.None;

        if (symbol.HasConstructorConstraint)
        {
            flags |= GenericParameterAttributes.DefaultConstructorConstraint;
        }
        if (symbol.HasReferenceTypeConstraint)
        {
            flags |= GenericParameterAttributes.ReferenceTypeConstraint;
        }
        if (symbol.HasValueTypeConstraint)
        {
            flags |= GenericParameterAttributes.NotNullableValueTypeConstraint;
        }

        // Note: Reflection API predates notnull and unmanaged constraints and there is no way to check for them

        return flags;
    }
    private static bool TypesEqual(Type type, ITypeSymbol typeSymbol, SemanticModel semanticModel)
    {
        if (typeSymbol.RuntimeType is { } runtimeType)
        {
            return type == runtimeType;
        }
        // GetTypeByMetadataName only accepts unbound metadata names, so skip it for shapes it can't resolve anyway
        if (!type.IsGenericType && !type.IsArray && !type.IsPointer && !type.IsByRef && !type.IsGenericParameter
            && semanticModel.Compilation.GetTypeByMetadataName(type.FullName) is { } otherTypeSymbol)
        {
            return SymbolEqualityComparer.Default.Equals(typeSymbol, otherTypeSymbol);
        }
        // Reflection Type.FullName and ITypeSymbol.ToDisplayString() are never the same shape (e.g. "System.Byte[]" vs "byte[]"),
        // so fall back to a structural comparison instead of comparing those strings directly
        return TypeMatches(type, typeSymbol);
    }
    /// <summary>
    /// Structurally compares a reflection <see cref="Type"/> against a Roslyn <see cref="ITypeSymbol"/>.
    /// </summary>
    private static bool TypeMatches(Type reflectionType, ITypeSymbol symbol)
    {
        if (reflectionType.IsByRef)
        {
            return TypeMatches(reflectionType.GetElementType(), symbol);
        }

        if (reflectionType.IsArray)
        {
            return symbol is IArrayTypeSymbol arraySymbol
                && arraySymbol.Rank == reflectionType.GetArrayRank()
                && TypeMatches(reflectionType.GetElementType(), arraySymbol.ElementType);
        }

        if (reflectionType.IsPointer)
        {
            return symbol is IPointerTypeSymbol pointerSymbol
                && TypeMatches(reflectionType.GetElementType(), pointerSymbol.PointedAtType);
        }

        if (reflectionType.IsGenericParameter)
        {
            // Position alone isn't enough: a type's 0th type parameter and a method's 0th type parameter are distinct positions
            return symbol is ITypeParameterSymbol typeParamSymbol
                && typeParamSymbol.Ordinal == reflectionType.GenericParameterPosition
                && (reflectionType.DeclaringMethod is not null) == (typeParamSymbol.TypeParameterKind == TypeParameterKind.Method);
        }

        if (reflectionType.IsGenericType)
        {
            if (symbol is not INamedTypeSymbol namedSymbol || !namedSymbol.IsGenericType)
            {
                return false;
            }

            var openReflectionType = reflectionType.GetGenericTypeDefinition();
            if (openReflectionType.FullName != GetReflectionFullName(namedSymbol.ConstructedFrom))
            {
                return false;
            }

            var reflectionArgs = reflectionType.GetGenericArguments();
            var symbolArgs = namedSymbol.TypeArguments;
            if (reflectionArgs.Length != symbolArgs.Length)
            {
                return false;
            }

            for (var i = 0; i < reflectionArgs.Length; i++)
            {
                if (!TypeMatches(reflectionArgs[i], symbolArgs[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return symbol is INamedTypeSymbol plainNamedSymbol && reflectionType.FullName == GetReflectionFullName(plainNamedSymbol);
    }
    /// <summary>
    /// Renders an <see cref="ITypeSymbol"/> in reflection's <see cref="Type.FullName"/> shape:
    /// namespace, '+' as the nested-type separator, and metadata arity suffix (e.g. "Task`1") instead of Roslyn's display format.
    /// </summary>
    private static string GetReflectionFullName(INamedTypeSymbol symbol)
    {
        var chain = new List<string>();
        for (var current = symbol; current is not null; current = current.ContainingType)
        {
            chain.Insert(0, current.MetadataName);
        }

        var nestedName = string.Join("+", chain);
        var ns = symbol.ContainingNamespace;
        return ns is null || ns.IsGlobalNamespace ? nestedName : $"{ns.ToDisplayString()}.{nestedName}";
    }
    /// <summary>
    /// Compares two sets of parameters for equality, including count, type matches and ref kinds.
    /// </summary>
    private static bool ParametersEqual(ParameterInfo[] expected, ImmutableArray<IParameterSymbol> actualSymbols, SemanticModel semanticModel)
    {
        if (expected.Length != actualSymbols.Length)
        {
            return false;
        }

        for (int i = 0; i < expected.Length; i++)
        {
            var expectedParam = expected[i];
            var actualParam = actualSymbols[i];

            // Compare ref kinds
            if (!RefKindsMatch(expectedParam, actualParam))
            {
                return false;
            }

            // Compare types - strip ref wrapper for comparison
            var expectedType = expectedParam.ParameterType.IsByRef ? expectedParam.ParameterType.GetElementType() : expectedParam.ParameterType;

            if (!TypesEqual(expectedType, actualParam.Type, semanticModel))
            {
                return false;
            }
        }

        return true;
    }
    private static bool RefKindsMatch(ParameterInfo paramInfo, IParameterSymbol symbol)
    {
        if (!paramInfo.ParameterType.IsByRef)
        {
            return symbol.RefKind == RefKind.None;
        }

        // Reflection needs IsOut/IsIn on the ParameterInfo (not just Type.IsByRef) to distinguish ref/out/in
        if (paramInfo.IsOut && !paramInfo.IsIn)
        {
            return symbol.RefKind == RefKind.Out;
        }

        if (paramInfo.IsIn)
        {
            return symbol.RefKind == RefKind.In;
        }

        return symbol.RefKind == RefKind.Ref;
    }

    // Copy from a decompilation
    private enum UnsafeAccessorKind
    {
        [Description("constructor")] Constructor = 0,
        [Description("method")] Method = 1,
        [Description("static method")] StaticMethod = 2,
        [Description("field")] Field = 3,
        [Description("static field")] StaticField = 4
    }
}
