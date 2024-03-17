using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YukoClientBase.Models.Progresses;
using YukoClientBase.ViewModels;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoClientBase.Views
{
    /// <summary>
    /// Логика взаимодействия для ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ProgressViewModel _viewModel;

        private volatile bool _isCompleted;

        public ProgressWindow(string title, BaseProgressModel model, bool isCancellable = true)
        {
            InitializeComponent();

            _cancellationTokenSource = new CancellationTokenSource();
            _viewModel = new ProgressViewModel(title, model, isCancellable);
            DataContext = _viewModel;

            Loaded += Window_Loaded;
            Closing += Window_Closing;
            Closed += Window_OnClosed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    _viewModel.Run(Dispatcher, _cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                Dispatcher.Invoke(() =>
                {
                    _isCompleted = true;
                    Close();
                });
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _viewModel.CancellationRequested();
            }

            e.Cancel = !_isCompleted;
        }

        private void Window_OnClosed(object sender, EventArgs e) => _cancellationTokenSource.Dispose();
    }
}