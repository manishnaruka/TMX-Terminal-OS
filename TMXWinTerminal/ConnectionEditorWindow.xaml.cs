using System;
using System.Windows;
using TMXWinTerminal.Models;
using TMXWinTerminal.ViewModels;

namespace TMXWinTerminal
{
    public partial class ConnectionEditorWindow : Window
    {
        public ConnectionEditorWindow()
        {
            InitializeComponent();
        }

        public Func<ConnectionEntry, bool> SaveHandler { get; set; }

        private void Window_OnLoaded(object sender, RoutedEventArgs e)
        {
            NameTextBox.Focus();
            NameTextBox.SelectAll();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ConnectionEditorViewModel;
            if (viewModel == null)
            {
                return;
            }

            if (SaveHandler == null || SaveHandler(viewModel.Entry.Clone()))
            {
                DialogResult = true;
            }
        }
    }
}
