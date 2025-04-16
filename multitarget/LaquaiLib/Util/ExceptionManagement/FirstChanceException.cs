namespace LaquaiLib.Util.ExceptionManagement;

/// <summary>
/// The exception that is thrown when an unhandled exception is wrapped or otherwise handled in a <see cref="FirstChanceExceptionHandlers"/> method.
/// </summary>
public class FirstChanceException : Exception
{
    /// <summary>
    /// Initializes a new <see cref="FirstChanceException"/>.
    /// </summary>
    /// <param name="message">A message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FirstChanceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private FirstChanceException()
    {
    }
}
