using System.Windows;
using YukoClientBase.ViewModels;

namespace YukoClientBase.Views
{
    /// <summary>
    /// Логика взаимодействия для MessageBoxWindow.xaml
    /// </summary>
    internal partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow(MessageBoxViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}