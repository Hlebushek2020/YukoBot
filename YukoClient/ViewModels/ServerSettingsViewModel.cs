using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows;
using YukoClient.Models;
using YukoClient.Models.Progress;
using YukoClientBase.Interfaces;

namespace YukoClient.ViewModels
{
    public class ServerSettingsViewModel : BindableBase, IViewTitle
    {
        #region Propirties
        public string Title { get => App.Name; }
        public Server Server { get; }
        public List<Channel> SelectedChannels { get; set; }
        public Channel SelectedChannel { get; set; }
        #endregion

        #region Commands
        public DelegateCommand RenameСhannelCommand { get; }
        public DelegateCommand RemoveSelectedChannelsCommand { get; }
        public DelegateCommand ClearChannelListCommand { get; }
        public DelegateCommand UpdateChannelListCommand { get; }
        #endregion

        public ServerSettingsViewModel(Server server)
        {
            Server = server;
            SelectedChannels = new List<Channel>();
            // commands
            RenameСhannelCommand = new DelegateCommand(() =>
            {
                if (SelectedChannel != null)
                {
                    RenameChannelWindow renameChannel = new RenameChannelWindow(SelectedChannel);
                    renameChannel.ShowDialog();
                }
            });
            RemoveSelectedChannelsCommand = new DelegateCommand(() =>
            {
                if (SelectedChannels.Count != 0)
                {
                    if (Models.Dialogs.MessageBox.Show("Удалить выбранные каналы?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        foreach (Channel channel in SelectedChannels)
                        {
                            Server.Channels.Remove(channel);
                        }
                    }
                }
            });
            ClearChannelListCommand = new DelegateCommand(() =>
            {
                if (Server.Channels.Count != 0)
                {
                    if (Models.Dialogs.MessageBox.Show("Очистить список каналов?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Server.Channels.Clear();
                    }
                }
            });
            UpdateChannelListCommand = new DelegateCommand(() =>
            {
                if (Models.Dialogs.MessageBox.Show("ВНИМАНИЕ! Все каналы будут удалены, вы действительно хотите продолжить?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    ProgressWindow progress = new ProgressWindow(new UpdateServer(server));
                    progress.ShowDialog();
                }
            });
        }
    }
}