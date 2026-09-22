using System.Windows.Threading;

using LaquaiLib.Threading;

namespace LaquaiLib.Windows.Extensions;

/// <inheritdoc/>
public static class ThreadingExtensions
{
    extension(Dispatcher dispatcher)
    {
        /// <summary>
        /// Gets an awaitable that causes continuations on <see langword="await"/>s on it to be invoked on the <see cref="Thread"/> associated with the specified <paramref name="dispatcher"/>.
        /// </summary>
        /// <returns>The awaitable object bound to the specified <paramref name="dispatcher"/>.</returns>
        public DispatcherAwaitable SwitchTo() => new DispatcherAwaitable(dispatcher);
    }
    extension(DispatcherObject dispatcherObject)
    {
        /// <summary>
        /// Gets an awaitable that causes continuations on <see langword="await"/>s on it to be invoked on the <see cref="Thread"/> associated with the <see cref="Dispatcher"/> that owns this <paramref name="dispatcherObject"/>.
        /// </summary>
        /// <returns>The awaitable object bound to the <see cref="Dispatcher"/> that owns the specified <paramref name="dispatcherObject"/>.</returns>
        public DispatcherAwaitable SwitchTo() => new DispatcherAwaitable(dispatcherObject.Dispatcher);
    }
}
