using System.CodeDom.Compiler;
using System.Text;

using LaquaiLib.Analyzers.Shared;
using LaquaiLib.Generators.Extensions;

namespace LaquaiLib.Generators;

[Generator(LanguageNames.CSharp)]
public class FullAccessProxyGenerator : IIncrementalGenerator
{
    public static readonly DiagnosticDescriptor TypeByNameNotFound = new DiagnosticDescriptor(
        id: "FAP001",
        title: "Type not found",
        messageFormat: "The type '{0}' could not be resolved. The source generator will not generate a proxy for this type.",
        description: "The type specified in the [FullAccessProxy] attribute could not be found during compilation. If you've not specified an assembly-qualified name, try that.",
        category: "FullAccessProxyGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarationSyntaxProvider = context.SyntaxProvider
            .ForAttributeWithMetadataNameOn<ClassDeclarationSyntax>("LaquaiLib.Analyzers.Shared.Attributes.FullAccessProxyAttribute");

        var withCompilation = context.CompilationProvider.Combine(classDeclarationSyntaxProvider.Collect());

        context.RegisterSourceOutput(withCompilation, static (spc, source) =>
        {
            var compilation = source.Left;

            for (var i = 0; i < source.Right.Length; i++)
            {
                var gasc = source.Right[i];
                var decl = Unsafe.As<ClassDeclarationSyntax>(gasc.TargetNode);
                var attribute = gasc.Attributes[0];

                var semanticModel = gasc.SemanticModel;
                if (semanticModel is null)
                {
                    return;
                }

                var proxyClassSymbol = (INamedTypeSymbol)gasc.TargetSymbol;

                if (attribute is null)
                {
                    return;
                }

                INamedTypeSymbol proxiedType = null;
                // Get the type argument from the attribute (e.g., MemoryStream)
                var type = attribute.ConstructorArguments[0];
                if (type.Kind == TypedConstantKind.Type)
                {
                    // typeof(...) yields an ITypeSymbol here, never a System.Type
                    proxiedType = type.Value as INamedTypeSymbol;
                }
                else if (type.Kind == TypedConstantKind.Primitive && type.Value is string fqTypeName)
                {
                    // Chances are high that the type won't be accessible, but we can try
                    proxiedType = compilation.GetTypeByMetadataName(NormalizeMetadataName(fqTypeName));
                }

                if (proxiedType is { TypeKind: not TypeKind.Error })
                {
                    // Generate the proxied members into the class
                    var proxyClassSource = GenerateProxyForClass(proxyClassSymbol, proxiedType, compilation);
                    spc.AddSource(GetHintName(proxyClassSymbol, proxiedType), SourceText.From(proxyClassSource, Encoding.UTF8));
                }
                else
                {
                    var location = gasc.Attributes.FirstOrDefault()?.ApplicationSyntaxReference?.GetSyntax().GetLocation();
                    location ??= decl.Identifier.GetLocation();
                    var diagnostic = Diagnostic.Create(TypeByNameNotFound, location, attribute.ConstructorArguments[0].Value);
                    spc.ReportDiagnostic(diagnostic);
                }
            }
        });
    }

    private static string GetHintName(INamedTypeSymbol proxyClassSymbol, INamedTypeSymbol proxiedType)
    {
        var full = proxyClassSymbol.ToDisplayString(SymbolDisplayFormats.FullyQualified);
        if (full.StartsWith("global::", StringComparison.Ordinal))
        {
            full = full.Substring("global::".Length);
        }
        var raw = $"{full}_{proxiedType.Name}";
        var sb = new StringBuilder(raw.Length);
        for (var i = 0; i < raw.Length; i++)
        {
            var c = raw[i];
            sb.Append(char.IsLetterOrDigit(c) || c is '_' or '.' ? c : '_');
        }
        sb.Append(".g.cs");
        return sb.ToString();
    }

    private static string GetAccessibilityKeyword(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Internal => "internal",
        Accessibility.Private => "private",
        Accessibility.Protected => "protected",
        Accessibility.ProtectedOrInternal => "protected internal",
        Accessibility.ProtectedAndInternal => "private protected",
        _ => "internal"
    };
    private static string GetTypeKindKeyword(INamedTypeSymbol type) => type.TypeKind switch
    {
        TypeKind.Struct => type.IsRecord ? "record struct" : "struct",
        TypeKind.Interface => "interface",
        _ => type.IsRecord ? "record" : "class"
    };
    private static string GetTypeParameterList(INamedTypeSymbol type) => type.TypeParameters.Length > 0
        ? "<" + string.Join(", ", type.TypeParameters.Select(static tp => tp.Name)) + ">"
        : "";

    private static string GenerateProxyForClass(INamedTypeSymbol proxyClassSymbol, INamedTypeSymbol proxiedType, Compilation compilation)
    {
        var proxyClassName = proxyClassSymbol.Name;

        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        using var writer = new IndentedTextWriter(sw);

        writer.WriteLine(SourceEmitHelper.GeneratedFileHeader);
        if (!proxyClassSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            writer.WriteLine($"namespace {proxyClassSymbol.ContainingNamespace.ToDisplayString()};");
        }

        // collect containing types outermost-first so we can nest the proxy declaration correctly
        var ancestors = new List<INamedTypeSymbol>();
        for (var t = proxyClassSymbol.ContainingType; t is not null; t = t.ContainingType)
        {
            ancestors.Add(t);
        }
        ancestors.Reverse();

        var openScopes = new Stack<IDisposable>();
        for (var i = 0; i < ancestors.Count; i++)
        {
            var ancestor = ancestors[i];
            var modifiers = new StringBuilder();
            modifiers.Append(GetAccessibilityKeyword(ancestor.DeclaredAccessibility)).Append(' ');
            if (ancestor.IsStatic)
            {
                modifiers.Append("static ");
            }
            if (ancestor.IsRefLikeType)
            {
                modifiers.Append("ref ");
            }
            if (ancestor.IsReadOnly)
            {
                modifiers.Append("readonly ");
            }
            modifiers.Append("partial ").Append(GetTypeKindKeyword(ancestor));

            writer.WriteLine($"{modifiers} {ancestor.Name}{GetTypeParameterList(ancestor)}");
            openScopes.Push(writer.Scope);
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Wraps an instance of <c>{proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)}</c> to provide full access to its members, regardless of visibility. Members with a result type (method return type or declared type for fields, events or properties) that is not public cannot be proxied.");
        writer.WriteLine($"/// <para/>Use the static <c>Create</c> methods to create instances using any of the instance constructors on that type. They also proxy non-public constructors.");
        writer.WriteLine("/// </summary>");

        writer.WriteLines(SourceEmitHelper.GeneratedCodeAttribute(typeof(FullAccessProxyGenerator)));

        // only interfaces we can actually forward every member of may appear in the base list, otherwise we'd emit CS0535
        var interfaces = GetProxyableInterfaces(proxiedType);

        var classModifiers = GetAccessibilityKeyword(proxyClassSymbol.DeclaredAccessibility);
        if (proxyClassSymbol.TypeKind == TypeKind.Class)
        {
            classModifiers += " sealed";
        }
        writer.Write($"{classModifiers} partial class {proxyClassName}{GetTypeParameterList(proxyClassSymbol)}");
        if (interfaces.Length > 0)
        {
            writer.Write(" : ");
            writer.Write(string.Join(", ", interfaces.Select(i => i.ToDisplayString(SymbolDisplayFormats.FullyQualified))));
        }
        writer.WriteLine();

        using (writer.Scope)
        {
            var members = GetProxyableMembers(proxiedType);
            var ctors = proxiedType.GetMembers().Where(m => m is IMethodSymbol { Name: ".ctor" }).ToArray();

            WriteInstanceField(writer, proxiedType);
            WriteProxyCtor(writer, proxyClassName, proxiedType, compilation);
            WriteStaticCtorProxies(writer, proxyClassName, proxiedType, Unsafe.As<IMethodSymbol[]>(ctors));
            WriteUnsafeAccessorUtility(writer, proxiedType, members, ctors);
            WriteInterfaceImplementations(writer, interfaces);

            var memberGroups = members
                .Where(m => !m.IsInterfaceImplementation)
                .GroupBy(m => m.Kind)
                .ToDictionary(g => g.Key, g => g.ToArray());

            foreach (var group in memberGroups)
            {
                var kind = group.Key;
                using var region = writer.Region($"Proxied members of kind: {kind}");

                foreach (var member in group.Value)
                {
                    switch (member)
                    {
                        case IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol when IsPubliclyAccessible(methodSymbol.ReturnType):
                            WriteMethodProxy(writer, methodSymbol, proxiedType);
                            break;
                        case IFieldSymbol fieldSymbol when IsPubliclyAccessible(fieldSymbol.Type):
                            WriteFieldProxy(writer, fieldSymbol, proxiedType);
                            break;
                        case IEventSymbol eventSymbol when IsPubliclyAccessible(eventSymbol.Type):
                            WriteEventProxy(writer, eventSymbol, proxiedType);
                            break;
                        case IPropertySymbol propertySymbol when IsPubliclyAccessible(propertySymbol.Type):
                            WritePropertyProxy(writer, propertySymbol, proxiedType);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        while (openScopes.Count > 0)
        {
            openScopes.Pop().Dispose();
        }

        writer.Flush();
        sw.Flush();
        return sb.ToString();
    }

    /// <summary>
    /// Collects the most-derived declaration of every proxyable member from <paramref name="proxiedType"/> up its
    /// base type chain, stopping before <see cref="object"/> (its members already exist on the proxy and can't be
    /// legally forwarded anyway). Overloads survive; overrides/shadows are deduplicated by signature, keeping the
    /// most-derived declaration (important for <c>[UnsafeAccessor]</c> targeting, see <see cref="WriteUnsafeAccessorUtility"/>).
    /// </summary>
    private static ISymbol[] GetProxyableMembers(INamedTypeSymbol proxiedType)
    {
        var seen = new HashSet<(SymbolKind Kind, string Name, int Arity, string Signature)>();
        var result = new List<ISymbol>();

        for (var current = proxiedType; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
        {
            var members = current.GetMembers();
            for (var i = 0; i < members.Length; i++)
            {
                var member = members[i];
                if (member is IMethodSymbol { Name: ".ctor" or ".cctor" })
                {
                    // constructors don't participate in the inheritance walk, only proxiedType's own flow to WriteStaticCtorProxies
                    continue;
                }
                if (member is IMethodSymbol { MethodKind: MethodKind.Destructor or MethodKind.UserDefinedOperator or MethodKind.Conversion or MethodKind.BuiltinOperator })
                {
                    continue;
                }
                if (member.IsImplicitlyDeclared && member is not IMethodSymbol { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove })
                {
                    // field-like events' add/remove accessors are implicitly declared too but still need forwarding,
                    // unlike backing fields/default ctors which this is meant to catch
                    continue;
                }
                if (member is IFieldSymbol { AssociatedSymbol: not null })
                {
                    // covers auto-property backing fields and field-like-event backing fields, neither is nameable in C#
                    continue;
                }

                var key = MakeMemberKey(member);
                if (!seen.Add(key))
                {
                    continue;
                }

                if (member.IsAbstract)
                {
                    // nothing to forward to
                    continue;
                }

                result.Add(member);
            }
        }

        return result.ToArray();
    }
    private static (SymbolKind Kind, string Name, int Arity, string Signature) MakeMemberKey(ISymbol member)
    {
        // return type intentionally excluded - C# can't overload on it, and including it would let an override slip through as a "new" member
        switch (member)
        {
            case IMethodSymbol method:
                return (SymbolKind.Method, method.Name, method.TypeParameters.Length, ParameterSignature(method.Parameters));
            case IPropertySymbol property:
                return (SymbolKind.Property, property.Name, 0, ParameterSignature(property.Parameters));
            default:
                return (member.Kind, member.Name, 0, "");
        }
    }
    private static string ParameterSignature(ImmutableArray<IParameterSymbol> parameters)
        => string.Join(",", parameters.Select(static p => p.RefKind + ":" + p.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified)));

    /// <summary>
    /// Filters <paramref name="proxiedType"/>'s interfaces down to those every member of which can be forwarded to the proxied instance.
    /// </summary>
    private static INamedTypeSymbol[] GetProxyableInterfaces(INamedTypeSymbol proxiedType)
    {
        var all = proxiedType.AllInterfaces;
        var result = new List<INamedTypeSymbol>(all.Length);
        for (var i = 0; i < all.Length; i++)
        {
            if (IsPubliclyAccessible(all[i]) && CanForwardAllMembers(all[i]))
            {
                result.Add(all[i]);
            }
        }
        return result.ToArray();
    }
    private static bool CanForwardAllMembers(INamedTypeSymbol iface)
    {
        var members = iface.GetMembers();
        for (var i = 0; i < members.Length; i++)
        {
            switch (members[i])
            {
                case INamedTypeSymbol:
                    break;
                case IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove }:
                    break;
                case IMethodSymbol method:
                    if (method.MethodKind is not MethodKind.Ordinary || method.IsStatic
                        || !IsPubliclyAccessible(method.ReturnType) || !AreParametersAccessible(method.Parameters))
                    {
                        return false;
                    }
                    break;
                case IPropertySymbol property:
                    // init-only setters can't be forwarded, assigning them outside an object initializer of the target is illegal
                    if (property.IsStatic || property.SetMethod is { IsInitOnly: true }
                        || !IsPubliclyAccessible(property.Type) || !AreParametersAccessible(property.Parameters))
                    {
                        return false;
                    }
                    break;
                case IEventSymbol @event:
                    if (@event.IsStatic || !IsPubliclyAccessible(@event.Type))
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }
        }
        return true;
    }
    private static bool AreParametersAccessible(ImmutableArray<IParameterSymbol> parameters)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            if (!IsPubliclyAccessible(parameters[i].Type))
            {
                return false;
            }
        }
        return true;
    }
    private static bool IsPubliclyAccessible(ITypeSymbol type)
    {
        switch (type)
        {
            case null:
                return false;
            case ITypeParameterSymbol or IDynamicTypeSymbol:
                return true;
            case IArrayTypeSymbol array:
                return IsPubliclyAccessible(array.ElementType);
            case IPointerTypeSymbol pointer:
                return IsPubliclyAccessible(pointer.PointedAtType);
        }

        for (ITypeSymbol current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility is not Accessibility.Public)
            {
                return false;
            }
        }
        if (type is INamedTypeSymbol { IsGenericType: true } named)
        {
            for (var i = 0; i < named.TypeArguments.Length; i++)
            {
                if (!IsPubliclyAccessible(named.TypeArguments[i]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static string ParameterString(ImmutableArray<IParameterSymbol> parameters) => string.Join(", ", parameters.Select(static p => p.ToDisplayString(SymbolDisplayFormats.FullyQualifiedParameter)));
    private static string ArgumentString(ImmutableArray<IParameterSymbol> parameters) => string.Join(", ", parameters.Select(static p => p.RefKind switch
    {
        RefKind.Ref or RefKind.RefReadOnlyParameter => "ref " + p.Name,
        RefKind.Out => "out " + p.Name,
        RefKind.In => "in " + p.Name,
        _ => p.Name
    }));

    private static string GetMethodTypeParameterList(IMethodSymbol method) => method.TypeParameters.Length > 0
        ? "<" + string.Join(", ", method.TypeParameters.Select(static tp => tp.Name)) + ">"
        : "";
    /// <summary>
    /// Renders the <c>where</c> clauses for <paramref name="typeParameters"/>, in the order required by C#
    /// ([UnsafeAccessor] requires them to match the target exactly, or an <see cref="InvalidProgramException"/> is thrown at runtime).
    /// </summary>
    private static string GetTypeParameterConstraintsClause(ImmutableArray<ITypeParameterSymbol> typeParameters)
    {
        if (typeParameters.Length == 0)
        {
            return "";
        }
        var sb = new StringBuilder();
        for (var i = 0; i < typeParameters.Length; i++)
        {
            var clause = RenderTypeParameterConstraintClause(typeParameters[i]);
            if (clause.Length == 0)
            {
                continue;
            }
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }
            sb.Append(clause);
        }
        return sb.Length > 0 ? " " + sb.ToString() : "";
    }
    private static string RenderTypeParameterConstraintClause(ITypeParameterSymbol tp)
    {
        var parts = new List<string>();
        // primary constraint must come first: unmanaged implies struct, so check it first
        if (tp.HasUnmanagedTypeConstraint)
        {
            parts.Add("unmanaged");
        }
        else if (tp.HasValueTypeConstraint)
        {
            parts.Add("struct");
        }
        else if (tp.HasReferenceTypeConstraint)
        {
            parts.Add("class");
        }
        else if (tp.HasNotNullConstraint)
        {
            parts.Add("notnull");
        }

        // base type constraint (if any) precedes interfaces in ConstraintTypes source order already
        for (var i = 0; i < tp.ConstraintTypes.Length; i++)
        {
            parts.Add(tp.ConstraintTypes[i].ToDisplayString(SymbolDisplayFormats.FullyQualified));
        }

        // struct/unmanaged imply new() and can't be combined with it explicitly
        if (tp.HasConstructorConstraint && !tp.HasValueTypeConstraint)
        {
            parts.Add("new()");
        }
        if (tp.AllowsRefLikeType)
        {
            parts.Add("allows ref struct");
        }

        return parts.Count > 0 ? $"where {tp.Name} : {string.Join(", ", parts)}" : "";
    }

    /// <summary>
    /// Determines whether emitting a member involving <paramref name="type"/> requires the <c>unsafe</c> keyword.
    /// </summary>
    private static bool RequiresUnsafeContext(ITypeSymbol type) => type switch
    {
        IPointerTypeSymbol or IFunctionPointerTypeSymbol => true,
        IArrayTypeSymbol array => RequiresUnsafeContext(array.ElementType),
        _ => false
    };
    private static bool RequiresUnsafeContext(IMethodSymbol method)
        => RequiresUnsafeContext(method.ReturnType) || method.Parameters.Any(static p => RequiresUnsafeContext(p.Type));
    private static bool RequiresUnsafeContext(IPropertySymbol property)
        => RequiresUnsafeContext(property.Type) || property.Parameters.Any(static p => RequiresUnsafeContext(p.Type));
    private static bool RequiresUnsafeContext(IFieldSymbol field) => RequiresUnsafeContext(field.Type);

    private static void WriteInterfaceImplementations(IndentedTextWriter writer, INamedTypeSymbol[] interfaces)
    {
        if (interfaces.Length == 0)
        {
            return;
        }

        using var region = writer.Region("Interface implementations");

        for (var i = 0; i < interfaces.Length; i++)
        {
            var iface = interfaces[i];
            var ifaceName = iface.ToDisplayString(SymbolDisplayFormats.FullyQualified);
            var members = iface.GetMembers();
            for (var k = 0; k < members.Length; k++)
            {
                // everything is emitted explicitly, that way it can never collide with the regularly proxied members
                switch (members[k])
                {
                    case IMethodSymbol { MethodKind: MethodKind.Ordinary } method:
                        WriteMethodProxyExplicit(writer, method, ifaceName);
                        break;
                    case IPropertySymbol property:
                        WritePropertyProxyExplicit(writer, property, ifaceName);
                        break;
                    case IEventSymbol @event:
                        WriteEventProxyExplicit(writer, @event, ifaceName);
                        break;
                }
            }
        }
    }

    private static void WriteInstanceField(IndentedTextWriter writer, INamedTypeSymbol proxiedType)
    {
        using var region = writer.Region("Instance field");

        // We split this into a field-prop combo to avoid the overhead of the property getter for internal use
        writer.WriteLine($"private readonly {proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)} _instance;");

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Gets the proxied instance of <c>{proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)}</c>.");
        writer.WriteLine("/// </summary>");
        writer.WriteLine($"public {proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)} Instance");
        using (writer.Scope)
        {
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine("get => _instance;");
        }
    }
    private static void WriteProxyCtor(IndentedTextWriter writer, string proxyClassName, INamedTypeSymbol proxiedType, Compilation compilation)
    {
        using var region = writer.Region("Proxy constructor");

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Initializes a new instance of this proxy class for <c>{proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)}</c>.");
        writer.WriteLine("/// </summary>");
        writer.WriteLine($"/// <param name=\"instance\">The instance of <c>{proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)}</c> to proxy. Must not be <see langword=\"null\"/>, otherwise an exception is thrown.</param>");
        writer.WriteLine($"public {proxyClassName}({proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)} instance)");
        using (writer.Scope)
        {
            writer.WriteLine("_instance = instance ?? throw new ArgumentNullException(nameof(instance));");
        }
    }
    private static void WriteStaticCtorProxies(IndentedTextWriter writer, string proxyClassName, INamedTypeSymbol proxiedType, IMethodSymbol[] ctors)
    {
        Debug.Assert(ctors.All(ctor => ctor.Name == ".ctor"));

        using var region = writer.Region("Static constructor proxies");

        for (var i = 0; i < ctors.Length; i++)
        {
            var ctor = ctors[i];

            writer.WriteLine("/// <summary>");
            writer.WriteLine($"/// Initializes a new instance of this proxy class for <c>{proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)}</c> using the following instance constructor overload of that type:");
            writer.WriteLine($"/// <para/><c>{ctor.ToDisplayString(SymbolDisplayFormats.FullyQualified).XmlEscape()}</c>");
            writer.WriteLine("/// </summary>");
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"public static {proxyClassName} Create({ctor.ParameterString})");
            using (writer.Scope)
            {
                writer.WriteLine($"return new {proxyClassName}(Accessors.ProxyCtor({ctor.ArgumentString}));");
            }
        }
    }
    private static void WriteUnsafeAccessorUtility(IndentedTextWriter writer, INamedTypeSymbol proxiedType, ISymbol[] members, ISymbol[] ctors)
    {
        using var region = writer.Region("Unsafe accessors utility");

        var proxiedTypeName = proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified);

        writer.WriteLine("private static class Accessors");
        using (writer.Scope)
        {
            for (var i = 0; i < ctors.Length; i++)
            {
                var ctorSymbol = (IMethodSymbol)ctors[i];
                writer.WriteLine(SourceEmitHelper.UnsafeAccessor_Ctor);
                writer.WriteLine($"public static extern {proxiedTypeName} ProxyCtor({ctorSymbol.ParameterString});");
            }

            for (var i = 0; i < members.Length; i++)
            {
                // [UnsafeAccessor] doesn't walk the base hierarchy, so target must be the member's OWN containing type, not proxiedType
                switch (members[i])
                {
                    // Methods will also hit property and event accessors, whichever are declared
                    case IMethodSymbol methodSymbol when methodSymbol.Name is not ".cctor" && IsPubliclyAccessible(methodSymbol.ReturnType)
                        && methodSymbol.ExplicitInterfaceImplementations.Length == 0 && !methodSymbol.IsInitOnly:
                    {
                        var targetTypeName = methodSymbol.ContainingType.ToDisplayString(SymbolDisplayFormats.FullyQualified);
                        writer.WriteLine(methodSymbol.IsStatic ? SourceEmitHelper.UnsafeAccessor_StaticMethod : SourceEmitHelper.UnsafeAccessor_Method);
                        var parameterString = methodSymbol.ParameterString;
                        if (parameterString.Length > 0)
                        {
                            parameterString = ", " + parameterString;
                        }
                        var unsafeKeyword = RequiresUnsafeContext(methodSymbol) ? "unsafe " : "";
                        var typeParameterList = GetMethodTypeParameterList(methodSymbol);
                        var constraintsClause = GetTypeParameterConstraintsClause(methodSymbol.TypeParameters);
                        writer.WriteLine($"public static {unsafeKeyword}extern {(methodSymbol.ReturnsByRef ? "ref " : methodSymbol.ReturnsByRefReadonly ? "ref readonly " : "")}{methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormats.FullyQualified)} {methodSymbol.Name}{typeParameterList}({targetTypeName} target{parameterString}){constraintsClause};");
                        break;
                    }
                    // Fields will also hit events
                    case IFieldSymbol fieldSymbol when IsPubliclyAccessible(fieldSymbol.Type):
                    {
                        var targetTypeName = fieldSymbol.ContainingType.ToDisplayString(SymbolDisplayFormats.FullyQualified);
                        writer.WriteLine(fieldSymbol.IsStatic ? SourceEmitHelper.UnsafeAccessor_StaticField : SourceEmitHelper.UnsafeAccessor_Field);
                        var unsafeKeyword = RequiresUnsafeContext(fieldSymbol) ? "unsafe " : "";
                        writer.WriteLine($"public static {unsafeKeyword}extern ref {fieldSymbol.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified)} {fieldSymbol.Name}({targetTypeName} target);");
                        break;
                    }

                    default:
                        break;
                        //throw new NotSupportedException($"Unsupported member type: {members[i].ToDisplayString(SymbolDisplayFormats.FullyQualified)}");
                }
            }
        }
    }

    private static void WriteMethodProxy(IndentedTextWriter writer, IMethodSymbol methodSymbol, INamedTypeSymbol proxiedType)
    {
        // Ignore non-public return types since we can't proxy those
        if (!IsPubliclyAccessible(methodSymbol.ReturnType))
        {
            return;
        }
        if (methodSymbol.IsAccessor || methodSymbol.ExplicitInterfaceImplementations.Length > 0)
        {
            return;
        }

        var parameterString = methodSymbol.ParameterString;
        var argumentString = methodSymbol.ArgumentString;
        if (argumentString.Length > 0)
        {
            argumentString = ", " + argumentString;
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Proxies the following method from <c>{proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)}</c>:");
        writer.WriteLine($"/// <para/><c>{methodSymbol.ToDisplayString(SymbolDisplayFormats.FullyQualified).XmlEscape()}({parameterString.XmlEscape()})</c>");
        writer.WriteLine("/// </summary>");

        var refPrefix = methodSymbol.ReturnsByRef ? "ref " : methodSymbol.ReturnsByRefReadonly ? "ref readonly " : "";
        var refReturn = refPrefix.Length > 0 ? "ref " : "";
        var typeParameterList = GetMethodTypeParameterList(methodSymbol);
        var constraintsClause = GetTypeParameterConstraintsClause(methodSymbol.TypeParameters);
        var unsafeKeyword = RequiresUnsafeContext(methodSymbol) ? "unsafe " : "";
        if (methodSymbol.IsStatic)
        {
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"public static {unsafeKeyword}{refPrefix}{methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormats.FullyQualified)} {methodSymbol.Name}{typeParameterList}({parameterString}){constraintsClause} => {refReturn}Accessors.{methodSymbol.Name}{typeParameterList}(null{argumentString});");
        }
        else
        {
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"public {unsafeKeyword}{refPrefix}{methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormats.FullyQualified)} {methodSymbol.Name}{typeParameterList}({parameterString}){constraintsClause} => {refReturn}Accessors.{methodSymbol.Name}{typeParameterList}(_instance{argumentString});");
        }
    }
    private static void WriteEventProxy(IndentedTextWriter writer, IEventSymbol eventSymbol, INamedTypeSymbol proxiedType)
    {
        if (!IsPubliclyAccessible(eventSymbol.Type))
        {
            return;
        }
        if (eventSymbol.ExplicitInterfaceImplementations.Length > 0)
        {
            return;
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Proxies the event <c>{proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)}.{eventSymbol.Name}</c>.");
        writer.WriteLine("/// </summary>");

        if (eventSymbol.IsStatic)
        {
            writer.WriteLine($"public static event {eventSymbol.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified)} {eventSymbol.Name}");
            using (writer.Scope)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"add => Accessors.add_{eventSymbol.Name}(null, value);");
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"remove => Accessors.remove_{eventSymbol.Name}(null, value);");
            }
        }
        else
        {
            writer.WriteLine($"public event {eventSymbol.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified)} {eventSymbol.Name}");
            using (writer.Scope)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"add => Accessors.add_{eventSymbol.Name}(_instance, value);");
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"remove => Accessors.remove_{eventSymbol.Name}(_instance, value);");
            }
        }
    }
    private static void WriteFieldProxy(IndentedTextWriter writer, IFieldSymbol field, INamedTypeSymbol proxiedType)
    {
        if (!IsPubliclyAccessible(field.Type))
        {
            return;
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Proxies the field <c>{proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)}.{field.Name}</c>.");
        writer.WriteLine("/// </summary>");

        // Fields will be proxied with a property of the same name
        var unsafeKeyword = RequiresUnsafeContext(field) ? "unsafe " : "";

        if (field.IsStatic)
        {
            writer.WriteLine($"public static {unsafeKeyword}{field.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified)} {field.Name}");
            using (writer.Scope)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"get => Accessors.{field.Name}(null);");
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"set => Accessors.{field.Name}(null) = value;");
            }
        }
        else
        {
            writer.WriteLine($"public {unsafeKeyword}{field.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified)} {field.Name}");
            using (writer.Scope)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"get => Accessors.{field.Name}(_instance);");
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"set => Accessors.{field.Name}(_instance) = value;");
            }
        }
    }

    private static void WritePropertyProxy(IndentedTextWriter writer, IPropertySymbol property, INamedTypeSymbol proxiedType)
    {
        if (!IsPubliclyAccessible(property.Type))
        {
            return;
        }
        if (property.ExplicitInterfaceImplementations.Length > 0)
        {
            return;
        }

        var refPrefix = property.ReturnsByRef ? "ref " : property.ReturnsByRefReadonly ? "ref readonly " : "";
        var type = property.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified);
        var declaration = property.IsIndexer ? $"this[{ParameterString(property.Parameters)}]" : property.Name;
        var target = property.IsStatic ? "null" : "_instance";
        var args = property.IsIndexer ? ArgumentString(property.Parameters) : "";
        var argsWithLeadingComma = args.Length > 0 ? ", " + args : "";
        var unsafeKeyword = RequiresUnsafeContext(property) ? "unsafe " : "";

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Proxies the {(property.IsIndexer ? "indexer" : "property")} <c>{proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified)}.{declaration.XmlEscape()}</c>.");
        writer.WriteLine("/// </summary>");

        writer.WriteLine($"{(property.IsStatic ? "public static" : "public")} {unsafeKeyword}{refPrefix}{type} {declaration}");
        using (writer.Scope)
        {
            if (refPrefix.Length > 0)
            {
                // ref-returning properties only get a get accessor forwarding by ref
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"get => ref Accessors.{property.GetMethod.Name}({target}{argsWithLeadingComma});");
                return;
            }
            if (property.GetMethod is not null)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"get => Accessors.{property.GetMethod.Name}({target}{argsWithLeadingComma});");
            }
            // init-only setters can't be meaningfully forwarded post-construction
            if (property.SetMethod is not null && !property.SetMethod.IsInitOnly)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"set => Accessors.{property.SetMethod.Name}({target}{argsWithLeadingComma}, value);");
            }
        }
    }

    /// <summary>
    /// Converts C#-style open-generic type names (e.g. <c>Namespace.Type&lt;&gt;</c>, <c>Outer&lt;,&gt;.Inner&lt;&gt;</c>)
    /// into CLR metadata names with backtick arities (e.g. <c>Namespace.Type`1</c>, <c>Outer`2+Inner`1</c>).
    /// Inputs that don't contain <c>&lt;</c> are returned as-is. Nested types may be separated by <c>.</c> or <c>+</c>;
    /// <c>.</c> separators between generic segments are rewritten to <c>+</c> for CLR consumption.
    /// </summary>
    private static string NormalizeMetadataName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.IndexOf('<') < 0)
        {
            return name;
        }

        var sb = new StringBuilder(name.Length);
        var lastGenericEnd = -1;
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (c == '<')
            {
                // Find matching '>' accounting for nesting
                var depth = 1;
                var j = i + 1;
                while (j < name.Length && depth > 0)
                {
                    if (name[j] == '<') depth++;
                    else if (name[j] == '>') depth--;
                    if (depth == 0) break;
                    j++;
                }
                if (j >= name.Length)
                {
                    // Unbalanced - bail out, return original
                    return name;
                }

                // Count arity from commas at depth 1
                var inner = name.Substring(i + 1, j - i - 1);
                var arity = 1;
                var d = 0;
                for (var k = 0; k < inner.Length; k++)
                {
                    var ic = inner[k];
                    if (ic == '<') d++;
                    else if (ic == '>') d--;
                    else if (ic == ',' && d == 0) arity++;
                }
                // Only treat as open generic if inner is empty or only commas/whitespace
                var isOpen = true;
                for (var k = 0; k < inner.Length; k++)
                {
                    var ic = inner[k];
                    if (ic != ',' && !char.IsWhiteSpace(ic))
                    {
                        isOpen = false;
                        break;
                    }
                }
                if (!isOpen)
                {
                    // Closed/constructed generic - GetTypeByMetadataName can't handle these anyway, return original
                    return name;
                }

                sb.Append('`').Append(arity);
                lastGenericEnd = sb.Length;
                i = j; // skip past '>'
            }
            else if (c == '.' && lastGenericEnd == sb.Length)
            {
                // '.' immediately after a generic arity indicates a nested type in CLR metadata
                sb.Append('+');
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private static void WriteMethodProxyExplicit(IndentedTextWriter writer, IMethodSymbol method, string ifaceName)
    {
        var typeParameters = method.TypeParameters.Length > 0
            ? "<" + string.Join(", ", method.TypeParameters.Select(static tp => tp.Name)) + ">"
            : "";
        var refPrefix = method.ReturnsByRef ? "ref " : method.ReturnsByRefReadonly ? "ref readonly " : "";
        var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormats.FullyQualified);

        // explicit interface implementations don't use Accessors since we need the interface cast anyway and the member is public through it
        writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
        writer.WriteLine($"{refPrefix}{returnType} {ifaceName}.{method.Name}{typeParameters}({ParameterString(method.Parameters)}) => {(refPrefix.Length > 0 ? "ref " : "")}(({ifaceName})_instance).{method.Name}{typeParameters}({ArgumentString(method.Parameters)});");
    }
    private static void WritePropertyProxyExplicit(IndentedTextWriter writer, IPropertySymbol property, string ifaceName)
    {
        var refPrefix = property.ReturnsByRef ? "ref " : property.ReturnsByRefReadonly ? "ref readonly " : "";
        var type = property.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified);
        var declaration = property.IsIndexer ? $"this[{ParameterString(property.Parameters)}]" : property.Name;
        var access = property.IsIndexer
            ? $"(({ifaceName})_instance)[{ArgumentString(property.Parameters)}]"
            : $"(({ifaceName})_instance).{property.Name}";

        writer.WriteLine($"{refPrefix}{type} {ifaceName}.{declaration}");
        using (writer.Scope)
        {
            if (refPrefix.Length > 0)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"get => ref {access};");
                return;
            }
            if (property.GetMethod is not null)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"get => {access};");
            }
            if (property.SetMethod is not null)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"set => {access} = value;");
            }
        }
    }
    private static void WriteEventProxyExplicit(IndentedTextWriter writer, IEventSymbol @event, string ifaceName)
    {
        writer.WriteLine($"event {@event.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified)} {ifaceName}.{@event.Name}");
        using (writer.Scope)
        {
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"add => (({ifaceName})_instance).{@event.Name} += value;");
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"remove => (({ifaceName})_instance).{@event.Name} -= value;");
        }
    }
}
