namespace LaquaiLib.Extensions;

/// <summary>
/// Provides Extension Methods for <see cref="Array"/> Types.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Reinterprets the reference to <paramref name="source"/> as <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>.
    /// This allows using Linq methods on multi-dimensional <see cref="Array"/>s.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="source">The <see cref="Array"/> to transform.</param>
    /// <returns>The reinterpreted reference to <paramref name="source"/>.</returns>
    public static IEnumerable<T> CastEnumerable<T>(this Array source) => System.Runtime.CompilerServices.Unsafe.As<IEnumerable<T>>(source);

    /// <summary>
    /// Attempts to retrieve the element at the specified index from the array if that index is valid for the array.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The array to retrieve the element from.</param>
    /// <param name="i">The index of the element to retrieve.</param>
    /// <param name="value">An <see langword="out"/> variable that receives the element at the specified index if it is valid.</param>
    /// <returns><see langword="true"/> if the index was valid and the element could be retrieved, otherwise <see langword="false"/>.</returns>
    public static bool TryGetAt<T>(this T[] array, int i, out T value)
    {
        if (i < array.Length && i >= 0)
        {
            value = array[i];
            return true;
        }
        value = default;
        return false;
    }
    /// <summary>
    /// Attempts to retrieve the element at specified index from the array if that index is valid for the array, otherwise returns the specified default value.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="array">The array to retrieve the element from.</param>
    /// <param name="i">The index of the element to retrieve.</param>
    /// <param name="defaultValue">The default value to return if the index is invalid.</param>
    /// <returns>The element at the specified index if it is valid, otherwise the specified default value.</returns>
    public static T GetAtOrDefault<T>(this T[] array, int i, T defaultValue = default)
    {
        if (array.GetLowerBound(0) <= i && i <= array.GetUpperBound(0))
        {
            return array[i];
        }
        return defaultValue;
    }

    /// <summary>
    /// Initializes the specified <paramref name="array"/> with the given <paramref name="value"/>.
    /// The assignment is shallow; reference types will be initialized with the same reference.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="value">The value to initialize the array with.</param>
    public static void Initialize<T>(this T[] array, T value)
    {
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }
    /// <summary>
    /// Initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static void Initialize<T>(this T[] array, Func<T> factory)
    {
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = factory();
        }
    }
    /// <summary>
    /// Initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static void Initialize<T>(this T[] array, Func<T, T> factory)
    {
        T last = default;
        for (var i = 0; i < array.Length; i++)
        {
            last = array[i] = factory(last);
        }
    }
    /// <summary>
    /// Initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the index in the array that is being initialized.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static void Initialize<T>(this T[] array, Func<int, T> factory)
    {
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = factory(i);
        }
    }
    /// <summary>
    /// Initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the index in the array that is being initialized and the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static void Initialize<T>(this T[] array, Func<int, T, T> factory)
    {
        T last = default;
        for (var i = 0; i < array.Length; i++)
        {
            last = array[i] = factory(i, last);
        }
    }
    /// <summary>
    /// Asynchronously initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task InitializeAsync<T>(this T[] array, Func<Task<T>> factory)
    {
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = await factory();
        }
    }
    /// <summary>
    /// Asynchronously initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task InitializeAsync<T>(this T[] array, Func<T, Task<T>> factory)
    {
        T last = default;
        for (var i = 0; i < array.Length; i++)
        {
            last = array[i] = await factory(last);
        }
    }
    /// <summary>
    /// Asynchronously initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the index in the array that is being initialized.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task InitializeAsync<T>(this T[] array, Func<int, Task<T>> factory)
    {
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = await factory(i);
        }
    }
    /// <summary>
    /// Asynchronously initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the index in the array that is being initialized and the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task InitializeAsync<T>(this T[] array, Func<int, T, Task<T>> factory)
    {
        T last = default;
        for (var i = 0; i < array.Length; i++)
        {
            last = array[i] = await factory(i, last);
        }
    }
}
