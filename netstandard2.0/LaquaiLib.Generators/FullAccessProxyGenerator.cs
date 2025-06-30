using System.CodeDom.Compiler;
using System.Collections.Frozen;
using System.Reflection;
using System.Text;

using LaquaiLib.Analyzers.Shared;
using LaquaiLib.Analyzers.Shared.Attributes;

namespace LaquaiLib.Generators;

[Generator(LanguageNames.CSharp)]
public class FullAccessProxyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarationSyntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
            static (context, _) =>
            {
                var classDeclarationSyntax = Unsafe.As<ClassDeclarationSyntax>(context.Node);
                var attribute = classDeclarationSyntax.AttributeLists.SelectMany(static x => x.Attributes)
                    .FirstOrDefault(static x => x.Name.ToString().Contains("FullAccessProxy"));
                if (attribute is not null)
                {
                    return classDeclarationSyntax;
                }
                return null;
            }
        ).Where(static m => m is not null);

        var compilationProvider = context.CompilationProvider;
        var combined = compilationProvider.Combine(classDeclarationSyntaxProvider.Collect());

        context.RegisterSourceOutput(combined, static (spc, source) =>
        {
            var (compilation, classDeclarations) = source;
            for (var i = 0; i < classDeclarations.Length; i++)
            {
                var decl = classDeclarations[i];
                if (decl is null) continue;
                var semanticModel = compilation.GetSemanticModel(decl.SyntaxTree);
                if (semanticModel is null) continue;

                // Get the class symbol
                var proxyClassSymbol = semanticModel.GetDeclaredSymbol(decl);
                if (proxyClassSymbol is null) return;
                var proxyClassName = proxyClassSymbol.Name;
                var namespaceName = proxyClassSymbol.ContainingNamespace.ToDisplayString();

                // Find the FullAccessProxy attribute and its type argument
                var attribute = proxyClassSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString().Contains("FullAccessProxy") == true);
                if (attribute is null) return;
                var attrClass = attribute.AttributeClass;

                // Get the type argument from the attribute (e.g., MemoryStream)
                var typeArg = attrClass.TypeArguments[0];
                var targetTypeString = typeArg.ToDisplayString();
                var proxiedType = Type.GetType(targetTypeString);

                // Attribute data
                var fullAccessProxyAttributeType = typeof(FullAccessProxyAttribute<>).MakeGenericType([proxiedType]);
                var attributeInstance = Unsafe.As<FullAccessProxyAttribute<object>>(Activator.CreateInstance(fullAccessProxyAttributeType));

                var properties = fullAccessProxyAttributeType.GetProperties().ToDictionary(p => p.Name);

                var bindingFlagsProp = properties[nameof(attributeInstance.BindingFlags)];
                var bindingFlagsPropNamedValue = attribute.NamedArguments.FirstOrDefault(kv => kv.Key == nameof(BindingFlags));
                if (bindingFlagsPropNamedValue.Value.Value is BindingFlags bf)
                {
                    attributeInstance.BindingFlags = bf;
                }

                var includeHierarchyProp = properties[nameof(attributeInstance.IncludeHierarchy)];
                var includeHierarchyPropNamedValue = attribute.NamedArguments.FirstOrDefault(kv => kv.Key == nameof(attributeInstance.IncludeHierarchy));
                if (includeHierarchyPropNamedValue.Value.Value is bool b)
                {
                    attributeInstance.IncludeHierarchy = b;
                }

                // Generate the proxied members into the class
                var proxyClassSource = GenerateProxyClass(namespaceName, proxyClassName, proxiedType, attributeInstance);

                spc.AddSource($"{proxyClassName}_{proxiedType.Name}.g.cs", SourceText.From(proxyClassSource, Encoding.UTF8));
            }
        });
    }

    private static string GenerateProxyClass(string namespaceName, string proxyClassName, Type proxiedType, FullAccessProxyAttribute<object> data)
    {
        var sb = new StringBuilder();
        using var sw = new StringWriter(sb);
        using var writer = new IndentedTextWriter(sw);

        writer.WriteLine(Constants.GeneratedFileHeader);
        writer.WriteLine($"namespace {namespaceName};");

        var interfaces = new HashSet<string>(proxiedType.GetInterfaces()
            .Select(static i => i.GetFriendlyName())
            .Where(static i => i is "System.IDisposable" or "System.IAsyncDisposable")
        );

        writer.Write($"public partial class {proxyClassName}");
        if (interfaces.Count > 0)
        {
            writer.Write(" : ");
            writer.WriteLine(string.Join(", ", interfaces));
        }
        else
        {
            writer.WriteLine();
        }
        writer.WriteLine("{");
        writer.Indent++;
        {
            var flags = data.BindingFlags;
            if (data.IncludeHierarchy)
            {
                flags |= BindingFlags.FlattenHierarchy;
            }

            var members = proxiedType.GetMembers(flags);

            WriteInstanceFields(writer, proxiedType);
            WriteProxyCtor(writer, proxyClassName, proxiedType);
            WriteStaticCtorProxies(writer, proxyClassName, proxiedType, Unsafe.As<ConstructorInfo[]>(Array.FindAll(members, m => m is ConstructorInfo)));
            WriteUnsafeAccessorUtility(writer, proxiedType, members);
            WriteInterfaceImpls(writer, interfaces);

            for (var i = 0; i < members.Length; i++)
            {
                // Skip if the member is not accessible or not a method/property/field/event
                switch (members[i])
                {
                    case MethodInfo method:
                        GenerateMethodProxy(writer, method, proxiedType);
                        break;
                    case FieldInfo field:
                        GenerateFieldProxy(writer, field, proxiedType);
                        break;
                }
            }

            writer.Indent--;
        }
        writer.WriteLine("}");
        writer.Flush();
        sw.Flush();
        return sb.ToString();
    }

    private static void WriteInterfaceImpls(IndentedTextWriter writer, HashSet<string> interfaces)
    {
        if (interfaces.Contains("System.IDisposable"))
        {
            writer.WriteLine("/// <inheritdoc/>");
            writer.WriteLine(Constants.MethodImpl_AggressiveInlining);
            writer.WriteLine("public void Dispose()");
            writer.WriteLine("{");
            writer.Indent++;
            {
                writer.WriteLine("if (_instance is IDisposable disposable)");
                writer.WriteLine("{");
                writer.Indent++;
                {
                    writer.WriteLine("disposable.Dispose();");
                }
                writer.Indent--;
                writer.WriteLine("}");
            }
            writer.Indent--;
            writer.WriteLine("}");
        }

        if (interfaces.Contains("System.IAsyncDisposable"))
        {
            writer.WriteLine("/// <inheritdoc/>");
            writer.WriteLine(Constants.MethodImpl_AggressiveInlining);
            writer.WriteLine("public async ValueTask DisposeAsync()");
            writer.WriteLine("{");
            writer.Indent++;
            {
                writer.WriteLine("if (_instance is IAsyncDisposable asyncDisposable)");
                writer.WriteLine("{");
                writer.Indent++;
                {
                    writer.WriteLine("await asyncDisposable.DisposeAsync().ConfigureAwait(false);");
                }
                writer.Indent--;
                writer.WriteLine("}");
            }
            writer.Indent--;
            writer.WriteLine("}");
        }
    }
    private static void WriteInstanceFields(IndentedTextWriter writer, Type proxiedType)
    {
        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// The proxied instance of <c>{proxiedType.FullName}</c>.");
        writer.WriteLine("/// </summary>");
        writer.WriteLine($"private readonly {proxiedType.FullName} _instance;");
        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Gets the proxied instance of <c>{proxiedType.FullName}</c>.");
        writer.WriteLine("/// </summary>");
        writer.WriteLine($"public {proxiedType.FullName} Instance => _instance;");
    }
    private static void WriteProxyCtor(IndentedTextWriter writer, string proxyClassName, Type proxiedType)
    {
        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Initializes a new instance of this proxy class for <c>{proxiedType.GetFriendlyName()}</c>.");
        writer.WriteLine("/// </summary>");
        writer.WriteLine($"/// <param name=\"instance\">The instance of <c>{proxiedType.FullName}</c> to proxy. Must not be <see langword=\"null\"/>, otherwise an exception is thrown.</param>");
        writer.WriteLine($"public {proxyClassName}({proxiedType.GetFriendlyName()} instance)");
        writer.WriteLine("{");
        writer.Indent++;
        {
            writer.WriteLine("_instance = instance ?? throw new ArgumentNullException(nameof(instance));");
        }
        writer.Indent--;
        writer.WriteLine("}");
    }
    private static void WriteStaticCtorProxies(IndentedTextWriter writer, string proxyClassName, Type proxiedType, ConstructorInfo[] ctors)
    {
        for (var i = 0; i < ctors.Length; i++)
        {
            var ctor = ctors[i];

            writer.WriteLine("/// <summary>");
            writer.WriteLine($"/// Initializes a new instance of this proxy class for <c>{proxiedType.GetFriendlyName()}</c> using the following instance constructor overload of that type:");
            writer.WriteLine($"/// <para/><c>{ctor.Signature}</c>");
            writer.WriteLine("/// </summary>");
            writer.WriteLine(Constants.MethodImpl_AggressiveInlining);
            writer.WriteLine($"public static {proxyClassName} FromProxiedConstructor({ctor.ParameterString})");
            writer.WriteLine("{");
            writer.Indent++;
            {
                writer.WriteLine($"return new {proxyClassName}(Accessors.ProxyCtor({ctor.ArgumentString}));");
            }
            writer.Indent--;
            writer.WriteLine("}");
        }
    }
    private static void WriteUnsafeAccessorUtility(IndentedTextWriter writer, Type proxiedType, MemberInfo[] members)
    {
        var proxiedTypeName = proxiedType.FullName;

        writer.WriteLine("private static class Accessors");
        writer.WriteLine("{");
        writer.Indent++;
        {
            for (var i = 0; i < members.Length; i++)
            {
                // We can only proxy members which have a final type (return or declared type) that is public
                switch (members[i])
                {
                    // Methods and fields get [UnsafeAccessor]
                    case MethodInfo method:
                        if (method.ReturnType.IsNotPublic)
                        {
                            continue;
                        }

                        writer.WriteLine(Constants.UnsafeAccessor_Method);
                        var parameterString = method.ParameterString;
                        if (parameterString.Length > 0)
                        {
                            parameterString = ", " + parameterString;
                        }
                        writer.WriteLine($"public static extern {(method.ReturnType.IsByRef ? "ref " : "")}{method.ReturnType.GetFriendlyName()} {method.Name}({proxiedTypeName} target{parameterString});");
                        break;
                    case FieldInfo field:
                        if (field.FieldType.IsNotPublic)
                        {
                            continue;
                        }

                        writer.WriteLine(Constants.UnsafeAccessor_Field);
                        writer.WriteLine($"public static extern ref {field.FieldType.GetFriendlyName()} {field.Name}({proxiedTypeName} target);");
                        break;

                    case ConstructorInfo ctor:
                        writer.WriteLine(Constants.UnsafeAccessor_Ctor);
                        var ctorParameterString = ctor.ParameterString;
                        writer.WriteLine($"public static extern {proxiedTypeName} ProxyCtor({ctorParameterString});");
                        break;

                    case PropertyInfo: // Ignore properties since their accessors will all end up in the MethodInfo case
                    case EventInfo: // Ignore events for now
                        // TODO: Think about how we do this
                        // Since events are basically just properties of a Delegate-derived type with add_/remove_ methods, the Accessors class will just provide those, which the parent class can then wrap in event syntax
                        break;
                    default:
                        break;
                        throw new NotSupportedException($"Unsupported member type: {members[i].GetType().Name}");
                }
            }
        }
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static HashSet<string> _ignoreMethods = ["object.Equals", "object.GetHashCode", "object.GetType", "object.ToString", "System.IDisposable.Dispose", "System.IAsyncDisposable.DisposeAsync"];
    private static void GenerateMethodProxy(IndentedTextWriter writer, MethodInfo method, Type proxiedType)
    {
        if (method.ReturnType.IsNotPublic)
        {
            return;
        }

        if (_ignoreMethods.Contains($"{method.DeclaringType.GetFriendlyName()}.{method.Name}"))
        {
            return;
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Proxies the following method from <c>{proxiedType.GetFriendlyName()}</c>:");
        writer.WriteLine($"/// <para/><c>{method.Signature}</c>");
        writer.WriteLine("/// </summary>");

        var parameterString = method.ParameterString;
        var argumentString = method.ArgumentString;
        if (argumentString.Length > 0)
        {
            argumentString = ", " + argumentString;
        }

        if (method.IsStatic)
        {
            writer.WriteLine(Constants.MethodImpl_AggressiveInlining);
            writer.WriteLine($"public {(method.ReturnType.IsByRef ? "ref " : "")}{method.ReturnType.GetFriendlyName()} {method.Name}({parameterString}) => Accessors.{method.Name}(null{argumentString});");
        }
        else
        {
            writer.WriteLine(Constants.MethodImpl_AggressiveInlining);
            writer.WriteLine($"public {(method.ReturnType.IsByRef ? "ref " : "")}{method.ReturnType.GetFriendlyName()} {method.Name}({parameterString}) => Accessors.{method.Name}(_instance{argumentString});");
        }
    }
    private static void GenerateFieldProxy(IndentedTextWriter writer, FieldInfo field, Type proxiedType)
    {
        if (field.FieldType.IsNotPublic)
        {
            return;
        }

        writer.WriteLine("/// <summary>");
        writer.WriteLine($"/// Proxies the field <c>{proxiedType.GetFriendlyName()}.{field.Name}</c>.");
        writer.WriteLine("/// </summary>");

        // Fields will be proxied with a property of the same name

        writer.WriteLine($"public {field.FieldType.GetFriendlyName()} {field.Name}");
        writer.WriteLine("{");
        writer.Indent++;
        {
            if (field.IsStatic)
            {
                writer.WriteLine(Constants.MethodImpl_AggressiveInlining);
                writer.WriteLine($"get => Accessors.{field.Name}(null);");
                writer.WriteLine(Constants.MethodImpl_AggressiveInlining);
                writer.WriteLine($"set => Accessors.{field.Name}(null) = value;");
            }
            else
            {
                writer.WriteLine(Constants.MethodImpl_AggressiveInlining);
                writer.WriteLine($"get => Accessors.{field.Name}(_instance);");
                writer.WriteLine(Constants.MethodImpl_AggressiveInlining);
                writer.WriteLine($"set => Accessors.{field.Name}(_instance) = value;");
            }
        }
        writer.Indent--;
        writer.WriteLine("}");
    }
}
