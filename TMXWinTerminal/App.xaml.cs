using System.Windows;
using TMXWinTerminal.Helpers;
using TMXWinTerminal.Services;

namespace TMXWinTerminal
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var pathService = new PathService();
            var jsonFileService = new JsonFileService();
            var settings = jsonFileService.Load(pathService.SettingsFilePath, new Models.AppSettings()) ?? new Models.AppSettings();
            ThemeManager.ApplyTheme(settings.ThemeMode);
        }
    }
}
