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
    public static void OnException<TDelegate>(this TDelegate del, Action<Exception> onException, params ReadOnlySpan<object> arguments)
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

    /// <summary>
    /// Gets the invocation list of the specified <see cref="Delegate"/> retyped as an array of <typeparamref name="TDelegate"/>.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the <see cref="Delegate"/>.</typeparam>
    /// <param name="del">The <see cref="Delegate"/> to get the invocation list of.</param>
    /// <returns>The invocation list of the specified <see cref="Delegate"/> retyped as an array of <typeparamref name="TDelegate"/>.</returns>
    /// <remarks>
    /// If not all elements of the invocation list can be cast to <typeparamref name="TDelegate"/>, consumers will run into non-sensical exceptions when attempting to call the delegates.
    /// </remarks>
    public static TDelegate[] GetInvocationList<TDelegate>(this Delegate del)
        where TDelegate : Delegate => AnyExtensions.As<TDelegate[]>(del.GetInvocationList());
}
