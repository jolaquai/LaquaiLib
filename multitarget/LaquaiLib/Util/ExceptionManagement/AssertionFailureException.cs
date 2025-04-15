using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LaquaiLib.Util.ExceptionManagement;

/// <summary>
/// The exception that is thrown when an assertion fails.
/// </summary>
public class AssertionFailureException<T> : Exception
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
    public T Value { get; }

    /// <summary>
    /// Initializes a new <see cref="AssertionFailureException{T}"/> with the value that failed an assertion and a message.
    /// </summary>
    /// <param name="value">The value that caused an assertion to fail.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="message">A message that described the assertion failure.</param>
    public AssertionFailureException(T value, Exception innerException = null, [CallerArgumentExpression(nameof(value))] string message = null)
        : base(string.IsNullOrWhiteSpace(message) ? MessageFromCaller : message, innerException)
    {
        Value = value;
    }
}
