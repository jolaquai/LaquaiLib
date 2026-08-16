namespace LaquaiLib.Generators.Tests;

public sealed class MetadataTypeNameTests
{
    private static CSharpCompilation Compile(params string[] sources) => GeneratorTestHost.CreateCompilation(sources);

    private static ITypeSymbol GetType(CSharpCompilation compilation, string metadataName)
        => compilation.GetTypeByMetadataName(metadataName);

    private static ITypeSymbol GetFieldType(CSharpCompilation compilation, string typeName, string fieldName)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        var field = type.GetMembers(fieldName).OfType<IFieldSymbol>().First();
        return field.Type;
    }

    private static ITypeSymbol GetMethodTypeParameter(CSharpCompilation compilation, string typeName, string methodName, int index)
    {
        var type = compilation.GetTypeByMetadataName(typeName);
        var method = type.GetMembers(methodName).OfType<IMethodSymbol>().First();
        return method.TypeParameters[index];
    }

    [Fact]
    public void SimpleTypeSameAssemblyHasNoQualifier()
    {
        var compilation = Compile("class C { }");
        var type = GetType(compilation, "C");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("C", result);
    }

    [Fact]
    public void SimpleTypeReferencedAssemblyHasQualifierWithSimpleName()
    {
        var compilation = Compile("class C { public System.Text.StringBuilder Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var fakeCompilationAssembly = compilation.GetTypeByMetadataName("C").ContainingAssembly;
        var result = MetadataTypeName.TryBuild(type, fakeCompilationAssembly);
        Assert.Equal("System.Text.StringBuilder, System.Private.CoreLib", result);
        Assert.DoesNotContain("Version=", result);
    }

    [Fact]
    public void SpecialTypeIntIsUnqualified()
    {
        var compilation = Compile("class C { }");
        var type = compilation.GetSpecialType(SpecialType.System_Int32);
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("System.Int32", result);
    }

    [Fact]
    public void SpecialTypeStringIsUnqualified()
    {
        var compilation = Compile("class C { }");
        var type = compilation.GetSpecialType(SpecialType.System_String);
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("System.String", result);
    }

    [Fact]
    public void NestedTypeRendersWithPlus()
    {
        var compilation = Compile("namespace Ns { class Outer { public class Inner { } } }");
        var type = GetType(compilation, "Ns.Outer+Inner");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Ns.Outer+Inner", result);
    }

    [Fact]
    public void PrivateNestedTypeStillRenders()
    {
        var compilation = Compile("namespace Ns { class Outer { private class Inner { } } }");
        var type = GetType(compilation, "Ns.Outer+Inner");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Ns.Outer+Inner", result);
    }

    [Fact]
    public void TwoLevelNestingRendersAllSegments()
    {
        var compilation = Compile("namespace Ns { class A { public class B { public class C { } } } }");
        var type = GetType(compilation, "Ns.A+B+C");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Ns.A+B+C", result);
    }

    [Fact]
    public void GenericTypeRendersMetadataArityAndArguments()
    {
        var compilation = Compile("class C { public System.Collections.Generic.List<int> Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var listAssembly = ((INamedTypeSymbol)type).ContainingAssembly;
        var result = MetadataTypeName.TryBuild(type, listAssembly);
        Assert.Equal("System.Collections.Generic.List`1[[System.Int32]]", result);
    }

    [Fact]
    public void GenericWithReferencedAssemblyArgumentCarriesItsOwnQualifier()
    {
        var compilation = Compile("class Box<T> { } class C { public Box<System.Text.StringBuilder> Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Box`1[[System.Text.StringBuilder, System.Private.CoreLib]]", result);
    }

    [Fact]
    public void NestedGenericArgumentsAreOutermostFirst()
    {
        var compilation = Compile("""
            namespace Ns
            {
                class Outer<TA>
                {
                    public class Inner<TB>
                    {
                    }
                }
                class A { }
                class B { }
                class C { public Outer<A>.Inner<B> Field; }
            }
            """);
        var type = GetFieldType(compilation, "Ns.C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Ns.Outer`1+Inner`1[[Ns.A],[Ns.B]]", result);
    }

    [Fact]
    public void OpenClassTypeParameterFirstIsBang0()
    {
        var compilation = Compile("class C<T1, T2> { public T1 Field1; public T2 Field2; }");
        var type = GetFieldType(compilation, "C`2", "Field1");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("!0", result);
    }

    [Fact]
    public void OpenClassTypeParameterSecondIsBang1()
    {
        var compilation = Compile("class C<T1, T2> { public T1 Field1; public T2 Field2; }");
        var type = GetFieldType(compilation, "C`2", "Field2");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("!1", result);
    }

    [Fact]
    public void MethodTypeParameterIsDoubleBang0()
    {
        var compilation = Compile("class C { public void M<T>() { } }");
        var type = GetMethodTypeParameter(compilation, "C", "M", 0);
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("!!0", result);
    }

    [Fact]
    public void SzArrayRendersBrackets()
    {
        var compilation = Compile("class Foo { } class C { public Foo[] Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Foo[]", result);
    }

    [Fact]
    public void TwoDimensionalArrayRendersComma()
    {
        var compilation = Compile("class Foo { } class C { public Foo[,] Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Foo[,]", result);
    }

    [Fact]
    public void ThreeDimensionalArrayRendersTwoCommas()
    {
        var compilation = Compile("class Foo { } class C { public Foo[,,] Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Foo[,,]", result);
    }

    [Fact]
    public void JaggedArrayRendersDoubleBrackets()
    {
        var compilation = Compile("class Foo { } class C { public Foo[][] Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Foo[][]", result);
    }

    [Fact]
    public void MixedRankAndJaggedArrayOrderingMatchesReflection()
    {
        var compilation = Compile("class C { public int[,][] Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("System.Int32[][,]", result);
    }

    [Fact]
    public void PointerRendersStar()
    {
        var compilation = Compile("unsafe class Foo { } unsafe class C { public Foo* Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Foo*", result);
    }

    [Fact]
    public void PointerToPointerRendersDoubleStar()
    {
        var compilation = Compile("unsafe class Foo { } unsafe class C { public Foo** Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("Foo**", result);
    }

    [Fact]
    public void NullableReferenceTypeStripsAnnotation()
    {
        var compilation = Compile("#nullable enable\nclass C { public string? Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Equal("System.String", result);
    }

    [Fact]
    public void NullableValueTypeRendersNullableOfT()
    {
        var compilation = Compile("class C { public int? Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var nullableAssembly = ((INamedTypeSymbol)type).ContainingAssembly;
        var result = MetadataTypeName.TryBuild(type, nullableAssembly);
        Assert.Equal("System.Nullable`1[[System.Int32]]", result);
    }

    [Fact]
    public void FunctionPointerIsUnrepresentable()
    {
        var compilation = Compile("unsafe class C { public delegate*<int, void> Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Null(result);
    }

    [Fact]
    public void ErrorTypeIsUnrepresentable()
    {
        var compilation = Compile("class C { public UndefinedType Field; }");
        var type = GetFieldType(compilation, "C", "Field");
        Assert.Equal(TypeKind.Error, type.TypeKind);
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.Null(result);
    }

    [Fact]
    public void GlobalNamespaceTypeHasNoLeadingDot()
    {
        var compilation = Compile("class C { }");
        var type = GetType(compilation, "C");
        var result = MetadataTypeName.TryBuild(type, compilation.Assembly);
        Assert.False(result.StartsWith(".", StringComparison.Ordinal));
    }

    [Fact]
    public void NullInputReturnsNull()
    {
        var compilation = Compile("class C { }");
        var result = MetadataTypeName.TryBuild(null, compilation.Assembly);
        Assert.Null(result);
    }
}
