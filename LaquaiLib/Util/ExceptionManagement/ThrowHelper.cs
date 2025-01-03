using System.Runtime.CompilerServices;

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
    /// <param name="constructorParamFactory">A <see cref="Func{T, TResult}"/> that is passed the first item in <paramref name="items"/> which does not match the given <paramref name="predicate"/> and returns an <see cref="Array"/> of nullable <see cref="object"/>s (or <see langword="null"/>) which is passed to the constructor of the exception to throw. If it returns <see langword="null"/>, the exception type's parameterless constructor is invoked.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> the items must pass.</param>
    /// <param name="items">The items to test.</param>
    public static void ThrowOnFirstOffender<TException, TTest>(Func<TTest, object[]> constructorParamFactory, Predicate<TTest> predicate, params ReadOnlySpan<TTest> items)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(typeof(TException));
        ArgumentNullException.ThrowIfNull(predicate);

        for (var i = 0; i < items.Length; i++)
        {
            if (!predicate(items[i]))
            {
                throw Activator.CreateInstance(typeof(TException), constructorParamFactory(items[i])).Cast<object, TException>();
            }
        }
    }
    /// <summary>
    /// Throws the specified <paramref name="exception"/> of type <typeparamref name="TException"/> on the first item in <paramref name="items"/> which does not match the given <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="TException">The <see cref="Type"/> of the exception to throw.</typeparam>
    /// <typeparam name="TTest">The <see cref="Type"/> of the items to test.</typeparam>
    /// <param name="exception">The <see cref="Exception"/> to throw.</param>
    /// <param name="predicate">The <see cref="Predicate{T}"/> the items must pass.</param>
    /// <param name="items">The items to test.</param>
    public static void ThrowOnFirstOffender<TException, TTest>(TException exception, Predicate<TTest> predicate, params ReadOnlySpan<TTest> items)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(predicate);

        for (var i = 0; i < items.Length; i++)
        {
            if (!predicate(items[i]))
            {
                throw exception;
            }
        }
    }

    #region ThrowOnNull overloads
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>. This method has overloads for up to 10 arguments.
    /// </summary>
    /// <param name="obj1">The <see cref="object"/> to test.</param>
    /// <param name="arg1">The name of the argument to test.</param>
    public static void ThrowOnNull(
        object obj1,
        [CallerArgumentExpression(nameof(obj1))] string arg1 = "") => ArgumentNullException.ThrowIfNull(obj1, arg1);
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>. This method has overloads for up to 10 arguments.
    /// </summary>
    /// <param name="obj1">An <see cref="object"/> to test.</param>
    /// <param name="obj2">An <see cref="object"/> to test.</param>
    /// <param name="arg1">The name of the first argument to test.</param>
    /// <param name="arg2">The name of the second argument to test.</param>
    public static void ThrowOnNull(
        object obj1,
        object obj2,
        [CallerArgumentExpression(nameof(obj1))] string arg1 = "",
        [CallerArgumentExpression(nameof(obj2))] string arg2 = "")
    {
        ArgumentNullException.ThrowIfNull(obj1, arg1);
        ArgumentNullException.ThrowIfNull(obj2, arg2);
    }
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>. This method has overloads for up to 10 arguments.
    /// </summary>
    /// <param name="obj1">An <see cref="object"/> to test.</param>
    /// <param name="obj2">An <see cref="object"/> to test.</param>
    /// <param name="obj3">An <see cref="object"/> to test.</param>
    /// <param name="arg1">The name of the first argument to test.</param>
    /// <param name="arg2">The name of the second argument to test.</param>
    /// <param name="arg3">The name of the third argument to test.</param>
    public static void ThrowOnNull(
        object obj1,
        object obj2,
        object obj3,
        [CallerArgumentExpression(nameof(obj1))] string arg1 = "",
        [CallerArgumentExpression(nameof(obj2))] string arg2 = "",
        [CallerArgumentExpression(nameof(obj3))] string arg3 = "")
    {
        ArgumentNullException.ThrowIfNull(obj1, arg1);
        ArgumentNullException.ThrowIfNull(obj2, arg2);
        ArgumentNullException.ThrowIfNull(obj3, arg3);
    }
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>. This method has overloads for up to 10 arguments.
    /// </summary>
    /// <param name="obj1">An <see cref="object"/> to test.</param>
    /// <param name="obj2">An <see cref="object"/> to test.</param>
    /// <param name="obj3">An <see cref="object"/> to test.</param>
    /// <param name="obj4">An <see cref="object"/> to test.</param>
    /// <param name="arg1">The name of the first argument to test.</param>
    /// <param name="arg2">The name of the second argument to test.</param>
    /// <param name="arg3">The name of the third argument to test.</param>
    /// <param name="arg4">The name of the fourth argument to test.</param>
    public static void ThrowOnNull(
        object obj1,
        object obj2,
        object obj3,
        object obj4,
        [CallerArgumentExpression(nameof(obj1))] string arg1 = "",
        [CallerArgumentExpression(nameof(obj2))] string arg2 = "",
        [CallerArgumentExpression(nameof(obj3))] string arg3 = "",
        [CallerArgumentExpression(nameof(obj4))] string arg4 = "")
    {
        ArgumentNullException.ThrowIfNull(obj1, arg1);
        ArgumentNullException.ThrowIfNull(obj2, arg2);
        ArgumentNullException.ThrowIfNull(obj3, arg3);
        ArgumentNullException.ThrowIfNull(obj4, arg4);
    }
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>. This method has overloads for up to 10 arguments.
    /// </summary>
    /// <param name="obj1">An <see cref="object"/> to test.</param>
    /// <param name="obj2">An <see cref="object"/> to test.</param>
    /// <param name="obj3">An <see cref="object"/> to test.</param>
    /// <param name="obj4">An <see cref="object"/> to test.</param>
    /// <param name="obj5">An <see cref="object"/> to test.</param>
    /// <param name="arg1">The name of the first argument to test.</param>
    /// <param name="arg2">The name of the second argument to test.</param>
    /// <param name="arg3">The name of the third argument to test.</param>
    /// <param name="arg4">The name of the fourth argument to test.</param>
    /// <param name="arg5">The name of the fifth argument to test.</param>
    public static void ThrowOnNull(
        object obj1,
        object obj2,
        object obj3,
        object obj4,
        object obj5,
        [CallerArgumentExpression(nameof(obj1))] string arg1 = "",
        [CallerArgumentExpression(nameof(obj2))] string arg2 = "",
        [CallerArgumentExpression(nameof(obj3))] string arg3 = "",
        [CallerArgumentExpression(nameof(obj4))] string arg4 = "",
        [CallerArgumentExpression(nameof(obj5))] string arg5 = "")
    {
        ArgumentNullException.ThrowIfNull(obj1, arg1);
        ArgumentNullException.ThrowIfNull(obj2, arg2);
        ArgumentNullException.ThrowIfNull(obj3, arg3);
        ArgumentNullException.ThrowIfNull(obj4, arg4);
        ArgumentNullException.ThrowIfNull(obj5, arg5);
    }
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>. This method has overloads for up to 10 arguments.
    /// </summary>
    /// <param name="obj1">An <see cref="object"/> to test.</param>
    /// <param name="obj2">An <see cref="object"/> to test.</param>
    /// <param name="obj3">An <see cref="object"/> to test.</param>
    /// <param name="obj4">An <see cref="object"/> to test.</param>
    /// <param name="obj5">An <see cref="object"/> to test.</param>
    /// <param name="obj6">An <see cref="object"/> to test.</param>
    /// <param name="arg1">The name of the first argument to test.</param>
    /// <param name="arg2">The name of the second argument to test.</param>
    /// <param name="arg3">The name of the third argument to test.</param>
    /// <param name="arg4">The name of the fourth argument to test.</param>
    /// <param name="arg5">The name of the fifth argument to test.</param>
    /// <param name="arg6">The name of the sixth argument to test.</param>
    public static void ThrowOnNull(
        object obj1,
        object obj2,
        object obj3,
        object obj4,
        object obj5,
        object obj6,
        [CallerArgumentExpression(nameof(obj1))] string arg1 = "",
        [CallerArgumentExpression(nameof(obj2))] string arg2 = "",
        [CallerArgumentExpression(nameof(obj3))] string arg3 = "",
        [CallerArgumentExpression(nameof(obj4))] string arg4 = "",
        [CallerArgumentExpression(nameof(obj5))] string arg5 = "",
        [CallerArgumentExpression(nameof(obj6))] string arg6 = "")
    {
        ArgumentNullException.ThrowIfNull(obj1, arg1);
        ArgumentNullException.ThrowIfNull(obj2, arg2);
        ArgumentNullException.ThrowIfNull(obj3, arg3);
        ArgumentNullException.ThrowIfNull(obj4, arg4);
        ArgumentNullException.ThrowIfNull(obj5, arg5);
        ArgumentNullException.ThrowIfNull(obj6, arg6);
    }
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>. This method has overloads for up to 10 arguments.
    /// </summary>
    /// <param name="obj1">An <see cref="object"/> to test.</param>
    /// <param name="obj2">An <see cref="object"/> to test.</param>
    /// <param name="obj3">An <see cref="object"/> to test.</param>
    /// <param name="obj4">An <see cref="object"/> to test.</param>
    /// <param name="obj5">An <see cref="object"/> to test.</param>
    /// <param name="obj6">An <see cref="object"/> to test.</param>
    /// <param name="obj7">An <see cref="object"/> to test.</param>
    /// <param name="arg1">The name of the first argument to test.</param>
    /// <param name="arg2">The name of the second argument to test.</param>
    /// <param name="arg3">The name of the third argument to test.</param>
    /// <param name="arg4">The name of the fourth argument to test.</param>
    /// <param name="arg5">The name of the fifth argument to test.</param>
    /// <param name="arg6">The name of the sixth argument to test.</param>
    /// <param name="arg7">The name of the seventh argument to test.</param>
    public static void ThrowOnNull(
        object obj1,
        object obj2,
        object obj3,
        object obj4,
        object obj5,
        object obj6,
        object obj7,
        [CallerArgumentExpression(nameof(obj1))] string arg1 = "",
        [CallerArgumentExpression(nameof(obj2))] string arg2 = "",
        [CallerArgumentExpression(nameof(obj3))] string arg3 = "",
        [CallerArgumentExpression(nameof(obj4))] string arg4 = "",
        [CallerArgumentExpression(nameof(obj5))] string arg5 = "",
        [CallerArgumentExpression(nameof(obj6))] string arg6 = "",
        [CallerArgumentExpression(nameof(obj7))] string arg7 = "")
    {
        ArgumentNullException.ThrowIfNull(obj1, arg1);
        ArgumentNullException.ThrowIfNull(obj2, arg2);
        ArgumentNullException.ThrowIfNull(obj3, arg3);
        ArgumentNullException.ThrowIfNull(obj4, arg4);
        ArgumentNullException.ThrowIfNull(obj5, arg5);
        ArgumentNullException.ThrowIfNull(obj6, arg6);
        ArgumentNullException.ThrowIfNull(obj7, arg7);
    }
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>. This method has overloads for up to 10 arguments.
    /// </summary>
    /// <param name="obj1">An <see cref="object"/> to test.</param>
    /// <param name="obj2">An <see cref="object"/> to test.</param>
    /// <param name="obj3">An <see cref="object"/> to test.</param>
    /// <param name="obj4">An <see cref="object"/> to test.</param>
    /// <param name="obj5">An <see cref="object"/> to test.</param>
    /// <param name="obj6">An <see cref="object"/> to test.</param>
    /// <param name="obj7">An <see cref="object"/> to test.</param>
    /// <param name="obj8">An <see cref="object"/> to test.</param>
    /// <param name="arg1">The name of the first argument to test.</param>
    /// <param name="arg2">The name of the second argument to test.</param>
    /// <param name="arg3">The name of the third argument to test.</param>
    /// <param name="arg4">The name of the fourth argument to test.</param>
    /// <param name="arg5">The name of the fifth argument to test.</param>
    /// <param name="arg6">The name of the sixth argument to test.</param>
    /// <param name="arg7">The name of the seventh argument to test.</param>
    /// <param name="arg8">The name of the eighth argument to test.</param>
    public static void ThrowOnNull(
        object obj1,
        object obj2,
        object obj3,
        object obj4,
        object obj5,
        object obj6,
        object obj7,
        object obj8,
        [CallerArgumentExpression(nameof(obj1))] string arg1 = "",
        [CallerArgumentExpression(nameof(obj2))] string arg2 = "",
        [CallerArgumentExpression(nameof(obj3))] string arg3 = "",
        [CallerArgumentExpression(nameof(obj4))] string arg4 = "",
        [CallerArgumentExpression(nameof(obj5))] string arg5 = "",
        [CallerArgumentExpression(nameof(obj6))] string arg6 = "",
        [CallerArgumentExpression(nameof(obj7))] string arg7 = "",
        [CallerArgumentExpression(nameof(obj8))] string arg8 = "")
    {
        ArgumentNullException.ThrowIfNull(obj1, arg1);
        ArgumentNullException.ThrowIfNull(obj2, arg2);
        ArgumentNullException.ThrowIfNull(obj3, arg3);
        ArgumentNullException.ThrowIfNull(obj4, arg4);
        ArgumentNullException.ThrowIfNull(obj5, arg5);
        ArgumentNullException.ThrowIfNull(obj6, arg6);
        ArgumentNullException.ThrowIfNull(obj7, arg7);
        ArgumentNullException.ThrowIfNull(obj8, arg8);
    }
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>. This method has overloads for up to 10 arguments.
    /// </summary>
    /// <param name="obj1">An <see cref="object"/> to test.</param>
    /// <param name="obj2">An <see cref="object"/> to test.</param>
    /// <param name="obj3">An <see cref="object"/> to test.</param>
    /// <param name="obj4">An <see cref="object"/> to test.</param>
    /// <param name="obj5">An <see cref="object"/> to test.</param>
    /// <param name="obj6">An <see cref="object"/> to test.</param>
    /// <param name="obj7">An <see cref="object"/> to test.</param>
    /// <param name="obj8">An <see cref="object"/> to test.</param>
    /// <param name="obj9">An <see cref="object"/> to test.</param>
    /// <param name="arg1">The name of the first argument to test.</param>
    /// <param name="arg2">The name of the second argument to test.</param>
    /// <param name="arg3">The name of the third argument to test.</param>
    /// <param name="arg4">The name of the fourth argument to test.</param>
    /// <param name="arg5">The name of the fifth argument to test.</param>
    /// <param name="arg6">The name of the sixth argument to test.</param>
    /// <param name="arg7">The name of the seventh argument to test.</param>
    /// <param name="arg8">The name of the eighth argument to test.</param>
    /// <param name="arg9">The name of the ninth argument to test.</param>
    public static void ThrowOnNull(
        object obj1,
        object obj2,
        object obj3,
        object obj4,
        object obj5,
        object obj6,
        object obj7,
        object obj8,
        object obj9,
        [CallerArgumentExpression(nameof(obj1))] string arg1 = "",
        [CallerArgumentExpression(nameof(obj2))] string arg2 = "",
        [CallerArgumentExpression(nameof(obj3))] string arg3 = "",
        [CallerArgumentExpression(nameof(obj4))] string arg4 = "",
        [CallerArgumentExpression(nameof(obj5))] string arg5 = "",
        [CallerArgumentExpression(nameof(obj6))] string arg6 = "",
        [CallerArgumentExpression(nameof(obj7))] string arg7 = "",
        [CallerArgumentExpression(nameof(obj8))] string arg8 = "",
        [CallerArgumentExpression(nameof(obj9))] string arg9 = "")
    {
        ArgumentNullException.ThrowIfNull(obj1, arg1);
        ArgumentNullException.ThrowIfNull(obj2, arg2);
        ArgumentNullException.ThrowIfNull(obj3, arg3);
        ArgumentNullException.ThrowIfNull(obj4, arg4);
        ArgumentNullException.ThrowIfNull(obj5, arg5);
        ArgumentNullException.ThrowIfNull(obj6, arg6);
        ArgumentNullException.ThrowIfNull(obj7, arg7);
        ArgumentNullException.ThrowIfNull(obj8, arg8);
        ArgumentNullException.ThrowIfNull(obj9, arg9);
    }
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>. This method has overloads for up to 10 arguments.
    /// </summary>
    /// <param name="obj1">An <see cref="object"/> to test.</param>
    /// <param name="obj2">An <see cref="object"/> to test.</param>
    /// <param name="obj3">An <see cref="object"/> to test.</param>
    /// <param name="obj4">An <see cref="object"/> to test.</param>
    /// <param name="obj5">An <see cref="object"/> to test.</param>
    /// <param name="obj6">An <see cref="object"/> to test.</param>
    /// <param name="obj7">An <see cref="object"/> to test.</param>
    /// <param name="obj8">An <see cref="object"/> to test.</param>
    /// <param name="obj9">An <see cref="object"/> to test.</param>
    /// <param name="obj10">An <see cref="object"/> to test.</param>
    /// <param name="arg1">The name of the first argument to test.</param>
    /// <param name="arg2">The name of the second argument to test.</param>
    /// <param name="arg3">The name of the third argument to test.</param>
    /// <param name="arg4">The name of the fourth argument to test.</param>
    /// <param name="arg5">The name of the fifth argument to test.</param>
    /// <param name="arg6">The name of the sixth argument to test.</param>
    /// <param name="arg7">The name of the seventh argument to test.</param>
    /// <param name="arg8">The name of the eighth argument to test.</param>
    /// <param name="arg9">The name of the ninth argument to test.</param>
    /// <param name="arg10">The name of the tenth argument to test.</param>
    public static void ThrowOnNull(
        object obj1,
        object obj2,
        object obj3,
        object obj4,
        object obj5,
        object obj6,
        object obj7,
        object obj8,
        object obj9,
        object obj10,
        [CallerArgumentExpression(nameof(obj1))] string arg1 = "",
        [CallerArgumentExpression(nameof(obj2))] string arg2 = "",
        [CallerArgumentExpression(nameof(obj3))] string arg3 = "",
        [CallerArgumentExpression(nameof(obj4))] string arg4 = "",
        [CallerArgumentExpression(nameof(obj5))] string arg5 = "",
        [CallerArgumentExpression(nameof(obj6))] string arg6 = "",
        [CallerArgumentExpression(nameof(obj7))] string arg7 = "",
        [CallerArgumentExpression(nameof(obj8))] string arg8 = "",
        [CallerArgumentExpression(nameof(obj9))] string arg9 = "",
        [CallerArgumentExpression(nameof(obj10))] string arg10 = "")
    {
        ArgumentNullException.ThrowIfNull(obj1, arg1);
        ArgumentNullException.ThrowIfNull(obj2, arg2);
        ArgumentNullException.ThrowIfNull(obj3, arg3);
        ArgumentNullException.ThrowIfNull(obj4, arg4);
        ArgumentNullException.ThrowIfNull(obj5, arg5);
        ArgumentNullException.ThrowIfNull(obj6, arg6);
        ArgumentNullException.ThrowIfNull(obj7, arg7);
        ArgumentNullException.ThrowIfNull(obj8, arg8);
        ArgumentNullException.ThrowIfNull(obj9, arg9);
        ArgumentNullException.ThrowIfNull(obj10, arg10);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> on the first item in the argument list which is <see langword="null"/>.
    /// This does not accurately report the name of the argument which is <see langword="null"/>.
    /// </summary>
    /// <param name="objs">The objects to test.</param>
    public static void ThrowOnNull(params ReadOnlySpan<object> objs)
    {
        for (var i = 0; i < objs.Length; i++)
        {
            var obj = objs[i];
            ArgumentNullException.ThrowIfNull(obj, $"objs[{i}]");
        }
    }
    #endregion

    #region ArgumentException factories
    /// <summary>
    /// Throws an <see cref="ArgumentException"/> or <see cref="ArgumentNullException"/> if the specified sequence is empty or <see langword="null"/> respectively. Note that this will enumerate the sequence.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the sequence.</typeparam>
    /// <param name="sequence">The sequence to test.</param>
    public static void ThrowIfNullOrEmpty<T>(IEnumerable<T> sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        if (!sequence.Any())
        {
            throw new ArgumentException("The sequence is empty.", nameof(sequence));
        }
    }
    #endregion
}
