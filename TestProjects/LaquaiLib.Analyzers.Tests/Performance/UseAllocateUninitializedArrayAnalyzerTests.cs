using LaquaiLib.Analyzers.Performance__0XXX_;

namespace LaquaiLib.Analyzers.Tests.Performance;

public class UseAllocateUninitializedArrayAnalyzerTests
{
    private static Task VerifyAnalyzer(string source)
        => new CSharpAnalyzerTest<UseAllocateUninitializedArrayAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = lf\n") } },
        }.RunAsync();

    private static Task VerifyNoDiagnostic(string source) => VerifyAnalyzer(source);

    #region well-known metadata struct sizes
    // Every entry of the table, pinned through the analyzer by its own boundary pair. Both directions break:
    // a larger entry lowers the threshold and trips the negative case, a smaller one raises it and trips the positive one.
    [Theory]
    [InlineData("System.Guid", 16)]
    [InlineData("System.DateTimeOffset", 16)]
    [InlineData("System.DateTime", 8)]
    [InlineData("System.TimeSpan", 8)]
    [InlineData("System.TimeOnly", 8)]
    [InlineData("System.Range", 8)]
    [InlineData("System.DateOnly", 4)]
    [InlineData("System.Index", 4)]
    [InlineData("System.Half", 2)]
    [InlineData("System.Numerics.Matrix4x4", 64)]
    [InlineData("System.Numerics.Matrix3x2", 24)]
    [InlineData("System.Numerics.Vector4", 16)]
    [InlineData("System.Numerics.Quaternion", 16)]
    [InlineData("System.Numerics.Plane", 16)]
    [InlineData("System.Numerics.Complex", 16)]
    [InlineData("System.Numerics.Vector3", 12)]
    [InlineData("System.Numerics.Vector2", 8)]
    [InlineData("System.Runtime.Intrinsics.Vector512<int>", 64)]
    [InlineData("System.Runtime.Intrinsics.Vector256<int>", 32)]
    [InlineData("System.Runtime.Intrinsics.Vector128<int>", 16)]
    [InlineData("System.Runtime.Intrinsics.Vector64<int>", 8)]
    public async Task WellKnownStructSizeIsPinned(string type, int size)
    {
        var threshold = 2048 / size;
        await VerifyAnalyzer(
            $$"""
            class C
            {
                {{type}}[] M() => {|LAQ0006:new|} {{type}}[{{threshold}}];
            }
            """
        );
        await VerifyNoDiagnostic(
            $$"""
            class C
            {
                {{type}}[] M() => new {{type}}[{{threshold - 1}}];
            }
            """
        );
    }
    #endregion

    #region offered
    // 2048 / 4
    [Fact]
    public Task IntAtThreshold()
        => VerifyAnalyzer(
            """
            class C
            {
                int[] M() => {|LAQ0006:new|} int[512];
            }
            """
        );

    // 2048 / 1
    [Fact]
    public Task ByteAtThreshold()
        => VerifyAnalyzer(
            """
            class C
            {
                byte[] M() => {|LAQ0006:new|} byte[2048];
            }
            """
        );

    // 2048 / 8
    [Fact]
    public Task LongAtThreshold()
        => VerifyAnalyzer(
            """
            class C
            {
                long[] M() => {|LAQ0006:new|} long[256];
            }
            """
        );

    [Fact]
    public Task EnumUsesItsUnderlyingType()
        => VerifyAnalyzer(
            """
            enum E : long { A }
            class C
            {
                E[] M() => {|LAQ0006:new|} E[256];
            }
            """
        );

    [Fact]
    public Task SourceStructSumsItsFields()
        => VerifyAnalyzer(
            """
            struct S { int a; int b; int c; int d; }
            class C
            {
                S[] M() => {|LAQ0006:new|} S[128];
            }
            """
        );

    [Fact]
    public Task ExplicitLayoutAtThreshold()
        => VerifyAnalyzer(
            """
            using System.Runtime.InteropServices;

            [StructLayout(LayoutKind.Explicit)]
            struct U
            {
                [FieldOffset(0)] int a;
                [FieldOffset(0)] float b;
                [FieldOffset(0)] uint c;
            }
            class C
            {
                U[] M() => {|LAQ0006:new|} U[512];
            }
            """
        );

    // 64 ints, so 2048 / 256
    [Fact]
    public Task InlineArrayScalesByItsLength()
        => VerifyAnalyzer(
            """
            using System.Runtime.CompilerServices;

            [InlineArray(64)]
            struct Buf64 { int _e0; }
            class C
            {
                Buf64[] M() => {|LAQ0006:new|} Buf64[8];
            }
            """
        );

    [Fact]
    public Task LengthMayBeAWiderIntegralConstant()
        => VerifyAnalyzer(
            """
            class C
            {
                int[] M() => {|LAQ0006:new|} int[512L];
            }
            """
        );

    // Past 2048 bytes per element the threshold divides down to 0, so any length clears it
    [Fact]
    public Task NonConstantLengthOnHugeElementType()
        => VerifyAnalyzer(
            """
            using System.Runtime.CompilerServices;

            [InlineArray(4096)]
            struct Big { byte _e0; }
            class C
            {
                Big[] M(int n) => {|LAQ0006:new|} Big[n];
            }
            """
        );

    // 4^15 nodes without memoization, all of them 4 bytes wide
    [Fact]
    public Task DeeplyNestedExplicitLayoutTerminates()
        => VerifyAnalyzer(
            """
            using System.Runtime.InteropServices;

            [StructLayout(LayoutKind.Explicit)] struct S0 { [FieldOffset(0)] int a; }
            [StructLayout(LayoutKind.Explicit)] struct S1 { [FieldOffset(0)] S0 a; [FieldOffset(0)] S0 b; [FieldOffset(0)] S0 c; [FieldOffset(0)] S0 d; }
            [StructLayout(LayoutKind.Explicit)] struct S2 { [FieldOffset(0)] S1 a; [FieldOffset(0)] S1 b; [FieldOffset(0)] S1 c; [FieldOffset(0)] S1 d; }
            [StructLayout(LayoutKind.Explicit)] struct S3 { [FieldOffset(0)] S2 a; [FieldOffset(0)] S2 b; [FieldOffset(0)] S2 c; [FieldOffset(0)] S2 d; }
            [StructLayout(LayoutKind.Explicit)] struct S4 { [FieldOffset(0)] S3 a; [FieldOffset(0)] S3 b; [FieldOffset(0)] S3 c; [FieldOffset(0)] S3 d; }
            [StructLayout(LayoutKind.Explicit)] struct S5 { [FieldOffset(0)] S4 a; [FieldOffset(0)] S4 b; [FieldOffset(0)] S4 c; [FieldOffset(0)] S4 d; }
            [StructLayout(LayoutKind.Explicit)] struct S6 { [FieldOffset(0)] S5 a; [FieldOffset(0)] S5 b; [FieldOffset(0)] S5 c; [FieldOffset(0)] S5 d; }
            [StructLayout(LayoutKind.Explicit)] struct S7 { [FieldOffset(0)] S6 a; [FieldOffset(0)] S6 b; [FieldOffset(0)] S6 c; [FieldOffset(0)] S6 d; }
            [StructLayout(LayoutKind.Explicit)] struct S8 { [FieldOffset(0)] S7 a; [FieldOffset(0)] S7 b; [FieldOffset(0)] S7 c; [FieldOffset(0)] S7 d; }
            [StructLayout(LayoutKind.Explicit)] struct S9 { [FieldOffset(0)] S8 a; [FieldOffset(0)] S8 b; [FieldOffset(0)] S8 c; [FieldOffset(0)] S8 d; }
            [StructLayout(LayoutKind.Explicit)] struct S10 { [FieldOffset(0)] S9 a; [FieldOffset(0)] S9 b; [FieldOffset(0)] S9 c; [FieldOffset(0)] S9 d; }
            [StructLayout(LayoutKind.Explicit)] struct S11 { [FieldOffset(0)] S10 a; [FieldOffset(0)] S10 b; [FieldOffset(0)] S10 c; [FieldOffset(0)] S10 d; }
            [StructLayout(LayoutKind.Explicit)] struct S12 { [FieldOffset(0)] S11 a; [FieldOffset(0)] S11 b; [FieldOffset(0)] S11 c; [FieldOffset(0)] S11 d; }
            [StructLayout(LayoutKind.Explicit)] struct S13 { [FieldOffset(0)] S12 a; [FieldOffset(0)] S12 b; [FieldOffset(0)] S12 c; [FieldOffset(0)] S12 d; }
            [StructLayout(LayoutKind.Explicit)] struct S14 { [FieldOffset(0)] S13 a; [FieldOffset(0)] S13 b; [FieldOffset(0)] S13 c; [FieldOffset(0)] S13 d; }
            [StructLayout(LayoutKind.Explicit)] struct S15 { [FieldOffset(0)] S14 a; [FieldOffset(0)] S14 b; [FieldOffset(0)] S14 c; [FieldOffset(0)] S14 d; }
            class C
            {
                S15[] M() => {|LAQ0006:new|} S15[512];
            }
            """
        );
    #endregion

    #region not offered
    [Fact]
    public Task IntBelowThreshold()
        => VerifyNoDiagnostic(
            """
            class C
            {
                int[] M() => new int[511];
            }
            """
        );

    [Fact]
    public Task ByteBelowThreshold()
        => VerifyNoDiagnostic(
            """
            class C
            {
                byte[] M() => new byte[2047];
            }
            """
        );

    [Fact]
    public Task LongBelowThreshold()
        => VerifyNoDiagnostic(
            """
            class C
            {
                long[] M() => new long[255];
            }
            """
        );

    // Half is 2 bytes, so the threshold is 1024; walking its reference assembly form yields 4 and would report here
    [Fact]
    public Task HalfIsNotFourBytes()
        => VerifyNoDiagnostic(
            """
            class C
            {
                System.Half[] M() => new System.Half[600];
            }
            """
        );

    // 8 bytes, so the threshold is 256; the reference assembly adds a fabricated int on top of key and value
    [Fact]
    public Task KeyValuePairIsNotFieldWalked()
        => VerifyNoDiagnostic(
            """
            class C
            {
                System.Collections.Generic.KeyValuePair<int, int>[] M() => new System.Collections.Generic.KeyValuePair<int, int>[200];
            }
            """
        );

    // bool? is 2 bytes, so the threshold is 1024
    [Fact]
    public Task NullableIsUnderlyingPlusAFlag()
        => VerifyNoDiagnostic(
            """
            class C
            {
                bool?[] M() => new bool?[500];
            }
            """
        );

    // The three fields overlap, so this is 4 bytes and not 12
    [Fact]
    public Task ExplicitLayoutBelowThreshold()
        => VerifyNoDiagnostic(
            """
            using System.Runtime.InteropServices;

            [StructLayout(LayoutKind.Explicit)]
            struct U
            {
                [FieldOffset(0)] int a;
                [FieldOffset(0)] float b;
                [FieldOffset(0)] uint c;
            }
            class C
            {
                U[] M() => new U[511];
            }
            """
        );

    [Fact]
    public Task InlineArrayBelowThreshold()
        => VerifyNoDiagnostic(
            """
            using System.Runtime.CompilerServices;

            [InlineArray(64)]
            struct Buf64 { int _e0; }
            class C
            {
                Buf64[] M() => new Buf64[7];
            }
            """
        );

    // Vector<T> is whatever the JIT picks per machine, so no bound can be established
    [Fact]
    public Task VariableWidthVectorNeverReports()
        => VerifyNoDiagnostic(
            """
            class C
            {
                System.Numerics.Vector<int>[] M() => new System.Numerics.Vector<int>[1024];
            }
            """
        );

    // GC.AllocateUninitializedArray hands these straight back to 'new T[length]'
    [Fact]
    public Task ReferenceElementTypeNeverReports()
        => VerifyNoDiagnostic(
            """
            class C
            {
                string[] M() => new string[5000];
            }
            """
        );

    [Fact]
    public Task ManagedStructElementTypeNeverReports()
        => VerifyNoDiagnostic(
            """
            struct S { object a; int b; }
            class C
            {
                S[] M() => new S[5000];
            }
            """
        );

    [Fact]
    public Task RefStructElementTypeNeverReports()
        => VerifyNoDiagnostic(
            """
            ref struct R { int x; }
            class C
            {
                void M() { var r = new {|CS0611:R|}[512]; }
            }
            """
        );

    [Fact]
    public Task JaggedArrayNeverReports()
        => VerifyNoDiagnostic(
            """
            class C
            {
                int[][] M() => new int[512][];
            }
            """
        );

    [Fact]
    public Task MultidimensionalArrayNeverReports()
        => VerifyNoDiagnostic(
            """
            class C
            {
                int[,] M() => new int[512, 512];
            }
            """
        );

    // GC.AllocateUninitializedArray only takes a length, so there is nothing to rewrite the initializer to
    [Fact]
    public Task InitializerNeverReports()
        => VerifyNoDiagnostic(
            """
            using System.Runtime.CompilerServices;

            [InlineArray(4096)]
            struct Big { byte _e0; }
            class C
            {
                Big[] M() => new Big[2] { default, default };
            }
            """
        );

    [Fact]
    public Task ZeroLengthNeverReports()
        => VerifyNoDiagnostic(
            """
            using System.Runtime.CompilerServices;

            [InlineArray(4096)]
            struct Big { byte _e0; }
            class C
            {
                Big[] M() => new Big[0];
            }
            """
        );

    [Fact]
    public Task NegativeLengthNeverReports()
        => VerifyNoDiagnostic(
            """
            using System.Runtime.CompilerServices;

            [InlineArray(4096)]
            struct Big { byte _e0; }
            class C
            {
                Big[] M() => new Big[{|CS0248:-1|}];
            }
            """
        );

    [Fact]
    public Task NonConstantLengthNeverReportsBelowTheThreshold()
        => VerifyNoDiagnostic(
            """
            class C
            {
                int[] M(int n) => new int[n];
            }
            """
        );
    #endregion
}
