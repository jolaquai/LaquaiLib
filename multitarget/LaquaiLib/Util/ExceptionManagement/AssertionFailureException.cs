using System.Diagnostics;
using System.Reflection;

namespace LaquaiLib.Util.ExceptionManagement;

/// <summary>
/// The exception that is thrown when an assertion fails.
/// </summary>
/// <remarks>
/// Initializes a new <see cref="AssertionFailureException{T}"/> with the value that failed an assertion and a message.
/// </remarks>
/// <param name="value">The value that caused an assertion to fail.</param>
/// <param name="innerException">The exception that is the cause of the current exception.</param>
/// <param name="message">A message that described the assertion failure.</param>
public class AssertionFailureException<T>(T value, Exception innerException = null, [CallerArgumentExpression(nameof(value))] string message = null) : Exception(string.IsNullOrWhiteSpace(message) ? MessageFromCaller : message, innerException)
{
    private const string _defaultMessage = "Assertion failed.";

    private static string MessageFromCaller
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => field ??= new StackFrame(1).GetMethod() is MethodBase method ? $"Assertion failed in {method.Name}." : _defaultMessage;
    }

    /// <summary>
    /// The value that caused an assertion to fail.
    /// </summary>
    public T Value { get; } = value;
}
