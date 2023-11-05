using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LaquaiLib.WPF;

/// <summary>
/// Serves as a template for a window that may be used to get data from the user.
/// </summary>
public partial class AsyncGetterWindow : Window
{
    protected readonly ManualResetEventSlim _mres = new ManualResetEventSlim();

    private AsyncGetterWindow()
    {
    }

    /// <summary>
    /// Transitions the window into a state where it is ready to respond to user actions and return a value.
    /// Calling code should call this method until it returns a non-null value OR doing so raises a <see cref="TaskCanceledException"/> (which happens when the user has closed the window without it being marked as ready to submit a value).
    /// </summary>
    /// <typeparam name="T">The type of the value to be returned.</typeparam>
    /// <param name="state">Optional parameter(s) to be used by the window.</param>
    protected async virtual Task<T?> GetValue<T>(object? state)
    {
        using var cts = new CancellationTokenSource();
        Closed += (_, _) => cts.Cancel();

        Show();
        Activate();

        // Initializing actions here...

        while (true)
        {
            if (_mres.IsSet)
            {
                // Instead of default, return the actual value, e.g. the contents of a TextBox
                return default;
            }
            if (cts.IsCancellationRequested)
            {
                // You should return null or some other value that indicates to your receiving code that the user canceled the submission
                return default;
            }

            await Task.Delay(10);
        }
    }

    /// <summary>
    /// Template: Marks the window as ready to return a value, which is then returned by <see cref="GetValue{T}(object?)"/>.
    /// </summary>
    private void Button_Click()
    {
        // Any other actions...

        _mres.Set();
    }
}
