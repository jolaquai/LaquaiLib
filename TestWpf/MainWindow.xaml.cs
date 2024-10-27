using System;
using System.Windows.Interop;

using LaquaiLib.Windows;

namespace TestWpf;

public partial class MainWindow : LowLevelWindow
{
    private static MainWindow Self { get; set; }
    private IServiceProvider ServiceProvider { get; }

    public MainWindow(IServiceProvider provider)
    {
        ServiceProvider = provider;

        Self = this;

        InitializeComponent();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e) { }
    public override nint OnMessageReceived(MSG message, ref bool handled)
    {
        switch (message.message)
        {
            case 0x0100: // WM_KEYDOWN
            {
                if (message.wParam == 0x1B) // VK_ESCAPE
                {
                    Close();
                    handled = true;
                    return 0;
                }
                break;
            }
        }

        return base.OnMessageReceived(message, ref handled);
    }
}
