using System.Collections.Generic;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using YukoClient.Models;
using YukoClient.Models.Progress;
using YukoClientBase.Views;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoClient.ViewModels
{
    public class ServerSettingsViewModel : BindableBase, IViewTitle
    {
        #region Propirties
        public string Title
        {
            get => App.Name;
        }

        public Server Server { get; }
        public List<Channel> SelectedChannels { get; set; }
        public Channel SelectedChannel { get; set; }
        #endregion

        #region Commands
        public DelegateCommand RenameChannelCommand { get; }
        public DelegateCommand RemoveSelectedChannelsCommand { get; }
        public DelegateCommand ClearChannelListCommand { get; }
        public DelegateCommand UpdateChannelListCommand { get; }
        #endregion

        public ServerSettingsViewModel(Server server)
        {
            Server = server;
            SelectedChannels = new List<Channel>();
            // commands
            RenameChannelCommand = new DelegateCommand(() =>
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
                    if (MessageBox.Show("Удалить выбранные каналы?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
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
                    if (SUI.Dialogs.MessageBox.Show("Очистить список каналов?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Server.Channels.Clear();
                    }
                }
            });
            UpdateChannelListCommand = new DelegateCommand(() =>
            {
                if (SUI.Dialogs.MessageBox.Show(
                        "ВНИМАНИЕ! Все каналы будут удалены, вы действительно хотите продолжить?", App.Name,
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    ProgressWindow progress = new ProgressWindow(Title, new UpdateServer(server));
                    progress.ShowDialog();
                }
            });
        }
    }
}