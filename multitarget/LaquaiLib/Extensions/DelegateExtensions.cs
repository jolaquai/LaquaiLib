namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Delegate"/> Type.
/// </summary>
public static class DelegateExtensions
{
    extension<TDelegate>(TDelegate del) where TDelegate : Delegate
    {
        /// <summary>
        /// Gets the invocation list of the specified <see cref="Delegate"/> forcibly retyped as an array of <typeparamref name="TDelegate"/>. This overload is useful when the delegate type is definitively known at compile-time (such as for <see langword="event"/> <see langword="delegate"/>s).
        /// </summary>
        /// <typeparam name="TDelegate">The type of the <see cref="Delegate"/>.</typeparam>
        /// <param name="del">The <typeparamref name="TDelegate"/> to get the invocation list of.</param>
        /// <returns>The invocation list of the specified <see cref="Delegate"/> retyped as an array of <typeparamref name="TDelegate"/>, or <see langword="null"/> of that type if the delegate has no invocation list.</returns>
        public TDelegate[] TypedInvocationList
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Unsafe.As<TDelegate[]>(del?.GetInvocationList());
        }
    }
    extension(Delegate del)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDelegate[] GetInvocationList<TDelegate>() where TDelegate : Delegate => Unsafe.As<TDelegate[]>(del?.GetInvocationList());

        /// <summary>
        /// Gets whether the specified <see cref="Delegate"/> is static.
        /// </summary>
        public bool IsStatic
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => del.Target is null && del.Method.IsStatic;
        }
        /// <summary>
        /// Gets whether the specified <see cref="Delegate"/> is a closure (that is, a delegate that captures state from its surrounding scope where it was defined).
        /// </summary>
        public bool IsClosure
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => del.Target?.GetType().Name.Contains("DisplayClass") is true;
        }
    }
}
