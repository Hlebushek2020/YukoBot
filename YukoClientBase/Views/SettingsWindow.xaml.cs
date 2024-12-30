using System;
using System.Windows;
using YukoClientBase.ViewModels;

namespace YukoClientBase.Views
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(string title)
        {
            InitializeComponent();
            DataContext = new SettingsViewModel(Close, title);
        }
    }
}