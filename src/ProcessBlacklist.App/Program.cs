namespace ProcessBlacklist.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal static class AppPaths
{
    internal static string SettingsDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProcessBlacklist");
    internal static string SettingsFile => Path.Combine(SettingsDirectory, "settings.json");
}
