namespace LaquaiLib;

internal static class AppState
{
    public static DirectoryInfo LocalAppData
    {
        get
        {
            var di = field ??= new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LaquaiLib"));
            di.Refresh();
            if (!di.Exists)
                di.Create();
            return di;
        }
    }
}
