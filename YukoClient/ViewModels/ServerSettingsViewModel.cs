using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows;
using YukoClient.Models;
using YukoClient.Models.Progresses;
using YukoClientBase.Views;
using MessageBox = YukoClientBase.Dialogs.MessageBox;

namespace YukoClient.ViewModels
{
    public class ServerSettingsViewModel : BindableBase
    {
        private List<Channel> _selectedChannels;
        private Channel _selectedChannel;

        #region Propirties
        public string Title => App.Name;
        public Server Server { get; }

        public List<Channel> SelectedChannels
        {
            get => _selectedChannels;
            set
            {
                _selectedChannels = value;
                RaisePropertyChanged();
                RemoveSelectedChannelsCommand.RaiseCanExecuteChanged();
            }
        }

        public Channel SelectedChannel
        {
            get => _selectedChannel;
            set
            {
                _selectedChannel = value;
                RaisePropertyChanged();
                RenameChannelCommand.RaiseCanExecuteChanged();
            }
        }
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

            // Commands
            RenameChannelCommand = new DelegateCommand(
                () =>
                {
                    RenameChannelWindow renameChannel = new RenameChannelWindow(SelectedChannel);
                    renameChannel.ShowDialog();
                },
                () => _selectedChannel != null);
            RemoveSelectedChannelsCommand = new DelegateCommand(
                () =>
                {
                    if (MessageBox.Show("Удалить выбранные каналы?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        foreach (Channel channel in SelectedChannels)
                            Server.Channels.Remove(channel);

                        ClearChannelListCommand.RaiseCanExecuteChanged();
                    }
                },
                () => _selectedChannels.Count != 0);
            ClearChannelListCommand = new DelegateCommand(
                () =>
                {
                    if (MessageBox.Show("Очистить список каналов?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Server.Channels.Clear();
                        ClearChannelListCommand.RaiseCanExecuteChanged();
                    }
                },
                () => Server.Channels.Count != 0);
            UpdateChannelListCommand = new DelegateCommand(() =>
            {
                if (MessageBox.Show(
                        "ВНИМАНИЕ! Все каналы будут удалены, вы действительно хотите продолжить?", App.Name,
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    OperationProgressWindow progress = new OperationProgressWindow(Title, new UpdateServer(server));
                    progress.ShowDialog();
                    ClearChannelListCommand.RaiseCanExecuteChanged();
                }
            });

            ClearChannelListCommand.RaiseCanExecuteChanged();
        }
    }
}