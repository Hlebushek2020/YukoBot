using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using YukoCollectionsClient.Models.Progress;
using YukoCollectionsClient.ViewModels;

namespace YukoCollectionsClient
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private readonly Base model;

        private bool isCompleted = false;

        public ProgressWindow(Base model)
        {
            InitializeComponent();
            this.model = model;
            DataContext = new ProgressViewModel(this.model);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    model.Run(Dispatcher);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Models.Dialogs.MessageBox.Show(ex.Message, App.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                Dispatcher.Invoke(() =>
                {
                    isCompleted = true;
                    Close();
                });
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !isCompleted;
        }
    }
}