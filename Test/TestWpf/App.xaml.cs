namespace TestWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        MainWindow = Setup().GetAwaiter().GetResult();
        MainWindow.Visibility = Visibility.Visible;
    }
    private static async Task<MainWindow> Setup()
    {
        var scope = await TestCore.TestCore.GetScope().ConfigureAwait(false);
        var provider = scope.ServiceProvider;
        return new MainWindow(provider);
    }
}
