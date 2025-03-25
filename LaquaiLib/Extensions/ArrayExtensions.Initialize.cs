namespace LaquaiLib.Extensions;

public partial class ArrayExtensions
{
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
    /// Initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the index of the slot in the array that is being initialized.
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
    /// Initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the index of the slot in the array that is being initialized and the previous iteration's value.
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
            array[i] = await factory().ConfigureAwait(false);
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
            last = array[i] = await factory(last).ConfigureAwait(false);
        }
    }
    /// <summary>
    /// Asynchronously initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the index of the slot in the array that is being initialized.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task InitializeAsync<T>(this T[] array, Func<int, Task<T>> factory)
    {
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = await factory(i).ConfigureAwait(false);
        }
    }
    /// <summary>
    /// Asynchronously initializes the specified <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the index of the slot in the array that is being initialized and the previous iteration's value.
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
            last = array[i] = await factory(i, last).ConfigureAwait(false);
        }
    }

    // I'd have loved to use stackalloc memory for the indices when we don't need to expose them to a consumer's Func<int[]...,
    // but since Array.SetValue wants arbitrary indices as an array anyway, this is the best we can do to minimize allocations
    // Unfortunately, each of these overloads is incompatible, so giving each their own local function is the best we can do
    /// <summary>
    /// Initializes the specified multidimensional <paramref name="array"/> with the given <paramref name="value"/>.
    /// The assignment is shallow; reference types will be initialized with the same reference.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="value">The value to initialize the array with.</param>
    public static void Initialize<T>(this Array array, T value)
    {
        ArgumentNullException.ThrowIfNull(array);
        var indices = new int[array.Rank];
        InitializeRecursive(0);

        void InitializeRecursive(int dimension)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);

            for (var i = lower; i <= upper; i++)
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                {
                    array.SetValue(value, indices);
                }
                else
                {
                    InitializeRecursive(dimension + 1);
                }
            }
        }
    }
    /// <summary>
    /// Initializes the specified multidimensional <paramref name="array"/> using the given <paramref name="factory"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static void Initialize<T>(this Array array, Func<T> factory)
    {
        ArgumentNullException.ThrowIfNull(array);
        var indices = new int[array.Rank];
        InitializeRecursive(0);

        void InitializeRecursive(int dimension)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);
            for (var i = lower; i <= upper; i++)
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                {
                    array.SetValue(factory(), indices);
                }
                else
                {
                    InitializeRecursive(dimension + 1);
                }
            }
        }
    }
    /// <summary>
    /// Initializes the specified multidimensional <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static void Initialize<T>(this Array array, Func<T, T> factory)
    {
        ArgumentNullException.ThrowIfNull(array);
        var indices = new int[array.Rank];
        T last = default;
        InitializeRecursive(0);

        void InitializeRecursive(int dimension)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);
            for (var i = lower; i <= upper; i++)
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                {
                    last = factory(last);
                    array.SetValue(last, indices);
                }
                else
                {
                    InitializeRecursive(dimension + 1);
                }
            }
        }
    }
    /// <summary>
    /// Initializes the specified multidimensional <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the indices of the slot in the array that is being initialized.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static void Initialize<T>(this Array array, Func<ReadOnlyMemory<int>, T> factory)
    {
        ArgumentNullException.ThrowIfNull(array);
        var indices = new int[array.Rank];
        InitializeRecursive(0);

        void InitializeRecursive(int dimension)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);
            for (var i = lower; i <= upper; i++)
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                {
                    array.SetValue(factory(indices), indices);
                }
                else
                {
                    InitializeRecursive(dimension + 1);
                }
            }
        }
    }
    /// <summary>
    /// Initializes the specified multidimensional <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the indices of the slot in the array that is being initialized and the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static void Initialize<T>(this Array array, Func<ReadOnlyMemory<int>, T, T> factory)
    {
        ArgumentNullException.ThrowIfNull(array);
        var indices = new int[array.Rank];
        T last = default;
        InitializeRecursive(0);

        void InitializeRecursive(int dimension)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);
            for (var i = lower; i <= upper; i++)
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                {
                    last = factory(indices, last);
                    array.SetValue(last, indices);
                }
                else
                {
                    InitializeRecursive(dimension + 1);
                }
            }
        }
    }
    /// <summary>
    /// Initializes the specified multidimensional <paramref name="array"/> using the given <paramref name="factory"/>.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static async Task InitializeAsync<T>(this Array array, Func<Task<T>> factory)
    {
        ArgumentNullException.ThrowIfNull(array);
        var indices = new int[array.Rank];
        await InitializeRecursive(0).ConfigureAwait(false);

        async ValueTask InitializeRecursive(int dimension)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);
            for (var i = lower; i <= upper; i++)
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                {
                    array.SetValue(await factory().ConfigureAwait(false), indices);
                }
                else
                {
                    await InitializeRecursive(dimension + 1).ConfigureAwait(false);
                }
            }
        }
    }
    /// <summary>
    /// Initializes the specified multidimensional <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static async Task InitializeAsync<T>(this Array array, Func<T, Task<T>> factory)
    {
        ArgumentNullException.ThrowIfNull(array);
        var indices = new int[array.Rank];
        T last = default;
        await InitializeRecursive(0).ConfigureAwait(false);

        async ValueTask InitializeRecursive(int dimension)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);
            for (var i = lower; i <= upper; i++)
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                {
                    last = await factory(last).ConfigureAwait(false);
                    array.SetValue(last, indices);
                }
                else
                {
                    await InitializeRecursive(dimension + 1).ConfigureAwait(false);
                }
            }
        }
    }
    /// <summary>
    /// Initializes the specified multidimensional <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the indices of the slot in the array that is being initialized.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static async Task InitializeAsync<T>(this Array array, Func<ReadOnlyMemory<int>, Task<T>> factory)
    {
        ArgumentNullException.ThrowIfNull(array);
        var indices = new int[array.Rank];
        await InitializeRecursive(0).ConfigureAwait(false);

        async ValueTask InitializeRecursive(int dimension)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);
            for (var i = lower; i <= upper; i++)
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                {
                    array.SetValue(await factory(indices).ConfigureAwait(false), indices);
                }
                else
                {
                    await InitializeRecursive(dimension + 1).ConfigureAwait(false);
                }
            }
        }
    }
    /// <summary>
    /// Initializes the specified multidimensional <paramref name="array"/> using the given <paramref name="factory"/>. It is passed the indices of the slot in the array that is being initialized and the previous iteration's value.
    /// </summary>
    /// <typeparam name="T">The Type of the items in the array.</typeparam>
    /// <param name="array">The <see cref="Array"/> of <typeparamref name="T"/> to initialize.</param>
    /// <param name="factory">The factory method that produces the values to initialize the array with.</param>
    public static async Task InitializeAsync<T>(this Array array, Func<ReadOnlyMemory<int>, T, Task<T>> factory)
    {
        ArgumentNullException.ThrowIfNull(array);
        var indices = new int[array.Rank];
        T last = default;
        await InitializeRecursive(0).ConfigureAwait(false);

        async ValueTask InitializeRecursive(int dimension)
        {
            var lower = array.GetLowerBound(dimension);
            var upper = array.GetUpperBound(dimension);
            for (var i = lower; i <= upper; i++)
            {
                indices[dimension] = i;
                if (dimension == array.Rank - 1)
                {
                    last = await factory(indices, last).ConfigureAwait(false);
                    array.SetValue(last, indices);
                }
                else
                {
                    await InitializeRecursive(dimension + 1).ConfigureAwait(false);
                }
            }
        }
    }
}
