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
        messageFormat: "The type '{0}' does not have a {1} named '{2}'",
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
        messageFormat: "[UnsafeAccessor] methods must be static, extern and return by-ref",
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
        messageFormat: "The return type of this {0} [UnsafeAccessor] method must be exactly '{1}'",
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

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclarationSyntax = Unsafe.As<MethodDeclarationSyntax>(context.Node);
        var semanticModel = context.SemanticModel;
        var compilation = context.Compilation;

        var methodDeclarationLocation = methodDeclarationSyntax.Identifier.GetLocation();

        // Quick syntax check before expensive semantic model lookup
        if (!methodDeclarationSyntax.AttributeLists.Any())
        {
            return;
        }

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
        if (methodSymbol == null)
        {
            return;
        }

        if (methodSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.ToDisplayString() == "System.Runtime.CompilerServices.UnsafeAccessorAttribute") is not { } uaaData)
        {
            return;
        }

        var unsafeAccessorKind = (UnsafeAccessorKind)(int)uaaData.ConstructorArguments[0].Value!;
        var description = typeof(UnsafeAccessorKind).GetField(unsafeAccessorKind.ToString())!.GetCustomAttribute<DescriptionAttribute>()!.Description;

        var memberName = methodDeclarationSyntax.Identifier.ToString();
        // Explicit name overrides method name
        if (uaaData.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value is string actualName)
        {
            memberName = actualName;
        }

        var parameters = methodSymbol.Parameters;
        var typeParameters = methodSymbol.TypeParameters;

        var thisParam = parameters.FirstOrDefault();
        var targetType = thisParam.Type;
        var returnType = methodSymbol.ReturnType;

        var restParams = parameters.Skip(1).ToImmutableArray();
        var signatureString = string.Join(", ", restParams.Select(p => p.Type.ToDisplayString()));

        // On constructors, the target type is actually the return type
        if (unsafeAccessorKind is UnsafeAccessorKind.Constructor)
        {
            targetType = methodSymbol.ReturnType;
            restParams = parameters;
            signatureString = string.Join(", ", restParams.Select(p => p.Type.ToDisplayString()));

            if (methodSymbol.ReturnsVoid)
            {
                if (thisParam is null)
                {
                    var diag = Diagnostic.Create(MissingTargetTypeForCtorDescriptor, methodDeclarationLocation);
                    context.ReportDiagnostic(diag);
                }
                else
                {
                    var diag = Diagnostic.Create(IncorrectReturnTypeDescriptor, methodDeclarationLocation, "constructor", thisParam.Type.ToDisplayString());
                    context.ReportDiagnostic(diag);
                }
                return;
            }

            // Check for existence of the member
            var constructors = (targetType as INamedTypeSymbol)?.Constructors;
            if (constructors is null)
            {
                var diag = Diagnostic.Create(MissingCtorDescriptor, methodDeclarationLocation, targetType.ToDisplayString(), signatureString);
                context.ReportDiagnostic(diag);
                return;
            }

            if (!constructors.Value.Any(ctor => ctor.Parameters.Select(p => p.Type).SequenceEqual(restParams.Select(p => p.Type), SymbolEqualityComparer.Default)))
            {
                var diag = Diagnostic.Create(MissingCtorDescriptor, methodDeclarationLocation, targetType.ToDisplayString(), signatureString);
                context.ReportDiagnostic(diag);
                return;
            }

            return;
        }
        // else, enfore 'static extern ref'
        else if ((!methodSymbol.ReturnsVoid && !methodSymbol.ReturnsByRef) || !methodSymbol.IsStatic || !methodSymbol.IsExtern)
        {
            var diag = Diagnostic.Create(InvalidDeclarationDescriptor, methodDeclarationLocation, description);
            context.ReportDiagnostic(diag);
            return;
        }

        if (parameters.Length == 0)
        {
            var diag = Diagnostic.Create(MissingTargetTypeDescriptor, methodDeclarationLocation);
            context.ReportDiagnostic(diag);
            return;
        }

        switch (unsafeAccessorKind)
        {
            case UnsafeAccessorKind.Method when memberName == ".ctor":
            {
                // Check for existence of the member
                var constructors = (targetType as INamedTypeSymbol)?.Constructors;
                if (constructors is null)
                {
                    var diag = Diagnostic.Create(MissingCtorDescriptor, methodDeclarationLocation, targetType.ToDisplayString(), signatureString);
                    context.ReportDiagnostic(diag);
                    return;
                }

                if (!constructors.Value.Any(ctor => ctor.Parameters.Select(p => p.Type).SequenceEqual(restParams.Select(p => p.Type), SymbolEqualityComparer.Default)))
                {
                    var diag = Diagnostic.Create(MissingCtorDescriptor, methodDeclarationLocation, targetType.ToDisplayString(), signatureString);
                    context.ReportDiagnostic(diag);
                    return;
                }

                // Check for correct return type
                if (!methodSymbol.ReturnsVoid)
                {
                    var diag = Diagnostic.Create(IncorrectReturnTypeDescriptor, methodDeclarationLocation, "constructor", "void");
                    context.ReportDiagnostic(diag);
                }
                break;
            }
            case UnsafeAccessorKind.Method:
            {
                // Check for existence of the member
                var methods = targetType!.GetMembers(memberName).OfType<IMethodSymbol>().Where(m => !m.IsStatic).ToArray();
                var flowControl = CheckMethods(context, methodDeclarationLocation, memberName, typeParameters, thisParam, targetType, returnType, restParams, signatureString, methods);
                if (!flowControl)
                {
                    return;
                }
                break;
            }
            case UnsafeAccessorKind.StaticMethod:
            {
                // Check for existence of the member
                var methods = targetType!.GetMembers(memberName).OfType<IMethodSymbol>().Where(m => m.IsStatic).ToArray();
                var flowControl = CheckMethods(context, methodDeclarationLocation, memberName, typeParameters, thisParam, targetType, returnType, restParams, signatureString, methods);
                if (!flowControl)
                {
                    return;
                }
                break;
            }
            case UnsafeAccessorKind.Field:
            {
                // Check for existence of the member
                var fields = targetType!.GetMembers(memberName).OfType<IFieldSymbol>().Where(f => !f.IsStatic).ToArray();
                var flowControl = CheckFields(context, methodDeclarationLocation, description, memberName, thisParam, targetType, returnType, fields);
                if (!flowControl)
                {
                    return;
                }

                break;
            }
            case UnsafeAccessorKind.StaticField:
            {
                // Check for existence of the member
                var fields = targetType!.GetMembers(memberName).OfType<IFieldSymbol>().Where(f => f.IsStatic).ToArray();
                var flowControl = CheckFields(context, methodDeclarationLocation, description, memberName, thisParam, targetType, returnType, fields);
                if (!flowControl)
                {
                    return;
                }

                break;
            }
        }

        // Beyond all other checks, the accessed type's type parameters must match the type parameters of the type containing the [UnsafeAccessor] method
        var requiredTypeParams = targetType is INamedTypeSymbol nts ? nts.TypeParameters : [];
        var containingTypeTypeParams = methodSymbol.ContainingType.TypeParameters;
        if (!ImmutableArrayExtensions.SequenceEqual(requiredTypeParams, containingTypeTypeParams, SymbolEqualityComparer.Default))
        {
            var reqNames = requiredTypeParams.Length == 0 ? "none" : $"<{string.Join(", ", requiredTypeParams.Select(tp => tp.ToDisplayString()))}>";
            var actualNames = containingTypeTypeParams.Length == 0 ? "none" : $"<{string.Join(", ", containingTypeTypeParams.Select(tp => tp.ToDisplayString()))}>";

            var containingTypeDecl = methodDeclarationSyntax.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            IEnumerable<Location> moreLocs = [];
            if (containingTypeDecl is not null)
            {
                moreLocs = [containingTypeDecl.Identifier.GetLocation()];
            }
            var diag = Diagnostic.Create(ContainingTypeTypeParameterMismatchDescriptor, methodDeclarationLocation, [], reqNames, actualNames);
            context.ReportDiagnostic(diag);
            return;
        }
    }

    private static bool CheckFields(SyntaxNodeAnalysisContext context, Location methodDeclarationLocation, string description, string memberName, IParameterSymbol thisParam, ITypeSymbol targetType, ITypeSymbol returnType, IFieldSymbol[] fields)
    {
        if (fields.FirstOrDefault(f => SymbolEqualityComparer.Default.Equals(f.Type, returnType)) is not { } targetFieldSymbol)
        {
            // The field may be missing, but there may be a property accessor with a prefix that could match
            var properties = targetType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic).ToArray();
            var events = targetType.GetMembers().OfType<IEventSymbol>().Where(e => !e.IsStatic).ToArray();
            // Bit of a disgusting check since only a get_ could ever match a field, but whatever
            if (properties.Any(p => p.GetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true))
            {
                var diag = Diagnostic.Create(InvalidPropertyAccessorDescriptor, methodDeclarationLocation);
                context.ReportDiagnostic(diag);
                return false;
            }
            else
            {
                var diag = Diagnostic.Create(MissingMemberDescriptor, methodDeclarationLocation, targetType.ToDisplayString(), description, memberName);
                context.ReportDiagnostic(diag);
                return false;
            }
        }

        if (!fields.Any(f => SymbolEqualityComparer.Default.Equals(f.Type, returnType)))
        {
            var diag = Diagnostic.Create(MissingMemberDescriptor, methodDeclarationLocation, targetType.ToDisplayString(), description, memberName);
            context.ReportDiagnostic(diag);
            return false;
        }

        // If struct, thisParam must be ref
        if (targetType.IsValueType && !thisParam.RefKind.HasFlag(RefKind.Ref))
        {
            var diag = Diagnostic.Create(InstanceMemberOnStructRequiresRefDescriptor, methodDeclarationLocation);
            context.ReportDiagnostic(diag);
            return false;
        }

        return true;
    }

    private static bool CheckMethods(SyntaxNodeAnalysisContext context, Location methodDeclarationLocation, string memberName, ImmutableArray<ITypeParameterSymbol> typeParameters, IParameterSymbol thisParam, ITypeSymbol targetType, ITypeSymbol returnType, ImmutableArray<IParameterSymbol> restParams, string signatureString, IMethodSymbol[] methods)
    {
        // Intentionally omitting type parameter check here so we can later differentiate between missing method and type parameter mismatch
        var targetMethodSymbol = methods.FirstOrDefault(m =>
            m.Parameters.Select(p => p.Type).SequenceEqual(restParams.Select(p => p.Type), SymbolEqualityComparer.Default)
            && SymbolEqualityComparer.Default.Equals(returnType, m.ReturnType)
        );
        targetMethodSymbol ??= methods.FirstOrDefault(m =>
            m.Parameters.Select(p => p.Type).SequenceEqual(restParams.Select(p => p.Type), SymbolEqualityComparer.Default)
        );

        if (targetMethodSymbol is not null)
        {
            // Check for mismatched type parameters
            var requiredTypeParams = targetMethodSymbol.TypeParameters;
            if (!ImmutableArrayExtensions.SequenceEqual(requiredTypeParams, typeParameters, SymbolEqualityComparer.Default))
            {
                var reqNames = requiredTypeParams.Length == 0 ? "none" : $"<{string.Join(", ", requiredTypeParams.Select(tp => tp.ToDisplayString()))}>";
                var actualNames = typeParameters.Length == 0 ? "none" : $"<{string.Join(", ", typeParameters.Select(tp => tp.ToDisplayString()))}>";

                var diag = Diagnostic.Create(MethodTypeParameterMismatchDescriptor, methodDeclarationLocation, reqNames, actualNames);
                context.ReportDiagnostic(diag);
                return false;
            }

            // Check for correct return type
            if (!SymbolEqualityComparer.Default.Equals(returnType, targetMethodSymbol.ReturnType))
            {
                var diag = Diagnostic.Create(IncorrectReturnTypeDescriptor, methodDeclarationLocation, "method", targetMethodSymbol.ReturnType.ToDisplayString());
                context.ReportDiagnostic(diag);
                return false;
            }
        }
        else
        {
            // The method may be missing, but there may be a property accessor with a prefix that could match
            var properties = targetType.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic).ToArray();
            var events = targetType.GetMembers().OfType<IEventSymbol>().Where(e => !e.IsStatic).ToArray();
            if (properties.Any(p => (p.GetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true || p.SetMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true)
                && p.Parameters.Select(p => p.Type).SequenceEqual(restParams.Select(p => p.Type), SymbolEqualityComparer.Default))
                || events.Any(e => (e.AddMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true || e.RemoveMethod?.Name?.EndsWith(memberName, StringComparison.OrdinalIgnoreCase) is true)
                && restParams.Length == 1 && SymbolEqualityComparer.Default.Equals(restParams[0], e.Type))
            )
            {
                var diag = Diagnostic.Create(InvalidPropertyAccessorNameDescriptor, methodDeclarationLocation);
                context.ReportDiagnostic(diag);
                return false;
            }
            else
            {
                var diag = Diagnostic.Create(MissingMethodDescriptor, methodDeclarationLocation, targetType.ToDisplayString(),
                    returnType.ToDisplayString(),
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
            var diag = Diagnostic.Create(InstanceMemberOnStructRequiresRefDescriptor, methodDeclarationLocation);
            context.ReportDiagnostic(diag);
            return false;
        }

        return true;
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
