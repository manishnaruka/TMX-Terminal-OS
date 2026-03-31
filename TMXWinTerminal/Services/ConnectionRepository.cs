using System;
using System.Collections.Generic;
using System.Linq;
using TMXWinTerminal.Models;

namespace TMXWinTerminal.Services
{
    public class ConnectionRepository
    {
        private readonly JsonFileService _jsonFileService;
        private readonly PathService _pathService;

        public ConnectionRepository(JsonFileService jsonFileService, PathService pathService)
        {
            _jsonFileService = jsonFileService;
            _pathService = pathService;
        }

        public List<ConnectionEntry> Load()
        {
            var loaded = _jsonFileService.Load(_pathService.ConnectionsFilePath, new List<ConnectionEntry>());
            return loaded
                .Where(entry => entry != null)
                .Select(Normalize)
                .OrderBy(entry => entry.Name)
                .ToList();
        }

        public void Save(IEnumerable<ConnectionEntry> entries)
        {
            var normalized = entries
                .Where(entry => entry != null)
                .Select(Normalize)
                .OrderBy(entry => entry.Name)
                .ToList();

            _jsonFileService.Save(_pathService.ConnectionsFilePath, normalized);
        }

        private static ConnectionEntry Normalize(ConnectionEntry entry)
        {
            var normalized = entry.Clone();
            normalized.Id = string.IsNullOrWhiteSpace(normalized.Id) ? Guid.NewGuid().ToString("N") : normalized.Id.Trim();
            normalized.Name = (normalized.Name ?? string.Empty).Trim();
            normalized.Host = (normalized.Host ?? string.Empty).Trim();
            normalized.Port = (normalized.Port ?? string.Empty).Trim();
            normalized.Username = (normalized.Username ?? string.Empty).Trim();
            normalized.PemFilePath = (normalized.PemFilePath ?? string.Empty).Trim();
            normalized.SshCommand = (normalized.SshCommand ?? string.Empty).Trim();
            normalized.Notes = normalized.Notes ?? string.Empty;
            if (normalized.CreatedAt == default(DateTime))
            {
                normalized.CreatedAt = DateTime.UtcNow;
            }

            if (normalized.UpdatedAt == default(DateTime))
            {
                normalized.UpdatedAt = normalized.CreatedAt;
            }

            return normalized;
        }
    }
}
