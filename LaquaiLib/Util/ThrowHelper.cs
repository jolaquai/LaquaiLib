using LaquaiLib.Extensions;

namespace LaquaiLib.Util;

/// <summary>
/// Provides methods which help to throw exceptions.
/// </summary>
public static class ThrowHelper
{
    /// <summary>
    /// Throws an instance of the given exception type <typeparamref name="TException"/> on the first item in <paramref name="items"/> which does not match the given <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="TException">The <see cref="Type"/> of the exception to throw.</typeparam>
    /// <typeparam name="TTest">The <see cref="Type"/> of the items to test.</typeparam>
    /// <param name="constructorParamFactory">A <see cref="Func{T, TResult}"/> that is passed the first item in <paramref name="items"/> which does not match the given <paramref name="predicate"/> and returns an <see cref="Array"/> of nullable <see cref="object"/>s (or <c>null</c>) which is passed to the constructor of the exception to throw. If it returns <c>null</c>, the exception type's parameterless constructor is invoked.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> the items must pass.</param>
    /// <param name="items">The items to test.</param>
    public static void ThrowOnFirstOffender<TException, TTest>(Func<TTest, object?[]?> constructorParamFactory, Predicate<TTest> predicate, params TTest[] items)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(typeof(TException));
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
        {
            if (!predicate(item))
            {
                throw Activator.CreateInstance(typeof(TException), constructorParamFactory(item)).Cast<TException>();
            }
        }
    }
}
