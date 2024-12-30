using System.Windows;
using YukoClient.Models;
using YukoClient.ViewModels;

namespace YukoClient
{
    /// <summary>
    /// Interaction logic for RenameChannelWindow.xaml
    /// </summary>
    public partial class RenameChannelWindow : Window
    {
        public RenameChannelWindow(Channel channel)
        {
            InitializeComponent();
            DataContext = new RenameChannelViewModel(Close, channel);
        }
    }
}