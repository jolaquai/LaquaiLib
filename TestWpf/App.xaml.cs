namespace TestWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        MainWindow = Setup().ConfigureAwait(false).GetAwaiter().GetResult();
        MainWindow.Visibility = Visibility.Visible;
    }
    private static async Task<MainWindow> Setup()
    {
        var scope = await TestCore.TestCore.GetScope();
        var provider = scope.ServiceProvider;
        return new MainWindow(provider);
    }
}
