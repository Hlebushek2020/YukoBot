using System;
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
            MainViewModel mvm = new MainViewModel
            {
                Close = new Action(Close)
            };
            DataContext = mvm;
        }
    }
}
