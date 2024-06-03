using System.Windows.Threading;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="DispatcherObject"/> Type.
/// </summary>
public static class DispatcherObjectExtensions
{
    /// <summary>
    /// Invokes the specified <paramref name="action"/> on the thread the <paramref name="dispatcherObject"/> was created on.
    /// </summary>
    /// <param name="dispatcherObject">The <see cref="DispatcherObject"/> of which the <see cref="Dispatcher"/> is used to invoke the <paramref name="action"/>.</param>
    /// <param name="action">The <see cref="Action"/> to invoke.</param>
    public static void Dispatch(this DispatcherObject dispatcherObject, Action action) => dispatcherObject.Dispatcher.Invoke(action);
    /// <summary>
    /// Invokes the specified <paramref name="method"/> with the given <paramref name="arguments"/> on the thread the <paramref name="dispatcherObject"/> was created on.
    /// </summary>
    /// <param name="dispatcherObject">The <see cref="DispatcherObject"/> of which the <see cref="Dispatcher"/> is used to invoke the <paramref name="method"/>.</param>
    /// <param name="method">The <see cref="Delegate"/> to invoke.</param>
    /// <param name="arguments">The arguments to pass to the <paramref name="method"/>.</param>
    public static void Dispatch(this DispatcherObject dispatcherObject, Delegate method, params object?[]? arguments) => dispatcherObject.Dispatcher.Invoke(method, arguments);
    /// <summary>
    /// Invokes the specified <paramref name="func"/> on the thread the <paramref name="dispatcherObject"/> was created on.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the <paramref name="func"/>.</typeparam>
    /// <param name="dispatcherObject">The <see cref="DispatcherObject"/> of which the <see cref="Dispatcher"/> is used to invoke the <paramref name="func"/>.</param>
    /// <param name="func">The <see cref="Func{TResult}"/> to invoke.</param>
    /// <returns>The result returned by the <paramref name="func"/>.</returns>
    public static T Dispatch<T>(this DispatcherObject dispatcherObject, Func<T> func) => dispatcherObject.Dispatcher.Invoke(func);
    /// <summary>
    /// Asynchronously invokes the specified <paramref name="action"/> on the thread the <paramref name="dispatcherObject"/> was created on.
    /// </summary>
    /// <param name="dispatcherObject">The <see cref="DispatcherObject"/> of which the <see cref="Dispatcher"/> is used to invoke the <paramref name="action"/>.</param>
    /// <param name="action">The <see cref="Action"/> to invoke.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task DispatchAsync(this DispatcherObject dispatcherObject, Action action) => await dispatcherObject.Dispatcher.InvokeAsync(action);
    /// <summary>
    /// Asynchronously invokes the specified <paramref name="asyncDelegate"/> on the thread the <paramref name="dispatcherObject"/> was created on.
    /// </summary>
    /// <param name="dispatcherObject">The <see cref="DispatcherObject"/> of which the <see cref="Dispatcher"/> is used to invoke the <paramref name="asyncDelegate"/>.</param>
    /// <param name="asyncDelegate">The <see cref="Func{TResult}"/> to invoke.</param>
    /// <returns>The <see cref="Task{TResult}"/> returned by the <paramref name="asyncDelegate"/>.</returns>
    public static async Task DispatchAsync(this DispatcherObject dispatcherObject, Func<Task> asyncDelegate) => await dispatcherObject.Dispatcher.InvokeAsync(asyncDelegate);
    /// <summary>
    /// Asynchronously invokes the specified <paramref name="asyncDelegate"/> on the thread the <paramref name="dispatcherObject"/> was created on.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the <paramref name="asyncDelegate"/>.</typeparam>
    /// <param name="dispatcherObject">The <see cref="DispatcherObject"/> of which the <see cref="Dispatcher"/> is used to invoke the <paramref name="asyncDelegate"/>.</param>
    /// <param name="asyncDelegate">The <see cref="Func{TResult}"/> to invoke.</param>
    /// <returns>The <see cref="Task{TResult}"/> returned by the <paramref name="asyncDelegate"/>.</returns>
    public static async Task<T> DispatchAsync<T>(this DispatcherObject dispatcherObject, Func<Task<T>> asyncDelegate) => await await dispatcherObject.Dispatcher.InvokeAsync(asyncDelegate);
}
