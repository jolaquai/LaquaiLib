using System.Windows.Interop;

using LaquaiLib.Util.WpfForms;
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

    private void MainWindow_Loaded(object sender, RoutedEventArgs e) { }
    protected override nint OnMessageReceived(MSG message, ref bool handled)
    {
        switch (message.message)
        {
            case (int)WindowMessage.WM_KEYDOWN:
            {
                if (message.wParam == (nint)VirtualKey.VK_ESCAPE)
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
