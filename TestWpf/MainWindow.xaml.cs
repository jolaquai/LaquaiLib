using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Interop;

using LaquaiLib.Util.Hotkeys;

namespace TestWpf;

public partial class MainWindow : Window
{
    private static MainWindow Self { get; set; }
    private IServiceProvider ServiceProvider { get; }

    public MainWindow(IServiceProvider provider)
    {
        ServiceProvider = provider;

        Self = this;

        InitializeComponent();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        Hotkeys.RegisterHotkey(FsModifiers.Control, Keys.NumPad5, () => MessageBox.Show("Ctrl+NumPad5 pressed!"), out var hk5, hwnd);
        Hotkeys.RegisterHotkey(FsModifiers.Control, Keys.NumPad6, () => MessageBox.Show("Ctrl+NumPad6 pressed!"), out var hk6, hwnd);
    }
}
