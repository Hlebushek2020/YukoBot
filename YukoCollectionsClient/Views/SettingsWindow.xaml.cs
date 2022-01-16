using System;
using System.Windows;
using YukoCollectionsClient.ViewModels;

namespace YukoCollectionsClient
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel
            {
                Close = new Action(Close)
            };
        }
    }
}
