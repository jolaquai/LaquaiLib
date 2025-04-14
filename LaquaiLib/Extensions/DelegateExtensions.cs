namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Delegate"/> Type.
/// </summary>
public static class DelegateExtensions
{
    /// <summary>
    /// Gets the invocation list of the specified <see cref="Delegate"/> forcibly retyped as an array of <typeparamref name="TDelegate"/>.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the <see cref="Delegate"/>.</typeparam>
    /// <param name="del">The <see cref="Delegate"/> to get the invocation list of.</param>
    /// <returns>The invocation list of the specified <see cref="Delegate"/> retyped as an array of <typeparamref name="TDelegate"/>, or <see langword="null"/> of that type if the delegate has no invocation list.</returns>
    /// <remarks>
    /// If not all elements of the invocation list can be cast to <typeparamref name="TDelegate"/>, consumers will run into non-sensical exceptions when attempting to call the delegates.
    /// </remarks>
    public static TDelegate[] GetInvocationList<TDelegate>(this Delegate del) where TDelegate : Delegate => System.Runtime.CompilerServices.Unsafe.As<TDelegate[]>(del?.GetInvocationList());
}
