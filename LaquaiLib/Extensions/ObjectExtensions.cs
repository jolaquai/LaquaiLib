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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is <c>null</c>.</exception>
    public static T Cast<T>(this object? obj)
    {
        // TODO: System.InvalidCastException: 'Unable to cast object of type 'System.Int32' to type 'System.Int64'.'
        ArgumentNullException.ThrowIfNull(obj);
        return (T)obj;
    }

    /// <summary>
    /// Safely casts an <see cref="object"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to cast <paramref name="obj"/> to.</typeparam>
    /// <param name="obj">The <see cref="object"/> to cast.</param>
    /// <returns>An instance of <typeparamref name="T"/> that has been produced by casting <paramref name="obj"/> or <c>null</c> if the cast failed.</returns>
    /// <remarks>Contrary to <see cref="Cast{T}(object?)"/>, this method never throws an exception.</remarks>
    public static T? As<T>(this object? obj)
        where T : class
    {
        return obj as T;
    }
}

