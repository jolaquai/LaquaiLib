namespace LaquaiLib.Util.ExceptionManagement;

/// <summary>
/// The exception that is thrown when an unhandled exception is wrapped or otherwise handled in a <see cref="FirstChanceExceptionHandlers"/> method.
/// </summary>
public class FirstChanceException : Exception
{
    private FirstChanceException(string message) : base(message)
    {
    }

    public FirstChanceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private FirstChanceException()
    {
    }
}
