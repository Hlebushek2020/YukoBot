using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YukoClientBase.Models.Progresses;
using YukoCollectionsClient.Models.Progress;
using YukoCollectionsClient.ViewModels;
using SUI = Sergey.UI.Extension;

namespace YukoCollectionsClient
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly bool _isCancelled;
        private readonly BaseProgressModel _model;

        private bool _isCompleted = false;

        public ProgressWindow(BaseProgressModel model, bool isCancelled = false)
        {
            InitializeComponent();
            Loaded += Window_Loaded;
            Closing += Window_Closing;
            Closed += Window_OnClosed;
            _model = model;
            _isCancelled = isCancelled;
            DataContext = new ProgressViewModel(_model);
            button_cancel.Visibility = isCancelled ? Visibility.Visible : Visibility.Collapsed;
            button_cancel.Click += (sender, args) => Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    _model.Run(Dispatcher, _cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        SUI.Dialogs.MessageBox.Show(ex.Message, App.Name, MessageBoxButton.OK,
                            MessageBoxImage.Error);
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
            if (!_cancellationTokenSource.IsCancellationRequested && _isCancelled)
            {
                _cancellationTokenSource.Cancel();
                _model.IsIndeterminate = true;
                _model.State = "Отмена";
            }
            e.Cancel = !_isCompleted;
        }

        private void Window_OnClosed(object sender, EventArgs e) =>
            _cancellationTokenSource.Dispose();
    }
}