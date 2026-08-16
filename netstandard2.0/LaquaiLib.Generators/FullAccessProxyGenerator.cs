using System.CodeDom.Compiler;
using System.Text;

using LaquaiLib.Analyzers.Shared;
using LaquaiLib.Generators.Extensions;

namespace LaquaiLib.Generators;

/// <summary>
/// A pending diagnostic report, reduced to data. <see cref="Location"/> is a Roslyn object and may not survive the transform, so the pieces needed to rebuild one are carried instead.
/// </summary>
internal sealed record ProxyDiagnosticInfo(string Id, EquatableArray<string> Args, string FilePath, TextSpan Span, LinePositionSpan LineSpan);

/// <summary>
/// The result of resolving one <c>[FullAccessProxy]</c> application. <see cref="Source"/> is <see langword="null"/> when the proxy could not be generated at all.
/// <para/>The emitted source text is the model: it is a string, so it compares by value and the pipeline never pins a Roslyn object.
/// </summary>
internal sealed record ProxyModel(string HintName, string Source, EquatableArray<ProxyDiagnosticInfo> Diagnostics);

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
    public static readonly DiagnosticDescriptor ProxySurfaceReduced = new DiagnosticDescriptor(
        id: "FAP002",
        title: "Proxy surface reduced to internal",
        messageFormat: "'{0}' is declared public, but '{1}' is not publicly accessible. The generated members that mention it were emitted as 'internal'.",
        description: "A partial type declaration's accessibility cannot be changed by a generator, so members whose signatures reference types that are not publicly accessible are emitted with reduced accessibility instead.",
        category: "FullAccessProxyGenerator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor MemberNotProxyable = new DiagnosticDescriptor(
        id: "FAP003",
        title: "Member could not be proxied",
        messageFormat: "The member '{0}' could not be proxied: {1}",
        description: "Not every member can be forwarded. Reference types that cannot be named are erased to 'object', but fields, events, by-ref returns, function pointers and inaccessible value types have no such representation.",
        category: "FullAccessProxyGenerator",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor ProxiedTypeNotNameable = new DiagnosticDescriptor(
        id: "FAP004",
        title: "Proxied type cannot be named",
        messageFormat: "The type '{0}' cannot be named from this assembly. Set IncludeInaccessible = true on the [FullAccessProxy] attribute to proxy it with erased type references.",
        description: "The proxy holds the proxied instance in a field, so the type must either be nameable from the proxying assembly or be erased to 'object' via UnsafeAccessorTypeAttribute.",
        category: "FullAccessProxyGenerator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static DiagnosticDescriptor GetDescriptor(string id) => id switch
    {
        "FAP001" => TypeByNameNotFound,
        "FAP002" => ProxySurfaceReduced,
        "FAP003" => MemberNotProxyable,
        "FAP004" => ProxiedTypeNotNameable,
        _ => null
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = context.SyntaxProvider
            .ForAttributeWithMetadataNameOn<ClassDeclarationSyntax, ProxyModel>("LaquaiLib.Analyzers.Shared.Attributes.FullAccessProxyAttribute", CreateModel)
            .WithTrackingName(GeneratorStepNames.FullAccessProxyModels)
            .Where(static model => model is not null)
            .WithTrackingName(GeneratorStepNames.FullAccessProxyFiltered);

        context.RegisterSourceOutput(models, static (spc, model) =>
        {
            foreach (var info in model.Diagnostics)
            {
                var descriptor = GetDescriptor(info.Id);
                if (descriptor is null)
                {
                    continue;
                }
                spc.ReportDiagnostic(Diagnostic.Create(descriptor, Location.Create(info.FilePath, info.Span, info.LineSpan), info.Args.AsImmutableArray().ToArray()));
            }
            if (model.Source is not null)
            {
                spc.AddSource(model.HintName, SourceText.From(model.Source, Encoding.UTF8));
            }
        });
    }

    /// <summary>
    /// Resolves one <c>[FullAccessProxy]</c> application all the way to emitted source text. Every symbol touch happens here; nothing downstream sees a Roslyn object.
    /// </summary>
    private static ProxyModel CreateModel(GeneratorAttributeSyntaxContext gasc, CancellationToken cancellationToken)
    {
        var semanticModel = gasc.SemanticModel;
        if (semanticModel is null)
        {
            return null;
        }

        var attribute = gasc.Attributes.FirstOrDefault();
        if (attribute is null || attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        if (gasc.TargetSymbol is not INamedTypeSymbol proxyClassSymbol)
        {
            return null;
        }

        // the compilation is reachable from inside the transform, so CompilationProvider is unnecessary and would only defeat caching
        var compilation = semanticModel.Compilation;

        INamedTypeSymbol proxiedType = null;
        var type = attribute.ConstructorArguments[0];
        if (type.Kind == TypedConstantKind.Type)
        {
            // typeof(...) yields an ITypeSymbol here, never a System.Type
            proxiedType = type.Value as INamedTypeSymbol;
        }
        else if (type.Kind == TypedConstantKind.Primitive && type.Value is string fqTypeName)
        {
            proxiedType = compilation.GetTypeByMetadataName(NormalizeMetadataName(fqTypeName));
        }

        var includeInaccessible = false;
        var namedArguments = attribute.NamedArguments;
        for (var i = 0; i < namedArguments.Length; i++)
        {
            if (namedArguments[i].Key == "IncludeInaccessible" && namedArguments[i].Value.Value is bool flag)
            {
                includeInaccessible = flag;
            }
        }

        var location = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation();
        location ??= Unsafe.As<ClassDeclarationSyntax>(gasc.TargetNode).Identifier.GetLocation();
        var filePath = location.SourceTree?.FilePath ?? "";
        var span = location.SourceSpan;
        var lineSpan = location.GetLineSpan().Span;

        if (proxiedType is null or { TypeKind: TypeKind.Error })
        {
            var failure = ImmutableArray.Create(new ProxyDiagnosticInfo("FAP001", ImmutableArray.Create(type.Value?.ToString() ?? ""), filePath, span, lineSpan));
            return new ProxyModel(null, null, failure);
        }

        var builder = new ProxyBuilder(compilation, proxiedType, proxyClassSymbol, includeInaccessible, filePath, span, lineSpan);
        var source = builder.Generate();
        return new ProxyModel(source is null ? null : GetHintName(proxyClassSymbol, proxiedType), source, builder.DiagnosticsArray);
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

    /// <summary>
    /// Collects the most-derived declaration of every proxyable member from <paramref name="proxiedType"/> up its
    /// base type chain, stopping before <see cref="object"/> (its members already exist on the proxy and can't be
    /// legally forwarded anyway). Overloads survive; overrides/shadows are deduplicated by signature, keeping the
    /// most-derived declaration (important for <c>[UnsafeAccessor]</c> targeting).
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
                if (member is INamedTypeSymbol)
                {
                    // nested type declarations are members too, but there is nothing to forward
                    continue;
                }
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

        return SortDeterministically([.. result]);
    }
    /// <summary>
    /// Imposes a total order on the collected members. The emitted source text <i>is</i> the incremental cache key and erasure collisions are resolved first-wins, so neither may depend on Roslyn's enumeration order.
    /// </summary>
    private static ISymbol[] SortDeterministically(ISymbol[] members)
    {
        var keys = new string[members.Length];
        for (var i = 0; i < keys.Length; i++)
        {
            var (kind, name, arity, signature) = MakeMemberKey(members[i]);
            keys[i] = $"{(int)kind}|{name}|{arity}|{signature}";
        }
        // MakeMemberKey already deduplicated, so no two keys can compare equal and the sort's instability is unobservable
        Array.Sort(keys, members, StringComparer.Ordinal);
        return members;
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
    /// Every constructor is emitted under the single name <c>ProxyCtor</c> and distinguished only by its parameter list, so an erasure collision's first-wins outcome would otherwise follow Roslyn's enumeration order.
    /// </summary>
    private static void SortCtorsDeterministically(IMethodSymbol[] ctors)
    {
        var keys = new string[ctors.Length];
        for (var i = 0; i < keys.Length; i++)
        {
            keys[i] = ParameterSignature(ctors[i].Parameters);
        }
        Array.Sort(keys, ctors, StringComparer.Ordinal);
    }

    /// <summary>
    /// Filters <paramref name="proxiedType"/>'s interfaces down to those every member of which can be forwarded to the proxied instance.
    /// <para/>Erasure can never help here: explicit implementations have to spell the interface member out, so there is no position an <c>object</c> placeholder could occupy.
    /// </summary>
    private static INamedTypeSymbol[] GetProxyableInterfaces(INamedTypeSymbol proxiedType)
    {
        var all = proxiedType.AllInterfaces;
        var result = new List<INamedTypeSymbol>(all.Length);
        for (var i = 0; i < all.Length; i++)
        {
            if (IsEffectivelyPublic(all[i]) && CanForwardAllMembers(all[i]))
            {
                result.Add(all[i]);
            }
        }
        return [.. result];
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
                        || !IsEffectivelyPublic(method.ReturnType) || !AreParametersAccessible(method.Parameters))
                    {
                        return false;
                    }
                    break;
                case IPropertySymbol property:
                    // init-only setters can't be forwarded, assigning them outside an object initializer of the target is illegal
                    if (property.IsStatic || property.SetMethod is { IsInitOnly: true }
                        || !IsEffectivelyPublic(property.Type) || !AreParametersAccessible(property.Parameters))
                    {
                        return false;
                    }
                    break;
                case IEventSymbol @event:
                    if (@event.IsStatic || !IsEffectivelyPublic(@event.Type))
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
            if (!IsEffectivelyPublic(parameters[i].Type))
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Determines whether <paramref name="type"/> is visible to every assembly, which is what decides whether a member mentioning it may be emitted as <c>public</c> (CS0050/CS0053).
    /// </summary>
    private static bool IsEffectivelyPublic(ITypeSymbol type)
    {
        switch (type)
        {
            case null:
                return false;
            case ITypeParameterSymbol or IDynamicTypeSymbol:
                return true;
            case IArrayTypeSymbol array:
                return IsEffectivelyPublic(array.ElementType);
            case IPointerTypeSymbol pointer:
                return IsEffectivelyPublic(pointer.PointedAtType);
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
                if (!IsEffectivelyPublic(named.TypeArguments[i]))
                {
                    return false;
                }
            }
        }
        return true;
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

    private static string EscapeIdentifier(string name)
        => Microsoft.CodeAnalysis.CSharp.SyntaxFacts.GetKeywordKind(name) != Microsoft.CodeAnalysis.CSharp.SyntaxKind.None ? "@" + name : name;

    /// <summary>
    /// Determines whether a forwarder for <paramref name="method"/> would hide a member the proxy inherits from <see cref="object"/>, which needs <c>new</c> to avoid CS0114/CS0108.
    /// </summary>
    private static bool HidesObjectMember(IMethodSymbol method)
    {
        if (method.TypeParameters.Length > 0)
        {
            return false;
        }
        return method.Name switch
        {
            "ToString" or "GetHashCode" or "GetType" => method.Parameters.Length == 0,
            "Equals" => method.Parameters.Length == 1 && method.Parameters[0].Type.SpecialType == SpecialType.System_Object,
            _ => false
        };
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

    private enum TypeRefKind
    {
        // default(TypeRef) must mean "can't be represented at all"
        Unsupported,
        Nameable,
        Erased
    }

    /// <summary>
    /// How a single type is going to appear in the emitted source, decided exactly once per position.
    /// </summary>
    private readonly struct TypeRef
    {
        public readonly TypeRefKind Kind;
        /// <summary>The C# text of the type: a fully qualified name, or <c>object</c>/<c>void*</c> when erased.</summary>
        public readonly string Text;
        /// <summary>The reflection name to feed <c>[UnsafeAccessorType]</c>. Non-<see langword="null"/> exactly when <see cref="Kind"/> is <see cref="TypeRefKind.Erased"/>.</summary>
        public readonly string MetadataName;
        /// <summary>Whether a member mentioning this type may be emitted as <c>public</c>.</summary>
        public readonly bool EffectivelyPublic;
        public readonly bool RequiresUnsafe;

        private TypeRef(TypeRefKind kind, string text, string metadataName, bool effectivelyPublic, bool requiresUnsafe)
        {
            Kind = kind;
            Text = text;
            MetadataName = metadataName;
            EffectivelyPublic = effectivelyPublic;
            RequiresUnsafe = requiresUnsafe;
        }

        public bool IsSupported => Kind != TypeRefKind.Unsupported;
        public bool IsErased => Kind == TypeRefKind.Erased;

        public static TypeRef Nameable(string text, bool effectivelyPublic, bool requiresUnsafe) => new TypeRef(TypeRefKind.Nameable, text, null, effectivelyPublic, requiresUnsafe);
        public static TypeRef Erased(string text, string metadataName) => new TypeRef(TypeRefKind.Erased, text, metadataName, true, text == "void*");
    }

    /// <summary>
    /// Everything the emitters need to know about one member, computed once so the accessor and the forwarder can never disagree.
    /// </summary>
    private sealed class MemberPlan
    {
        public ISymbol Symbol;
        /// <summary>The type of the accessor's <c>target</c> parameter, i.e. the member's own containing type.</summary>
        public TypeRef Target;
        /// <summary>Return type, field type, event type or property type.</summary>
        public TypeRef Result;
        public TypeRef[] Parameters;
        public bool Supported;
        public string SkipReason;
        /// <summary>Whether the forwarder has to be emitted as <c>internal</c> rather than <c>public</c>.</summary>
        public bool Clamp;
        public bool EmitAccessor;
        public bool EmitForwarder;
        public bool RequiresUnsafe;
    }

    private static readonly TypeRef[] _noParameters = [];

    /// <summary>
    /// Resolves and emits one proxy. Instantiated per <c>[FullAccessProxy]</c> application, inside the transform.
    /// </summary>
    private sealed class ProxyBuilder
    {
        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _proxiedType;
        private readonly INamedTypeSymbol _proxyClassSymbol;
        private readonly bool _allowErasure;
        private readonly string _filePath;
        private readonly TextSpan _span;
        private readonly LinePositionSpan _lineSpan;
        private readonly List<ProxyDiagnosticInfo> _diagnostics = [];

        private TypeRef _proxiedRef;
        private string _proxiedDisplay;

        public ProxyBuilder(Compilation compilation, INamedTypeSymbol proxiedType, INamedTypeSymbol proxyClassSymbol, bool allowErasure, string filePath, TextSpan span, LinePositionSpan lineSpan)
        {
            _compilation = compilation;
            _proxiedType = proxiedType;
            _proxyClassSymbol = proxyClassSymbol;
            _allowErasure = allowErasure;
            _filePath = filePath;
            _span = span;
            _lineSpan = lineSpan;
        }

        public EquatableArray<ProxyDiagnosticInfo> DiagnosticsArray => new EquatableArray<ProxyDiagnosticInfo>([.. _diagnostics]);

        private void Report(string id, params string[] args) => _diagnostics.Add(new ProxyDiagnosticInfo(id, ImmutableArray.Create(args), _filePath, _span, _lineSpan));
        private void ReportSkip(ISymbol member, string reason)
        {
            // staying silent by default keeps existing proxies noise-free; opting in means you asked for maximum coverage and deserve to know what's still missing
            if (_allowErasure)
            {
                Report("FAP003", member.ToDisplayString(), reason);
            }
        }

        #region Type resolution
        /// <summary>
        /// Whether <paramref name="type"/> can be spelled out from the assembly being compiled. Unlike <see cref="IsEffectivelyPublic"/> this honours <c>internal</c> and <c>InternalsVisibleTo</c>.
        /// </summary>
        private bool IsAccessible(ITypeSymbol type)
        {
            switch (type)
            {
                case null:
                    return false;
                case ITypeParameterSymbol or IDynamicTypeSymbol:
                    return true;
                case IArrayTypeSymbol array:
                    return IsAccessible(array.ElementType);
                case IPointerTypeSymbol pointer:
                    return IsAccessible(pointer.PointedAtType);
                case IFunctionPointerTypeSymbol:
                    // signature-identical function pointer types can't be reliably matched by [UnsafeAccessor]
                    return false;
                case IErrorTypeSymbol:
                    return false;
            }

            if (!_compilation.IsSymbolAccessibleWithin(type, _compilation.Assembly))
            {
                return false;
            }
            // IsSymbolAccessibleWithin doesn't look at type arguments
            if (type is INamedTypeSymbol { IsGenericType: true } named)
            {
                for (var i = 0; i < named.TypeArguments.Length; i++)
                {
                    if (!IsAccessible(named.TypeArguments[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private TypeRef ResolveType(ITypeSymbol type, bool byRefPosition)
        {
            if (type is null)
            {
                return default;
            }
            if (IsAccessible(type))
            {
                return TypeRef.Nameable(type.ToDisplayString(SymbolDisplayFormats.FullyQualified), IsEffectivelyPublic(type), RequiresUnsafeContext(type));
            }
            if (!_allowErasure)
            {
                return default;
            }
            // the runtime refuses [UnsafeAccessorType] on by-ref returns (it would be a type safety hole) and on value types
            if (byRefPosition || type.IsValueType || type is IFunctionPointerTypeSymbol)
            {
                return default;
            }
            var metadataName = MetadataTypeName.TryBuild(type, _compilation.Assembly);
            return metadataName is null ? default : TypeRef.Erased(type is IPointerTypeSymbol ? "void*" : "object", metadataName);
        }

        /// <summary>
        /// Resolution for positions that can never be erased, i.e. field and event types.
        /// </summary>
        private TypeRef ResolveExact(ITypeSymbol type)
            => type is not null && IsAccessible(type)
                ? TypeRef.Nameable(type.ToDisplayString(SymbolDisplayFormats.FullyQualified), IsEffectivelyPublic(type), RequiresUnsafeContext(type))
                : default;

        private TypeRef[] ResolveParameters(ImmutableArray<IParameterSymbol> parameters)
        {
            if (parameters.Length == 0)
            {
                return _noParameters;
            }
            var result = new TypeRef[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                result[i] = ResolveType(parameters[i].Type, false);
            }
            return result;
        }
        #endregion

        #region Planning
        private MemberPlan PlanMember(ISymbol member)
        {
            var plan = new MemberPlan
            {
                Symbol = member,
                Parameters = _noParameters,
                Target = ResolveType(member.ContainingType, false)
            };

            ImmutableArray<IParameterSymbol> parameters = default;
            switch (member)
            {
                case IMethodSymbol method:
                    plan.Result = ResolveType(method.ReturnType, method.ReturnsByRef || method.ReturnsByRefReadonly);
                    parameters = method.Parameters;
                    plan.Parameters = ResolveParameters(parameters);
                    break;
                case IPropertySymbol property:
                    plan.Result = ResolveType(property.Type, property.ReturnsByRef || property.ReturnsByRefReadonly);
                    parameters = property.Parameters;
                    plan.Parameters = ResolveParameters(parameters);
                    break;
                case IFieldSymbol field:
                    // field accessors are 'ref T' returns, which the runtime refuses to erase
                    plan.Result = ResolveExact(field.Type);
                    break;
                case IEventSymbol @event:
                    // 'event object' isn't a delegate type, so there is nothing to erase to
                    plan.Result = ResolveExact(@event.Type);
                    break;
                default:
                    plan.Result = default;
                    break;
            }

            if (!plan.Target.IsSupported)
            {
                plan.SkipReason = $"its declaring type '{member.ContainingType.ToDisplayString()}' cannot be referenced from this assembly";
            }
            else if (!plan.Result.IsSupported)
            {
                plan.SkipReason = $"its result type '{GetResultType(member)?.ToDisplayString() ?? "?"}' cannot be referenced from this assembly";
            }
            else
            {
                for (var i = 0; i < plan.Parameters.Length; i++)
                {
                    if (!plan.Parameters[i].IsSupported)
                    {
                        plan.SkipReason = $"the type '{parameters[i].Type.ToDisplayString()}' of parameter '{parameters[i].Name}' cannot be referenced from this assembly";
                        break;
                    }
                }
            }

            plan.Supported = plan.SkipReason is null;
            if (!plan.Supported)
            {
                return plan;
            }

            plan.EmitAccessor = true;
            plan.EmitForwarder = true;
            plan.RequiresUnsafe = plan.Result.RequiresUnsafe;
            // the accessor lives inside a private nested class, so only the forwarder's accessibility is constrained
            plan.Clamp = !plan.Result.EffectivelyPublic || !plan.Target.EffectivelyPublic;
            for (var i = 0; i < plan.Parameters.Length; i++)
            {
                plan.RequiresUnsafe |= plan.Parameters[i].RequiresUnsafe;
                plan.Clamp |= !plan.Parameters[i].EffectivelyPublic;
            }
            return plan;
        }

        private static ITypeSymbol GetResultType(ISymbol member) => member switch
        {
            IMethodSymbol method => method.ReturnType,
            IPropertySymbol property => property.Type,
            IFieldSymbol field => field.Type,
            IEventSymbol @event => @event.Type,
            _ => null
        };

        private static string SignatureOf(IReadOnlyList<IParameterSymbol> parameters, TypeRef[] refs)
        {
            if (refs.Length == 0)
            {
                return "";
            }
            var sb = new StringBuilder();
            for (var i = 0; i < refs.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }
                sb.Append(parameters[i].RefKind).Append(':').Append(refs[i].Text);
            }
            return sb.ToString();
        }
        #endregion

        #region Rendering primitives
        private static string RenderErasedParameter(IParameterSymbol parameter, in TypeRef typeRef, bool forAccessor)
        {
            var sb = new StringBuilder();
            if (forAccessor)
            {
                sb.Append(SourceEmitHelper.UnsafeAccessorTypeParameter(typeRef.MetadataName));
            }
            switch (parameter.RefKind)
            {
                case RefKind.Ref:
                    sb.Append("ref ");
                    break;
                case RefKind.Out:
                    sb.Append("out ");
                    break;
                case RefKind.In:
                    sb.Append("in ");
                    break;
                case RefKind.RefReadOnlyParameter:
                    sb.Append("ref readonly ");
                    break;
            }
            sb.Append(typeRef.Text).Append(' ').Append(EscapeIdentifier(parameter.Name));
            if (parameter.HasExplicitDefaultValue)
            {
                // the only default an erased reference can carry
                sb.Append(" = null");
            }
            return sb.ToString();
        }
        private static string RenderParameters(ImmutableArray<IParameterSymbol> parameters, TypeRef[] refs, bool forAccessor)
        {
            if (parameters.Length == 0)
            {
                return "";
            }
            var parts = new string[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                parts[i] = refs[i].IsErased
                    ? RenderErasedParameter(parameters[i], refs[i], forAccessor)
                    : parameters[i].ToDisplayString(SymbolDisplayFormats.FullyQualifiedParameter);
            }
            return string.Join(", ", parts);
        }
        private static string RenderArguments(ImmutableArray<IParameterSymbol> parameters)
        {
            if (parameters.Length == 0)
            {
                return "";
            }
            var parts = new string[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                var name = EscapeIdentifier(p.Name);
                parts[i] = p.RefKind switch
                {
                    RefKind.Ref or RefKind.RefReadOnlyParameter => "ref " + name,
                    RefKind.Out => "out " + name,
                    RefKind.In => "in " + name,
                    _ => name
                };
            }
            return string.Join(", ", parts);
        }
        private static string RenderTargetParameter(in TypeRef target)
            => target.IsErased
                ? SourceEmitHelper.UnsafeAccessorTypeParameter(target.MetadataName) + "object target"
                : target.Text + " target";
        #endregion

        #region Emission
        public string Generate()
        {
            _proxiedDisplay = _proxiedType.ToDisplayString(SymbolDisplayFormats.FullyQualified);
            _proxiedRef = ResolveType(_proxiedType, false);
            if (!_proxiedRef.IsSupported)
            {
                Report("FAP004", _proxiedType.ToDisplayString());
                return null;
            }
            if (_proxyClassSymbol.DeclaredAccessibility is Accessibility.Public && !_proxiedRef.EffectivelyPublic)
            {
                Report("FAP002", _proxyClassSymbol.Name, _proxiedType.ToDisplayString());
            }

            var members = GetProxyableMembers(_proxiedType);
            var plans = new MemberPlan[members.Length];
            var planBySymbol = new Dictionary<ISymbol, MemberPlan>(members.Length, SymbolEqualityComparer.Default);
            for (var i = 0; i < members.Length; i++)
            {
                plans[i] = PlanMember(members[i]);
                planBySymbol[members[i]] = plans[i];
            }

            var ctorSymbols = _proxiedType.GetMembers().OfType<IMethodSymbol>().Where(static m => m.Name == ".ctor").ToArray();
            SortCtorsDeterministically(ctorSymbols);
            var ctorPlans = new MemberPlan[ctorSymbols.Length];
            for (var i = 0; i < ctorSymbols.Length; i++)
            {
                ctorPlans[i] = PlanConstructor(ctorSymbols[i]);
            }

            ResolveCollisions(plans, ctorPlans);

            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            using var writer = new IndentedTextWriter(sw);

            writer.WriteLine(SourceEmitHelper.GeneratedFileHeader);
            if (!_proxyClassSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                writer.WriteLine($"namespace {_proxyClassSymbol.ContainingNamespace.ToDisplayString()};");
            }

            // collect containing types outermost-first so we can nest the proxy declaration correctly
            var ancestors = new List<INamedTypeSymbol>();
            for (var t = _proxyClassSymbol.ContainingType; t is not null; t = t.ContainingType)
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
            writer.WriteLine($"/// Wraps an instance of <c>{_proxiedDisplay}</c> to provide full access to its members, regardless of visibility.");
            writer.WriteLine($"/// <para/>Use the static <c>Create</c> methods to create instances using any of the instance constructors on that type. They also proxy non-public constructors.");
            writer.WriteLine("/// <para/>Types that cannot be named from this assembly are erased to <c>object</c> when <c>IncludeInaccessible</c> is set. Members are skipped only when even that is impossible, which is the case for fields and events of such types, <see langword=\"ref\"/>-returning members, function pointers and inaccessible value types.");
            writer.WriteLine("/// </summary>");

            writer.WriteLines(SourceEmitHelper.GeneratedCodeAttribute(typeof(FullAccessProxyGenerator)));

            // only interfaces we can actually forward every member of may appear in the base list, otherwise we'd emit CS0535
            var interfaces = GetProxyableInterfaces(_proxiedType);

            var classModifiers = GetAccessibilityKeyword(_proxyClassSymbol.DeclaredAccessibility);
            if (_proxyClassSymbol.TypeKind == TypeKind.Class)
            {
                classModifiers += " sealed";
            }
            writer.Write($"{classModifiers} partial class {_proxyClassSymbol.Name}{GetTypeParameterList(_proxyClassSymbol)}");
            if (interfaces.Length > 0)
            {
                writer.Write(" : ");
                writer.Write(string.Join(", ", interfaces.Select(static i => i.ToDisplayString(SymbolDisplayFormats.FullyQualified))));
            }
            writer.WriteLine();

            using (writer.Scope)
            {
                WriteInstanceField(writer);
                WriteProxyCtor(writer);
                WriteStaticCtorProxies(writer, ctorSymbols, ctorPlans);
                WriteUnsafeAccessorUtility(writer, plans, ctorSymbols, ctorPlans);
                WriteInterfaceImplementations(writer, interfaces);

                var memberGroups = plans
                    .Where(static p => !p.Symbol.IsInterfaceImplementation)
                    .GroupBy(static p => p.Symbol.Kind)
                    .ToDictionary(static g => g.Key, static g => g.ToArray());

                foreach (var group in memberGroups)
                {
                    var kind = group.Key;
                    using var region = writer.Region($"Proxied members of kind: {kind}");

                    foreach (var plan in group.Value)
                    {
                        if (!plan.Supported || !plan.EmitForwarder)
                        {
                            continue;
                        }
                        switch (plan.Symbol)
                        {
                            case IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol:
                                WriteMethodProxy(writer, methodSymbol, plan);
                                break;
                            case IFieldSymbol fieldSymbol:
                                WriteFieldProxy(writer, fieldSymbol, plan);
                                break;
                            case IEventSymbol eventSymbol:
                                WriteEventProxy(writer, eventSymbol, plan, planBySymbol);
                                break;
                            case IPropertySymbol propertySymbol:
                                WritePropertyProxy(writer, propertySymbol, plan, planBySymbol);
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

        private MemberPlan PlanConstructor(IMethodSymbol ctor)
        {
            var plan = new MemberPlan
            {
                Symbol = ctor,
                Target = _proxiedRef,
                Result = _proxiedRef,
                Parameters = ResolveParameters(ctor.Parameters)
            };
            for (var i = 0; i < plan.Parameters.Length; i++)
            {
                if (!plan.Parameters[i].IsSupported)
                {
                    plan.SkipReason = $"the type '{ctor.Parameters[i].Type.ToDisplayString()}' of parameter '{ctor.Parameters[i].Name}' cannot be referenced from this assembly";
                    return plan;
                }
            }
            plan.Supported = true;
            plan.EmitAccessor = true;
            plan.EmitForwarder = true;
            plan.Clamp = !_proxiedRef.EffectivelyPublic;
            for (var i = 0; i < plan.Parameters.Length; i++)
            {
                plan.RequiresUnsafe |= plan.Parameters[i].RequiresUnsafe;
                plan.Clamp |= !plan.Parameters[i].EffectivelyPublic;
            }
            return plan;
        }

        /// <summary>
        /// Erasing types collapses overloads that differed only in their erased parameters, which would be a duplicate member.
        /// Decided up front because <c>Create</c> is emitted before the accessors it calls.
        /// </summary>
        private void ResolveCollisions(MemberPlan[] plans, MemberPlan[] ctorPlans)
        {
            var ctorKeys = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < ctorPlans.Length; i++)
            {
                var plan = ctorPlans[i];
                if (!plan.Supported)
                {
                    ReportSkip(plan.Symbol, plan.SkipReason);
                    continue;
                }
                var ctor = (IMethodSymbol)plan.Symbol;
                if (!ctorKeys.Add(SignatureOf(ctor.Parameters, plan.Parameters)))
                {
                    plan.EmitAccessor = false;
                    plan.EmitForwarder = false;
                    ReportSkip(ctor, "erasing its parameter types makes it indistinguishable from another constructor overload");
                }
            }

            var accessorKeys = new HashSet<string>(StringComparer.Ordinal);
            var forwarderKeys = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < plans.Length; i++)
            {
                var plan = plans[i];
                if (!plan.Supported)
                {
                    ReportSkip(plan.Symbol, plan.SkipReason);
                    continue;
                }

                var accessorKey = AccessorKey(plan);
                if (accessorKey is not null && !accessorKeys.Add(accessorKey))
                {
                    plan.EmitAccessor = false;
                    plan.EmitForwarder = false;
                    ReportSkip(plan.Symbol, "erasing its signature makes it indistinguishable from another member of the proxied type");
                    continue;
                }

                var forwarderKey = ForwarderKey(plan);
                if (forwarderKey is not null && !forwarderKeys.Add(forwarderKey))
                {
                    plan.EmitForwarder = false;
                    ReportSkip(plan.Symbol, "erasing its signature makes the forwarding member indistinguishable from another one");
                }
            }
        }
        private static string AccessorKey(MemberPlan plan) => plan.Symbol switch
        {
            IMethodSymbol method => $"M:{method.Name}`{method.TypeParameters.Length}({plan.Target.Text}|{SignatureOf(method.Parameters, plan.Parameters)})",
            IFieldSymbol field => $"F:{field.Name}({plan.Target.Text})",
            _ => null
        };
        private static string ForwarderKey(MemberPlan plan) => plan.Symbol switch
        {
            IMethodSymbol { MethodKind: MethodKind.Ordinary } method => $"N:{method.Name}`{method.TypeParameters.Length}({SignatureOf(method.Parameters, plan.Parameters)})",
            IPropertySymbol property => property.IsIndexer ? $"I:({SignatureOf(property.Parameters, plan.Parameters)})" : "N:" + property.Name,
            IFieldSymbol field => "N:" + field.Name,
            IEventSymbol @event => "N:" + @event.Name,
            _ => null
        };
        private static bool AccessorAvailable(Dictionary<ISymbol, MemberPlan> planBySymbol, IMethodSymbol accessor)
            => accessor is null || !planBySymbol.TryGetValue(accessor, out var plan) || (plan.Supported && plan.EmitAccessor);

        private void WriteInstanceField(IndentedTextWriter writer)
        {
            using var region = writer.Region("Instance field");

            // We split this into a field-prop combo to avoid the overhead of the property getter for internal use
            writer.WriteLine($"private readonly {_proxiedRef.Text} _instance;");

            writer.WriteLine("/// <summary>");
            writer.WriteLine($"/// Gets the proxied instance of <c>{_proxiedDisplay}</c>.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine($"{(_proxiedRef.EffectivelyPublic ? "public" : "internal")} {_proxiedRef.Text} Instance");
            using (writer.Scope)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine("get => _instance;");
            }
        }
        private void WriteProxyCtor(IndentedTextWriter writer)
        {
            using var region = writer.Region("Proxy constructor");

            writer.WriteLine("/// <summary>");
            writer.WriteLine($"/// Initializes a new instance of this proxy class for <c>{_proxiedDisplay}</c>.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine($"/// <param name=\"instance\">The instance of <c>{_proxiedDisplay}</c> to proxy. Must not be <see langword=\"null\"/>, otherwise an exception is thrown.</param>");
            writer.WriteLine($"{(_proxiedRef.EffectivelyPublic ? "public" : "internal")} {_proxyClassSymbol.Name}({_proxiedRef.Text} instance)");
            using (writer.Scope)
            {
                writer.WriteLine("_instance = instance ?? throw new ArgumentNullException(nameof(instance));");
            }
        }
        private void WriteStaticCtorProxies(IndentedTextWriter writer, IMethodSymbol[] ctors, MemberPlan[] ctorPlans)
        {
            using var region = writer.Region("Static constructor proxies");

            for (var i = 0; i < ctors.Length; i++)
            {
                var plan = ctorPlans[i];
                if (!plan.Supported || !plan.EmitForwarder)
                {
                    continue;
                }
                var ctor = ctors[i];

                writer.WriteLine("/// <summary>");
                writer.WriteLine($"/// Initializes a new instance of this proxy class for <c>{_proxiedDisplay}</c> using the following instance constructor overload of that type:");
                writer.WriteLine($"/// <para/><c>{ctor.ToDisplayString(SymbolDisplayFormats.FullyQualified).XmlEscape()}</c>");
                writer.WriteLine("/// </summary>");
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"{(plan.Clamp ? "internal" : "public")} static {(plan.RequiresUnsafe ? "unsafe " : "")}{_proxyClassSymbol.Name} Create({RenderParameters(ctor.Parameters, plan.Parameters, false)})");
                using (writer.Scope)
                {
                    writer.WriteLine($"return new {_proxyClassSymbol.Name}(Accessors.ProxyCtor({RenderArguments(ctor.Parameters)}));");
                }
            }
        }
        private void WriteUnsafeAccessorUtility(IndentedTextWriter writer, MemberPlan[] plans, IMethodSymbol[] ctors, MemberPlan[] ctorPlans)
        {
            using var region = writer.Region("Unsafe accessors utility");

            writer.WriteLine("private static class Accessors");
            using (writer.Scope)
            {
                for (var i = 0; i < ctors.Length; i++)
                {
                    var plan = ctorPlans[i];
                    if (!plan.Supported || !plan.EmitAccessor)
                    {
                        continue;
                    }
                    writer.WriteLine(SourceEmitHelper.UnsafeAccessor_Ctor);
                    if (_proxiedRef.IsErased)
                    {
                        writer.WriteLine(SourceEmitHelper.UnsafeAccessorTypeReturn(_proxiedRef.MetadataName));
                    }
                    writer.WriteLine($"public static {(plan.RequiresUnsafe ? "unsafe " : "")}extern {_proxiedRef.Text} ProxyCtor({RenderParameters(ctors[i].Parameters, plan.Parameters, true)});");
                }

                for (var i = 0; i < plans.Length; i++)
                {
                    var plan = plans[i];
                    if (!plan.Supported || !plan.EmitAccessor)
                    {
                        continue;
                    }

                    // [UnsafeAccessor] doesn't walk the base hierarchy, so target must be the member's OWN containing type, not the proxied type
                    switch (plan.Symbol)
                    {
                        // Methods will also hit property and event accessors, whichever are declared
                        case IMethodSymbol methodSymbol when methodSymbol.Name is not ".cctor" && methodSymbol.ExplicitInterfaceImplementations.Length == 0 && !methodSymbol.IsInitOnly:
                        {
                            writer.WriteLine(methodSymbol.IsStatic ? SourceEmitHelper.UnsafeAccessor_StaticMethod : SourceEmitHelper.UnsafeAccessor_Method);
                            if (plan.Result.IsErased)
                            {
                                writer.WriteLine(SourceEmitHelper.UnsafeAccessorTypeReturn(plan.Result.MetadataName));
                            }
                            var parameterString = RenderParameters(methodSymbol.Parameters, plan.Parameters, true);
                            if (parameterString.Length > 0)
                            {
                                parameterString = ", " + parameterString;
                            }
                            var unsafeKeyword = plan.RequiresUnsafe ? "unsafe " : "";
                            var typeParameterList = GetMethodTypeParameterList(methodSymbol);
                            var constraintsClause = GetTypeParameterConstraintsClause(methodSymbol.TypeParameters);
                            var refPrefix = methodSymbol.ReturnsByRef ? "ref " : methodSymbol.ReturnsByRefReadonly ? "ref readonly " : "";
                            writer.WriteLine($"public static {unsafeKeyword}extern {refPrefix}{plan.Result.Text} {methodSymbol.Name}{typeParameterList}({RenderTargetParameter(plan.Target)}{parameterString}){constraintsClause};");
                            break;
                        }
                        // Fields will also hit events
                        case IFieldSymbol fieldSymbol:
                        {
                            writer.WriteLine(fieldSymbol.IsStatic ? SourceEmitHelper.UnsafeAccessor_StaticField : SourceEmitHelper.UnsafeAccessor_Field);
                            writer.WriteLine($"public static {(plan.RequiresUnsafe ? "unsafe " : "")}extern ref {plan.Result.Text} {fieldSymbol.Name}({RenderTargetParameter(plan.Target)});");
                            break;
                        }

                        default:
                            break;
                    }
                }
            }
        }

        private void WriteMethodProxy(IndentedTextWriter writer, IMethodSymbol methodSymbol, MemberPlan plan)
        {
            if (methodSymbol.IsAccessor || methodSymbol.ExplicitInterfaceImplementations.Length > 0)
            {
                return;
            }

            var parameterString = RenderParameters(methodSymbol.Parameters, plan.Parameters, false);
            var argumentString = RenderArguments(methodSymbol.Parameters);
            if (argumentString.Length > 0)
            {
                argumentString = ", " + argumentString;
            }

            writer.WriteLine("/// <summary>");
            writer.WriteLine($"/// Proxies the following method from <c>{_proxiedDisplay}</c>:");
            writer.WriteLine($"/// <para/><c>{methodSymbol.ToDisplayString(SymbolDisplayFormats.FullyQualified).XmlEscape()}({parameterString.XmlEscape()})</c>");
            writer.WriteLine("/// </summary>");

            var refPrefix = methodSymbol.ReturnsByRef ? "ref " : methodSymbol.ReturnsByRefReadonly ? "ref readonly " : "";
            var refReturn = refPrefix.Length > 0 ? "ref " : "";
            var typeParameterList = GetMethodTypeParameterList(methodSymbol);
            var constraintsClause = GetTypeParameterConstraintsClause(methodSymbol.TypeParameters);
            var unsafeKeyword = plan.RequiresUnsafe ? "unsafe " : "";
            var accessibility = plan.Clamp ? "internal" : "public";
            var newKeyword = HidesObjectMember(methodSymbol) ? "new " : "";
            var target = methodSymbol.IsStatic ? "null" : "_instance";

            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"{accessibility} {newKeyword}{(methodSymbol.IsStatic ? "static " : "")}{unsafeKeyword}{refPrefix}{plan.Result.Text} {methodSymbol.Name}{typeParameterList}({parameterString}){constraintsClause} => {refReturn}Accessors.{methodSymbol.Name}{typeParameterList}({target}{argumentString});");
        }
        private void WriteEventProxy(IndentedTextWriter writer, IEventSymbol eventSymbol, MemberPlan plan, Dictionary<ISymbol, MemberPlan> planBySymbol)
        {
            if (eventSymbol.ExplicitInterfaceImplementations.Length > 0)
            {
                return;
            }
            if (!AccessorAvailable(planBySymbol, eventSymbol.AddMethod) || !AccessorAvailable(planBySymbol, eventSymbol.RemoveMethod))
            {
                return;
            }

            writer.WriteLine("/// <summary>");
            writer.WriteLine($"/// Proxies the event <c>{_proxiedDisplay}.{eventSymbol.Name}</c>.");
            writer.WriteLine("/// </summary>");

            var target = eventSymbol.IsStatic ? "null" : "_instance";
            writer.WriteLine($"{(plan.Clamp ? "internal" : "public")} {(eventSymbol.IsStatic ? "static " : "")}event {plan.Result.Text} {eventSymbol.Name}");
            using (writer.Scope)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"add => Accessors.add_{eventSymbol.Name}({target}, value);");
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"remove => Accessors.remove_{eventSymbol.Name}({target}, value);");
            }
        }
        private void WriteFieldProxy(IndentedTextWriter writer, IFieldSymbol field, MemberPlan plan)
        {
            writer.WriteLine("/// <summary>");
            writer.WriteLine($"/// Proxies the field <c>{_proxiedDisplay}.{field.Name}</c>.");
            writer.WriteLine("/// </summary>");

            // Fields will be proxied with a property of the same name
            var target = field.IsStatic ? "null" : "_instance";
            writer.WriteLine($"{(plan.Clamp ? "internal" : "public")} {(field.IsStatic ? "static " : "")}{(plan.RequiresUnsafe ? "unsafe " : "")}{plan.Result.Text} {field.Name}");
            using (writer.Scope)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"get => Accessors.{field.Name}({target});");
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"set => Accessors.{field.Name}({target}) = value;");
            }
        }
        private void WritePropertyProxy(IndentedTextWriter writer, IPropertySymbol property, MemberPlan plan, Dictionary<ISymbol, MemberPlan> planBySymbol)
        {
            if (property.ExplicitInterfaceImplementations.Length > 0)
            {
                return;
            }

            var getAvailable = property.GetMethod is not null && AccessorAvailable(planBySymbol, property.GetMethod);
            var setAvailable = property.SetMethod is not null && !property.SetMethod.IsInitOnly && AccessorAvailable(planBySymbol, property.SetMethod);
            if (!getAvailable && !setAvailable)
            {
                return;
            }

            var refPrefix = property.ReturnsByRef ? "ref " : property.ReturnsByRefReadonly ? "ref readonly " : "";
            var declaration = property.IsIndexer ? $"this[{RenderParameters(property.Parameters, plan.Parameters, false)}]" : property.Name;
            var target = property.IsStatic ? "null" : "_instance";
            var args = property.IsIndexer ? RenderArguments(property.Parameters) : "";
            var argsWithLeadingComma = args.Length > 0 ? ", " + args : "";
            var unsafeKeyword = plan.RequiresUnsafe ? "unsafe " : "";

            writer.WriteLine("/// <summary>");
            writer.WriteLine($"/// Proxies the {(property.IsIndexer ? "indexer" : "property")} <c>{_proxiedDisplay}.{declaration.XmlEscape()}</c>.");
            writer.WriteLine("/// </summary>");

            writer.WriteLine($"{(plan.Clamp ? "internal" : "public")}{(property.IsStatic ? " static" : "")} {unsafeKeyword}{refPrefix}{plan.Result.Text} {declaration}");
            using (writer.Scope)
            {
                if (refPrefix.Length > 0)
                {
                    // ref-returning properties only get a get accessor forwarding by ref
                    writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                    writer.WriteLine($"get => ref Accessors.{property.GetMethod.Name}({target}{argsWithLeadingComma});");
                    return;
                }
                if (getAvailable)
                {
                    writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                    writer.WriteLine($"get => Accessors.{property.GetMethod.Name}({target}{argsWithLeadingComma});");
                }
                // init-only setters can't be meaningfully forwarded post-construction
                if (setAvailable)
                {
                    writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                    writer.WriteLine($"set => Accessors.{property.SetMethod.Name}({target}{argsWithLeadingComma}, value);");
                }
            }
        }

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
        private static void WriteMethodProxyExplicit(IndentedTextWriter writer, IMethodSymbol method, string ifaceName)
        {
            var typeParameters = GetMethodTypeParameterList(method);
            var refPrefix = method.ReturnsByRef ? "ref " : method.ReturnsByRefReadonly ? "ref readonly " : "";
            var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormats.FullyQualified);

            // explicit interface implementations don't use Accessors since we need the interface cast anyway and the member is public through it
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"{refPrefix}{returnType} {ifaceName}.{method.Name}{typeParameters}({RenderParameters(method.Parameters, NameableRefs(method.Parameters), false)}) => {(refPrefix.Length > 0 ? "ref " : "")}(({ifaceName})_instance).{method.Name}{typeParameters}({RenderArguments(method.Parameters)});");
        }
        private static void WritePropertyProxyExplicit(IndentedTextWriter writer, IPropertySymbol property, string ifaceName)
        {
            var refPrefix = property.ReturnsByRef ? "ref " : property.ReturnsByRefReadonly ? "ref readonly " : "";
            var type = property.Type.ToDisplayString(SymbolDisplayFormats.FullyQualified);
            var declaration = property.IsIndexer ? $"this[{RenderParameters(property.Parameters, NameableRefs(property.Parameters), false)}]" : property.Name;
            var access = property.IsIndexer
                ? $"(({ifaceName})_instance)[{RenderArguments(property.Parameters)}]"
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
        /// <summary>
        /// Interface members are always spelled out verbatim, so they need a matching set of trivially nameable refs.
        /// </summary>
        private static TypeRef[] NameableRefs(ImmutableArray<IParameterSymbol> parameters)
        {
            if (parameters.Length == 0)
            {
                return _noParameters;
            }
            var result = new TypeRef[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                result[i] = TypeRef.Nameable(null, true, false);
            }
            return result;
        }
        #endregion
    }
}
