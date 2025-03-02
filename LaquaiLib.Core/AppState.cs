namespace LaquaiLib.Core;

internal static class AppState
{
    public static DirectoryInfo AppData
    {
        get
        {
            field ??= new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LaquaiLib"));
            if (!field.Exists)
            {
                field.Create();
            }
            return field;
        }
    }
}
