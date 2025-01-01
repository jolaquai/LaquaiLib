namespace LaquaiLib.Util.ExceptionManagement;

/// <summary>
/// Contains methods to assert conditions which throw an <see cref="AssertionFailureException{T}"/> if the condition is not met.
/// </summary>
public static class Assert
{
    /// <summary>
    /// Asserts that a specified condition is <see langword="true"/>. If not, an <see cref="AssertionFailureException{T}"/> is thrown.
    /// </summary>
    /// <param name="condition">The condition to assert to be <see langword="true"/>.</param>
    /// <returns>The result of the invocation of <paramref name="condition"/> if it is <see langword="true"/>, otherwise an <see cref="AssertionFailureException{T}"/> is thrown.</returns>
    public static bool That(Func<bool> condition) => condition() ? true : throw new AssertionFailureException<bool>(false);
    /// <summary>
    /// Asserts that a specified condition is <see langword="true"/>. If not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <param name="condition">The condition to assert to be <see langword="true"/>.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails.</param>
    /// <returns>The result of the invocation of <paramref name="condition"/> if it is <see langword="true"/>, otherwise an <see cref="AssertionFailureException{T}"/> is thrown.</returns>
    public static bool That(Func<bool> condition, string message) => condition() ? true : throw new AssertionFailureException<bool>(false, message);
    /// <summary>
    /// Asserts that a specified condition is <see langword="true"/>. If not, an <see cref="AssertionFailureException{T}"/> is thrown.
    /// </summary>
    /// <param name="condition">The condition to assert to be <see langword="true"/>.</param>
    /// <returns>The result of the invocation of <paramref name="condition"/> if it is <see langword="true"/>, otherwise an <see cref="AssertionFailureException{T}"/> is thrown.</returns>
    public static bool That(bool condition) => condition ? true : throw new AssertionFailureException<bool>(false);
    /// <summary>
    /// Asserts that a specified condition is <see langword="true"/>. If not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <param name="condition">The condition to assert to be <see langword="true"/>.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails.</param>
    /// <returns><see langword="true"/> if <paramref name="condition"/> is <see langword="true"/>, otherwise an <see cref="AssertionFailureException{T}"/> is thrown.</returns>
    public static bool That(bool condition, string message) => condition ? true : throw new AssertionFailureException<bool>(false, message);

    /// <summary>
    /// Asserts that a specified value is <see langword="null"/>.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the value to operate on.</typeparam>
    /// <param name="value">The value to assert to be not <see langword="null"/>.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    public static T IsNull<T>(T value, string message = "") where T : class => value is null ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is not <see langword="null"/>.
    /// If it is, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the value to operate on.</typeparam>
    /// <param name="value">The value to assert to be not <see langword="null"/>.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsNotNull<T>(T value, string message = "") where T : class => value ?? throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is greater than another value.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the values to operate on.</typeparam>
    /// <param name="value">The value to assert to be greater than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare <paramref name="value"/> to.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    /// <exception cref="AssertionFailureException{T}">Thrown if <paramref name="value" /> is not greater than <paramref name="other"/>.</exception>
    public static T IsGreaterThan<T>(T value, T other, string message = "") where T : IComparable<T>
    {
        return value.CompareTo(other) > 0
            ? value
            : throw new AssertionFailureException<T>(value, string.IsNullOrWhiteSpace(message)
                ? $"{nameof(IsGreaterThan)} assertion failed."
                : message
            );
    }
    /// <summary>
    /// Asserts that a specified value is greater than or equal to another value.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the values to operate on.</typeparam>
    /// <param name="value">The value to assert to be greater than or equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare <paramref name="value"/> to.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsGreaterThanOrEqualTo<T>(T value, T other, string message = "") where T : IComparable<T> => value.CompareTo(other) >= 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is less than another value.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the values to operate on.</typeparam>
    /// <param name="value">The value to assert to be less than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare <paramref name="value"/> to.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsLessThan<T>(T value, T other, string message = "") where T : IComparable<T> => value.CompareTo(other) < 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is less than or equal to another value.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the values to operate on.</typeparam>
    /// <param name="value">The value to assert to be less than or equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare <paramref name="value"/> to.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsLessThanOrEqualTo<T>(T value, T other, string message = "") where T : IComparable<T> => value.CompareTo(other) <= 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is equal to another value.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the values to operate on.</typeparam>
    /// <param name="value">The value to assert to be equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare <paramref name="value"/> to.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsEqualTo<T>(T value, T other, string message = "") where T : IComparable<T> => value.CompareTo(other) == 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is in a specified range.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the values to operate on.</typeparam>
    /// <param name="value">The value to assert to be greater than or equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare <paramref name="value"/> to.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsNotEqualTo<T>(T value, T other, string message = "") where T : IComparable<T> => value.CompareTo(other) != 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is not equal to another value.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the values to operate on.</typeparam>
    /// <param name="value">The value to assert to be within the range of <paramref name="min"/> and <paramref name="max"/>.</param>
    /// <param name="min">The inclusive lower bound of the range to compare <paramref name="value"/> to.</param>
    /// <param name="max">The inclusive upper bound of the range to compare <paramref name="value"/> to.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsInRange<T>(T value, T min, T max, string message = "") where T : IComparable<T> => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is not in a specified range.
    /// If it is, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the values to operate on.</typeparam>
    /// <param name="value">The value to assert to be outside the range of <paramref name="min"/> and <paramref name="max"/>.</param>
    /// <param name="min">The inclusive lower bound of the range to compare <paramref name="value"/> to.</param>
    /// <param name="max">The inclusive upper bound of the range to compare <paramref name="value"/> to.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsNotInRange<T>(T value, T min, T max, string message = "") where T : IComparable<T> => value.CompareTo(min) < 0 || value.CompareTo(max) > 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is positive.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the value to operate on.</typeparam>
    /// <param name="value">The value to assert to be positive.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsPositive<T>(T value, string message = "") where T : IComparable<T> => value.CompareTo(default) > 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is negative.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the value to operate on.</typeparam>
    /// <param name="value">The value to assert to be negative.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsNegative<T>(T value, string message = "") where T : IComparable<T> => value.CompareTo(default) < 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is zero.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the value to operate on.</typeparam>
    /// <param name="value">The value to assert to be zero.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsZero<T>(T value, string message = "") where T : IComparable<T> => value.CompareTo(default) == 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is not zero.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the value to operate on.</typeparam>
    /// <param name="value">The value to assert to be not equal to zero.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsNonZero<T>(T value, string message = "") where T : IComparable<T> => value.CompareTo(default) != 0 ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is the <see langword="default"/> for its type, i.e. precisely the result of the expression <c><see langword="default"/>(TSelf)</c>.
    /// If it is not, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the value to operate on.</typeparam>
    /// <param name="value">The value to assert to be positive.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsDefault<T>(T value, string message = "") => value.Equals(default) ? value : throw new AssertionFailureException<T>(value, message);
    /// <summary>
    /// Asserts that a specified value is not the <see langword="default"/> for its type, i.e. precisely the result of the expression <c><see langword="default"/>(TSelf)</c>.
    /// If it is, an <see cref="AssertionFailureException{T}"/> is thrown with the specified message.
    /// </summary>
    /// <typeparam name="T">The type of the value to operate on.</typeparam>
    /// <param name="value">The value to assert to be positive.</param>
    /// <param name="message">The message to include in the <see cref="AssertionFailureException{T}"/> if the assertion fails. May be <see langword="null"/> or empty to use the default message.</param>
    /// <returns><paramref name="value"/> if the assertion succeeds, otherwise the method will not return.</returns>
    public static T IsNotDefault<T>(T value, string message = "") => !value.Equals(default) ? value : throw new AssertionFailureException<T>(value, message);
}
