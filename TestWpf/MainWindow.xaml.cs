using System;

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
    }
}
