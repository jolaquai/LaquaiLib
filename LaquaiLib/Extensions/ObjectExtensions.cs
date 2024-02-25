using LaquaiLib.Util.ExceptionManagement;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for the <see cref="object"/> Type.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Casts an <see cref="object"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to cast <paramref name="obj"/> to.</typeparam>
    /// <param name="obj">The <see cref="object"/> to cast.</param>
    /// <returns>An instance of <typeparamref name="T"/> that has been produced by casting <paramref name="obj"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is <see langword="null"/>.</exception>
    public static T Cast<T>(this object? obj)
    {
        return Try.First(
            () => (T)obj,
            () => (T)Convert.ChangeType(obj, typeof(T))!
        );
    }

    /// <summary>
    /// Safely casts an <see cref="object"/> to <typeparamref name="T"/> using <c>as</c>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to cast <paramref name="obj"/> to.</typeparam>
    /// <param name="obj">The <see cref="object"/> to cast.</param>
    /// <returns>An instance of <typeparamref name="T"/> that has been produced by casting <paramref name="obj"/> or <see langword="null"/> if the cast failed.</returns>
    /// <remarks>Contrary to <see cref="Cast{T}(object?)"/>, this method never throws an exception.</remarks>
    public static T? As<T>(this object? obj)
        where T : class
    {
        return obj as T;
    }
}

