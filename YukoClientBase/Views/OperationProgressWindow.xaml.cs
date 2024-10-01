using System;
using System.ComponentModel;
using System.Windows;
using YukoClientBase.ViewModels;
using MessageBox = YukoClientBase.Dialogs.MessageBox;

namespace YukoClientBase.Views
{
    /// <summary>
    /// Логика взаимодействия для ProgressWindow.xaml
    /// </summary>
    public partial class OperationProgressWindow : Window
    {
        private readonly OperationProgressViewModel _viewModel;

        private bool _isCompleted = false;

        public OperationProgressWindow(OperationProgressViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = _viewModel;

            Closing += OnClosing;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.Operation();
            }
            catch (OperationCanceledException) { }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _isCompleted = true;

            Close();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_isCompleted)
                return;

            if (_viewModel.IsCancellable && _viewModel.WindowClosingConfirmation() == MessageBoxResult.Yes)
                _viewModel.CancelCommand.Execute();

            e.Cancel = true;
        }
    }
}