using LaquaiLib.Analyzers.Quality__1XXX_;

namespace LaquaiLib.Analyzers.Tests.Quality;

public class MissingDoesNotReturnAnalyzerTests
{
    private static Task VerifyAnalyzer(string source)
        => new CSharpAnalyzerTest<MissingDoesNotReturnAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestState = { AnalyzerConfigFiles = { ("/.editorconfig", "root = true\n\n[*.cs]\nend_of_line = crlf\n") } },
        }.RunAsync();

    private static Task VerifyNoDiagnostic(string source) => VerifyAnalyzer(source);

    private const string Using =
        """
        using System;
        using System.Diagnostics.CodeAnalysis;

        """;

    #region offered
    [Fact]
    public Task DirectCallToDoesNotReturnMethod()
        => VerifyAnalyzer(
            Using + """
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                static void {|LAQ1002:M|}() => Throw();
            }
            """
        );

    [Fact]
    public Task IndirectCallThroughAChainIsFollowed()
        => VerifyAnalyzer(
            Using + """
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                static void {|LAQ1002:Helper|}() => Throw();
                static void {|LAQ1002:M|}() => Helper();
            }
            """
        );

    [Fact]
    public Task BlockBodyCallAsFirstStatement()
        => VerifyAnalyzer(
            Using + """
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                static void {|LAQ1002:M|}()
                {
                    Throw();
                }
            }
            """
        );

    [Fact]
    public Task ReturnStatementCallingDoesNotReturnMethod()
        => VerifyAnalyzer(
            Using + """
            class C
            {
                [DoesNotReturn] static int Throw() => throw new Exception();
                static int {|LAQ1002:M|}()
                {
                    return Throw();
                }
            }
            """
        );

    [Fact]
    public Task PropertyGetAccessorCallingDoesNotReturnMethod()
        => VerifyAnalyzer(
            Using + """
            class C
            {
                [DoesNotReturn] static void ThrowHelper() => throw new Exception();
                long Length
                {
                    {|LAQ1002:get|}
                    {
                        ThrowHelper();
                        return default;
                    }
                }
            }
            """
        );

    [Fact]
    public Task SetAccessorAssigningToDoesNotReturnSetter()
        => VerifyAnalyzer(
            Using + """
            class C
            {
                long Position
                {
                    [DoesNotReturn] set => throw new NotSupportedException();
                }
                long Other
                {
                    {|LAQ1002:set|} => Position = value;
                }
            }
            """
        );

    [Fact]
    public Task ExpressionBodiedPropertyReadingDoesNotReturnGetter()
        => VerifyAnalyzer(
            Using + """
            class C
            {
                long Position
                {
                    [DoesNotReturn] get => throw new NotSupportedException();
                }
                long {|LAQ1002:Other|} => Position;
            }
            """
        );

    [Fact]
    public Task IndexerGetAccessorCallingDoesNotReturnMethod()
        => VerifyAnalyzer(
            Using + """
            class C
            {
                [DoesNotReturn] static int ThrowHelper() => throw new Exception();
                int this[int i]
                {
                    {|LAQ1002:get|} => ThrowHelper();
                }
            }
            """
        );
    #endregion

    #region not offered
    [Fact]
    public Task AlreadyMarkedNeverReports()
        => VerifyNoDiagnostic(
            Using + """
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                [DoesNotReturn] static void M() => Throw();
            }
            """
        );

    [Fact]
    public Task ConditionalCallIsNotTheFirstThingUnconditionally()
        => VerifyNoDiagnostic(
            Using + """
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                static void M(bool b)
                {
                    if (b)
                        Throw();
                }
            }
            """
        );

    [Fact]
    public Task DoesNotReturnCallIsNotTheFirstStatement()
        => VerifyNoDiagnostic(
            Using + """
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                static void M()
                {
                    Console.WriteLine();
                    Throw();
                }
            }
            """
        );

    [Fact]
    public Task TargetNotMarkedDoesNotReturnNeverReports()
        => VerifyNoDiagnostic(
            Using + """
            class C
            {
                static void Helper() { }
                static void M() => Helper();
            }
            """
        );

    [Fact]
    public Task AsyncMethodsAreNeverFollowedInto()
        => VerifyNoDiagnostic(
            Using + """
            using System.Threading.Tasks;
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                static async Task M()
                {
                    Throw();
                    await Task.Yield();
                }
            }
            """
        );

    [Fact]
    public Task IteratorsAreNeverFollowedInto()
        => VerifyNoDiagnostic(
            Using + """
            using System.Collections.Generic;
            class C
            {
                [DoesNotReturn] static void Throw() => throw new Exception();
                static IEnumerable<int> M()
                {
                    Throw();
                    yield return 1;
                }
            }
            """
        );
    #endregion
}
