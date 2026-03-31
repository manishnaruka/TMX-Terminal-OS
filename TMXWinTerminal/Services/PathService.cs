using System;
using System.IO;

namespace TMXWinTerminal.Services
{
    public class PathService
    {
        private readonly string _baseDirectory;

        public PathService()
        {
            _baseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TMXWinTerminal");
        }

        public string EnsureAppDataDirectory()
        {
            Directory.CreateDirectory(_baseDirectory);
            return _baseDirectory;
        }

        public string ConnectionsFilePath => Path.Combine(EnsureAppDataDirectory(), "connections.json");

        public string SettingsFilePath => Path.Combine(EnsureAppDataDirectory(), "settings.json");

        public string ScriptCacheDirectory
        {
            get
            {
                var path = Path.Combine(EnsureAppDataDirectory(), "scripts");
                Directory.CreateDirectory(path);
                return path;
            }
        }
    }
}
