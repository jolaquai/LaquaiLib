using LaquaiLib.Analyzers.Validity__9XXX_;

namespace LaquaiLib.Analyzers.Tests.Validity;

public class UnsafeAccessorValidatorsTests
{
    private static Task VerifyAnalyzer(string source)
        => new CSharpAnalyzerTest<UnsafeAccessorValidators, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            // UnsafeAccessorValidators registers 3 distinct descriptors (member/method/ctor) all under id LAQ9001;
            // {|LAQ9001:...|} markup is otherwise ambiguous between them. All cases below want MissingMemberDescriptor,
            // which is first in SupportedDiagnostics.
            MarkupOptions = MarkupOptions.UseFirstDescriptor,
            // Without this the formatter reflows with Environment.NewLine, making the expected sources platform-dependent
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = lf\n") } },
        }.RunAsync();

    private static Task VerifyNoDiagnostic(string source) => VerifyAnalyzer(source);

    // Regression coverage for the real bug that exposed this: the reflection fallback used for BCL targets
    // couldn't resolve any generic type (Type.GetType can't parse C#'s '<T>' syntax), so it silently bailed
    // for every accessor targeting List<T>/Queue<T>/Stack<T> instead of reporting a missing member.
    #region generic BCL targets
    [Fact]
    public Task GenericBclTargetWithWrongFieldNameReports()
        => VerifyAnalyzer(
            """
            using System.Runtime.CompilerServices;
            using System.Collections.Generic;

            public static class StackAccessors<T>
            {
                [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T[] {|LAQ9001:_items|}(Stack<T> _);
            }
            """
        );

    [Fact]
    public Task GenericBclTargetOnStackWithCorrectFieldNameDoesNotReport()
        => VerifyNoDiagnostic(
            """
            using System.Runtime.CompilerServices;
            using System.Collections.Generic;

            public static class StackAccessors<T>
            {
                [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T[] _array(Stack<T> _);
            }
            """
        );

    [Fact]
    public Task GenericBclTargetOnListWithCorrectFieldNameDoesNotReport()
        => VerifyNoDiagnostic(
            """
            using System.Runtime.CompilerServices;
            using System.Collections.Generic;

            public static class ListAccessors<T>
            {
                [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref T[] _items(List<T> _);
            }
            """
        );

    [Fact]
    public Task GenericBclTargetOnQueueWithWrongFieldTypeReports()
        => VerifyAnalyzer(
            """
            using System.Runtime.CompilerServices;
            using System.Collections.Generic;

            public static class QueueAccessors<T>
            {
                [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref int {|LAQ9001:_array|}(Queue<T> _);
            }
            """
        );
    #endregion

    #region non-generic BCL targets (pre-existing behavior, must not regress)
    [Fact]
    public Task NonGenericBclTargetWithWrongFieldNameStillReports()
        => VerifyAnalyzer(
            """
            using System.Runtime.CompilerServices;
            using System.IO;

            public static class MemoryStreamAccessors
            {
                [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref byte[] {|LAQ9001:_notARealField|}(MemoryStream _);
            }
            """
        );

    [Fact]
    public Task NonGenericBclTargetWithCorrectFieldNameDoesNotReport()
        => VerifyNoDiagnostic(
            """
            using System.Runtime.CompilerServices;
            using System.IO;

            public static class MemoryStreamAccessors
            {
                [UnsafeAccessor(UnsafeAccessorKind.Field)] public static extern ref byte[] _buffer(MemoryStream _);
            }
            """
        );
    #endregion
}
