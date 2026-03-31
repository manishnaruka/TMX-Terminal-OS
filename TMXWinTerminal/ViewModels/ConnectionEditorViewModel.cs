using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using TMXWinTerminal.Helpers;
using TMXWinTerminal.Models;
using TMXWinTerminal.Services;

namespace TMXWinTerminal.ViewModels
{
    public class ConnectionEditorViewModel : ViewModelBase
    {
        private readonly SshCommandBuilder _sshCommandBuilder;
        private string _statusMessage;

        public ConnectionEditorViewModel(ConnectionEntry entry, SshCommandBuilder sshCommandBuilder, string dialogTitle)
        {
            Entry = entry ?? ConnectionEntry.CreateEmpty();
            _sshCommandBuilder = sshCommandBuilder ?? throw new ArgumentNullException(nameof(sshCommandBuilder));
            DialogTitle = string.IsNullOrWhiteSpace(dialogTitle) ? "SSH Connection" : dialogTitle;

            Entry.PropertyChanged += EntryOnPropertyChanged;

            BrowsePemCommand = new RelayCommand(BrowsePemFile);
            GenerateCommand = new RelayCommand(GenerateSshCommand);
            CopyCommand = new RelayCommand(CopyResolvedCommand);
            OpenPemFolderCommand = new RelayCommand(OpenPemFolder, () => !string.IsNullOrWhiteSpace(Entry.PemFilePath));

            StatusMessage = "Edit the SSH details, then save the connection.";
        }

        public ConnectionEntry Entry { get; }

        public string DialogTitle { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string ResolvedCommandPreview => _sshCommandBuilder.Build(Entry);

        public ICommand BrowsePemCommand { get; }

        public ICommand GenerateCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand OpenPemFolderCommand { get; }

        private void EntryOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ResolvedCommandPreview));
            CommandManager.InvalidateRequerySuggested();
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
                Entry.PemFilePath = dialog.FileName;
                StatusMessage = "Selected PEM/private key path.";
            }
        }

        private void GenerateSshCommand()
        {
            if (!string.IsNullOrWhiteSpace(Entry.Port))
            {
                int parsedPort;
                if (!int.TryParse(Entry.Port.Trim(), out parsedPort) || parsedPort < 1 || parsedPort > 65535)
                {
                    MessageBox.Show("Port must be a number between 1 and 65535.", "Cannot Generate Command", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(Entry.Host) || string.IsNullOrWhiteSpace(Entry.Username))
            {
                MessageBox.Show("Host and Username are required to generate a command.", "Cannot Generate Command", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Entry.SshCommand = _sshCommandBuilder.Build(new ConnectionEntry
            {
                Host = Entry.Host,
                Port = Entry.Port,
                Username = Entry.Username,
                PemFilePath = Entry.PemFilePath,
                SshCommand = string.Empty
            });
            StatusMessage = "SSH command generated from the entry details.";
        }

        private void CopyResolvedCommand()
        {
            var command = _sshCommandBuilder.Build(Entry);
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
            if (string.IsNullOrWhiteSpace(Entry.PemFilePath))
            {
                return;
            }

            var path = Entry.PemFilePath.Trim();
            if (!System.IO.File.Exists(path))
            {
                MessageBox.Show("The PEM/private key path does not exist.", "File Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Process.Start("explorer.exe", "/select,\"" + path + "\"");
            StatusMessage = "Opened the key file location in Explorer.";
        }
    }
}
