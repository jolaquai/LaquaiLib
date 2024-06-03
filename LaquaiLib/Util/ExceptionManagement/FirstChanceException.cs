namespace LaquaiLib.Util.ExceptionManagement;

/// <summary>
/// The exception that is thrown when an unhandled exception is wrapped or otherwise handled in a <see cref="FirstChanceExceptionHandlers"/> method.
/// </summary>
public class FirstChanceException : Exception
{
    public FirstChanceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private FirstChanceException()
    {
    }
}
