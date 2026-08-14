namespace LaquaiLib.Generators.Tests;

public class EnumExpanderGeneratorEmissionTests
{
    [Fact]
    public void SimplePublicEnumEmitsDataClassWithAllArrays()
    {
        var result = GeneratorTestHost.RunGenerator(new EnumExpanderGenerator(),
            """
            namespace TestNs
            {
                public enum Color
                {
                    Red,
                    Green,
                    Blue
                }
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "EnumExpanderGenerator");

        Assert.Contains("namespace TestNs", source);
        Assert.Contains("public static class ColorData", source);
        Assert.Contains("public static string[] Names { get; } = [\"Red\", \"Green\", \"Blue\"];", source);
        Assert.Contains("public static global::TestNs.Color[] Values { get; } = [", source);
        Assert.Contains("global::TestNs.Color.Red,", source);
        Assert.Contains("global::TestNs.Color.Green,", source);
        Assert.Contains("global::TestNs.Color.Blue,", source);
        Assert.Contains("public static int[] UnderlyingValues { get; } = [0, 1, 2];", source);
        Assert.Contains("public static global::GeneratorTestAssembly.EnumFieldData<global::TestNs.Color, int>[] Data { get; } = [", source);
        Assert.Contains("new(\"Red\", global::TestNs.Color.Red, 0, null, 0),", source);
        Assert.Contains("new(\"Green\", global::TestNs.Color.Green, 1, null, 1),", source);
        Assert.Contains("new(\"Blue\", global::TestNs.Color.Blue, 2, null, 2),", source);
    }

    [Fact]
    public void FlagsEnumWithExplicitPowerOfTwoValuesIsExpandedCorrectly()
    {
        var result = GeneratorTestHost.RunGenerator(new EnumExpanderGenerator(),
            """
            namespace TestNs
            {
                [Flags]
                public enum Permissions
                {
                    None = 0,
                    Read = 1,
                    Write = 2,
                    Execute = 4
                }
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "EnumExpanderGenerator");

        Assert.Contains("public static string[] Names { get; } = [\"None\", \"Read\", \"Write\", \"Execute\"];", source);
        Assert.Contains("public static int[] UnderlyingValues { get; } = [0, 1, 2, 4];", source);
        Assert.Contains("new(\"None\", global::TestNs.Permissions.None, 0, null, 0),", source);
        Assert.Contains("new(\"Read\", global::TestNs.Permissions.Read, 1, null, 1),", source);
        Assert.Contains("new(\"Write\", global::TestNs.Permissions.Write, 2, null, 2),", source);
        Assert.Contains("new(\"Execute\", global::TestNs.Permissions.Execute, 4, null, 3),", source);
    }

    [Fact]
    public void EnumNestedInsideClassUsesFullyQualifiedContainingTypePath()
    {
        var result = GeneratorTestHost.RunGenerator(new EnumExpanderGenerator(),
            """
            namespace TestNs
            {
                public class Container
                {
                    public enum Nested
                    {
                        A,
                        B
                    }
                }
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "EnumExpanderGenerator");

        Assert.Contains("namespace TestNs", source);
        Assert.Contains("public static class NestedData", source);
        Assert.Contains("public static global::TestNs.Container.Nested[] Values { get; } = [", source);
        Assert.Contains("global::TestNs.Container.Nested.A,", source);
        Assert.Contains("global::TestNs.Container.Nested.B,", source);
        Assert.Contains("new(\"A\", global::TestNs.Container.Nested.A, 0, null, 0),", source);
        Assert.Contains("new(\"B\", global::TestNs.Container.Nested.B, 1, null, 1),", source);
    }

    [Fact]
    public void NonIntUnderlyingTypesAreTypedCorrectlyInUnderlyingValues()
    {
        var result = GeneratorTestHost.RunGenerator(new EnumExpanderGenerator(),
            """
            namespace TestNs
            {
                public enum ByteEnum : byte
                {
                    A = 1,
                    B = 2
                }
                public enum LongEnum : long
                {
                    A = 1,
                    B = 2
                }
                public enum ULongEnum : ulong
                {
                    A = 1,
                    B = 2
                }
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "EnumExpanderGenerator");

        Assert.Contains("public static byte[] UnderlyingValues { get; } = [1, 2];", source);
        Assert.Contains("public static global::GeneratorTestAssembly.EnumFieldData<global::TestNs.ByteEnum, byte>[] Data { get; } = [", source);

        Assert.Contains("public static long[] UnderlyingValues { get; } = [1, 2];", source);
        Assert.Contains("public static global::GeneratorTestAssembly.EnumFieldData<global::TestNs.LongEnum, long>[] Data { get; } = [", source);

        Assert.Contains("public static ulong[] UnderlyingValues { get; } = [1, 2];", source);
        Assert.Contains("public static global::GeneratorTestAssembly.EnumFieldData<global::TestNs.ULongEnum, ulong>[] Data { get; } = [", source);
    }

    [Fact]
    public void EmptyEnumIsSkippedEntirely()
    {
        var result = GeneratorTestHost.RunGenerator(new EnumExpanderGenerator(),
            """
            namespace TestNs
            {
                public enum EmptyEnum
                {
                }
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "EnumExpanderGenerator");

        // No fields => the generator's early "continue" skips this enum without emitting a Data class,
        // the enclosing region, or (since it's the only enum) even the shared EnumFieldData record struct.
        Assert.DoesNotContain("EmptyEnumData", source);
        Assert.DoesNotContain("EnumFieldData", source);
        Assert.True(string.IsNullOrWhiteSpace(source), $"Expected no emitted source for an enum with zero members, got:{Environment.NewLine}{source}");
    }

    [Fact]
    public void DuplicateMemberValuesBothAppearWithDistinctDeclaredPositions()
    {
        var result = GeneratorTestHost.RunGenerator(new EnumExpanderGenerator(),
            """
            namespace TestNs
            {
                public enum DupEnum
                {
                    First = 1,
                    Second = 1
                }
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "EnumExpanderGenerator");

        Assert.Contains("public static string[] Names { get; } = [\"First\", \"Second\"];", source);
        Assert.Contains("new(\"First\", global::TestNs.DupEnum.First, 1, null, 0),", source);
        Assert.Contains("new(\"Second\", global::TestNs.DupEnum.Second, 1, null, 1),", source);
    }

    [Fact]
    public void EnumInGlobalNamespaceIsHandled()
    {
        var result = GeneratorTestHost.RunGenerator(new EnumExpanderGenerator(),
            """
            public enum GlobalEnum
            {
                X,
                Y
            }
            """
        );

        // Regression guard: symbol.ContainingNamespace.ToDisplayString() for the global namespace yields "",
        // which would emit the syntactically-invalid "namespace" (no name) unless the generator special-cases it.
        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "EnumExpanderGenerator");
        Assert.Contains("public static class GlobalEnumData", source);
        Assert.Contains("new(\"X\", global::GlobalEnum.X, 0, null, 0),", source);
        Assert.Contains("new(\"Y\", global::GlobalEnum.Y, 1, null, 1),", source);
    }

    [Fact]
    public void DescriptionAttributeFlowsIntoDataDescriptionSlotAndAbsentIsNull()
    {
        var result = GeneratorTestHost.RunGenerator(new EnumExpanderGenerator(),
            """
            namespace TestNs
            {
                public enum DescEnum
                {
                    [System.ComponentModel.Description("First value")]
                    A,
                    B
                }
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "EnumExpanderGenerator");

        Assert.Contains("new(\"A\", global::TestNs.DescEnum.A, 0, \"First value\", 0),", source);
        Assert.Contains("new(\"B\", global::TestNs.DescEnum.B, 1, null, 1),", source);
    }

    [Fact]
    public void InternalEnumEmitsInternalDataClass()
    {
        var result = GeneratorTestHost.RunGenerator(new EnumExpanderGenerator(),
            """
            namespace TestNs
            {
                internal enum InternalEnum
                {
                    A,
                    B
                }
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "EnumExpanderGenerator");

        Assert.Contains("internal static class InternalEnumData", source);
        Assert.DoesNotContain("public static class InternalEnumData", source);
    }

    [Fact]
    public void MultipleEnumsShareASingleEnumFieldDataRecordStruct()
    {
        var result = GeneratorTestHost.RunGenerator(new EnumExpanderGenerator(),
            """
            namespace TestNs
            {
                public enum FirstEnum
                {
                    A,
                    B
                }
                public enum SecondEnum
                {
                    C,
                    D
                }
                public enum ThirdEnum
                {
                    E,
                    F
                }
            }
            """
        );

        GeneratorTestHost.AssertNoCompilationErrors(result);
        var source = GeneratorTestHost.GetGeneratedSource(result, "EnumExpanderGenerator");

        var recordStructDecls = new Regex(@"record struct EnumFieldData<TEnum, TUnderlying>").Matches(source);
        Assert.Single(recordStructDecls);

        Assert.Contains("public static class FirstEnumData", source);
        Assert.Contains("public static class SecondEnumData", source);
        Assert.Contains("public static class ThirdEnumData", source);
    }
}
