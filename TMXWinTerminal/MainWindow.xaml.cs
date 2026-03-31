using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TMXWinTerminal.Models;
using TMXWinTerminal.Services;
using TMXWinTerminal.ViewModels;

namespace TMXWinTerminal
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly SshCommandBuilder _sshCommandBuilder;

        public MainWindow()
        {
            InitializeComponent();

            var pathService = new PathService();
            var jsonFileService = new JsonFileService();
            _sshCommandBuilder = new SshCommandBuilder();
            var validationService = new ValidationService(_sshCommandBuilder);
            var connectionRepository = new ConnectionRepository(jsonFileService, pathService);
            var settingsRepository = new SettingsRepository(jsonFileService, pathService);
            var terminalLauncher = new TerminalLauncher(pathService, _sshCommandBuilder);

            _viewModel = new MainViewModel(connectionRepository, settingsRepository, validationService, _sshCommandBuilder, terminalLauncher);
            _viewModel.EditEntryRequested += OnEditEntryRequested;
            DataContext = _viewModel;

            Width = _viewModel.WindowWidth;
            Height = _viewModel.WindowHeight;
            if (_viewModel.IsWindowMaximized)
            {
                WindowState = WindowState.Maximized;
            }

            Closing += OnClosing;
        }

        private void EntriesListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.ConnectCommand.CanExecute(null))
            {
                _viewModel.ConnectCommand.Execute(null);
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            _viewModel.SaveWindowState(this);
            _viewModel.PersistState();
        }

        private void OnEditEntryRequested(ConnectionEntry entry, bool isNew)
        {
            var editor = new ConnectionEditorWindow
            {
                Owner = this,
                DataContext = new ConnectionEditorViewModel(entry, _sshCommandBuilder, isNew ? "New SSH Connection" : "Edit SSH Connection"),
                SaveHandler = _viewModel.TrySaveEditedEntry
            };

            editor.ShowDialog();
        }
    }
}
