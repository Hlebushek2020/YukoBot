using System;
using System.Windows;
using YukoCollectionsClient.ViewModels;

namespace YukoCollectionsClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel
            {
                Close = new Action(Close)
            };
        }
    }
}