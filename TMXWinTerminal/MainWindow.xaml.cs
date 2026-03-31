using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TMXWinTerminal.ViewModels;

namespace TMXWinTerminal
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var pathService = new Services.PathService();
            var jsonFileService = new Services.JsonFileService();
            var sshCommandBuilder = new Services.SshCommandBuilder();
            var validationService = new Services.ValidationService(sshCommandBuilder);
            var connectionRepository = new Services.ConnectionRepository(jsonFileService, pathService);
            var settingsRepository = new Services.SettingsRepository(jsonFileService, pathService);
            var terminalLauncher = new Services.TerminalLauncher(pathService, sshCommandBuilder);

            _viewModel = new MainViewModel(connectionRepository, settingsRepository, validationService, sshCommandBuilder, terminalLauncher);
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
    }
}
