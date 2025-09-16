using System.CodeDom.Compiler;
using System.Text;

using LaquaiLib.Analyzers.Shared;
using LaquaiLib.Generators.Extensions;

namespace LaquaiLib.Generators;

[Generator(LanguageNames.CSharp)]
public class FullAccessProxyGenerator : IIncrementalGenerator
{
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
                var namespaceName = proxyClassSymbol.ContainingNamespace.ToDisplayString();

                if (attribute is null)
                {
                    return;
                }

                INamedTypeSymbol proxiedType = null;
                // Get the type argument from the attribute (e.g., MemoryStream)
                var type = attribute.ConstructorArguments[0];
                if (type.Kind == TypedConstantKind.Primitive && type.Value is string fqTypeName)
                {
                    // Chances are high that the type won't be accessible, but we can try
                    proxiedType = compilation.GetTypeByMetadataName(fqTypeName);
                }
                else if (type.Kind == TypedConstantKind.Type && type.Value is Type typeToProxy)
                {
                    proxiedType = compilation.GetTypeByMetadataName(typeToProxy.FullName ?? typeToProxy.Name);
                }
                Debug.Assert(proxiedType is not null);

                // Generate the proxied members into the class
                var proxyClassSource = GenerateProxyForClass(namespaceName, proxyClassSymbol.Name, proxiedType, compilation);
                spc.AddSource($"{proxyClassSymbol.Name}_{proxiedType.Name}.g.cs", SourceText.From(proxyClassSource, Encoding.UTF8));
            }
        });
    }

    private static string GenerateProxyForClass(string namespaceName, string proxyClassName, INamedTypeSymbol proxiedType, Compilation compilation)
    {
        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        using var writer = new IndentedTextWriter(sw);

        writer.WriteLine(SourceEmitHelper.GeneratedFileHeader);
        writer.WriteLine($"namespace {namespaceName};");

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Wraps an instance of <c>{proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}</c> to provide full access to its members, regardless of visibility. Members with a result type (method return type or declared type for fields, events or properties) that is not public cannot be proxied.");
        writer.WriteLine($"/// <para/>Use the static <c>Create</c> methods to create instances using any of the instance constructors on that type. They also proxy non-public constructors.");
        writer.WriteLine("/// </summary>");

        writer.WriteLines(SourceEmitHelper.GeneratedCodeAttribute(typeof(FullAccessProxyGenerator)));

        writer.Write($"public sealed partial class {proxyClassName}");
        if (proxiedType.AllInterfaces.Length > 0)
        {
            writer.Write(" : ");
            writer.Write(string.Join(", ", proxiedType.AllInterfaces.Select(i => i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))));
        }
        writer.WriteLine();

        using (writer.Scope)
        {
            var members = proxiedType.GetMembers();

            WriteInstanceField(writer, proxiedType);
            WriteProxyCtor(writer, proxyClassName, proxiedType, compilation);
            WriteStaticCtorProxies(writer, proxyClassName, proxiedType, Unsafe.As<IMethodSymbol[]>(members.Where(m => m is IMethodSymbol { Name: ".ctor" }).ToArray()));
            WriteUnsafeAccessorUtility(writer, proxiedType, members);
            WriteInterfaceImplementations(writer, proxiedType);

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
                        case IMethodSymbol methodSymbol when methodSymbol.Name is not (".ctor" or ".cctor") && methodSymbol.ReturnType.DeclaredAccessibility is Accessibility.Public:
                            WriteMethodProxy(writer, methodSymbol, proxiedType);
                            break;
                        case IFieldSymbol fieldSymbol when fieldSymbol.Type.DeclaredAccessibility is Accessibility.Public:
                            WriteFieldProxy(writer, fieldSymbol, proxiedType);
                            break;
                        case IEventSymbol eventSymbol when eventSymbol.Type.DeclaredAccessibility is Accessibility.Public:
                            WriteEventProxy(writer, eventSymbol, proxiedType);
                            break;
                        default:
                            // Ignore other members like properties or indexers
                            break;
                    }
                }
            }
        }
        writer.Flush();
        sw.Flush();
        return sb.ToString();
    }

    private static void WriteInterfaceImplementations(IndentedTextWriter writer, INamedTypeSymbol proxiedType)
    {
        var interfaces = proxiedType.AllInterfaces;
        if (interfaces.IsDefaultOrEmpty)
        {
            return;
        }

        using var region = writer.Region("Interface implementations");

        for (var i = 0; i < interfaces.Length; i++)
        {
            var iface = interfaces[i];
            var members = iface.GetMembers();
            for (var k = 0; k < members.Length; k++)
            {
                var member = members[k];

                switch (member)
                {
                    case IMethodSymbol implementedMethodSymbol when
                        !implementedMethodSymbol.IsAccessor
                        && proxiedType.FindImplementationForInterfaceMember(member) is IMethodSymbol implementationMethodSymbol
                        && implementationMethodSymbol.ReturnType.DeclaredAccessibility is Accessibility.Public
                        && implementationMethodSymbol.ExplicitInterfaceImplementations.Length > 0:
                    {
                        WriteMethodProxyExplicit(writer, implementedMethodSymbol, implementationMethodSymbol, iface);
                        break;
                    }
                    case IEventSymbol implementedEventSymbol when
                        proxiedType.FindImplementationForInterfaceMember(member) is IEventSymbol implementationEventSymbol
                        && implementationEventSymbol.Type.DeclaredAccessibility is Accessibility.Public
                        && implementationEventSymbol.ExplicitInterfaceImplementations.Length > 0:
                    {
                        WriteEventProxyExplicit(writer, implementedEventSymbol, implementationEventSymbol, iface);
                        break;
                    }
                }
            }
        }
    }

    private static void WriteInstanceField(IndentedTextWriter writer, INamedTypeSymbol proxiedType)
    {
        using var region = writer.Region("Instance field");

        // We split this into a field-prop combo to avoid the overhead of the property getter for internal use
        writer.WriteLine($"private readonly {proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} _instance;");

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Gets the proxied instance of <c>{proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}</c>.");
        writer.WriteLine("/// </summary>");
        writer.WriteLine($"public {proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} Instance");
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
        writer.WriteLine($"/// Initializes a new instance of this proxy class for <c>{proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}</c>.");
        writer.WriteLine("/// </summary>");
        writer.WriteLine($"/// <param name=\"instance\">The instance of <c>{proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}</c> to proxy. Must not be <see langword=\"null\"/>, otherwise an exception is thrown.</param>");
        writer.WriteLine($"public {proxyClassName}({proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} instance)");
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
            writer.WriteLine($"/// Initializes a new instance of this proxy class for <c>{proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}</c> using the following instance constructor overload of that type:");
            writer.WriteLine($"/// <para/><c>{ctor.ToDisplayString().XmlEscape()}</c>");
            writer.WriteLine("/// </summary>");
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"public static {proxyClassName} Create({ctor.ParameterString})");
            using (writer.Scope)
            {
                writer.WriteLine($"return new {proxyClassName}(Accessors.ProxyCtor({ctor.ArgumentString}));");
            }
        }
    }
    private static void WriteUnsafeAccessorUtility(IndentedTextWriter writer, INamedTypeSymbol proxiedType, ImmutableArray<ISymbol> members)
    {
        using var region = writer.Region("Unsafe accessors utility");

        var proxiedTypeName = proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        writer.WriteLine("private static class Accessors");
        using (writer.Scope)
        {
            for (var i = 0; i < members.Length; i++)
            {
                // We can only proxy members which have a final type (return or declared type) that is public
                switch (members[i])
                {
                    // Methods (including ctors) and fields get [UnsafeAccessor]
                    case IMethodSymbol ctorSymbol when ctorSymbol.Name is ".ctor":
                    {
                        writer.WriteLine(SourceEmitHelper.UnsafeAccessor_Ctor);
                        writer.WriteLine($"public static extern {proxiedTypeName} ProxyCtor({ctorSymbol.ParameterString});");
                        break;
                    }
                    // Methods will also hit property and event accessors, whichever are declared
                    case IMethodSymbol methodSymbol when methodSymbol.Name is not ".cctor" && methodSymbol.ReturnType.DeclaredAccessibility is Accessibility.Public
                        && methodSymbol.ExplicitInterfaceImplementations.Length == 0:
                    {
                        writer.WriteLine(SourceEmitHelper.UnsafeAccessor_Method);
                        var parameterString = methodSymbol.ParameterString;
                        if (parameterString.Length > 0)
                        {
                            parameterString = ", " + parameterString;
                        }
                        writer.WriteLine($"public static extern {(methodSymbol.ReturnType.IsRefLikeType ? "ref " : "")}{methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {methodSymbol.Name}({proxiedTypeName} target{parameterString});");
                        break;
                    }
                    // Fields will also hit events
                    case IFieldSymbol fieldSymbol when fieldSymbol.Type.DeclaredAccessibility is Accessibility.Public:
                    {
                        writer.WriteLine(SourceEmitHelper.UnsafeAccessor_Field);
                        writer.WriteLine($"public static extern ref {fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {fieldSymbol.Name}({proxiedTypeName} target);");
                        break;
                    }

                    default:
                        break;
                        //throw new NotSupportedException($"Unsupported member type: {members[i].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");
                }
            }
        }
    }

    private static void WriteMethodProxy(IndentedTextWriter writer, IMethodSymbol methodSymbol, INamedTypeSymbol proxiedType)
    {
        // Ignore non-public return types since we can't proxy those
        if (methodSymbol.ReturnType.DeclaredAccessibility is not Accessibility.Public)
        {
            return;
        }
        if (methodSymbol.IsAccessor || methodSymbol.ExplicitInterfaceImplementations.Length > 0)
        {
            return;
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Proxies the following method from <c>{proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}</c>:");
        writer.WriteLine($"/// <para/><c>{methodSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).XmlEscape()}</c>");
        writer.WriteLine("/// </summary>");

        var parameterString = methodSymbol.ParameterString;
        var argumentString = methodSymbol.ArgumentString;
        if (argumentString.Length > 0)
        {
            argumentString = ", " + argumentString;
        }

        if (methodSymbol.IsStatic)
        {
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"public static {(methodSymbol.ReturnType.IsRefLikeType ? "ref " : "")}{methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {methodSymbol.Name}({parameterString}) => Accessors.{methodSymbol.Name}(null{argumentString});");
        }
        else
        {
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"public {(methodSymbol.ReturnType.IsRefLikeType ? "ref " : "")}{methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {methodSymbol.Name}({parameterString}) => Accessors.{methodSymbol.Name}(_instance{argumentString});");
        }
    }
    private static void WriteEventProxy(IndentedTextWriter writer, IEventSymbol eventSymbol, INamedTypeSymbol proxiedType)
    {
        if (eventSymbol.Type.DeclaredAccessibility is not Accessibility.Public)
        {
            return;
        }
        if (eventSymbol.ExplicitInterfaceImplementations.Length > 0)
        {
            return;
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Proxies the event <c>{proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{eventSymbol.Name}</c>.");
        writer.WriteLine("/// </summary>");

        if (eventSymbol.IsStatic)
        {
            writer.WriteLine($"public static event {eventSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {eventSymbol.Name}");
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
            writer.WriteLine($"public event {eventSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {eventSymbol.Name}");
            using (writer.Scope)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"add => Accessors.add_{eventSymbol.Name}(null, value);");
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"remove => Accessors.remove_{eventSymbol.Name}(null, value);");
            }
        }
    }
    private static void WriteFieldProxy(IndentedTextWriter writer, IFieldSymbol field, INamedTypeSymbol proxiedType)
    {
        if (field.Type.DeclaredAccessibility is not Accessibility.Public)
        {
            return;
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Proxies the field <c>{proxiedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{field.Name}</c>.");
        writer.WriteLine("/// </summary>");

        // Fields will be proxied with a property of the same name

        if (field.IsStatic)
        {
            writer.WriteLine($"public static {field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {field.Name}");
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
            writer.WriteLine($"public {field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {field.Name}");
            using (writer.Scope)
            {
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"get => Accessors.{field.Name}(_instance);");
                writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
                writer.WriteLine($"set => Accessors.{field.Name}(_instance) = value;");
            }
        }
    }

    private static void WriteMethodProxyExplicit(IndentedTextWriter writer, IMethodSymbol implementedMethodSymbol, IMethodSymbol implementationMethodSymbol, INamedTypeSymbol fromInterface)
    {
        var parameterString = implementationMethodSymbol.ParameterString;
        var argumentString = implementationMethodSymbol.ArgumentString;

        // Explict interface implementations won't use Accessors since we'll need the interface cast anyway and the method will be public
        writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
        writer.WriteLine($"{(implementationMethodSymbol.ReturnType.IsRefLikeType ? "ref " : "")}{implementationMethodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {fromInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{implementedMethodSymbol.Name}({parameterString}) => (({fromInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})_instance).{implementedMethodSymbol.Name}({argumentString});");
    }
    private static void WriteEventProxyExplicit(IndentedTextWriter writer, IEventSymbol implementedEventSymbol, IEventSymbol implementationEventSymbol, INamedTypeSymbol fromInterface)
    {
        writer.WriteLine($"event {implementationEventSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {fromInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{implementedEventSymbol.Name}");

        // Explict interface implementations won't use Accessors since we'll need the interface cast anyway and the event will be public
        using (writer.Scope)
        {
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"add => (({fromInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})_instance).{implementedEventSymbol.Name} += value;");
            writer.WriteLine(SourceEmitHelper.MethodImpl_AggressiveInlining);
            writer.WriteLine($"remove => (({fromInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})_instance).{implementedEventSymbol.Name} -= value;");
        }
    }
}
