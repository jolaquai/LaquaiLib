namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Delegate"/> Type.
/// </summary>
public static class DelegateExtensions
{
    /// <summary>
    /// Executes the specified <see cref="Delegate"/> (dynamically, causing considerable overhead) and catches any exceptions that occur, instead passing it to the specified <see cref="Action{T}"/>.
    /// </summary>
    /// <param name="del">The <see cref="Delegate"/> to execute.</param>
    /// <param name="onException">The <see cref="Action{T}"/> to execute when an exception occurs.</param>
    /// <param name="arguments">The arguments to pass to the <see cref="Delegate"/>.</param>
    public static void OnException<TDelegate>(this TDelegate del, Action<Exception> onException, params ReadOnlySpan<object?> arguments)
        where TDelegate : Delegate
    {
        try
        {
            del.DynamicInvoke(arguments.ToArray());
        }
        catch (Exception ex)
        {
            onException(ex);
        }
    }
}
