using TMXWinTerminal.Models;

namespace TMXWinTerminal.Services
{
    public class SettingsRepository
    {
        private readonly JsonFileService _jsonFileService;
        private readonly PathService _pathService;

        public SettingsRepository(JsonFileService jsonFileService, PathService pathService)
        {
            _jsonFileService = jsonFileService;
            _pathService = pathService;
        }

        public AppSettings Load()
        {
            return _jsonFileService.Load(_pathService.SettingsFilePath, new AppSettings()) ?? new AppSettings();
        }

        public void Save(AppSettings settings)
        {
            _jsonFileService.Save(_pathService.SettingsFilePath, settings ?? new AppSettings());
        }
    }
}
