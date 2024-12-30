using System.Windows;
using YukoClient.ViewModels;

namespace YukoClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainViewModel viewModel = new MainViewModel();
            viewModel.FullscreenEvent += ViewModelOnFullscreenEvent;
            DataContext = viewModel;
        }

        private void ViewModelOnFullscreenEvent()
        {
            if (WindowStyle == WindowStyle.None)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }
    }
}