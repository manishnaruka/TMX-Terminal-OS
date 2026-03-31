using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TMXWinTerminal.Helpers;
using TMXWinTerminal.Models;
using TMXWinTerminal.Services;

namespace TMXWinTerminal.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ConnectionRepository _connectionRepository;
        private readonly SettingsRepository _settingsRepository;
        private readonly ValidationService _validationService;
        private readonly SshCommandBuilder _sshCommandBuilder;
        private readonly TerminalLauncher _terminalLauncher;
        private readonly AppSettings _settings;
        private ConnectionEntry _selectedEntry;
        private ConnectionEntry _currentEntry;
        private string _searchText;
        private string _statusMessage;

        public MainViewModel(
            ConnectionRepository connectionRepository,
            SettingsRepository settingsRepository,
            ValidationService validationService,
            SshCommandBuilder sshCommandBuilder,
            TerminalLauncher terminalLauncher)
        {
            _connectionRepository = connectionRepository;
            _settingsRepository = settingsRepository;
            _validationService = validationService;
            _sshCommandBuilder = sshCommandBuilder;
            _terminalLauncher = terminalLauncher;
            _settings = _settingsRepository.Load();

            Entries = new ObservableCollection<ConnectionEntry>(_connectionRepository.Load());
            EntriesView = CollectionViewSource.GetDefaultView(Entries);
            EntriesView.SortDescriptions.Add(new SortDescription(nameof(ConnectionEntry.Name), ListSortDirection.Ascending));
            EntriesView.Filter = FilterEntry;

            TerminalPreferences = Enum.GetValues(typeof(TerminalPreference)).Cast<TerminalPreference>().ToList();

            NewCommand = new RelayCommand(CreateNewEntry);
            SaveCommand = new RelayCommand(SaveCurrentEntry, () => CurrentEntry != null);
            DeleteCommand = new RelayCommand(DeleteSelectedEntry, () => SelectedEntry != null);
            DuplicateCommand = new RelayCommand(DuplicateSelectedEntry, () => SelectedEntry != null);
            ConnectCommand = new RelayCommand(ConnectCurrentEntry, () => CurrentEntry != null);
            BrowsePemCommand = new RelayCommand(BrowsePemFile, () => CurrentEntry != null);
            GenerateCommand = new RelayCommand(GenerateSshCommand, () => CurrentEntry != null);
            CopyCommand = new RelayCommand(CopyResolvedCommand, () => CurrentEntry != null);
            OpenPemFolderCommand = new RelayCommand(OpenPemFolder, () => CurrentEntry != null && !string.IsNullOrWhiteSpace(CurrentEntry.PemFilePath));

            if (Entries.Count > 0)
            {
                var preferred = Entries.FirstOrDefault(entry => entry.Id == _settings.LastSelectedEntryId) ?? Entries[0];
                SelectedEntry = preferred;
            }
            else
            {
                CreateNewEntry();
            }

            StatusMessage = "Ready.";
        }

        public ObservableCollection<ConnectionEntry> Entries { get; }

        public ICollectionView EntriesView { get; }

        public IList<TerminalPreference> TerminalPreferences { get; }

        public ICommand NewCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand DuplicateCommand { get; }

        public ICommand ConnectCommand { get; }

        public ICommand BrowsePemCommand { get; }

        public ICommand GenerateCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand OpenPemFolderCommand { get; }

        public ConnectionEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (!SetProperty(ref _selectedEntry, value))
                {
                    return;
                }

                _settings.LastSelectedEntryId = value?.Id ?? string.Empty;
                LoadCurrentEntryFromSelection();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ConnectionEntry CurrentEntry
        {
            get => _currentEntry;
            private set
            {
                if (_currentEntry != null)
                {
                    _currentEntry.PropertyChanged -= CurrentEntryOnPropertyChanged;
                }

                if (SetProperty(ref _currentEntry, value))
                {
                    if (_currentEntry != null)
                    {
                        _currentEntry.PropertyChanged += CurrentEntryOnPropertyChanged;
                    }

                    OnPropertyChanged(nameof(ResolvedCommandPreview));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    EntriesView.Refresh();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public TerminalPreference SelectedTerminalPreference
        {
            get => _settings.TerminalPreference;
            set
            {
                if (_settings.TerminalPreference != value)
                {
                    _settings.TerminalPreference = value;
                    OnPropertyChanged();
                    PersistSettings();
                    StatusMessage = value == TerminalPreference.Auto ? "Terminal preference set to Auto." : "Terminal preference saved.";
                }
            }
        }

        public string ResolvedCommandPreview => _sshCommandBuilder.Build(CurrentEntry);

        public double WindowWidth => _settings.WindowWidth;

        public double WindowHeight => _settings.WindowHeight;

        public bool IsWindowMaximized => _settings.IsWindowMaximized;

        public void SaveWindowState(Window window)
        {
            if (window == null)
            {
                return;
            }

            _settings.IsWindowMaximized = window.WindowState == WindowState.Maximized;
            if (window.WindowState == WindowState.Normal)
            {
                _settings.WindowWidth = window.Width;
                _settings.WindowHeight = window.Height;
            }

            PersistSettings();
        }

        public void PersistState()
        {
            PersistConnections();
            PersistSettings();
        }

        private void CurrentEntryOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ResolvedCommandPreview));
            CommandManager.InvalidateRequerySuggested();
        }

        private bool FilterEntry(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return true;
            }

            var entry = item as ConnectionEntry;
            if (entry == null)
            {
                return false;
            }

            var term = SearchText.Trim();
            return Contains(entry.Name, term) ||
                   Contains(entry.Host, term) ||
                   Contains(entry.Username, term) ||
                   Contains(entry.Notes, term) ||
                   Contains(entry.SshCommand, term);
        }

        private static bool Contains(string source, string term)
        {
            return !string.IsNullOrWhiteSpace(source) && source.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void LoadCurrentEntryFromSelection()
        {
            CurrentEntry = SelectedEntry != null ? SelectedEntry.Clone() : ConnectionEntry.CreateEmpty();
        }

        private void CreateNewEntry()
        {
            SelectedEntry = null;
            CurrentEntry = ConnectionEntry.CreateEmpty();
            StatusMessage = "Creating a new connection entry.";
        }

        private void SaveCurrentEntry()
        {
            var outcome = _validationService.ValidateForSave(CurrentEntry);
            if (outcome.HasErrors)
            {
                MessageBox.Show(string.Join(Environment.NewLine, outcome.Errors), "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusMessage = "Fix validation errors before saving.";
                return;
            }

            if (outcome.Warnings.Count > 0)
            {
                StatusMessage = string.Join(" ", outcome.Warnings);
            }

            var now = DateTime.UtcNow;
            ConnectionEntry target;
            if (SelectedEntry != null && !string.IsNullOrWhiteSpace(SelectedEntry.Id) && SelectedEntry.Id == CurrentEntry.Id)
            {
                target = SelectedEntry;
                var createdAt = SelectedEntry.CreatedAt == default(DateTime) ? now : SelectedEntry.CreatedAt;
                CurrentEntry.CreatedAt = createdAt;
                CurrentEntry.UpdatedAt = now;
                target.CopyFrom(CurrentEntry.Clone());
            }
            else
            {
                target = CurrentEntry.Clone();
                target.Id = string.IsNullOrWhiteSpace(target.Id) ? Guid.NewGuid().ToString("N") : target.Id;
                target.CreatedAt = target.CreatedAt == default(DateTime) ? now : target.CreatedAt;
                target.UpdatedAt = now;
                Entries.Add(target);
            }

            PersistConnections();
            SelectedEntry = target;
            EntriesView.Refresh();
            StatusMessage = "Saved connection entry.";
        }

        private void DeleteSelectedEntry()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            var result = MessageBox.Show("Delete '" + SelectedEntry.Name + "'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var index = Entries.IndexOf(SelectedEntry);
            Entries.Remove(SelectedEntry);
            PersistConnections();

            if (Entries.Count == 0)
            {
                CreateNewEntry();
            }
            else
            {
                SelectedEntry = Entries[Math.Max(0, index - 1)];
            }

            StatusMessage = "Connection entry deleted.";
        }

        private void DuplicateSelectedEntry()
        {
            if (SelectedEntry == null)
            {
                return;
            }

            var duplicate = SelectedEntry.Clone();
            duplicate.Id = Guid.NewGuid().ToString("N");
            duplicate.Name = string.IsNullOrWhiteSpace(duplicate.Name) ? "Copy" : duplicate.Name + " Copy";
            duplicate.CreatedAt = DateTime.UtcNow;
            duplicate.UpdatedAt = duplicate.CreatedAt;
            Entries.Add(duplicate);
            PersistConnections();
            SelectedEntry = duplicate;
            StatusMessage = "Connection duplicated.";
        }

        private void ConnectCurrentEntry()
        {
            var outcome = _validationService.ValidateForConnect(CurrentEntry);
            if (outcome.HasErrors)
            {
                MessageBox.Show(string.Join(Environment.NewLine, outcome.Errors), "Cannot Connect", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusMessage = "Connection blocked by validation.";
                return;
            }

            if (outcome.Warnings.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, outcome.Warnings), "Connection Warning", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            var launchResult = _terminalLauncher.Launch(CurrentEntry, SelectedTerminalPreference);
            if (!launchResult.Success)
            {
                MessageBox.Show(launchResult.Message, "Launch Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Terminal launch failed.";
                return;
            }

            StatusMessage = launchResult.Message;
        }

        private void BrowsePemFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select PEM or private key file",
                Filter = "PEM and key files|*.pem;*.ppk;*.key;*id_*|All files|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                CurrentEntry.PemFilePath = dialog.FileName;
                StatusMessage = "Selected PEM/private key path.";
            }
        }

        private void GenerateSshCommand()
        {
            var validation = _validationService.ValidateForSave(CurrentEntry);
            if (validation.Errors.Any(error => error.StartsWith("Port", StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(string.Join(Environment.NewLine, validation.Errors), "Cannot Generate Command", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentEntry.Host) || string.IsNullOrWhiteSpace(CurrentEntry.Username))
            {
                MessageBox.Show("Host and Username are required to generate a command.", "Cannot Generate Command", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CurrentEntry.SshCommand = _sshCommandBuilder.Build(new ConnectionEntry
            {
                Host = CurrentEntry.Host,
                Port = CurrentEntry.Port,
                Username = CurrentEntry.Username,
                PemFilePath = CurrentEntry.PemFilePath,
                SshCommand = string.Empty
            });
            StatusMessage = "SSH command generated from the entry details.";
        }

        private void CopyResolvedCommand()
        {
            var command = _sshCommandBuilder.Build(CurrentEntry);
            if (string.IsNullOrWhiteSpace(command))
            {
                MessageBox.Show("There is no SSH command to copy yet.", "Nothing to Copy", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Clipboard.SetText(command);
            StatusMessage = "SSH command copied to the clipboard.";
        }

        private void OpenPemFolder()
        {
            if (CurrentEntry == null || string.IsNullOrWhiteSpace(CurrentEntry.PemFilePath))
            {
                return;
            }

            var path = CurrentEntry.PemFilePath.Trim();
            if (!System.IO.File.Exists(path))
            {
                MessageBox.Show("The PEM/private key path does not exist.", "File Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Process.Start("explorer.exe", "/select,\"" + path + "\"");
            StatusMessage = "Opened the key file location in Explorer.";
        }

        private void PersistConnections()
        {
            _connectionRepository.Save(Entries);
        }

        private void PersistSettings()
        {
            _settingsRepository.Save(_settings);
        }
    }
}


