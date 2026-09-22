using System.Globalization;
using System.Windows.Data;

namespace LaquaiLib.Windows.ValueConverters;

/// <summary>
/// Represents an <see cref="IValueConverter"/> implementation that inverts a <see cref="bool"/> value.ss
/// </summary>
public partial class BoolInverterConverter : IValueConverter
{
    /// <summary>
    /// Converts a <see cref="bool"/> value to its inverse.
    /// </summary>
    /// <param name="value">The input bool value.</param>
    /// <param name="targetType">The <see cref="Type"/> that the receiving context expects the output of this method to be of.</param>
    /// <param name="parameter">An additional parameter passed by the calling context. May be null.</param>
    /// <param name="culture">Which culture to use when converting / formatting the value.</param>
    /// <returns>The inverse of the input <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the input <paramref name="value"/> is not a <see cref="bool"/>.</exception>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b
            ? (object)!b
            : throw new ArgumentException($"Input value was expected to be a '{typeof(bool).FullName}' value, received '{value?.GetType().FullName ?? "null"}' instead.", nameof(value));

    /// <summary>
    /// Converts an "inverse" <see cref="bool"/> value to its original value.
    /// </summary>
    /// <param name="value">The input bool value.</param>
    /// <param name="targetType">The <see cref="Type"/> that the receiving context expects the output of this method to be of.</param>
    /// <param name="parameter">An additional parameter passed by the calling context. May be null.</param>
    /// <param name="culture">Which culture to use when converting / formatting the value.</param>
    /// <returns>The inverse of the input <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the input <paramref name="value"/> is not a <see cref="bool"/>.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value, targetType, parameter, culture);
}
