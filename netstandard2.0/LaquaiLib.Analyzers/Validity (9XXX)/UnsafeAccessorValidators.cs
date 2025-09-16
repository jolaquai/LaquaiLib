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
        var targetTypeSymbol = uaaThisParam.Type;
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
        if (uaaParameters.Length == 0)
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
            var diag = Diagnostic.Create(ContainingTypeTypeParameterMismatchDescriptor, reportLocation, [], reqNames, actualNames);
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
    private static bool CheckMethodsRoslyn(SyntaxNodeAnalysisContext context, Location reportLocation, string memberName, ImmutableArray<ITypeParameterSymbol> uaaTypeParameters, IParameterSymbol uaaThisParam, ITypeSymbol targetTypeSymbol, ITypeSymbol uaaReturnTypeSymbol, ImmutableArray<IParameterSymbol> uaaRestParams, string signatureString, IMethodSymbol[] methodSymbols)
    {
        // Intentionally omitting type parameter check here so we can later differentiate between missing method and type parameter mismatch
        var targetMethodSymbol = methodSymbols.FirstOrDefault(m =>
            m.Parameters.Select(p => p.Type).SequenceEqual(uaaRestParams.Select(p => p.Type), SymbolEqualityComparer.Default)
            && SymbolEqualityComparer.Default.Equals(uaaReturnTypeSymbol, m.ReturnType)
        );
        targetMethodSymbol ??= methodSymbols.FirstOrDefault(m =>
            m.Parameters.Select(p => p.Type).SequenceEqual(uaaRestParams.Select(p => p.Type), SymbolEqualityComparer.Default)
        );

        if (targetMethodSymbol is not null)
        {
            // Check for mismatched type parameters
            var requiredTypeParams = targetMethodSymbol.TypeParameters;
            if (!ImmutableArrayExtensions.SequenceEqual(requiredTypeParams, uaaTypeParameters, SymbolEqualityComparer.Default))
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
        if (uaaParameters.Length == 0)
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

                if (!ctors.Any(ctor => ctor.GetParameters().Select(p => p.ParameterType).SequenceEqual(uaaRestParams.Select(p => p.Type.RuntimeType))))
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

        if (!TypeParametersEqual(requiredTypeParams, uaaTypeParameters))
        {
            var reqNames = requiredTypeParams.Length == 0 ? "none" : $"<{string.Join(", ", requiredTypeParams.Select(tp => tp.ToDisplayString()))}>";
            var actualNames = uaaTypeParameters.Length == 0 ? "none" : $"<{string.Join(", ", uaaTypeParameters.Select(tp => tp.ToDisplayString()))}>";

            var containingTypeDecl = uaaMethodDeclarationSyntax.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            IEnumerable<Location> moreLocs = [];
            if (containingTypeDecl is not null)
            {
                moreLocs = [containingTypeDecl.Identifier.GetLocation()];
            }
            var diag = Diagnostic.Create(ContainingTypeTypeParameterMismatchDescriptor, reportLocation, [], reqNames, actualNames);
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
        var targetMethodInfo = methodInfos.FirstOrDefault(m =>
            ParametersEqual(m.GetParameters().Select(p => p.ParameterType).ToArray(), restParams)
            && TypesEqual(m.ReturnType, uaaReturnType, context.SemanticModel)
        );
        targetMethodInfo ??= methodInfos.FirstOrDefault(m =>
            ParametersEqual(m.GetParameters().Select(p => p.ParameterType).ToArray(), restParams)
        );
        targetMethodInfo ??= methodInfos.FirstOrDefault();

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
                && ParametersEqual(p.GetIndexParameters().Select(p => p.ParameterType).ToArray(), restParams))
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

    // Quick check using SymbolEqualityComparer takes care of type parameters
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TypeParametersEqual(ImmutableArray<ITypeParameterSymbol> expected, ImmutableArray<ITypeParameterSymbol> actualSymbols)
        => ImmutableArrayExtensions.SequenceEqual(expected, actualSymbols, SymbolEqualityComparer.Default);
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
        var allTypesResolved = asRtTypes.Any(t => t is null);

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
        if (semanticModel.Compilation.GetTypeByMetadataName(type.FullName) is { } otherTypeSymbol)
        {
            return SymbolEqualityComparer.Default.Equals(typeSymbol, otherTypeSymbol);
        }
        // Use names if we can't resolve the type to the other respective form
        return type.FullName == typeSymbol.ToDisplayString();
    }
    /// <summary>
    /// Compares two sets of parameters for equality, including count, type matches and ref kinds.
    /// </summary>
    private static bool ParametersEqual(Type[] expected, ImmutableArray<IParameterSymbol> actualSymbols)
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
            var expectedType = expectedParam.IsByRef ? expectedParam.GetElementType() : expectedParam;
            var actualTypeName = actualParam.Type.ToDisplayString();

            if (expectedType.FullName != actualTypeName)
            {
                return false;
            }
        }

        return true;
    }
    private static bool RefKindsMatch(Type paramType, IParameterSymbol symbol)
    {
        if (!paramType.IsByRef && symbol.RefKind == RefKind.None)
        {
            return true;
        }

        if (paramType.IsByRef)
        {
            // Reflection doesn't distinguish ref/in, both show as IsByRef
            return symbol.RefKind is RefKind.Ref or RefKind.In;
        }

        // Check IsOut via parameter attributes if needed
        return false;
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
