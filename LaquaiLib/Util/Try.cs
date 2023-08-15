namespace LaquaiLib.Util;

/// <summary>
/// Provides static methods that wrap <c>try-catch</c> blocks.
/// </summary>
/// <remarks>All methods in this class explicitly allow <c>null</c> returns.</remarks>
public static class Try
{
    // Variant 1 groups operate directly with methods
    // Variant 2 groups operate on items which are tested by methods

    #region Execute
    // These would literally just be a usual try-catch around the method body, do that yourself...
    #endregion

    #region ExecuteOrDefault
    // These methods execute parameterless Func<T>s and propagate their return value if they succeed, otherwise returning a default value, either passed directly or produced by a Func<T>

    /// <summary>
    /// Attempts to execute the given parameterless <see cref="Func{TResult}"/> and propagates its return value if it succeeds, otherwise returning <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the value returned by <paramref name="method"/>.</typeparam>
    /// <param name="method">The <see cref="Func{TResult}"/> to execute.</param>
    /// <param name="defaultValue">The value to return if <paramref name="method"/> fails.</param>
    /// <returns>The value returned by <paramref name="method"/> if it succeeds, otherwise <typeparamref name="T"/>'s <c>default</c> value.</returns>
    public static T? ExecuteOrDefault<T>(T? defaultValue, Func<T?> method)
    {
        try
        {
            return method();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Attempts to execute the given parameterless <see cref="Func{TResult}"/> and propagates its return value if it succeeds, otherwise returning the value produced by <paramref name="defaultValueFactory"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the value returned by <paramref name="method"/>.</typeparam>
    /// <param name="method">The <see cref="Func{TResult}"/> to execute.</param>
    /// <param name="defaultValueFactory">The <see cref="Func{TResult}"/> that returns the default value to propagate if <paramref name="method"/> throws an exception.</param>
    /// <returns>The value returned by <paramref name="method"/> if it succeeds, otherwise the value produced by <paramref name="defaultValueFactory"/>.</returns>
    public static T? ExecuteOrDefault<T>(Func<T?> defaultValueFactory, Func<T?> method)
    {
        try
        {
            return method();
        }
        catch
        {
            return defaultValueFactory();
        }
    }
    #endregion

    #region First (variant 1)
    // These methods execute parameterless Func<T>s and propagate the return value of the first that succeeds, otherwise throwing an AggregateException containing all thrown exceptions

    /// <summary>
    /// Attempts to execute the given parameterless <see cref="Func{TResult}"/>s and propagates its return value if it succeeds, otherwise throwing an <see cref="AggregateException"/> composed of all exceptions thrown by the <paramref name="methods"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the value that is ultimately returned.</typeparam>
    /// <param name="methods">The methods to try.</param>
    /// <returns>The value of the first method that doesn't throw an exception.</returns>
    /// <exception cref="AggregateException">Thrown if all methods throw an exception.</exception>
    public static T? First<T>(IEnumerable<Func<T?>> methods)
    {
        var exceptions = new List<Exception>();
        foreach (var method in methods)
        {
            try
            {
                return method();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        throw new AggregateException(exceptions);
    }
    /// <inheritdoc cref="First{T}(IEnumerable{Func{T}})"/>
    public static T? First<T>(params Func<T?>[] methods) => First((IEnumerable<Func<T?>>)methods);
    #endregion

    #region First (variant 2)
    // These methods test items in collection using a test Action<T> and return the first item that the test method does not throw an exception for, otherwise throwing an AggregateException containing all thrown exceptions

    /// <inheritdoc cref="First{T}(Action{T}, IEnumerable{T})"/>
    public static T? First<T>(Action<T?> test, params T?[] items) => First(test, (IEnumerable<T?>)items);
    /// <summary>
    /// Tests each item in <paramref name="items"/> using <paramref name="test"/> and returns the first item which <paramref name="test"/> does not throw an exception for, otherwise throwing an <see cref="AggregateException"/> composed of all exceptions thrown by <paramref name="test"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items to test.</typeparam>
    /// <param name="test">The <see cref="Action{T}"/> to test each item with.</param>
    /// <param name="items">The items to test.</param>
    /// <returns>The first item in <paramref name="items"/> that <paramref name="test"/> does not throw an exception for.</returns>
    public static T? First<T>(Action<T?> test, IEnumerable<T?> items)
    {
        var exceptions = new List<Exception>();
        foreach (var item in items)
        {
            try
            {
                test(item);
                return item;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        throw new AggregateException(exceptions);
    }
    #endregion

    #region FirstOrDefault (variant 1)
    // These methods execute parameterless Func<T>s and propagate the return value of the first that succeeds, otherwise returning a default value, either passed directly or produced by a Func<T>

    /// <summary>
    /// Attempts to execute the given parameterless <see cref="Func{TResult}"/>s and propagates the return value of the first succeeds, otherwise returning <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the value that is ultimately returned.</typeparam>
    /// <param name="defaultValue">The default value to return if all methods throw an exception.</param>
    /// <param name="methods">The methods to try.</param>
    /// <returns>The first value returned by a method that doesn't throw an exception, or the default value if all methods throw an exception.</returns>
    public static T? FirstOrDefault<T>(T? defaultValue, IEnumerable<Func<T?>> methods)
    {
        foreach (var method in methods)
        {
            try
            {
                return method();
            }
            catch
            { }
        }
        return defaultValue;
    }
    /// <inheritdoc cref="FirstOrDefault{T}(T, IEnumerable{Func{T}})"/>
    public static T? FirstOrDefault<T>(T? defaultValue, params Func<T?>[] methods) => FirstOrDefault(defaultValue, (IEnumerable<Func<T?>>)methods);
    /// <inheritdoc cref="FirstOrDefault{T}(Func{T}, IEnumerable{Func{T}})"/>
    public static T? FirstOrDefault<T>(Func<T?> defaultValueFactory, params Func<T?>[] methods) => FirstOrDefault(defaultValueFactory, (IEnumerable<Func<T?>>)methods);
    /// <summary>
    /// Attempts to execute the given parameterless <see cref="Func{TResult}"/>s and propagates the return value of the first one that succeeds, otherwise returning the value produced by <paramref name="defaultValueFactory"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the value that is ultimately returned.</typeparam>
    /// <param name="defaultValueFactory">The <see cref="Func{TResult}"/> that returns the default value to propagate if all <paramref name="methods"/> throw an exception. This is executed outside of the <c>try-catch</c> block and so is expected to always succeed.</param>
    /// <param name="methods">The methods to try.</param>
    /// <returns>The first value returned by a method that doesn't throw an exception, otherwise the value produced by <paramref name="defaultValueFactory"/>.</returns>
    public static T? FirstOrDefault<T>(Func<T?> defaultValueFactory, IEnumerable<Func<T?>> methods)
    {
        foreach (var method in methods)
        {
            try
            {
                return method();
            }
            catch
            { }
        }
        return defaultValueFactory();
    }
    #endregion

    #region FirstOrDefault (variant 2)
    // These methods test items in collection using a test Action<T> and return the first item that the test method does not throw an exception for, otherwise returning a default value, either passed directly or produced by a Func<T>

    /// <inheritdoc cref="FirstOrDefault{T}(T, Action{T}, IEnumerable{T})"/>
    public static T? FirstOrDefault<T>(T? defaultValue, Action<T?> test, params T?[] items) => FirstOrDefault(defaultValue, test, (IEnumerable<T?>)items);
    /// <summary>
    /// Tests each item in <paramref name="items"/> using <paramref name="test"/> and returns the first item which <paramref name="test"/> does not throw an exception for, otherwise returning <paramref name="defaultValue"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items to test.</typeparam>
    /// <param name="defaultValue">The default value to return if all items throw an exception.</param>
    /// <param name="test">The <see cref="Action{T}"/> to test each item with.</param>
    /// <param name="items">The items to test.</param>
    /// <returns>The first item in <paramref name="items"/> that <paramref name="test"/> does not throw an exception for, otherwise <paramref name="defaultValue"/>.</returns>
    public static T? FirstOrDefault<T>(T? defaultValue, Action<T?> test, IEnumerable<T?> items)
    {
        foreach (var item in items)
        {
            try
            {
                test(item);
                return item;
            }
            catch
            { }
        }
        return defaultValue;
    }

    /// <inheritdoc cref="FirstOrDefault{T}(Func{T}, Action{T}, IEnumerable{T})"/>
    public static T? FirstOrDefault<T>(Func<T?> defaultValueFactory, Action<T?> test, params T?[] items) => FirstOrDefault(defaultValueFactory, test, (IEnumerable<T?>)items);
    /// <summary>
    /// Tests each item in <paramref name="items"/> using <paramref name="test"/> and returns the first item which <paramref name="test"/> does not throw an exception for, otherwise returning the value produced by <paramref name="defaultValueFactory"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items to test.</typeparam>
    /// <param name="defaultValueFactory">The <see cref="Func{TResult}"/> that returns the default value to propagate if all <paramref name="items"/> cause an exception in <paramref name="test"/>. This is executed outside of the <c>try-catch</c> block and so is expected to always succeed.</param>
    /// <param name="test">The <see cref="Action{T}"/> to test each item with.</param>
    /// <param name="items">The items to test.</param>
    /// <returns>The first item in <paramref name="items"/> that <paramref name="test"/> does not throw an exception for, otherwise returning the value produced by <paramref name="defaultValueFactory"/>.</returns>
    public static T? FirstOrDefault<T>(Func<T?> defaultValueFactory, Action<T?> test, IEnumerable<T?> items)
    {
        foreach (var item in items)
        {
            try
            {
                test(item);
                return item;
            }
            catch
            { }
        }
        return defaultValueFactory();
    }
    #endregion
}
