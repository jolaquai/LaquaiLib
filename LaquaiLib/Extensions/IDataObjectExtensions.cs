using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaquaiLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="IDataObject"/> Type.
/// </summary>
public static class IDataObjectExtensions
{
    /// <summary>
    /// Attempts to retrieve the data with the specified <paramref name="format"/> from this <see cref="IDataObject"/>.
    /// </summary>
    /// <param name="dataObject">The <see cref="IDataObject"/> to retrieve the data from.</param>
    /// <param name="format">The format of the data to retrieve.</param>
    /// <param name="data">A <c>ref <see cref="object"/>?</c> variable that receives the data if the retrieval was successful.</param>
    /// <returns><c>true</c> if the retrieval was successful, otherwise <c>false</c>.</returns>
    public static bool TryGetData(this IDataObject dataObject, string format, [NotNullWhen(true)] out object data)
    {
        if (dataObject.GetDataPresent(format))
        {
            data = dataObject.GetData(format)!;
            return true;
        }
        else
        {
            data = default;
            return false;
        }
    }

    /// <summary>
    /// Attempts to retrieve the data with the specified <paramref name="format"/> from this <see cref="IDataObject"/>.
    /// </summary>
    /// <typeparam name="T">The actual type of which the data is expected to be.</typeparam>
    /// <param name="dataObject">The <see cref="IDataObject"/> to retrieve the data from.</param>
    /// <param name="format">The format of the data to retrieve.</param>
    /// <param name="data">A <c>ref <typeparamref name="T"/>?</c> variable that receives the data if the retrieval was successful.</param>
    /// <returns><c>true</c> if the retrieval was successful, otherwise <c>false</c>.</returns>
    public static bool TryGetData<T>(this IDataObject dataObject, string format, [NotNullWhen(true)] out T? data)
    {
        if (dataObject.TryGetData(format, out var _data))
        {
            data = (T)_data;
            return true;
        }
        else
        {
            data = default;
            return false;
        }
    }
}
