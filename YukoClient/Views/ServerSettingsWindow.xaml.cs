using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using YukoClient.Models;
using YukoClient.ViewModels;

namespace YukoClient
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ServerSettingsWindow : Window
    {
        private readonly ServerSettingsViewModel viewModel;

        public ServerSettingsWindow(Server server)
        {
            InitializeComponent();
            viewModel = new ServerSettingsViewModel(server);
            DataContext = viewModel;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            viewModel.SelectedChannels = new List<Channel>(listView.SelectedItems.Cast<Channel>());
        }
    }
}