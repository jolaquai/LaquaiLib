using System.Globalization;
using System.Windows.Data;

namespace LaquaiLib.WPF.ValueConverters;

/// <summary>
/// Represents an <see cref="IValueConverter"/> implementation that returns a <see cref="bool"/> value depending on whether the given input is null.
/// </summary>
public partial class NotNullToBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts a given <paramref name="value"/> to a <see cref="bool"/> value depending on whether is is <see langword="null"/>.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <param name="targetType">The <see cref="Type"/> that the receiving context expects the output of this method to be of.</param>
    /// <param name="parameter">An additional parameter passed by the calling context. May be null.</param>
    /// <param name="culture">Which culture to use when converting / formatting the value.</param>
    /// <returns><see langword="true"/> if the given input <paramref name="value"/> is not null, otherwise <see langword="false"/>.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is not null;

    /// <summary>
    /// [Unsupported] Converts a previously converted <paramref name="value"/> back to its original value. This method unconditionally throws a <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <param name="targetType">The <see cref="Type"/> that the receiving context expects the output of this method to be of.</param>
    /// <param name="parameter">An additional parameter passed by the calling context. May be null.</param>
    /// <param name="culture">Which culture to use when converting / formatting the value.</param>
    /// <returns><see langword="true"/> if the given input <paramref name="value"/> is not null, otherwise <see langword="false"/>.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
