using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace TMXWinTerminal.Services
{
    public class JsonFileService
    {
        public T Load<T>(string path, T fallback) where T : class
        {
            if (!File.Exists(path))
            {
                return fallback;
            }

            try
            {
                using (var stream = File.OpenRead(path))
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    return serializer.ReadObject(stream) as T ?? fallback;
                }
            }
            catch
            {
                BackupCorruptedFile(path);
                return fallback;
            }
        }

        public void Save<T>(string path, T value)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = path + ".tmp";
            using (var stream = File.Create(tempPath))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, value);
            }

            if (File.Exists(path))
            {
                File.Copy(tempPath, path, true);
                File.Delete(tempPath);
            }
            else
            {
                File.Move(tempPath, path);
            }
        }

        private static void BackupCorruptedFile(string path)
        {
            try
            {
                var backupPath = path + ".broken-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                File.Copy(path, backupPath, true);
            }
            catch
            {
            }
        }
    }
}
