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
        private readonly ServerSettingsViewModel _viewModel;

        public ServerSettingsWindow(Server server)
        {
            InitializeComponent();
            _viewModel = new ServerSettingsViewModel(server);
            DataContext = _viewModel;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            _viewModel.SelectedChannels = new List<Channel>(listView.SelectedItems.Cast<Channel>());
        }
    }
}