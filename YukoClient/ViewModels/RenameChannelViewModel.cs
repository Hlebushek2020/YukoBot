using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using YukoClient.Models;
using YukoClientBase.Interfaces;
using SUI = Sergey.UI.Extension;

namespace YukoClient.ViewModels
{
    public class RenameChannelViewModel : BindableBase, ICloseableView, IViewTitle
    {
        #region Fields
        private string newChannelName;
        private Channel channel;
        #endregion

        #region Propirties
        public string Title { get => App.Name; }
        public Action Close { get; set; }
        public Channel Channel
        {
            get => channel;
            set
            {
                channel = value;
                RaisePropertyChanged();
                NewChannelName = channel.Name;
            }
        }
        public string NewChannelName
        {
            get => newChannelName;
            set
            {
                newChannelName = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Commands
        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand CloseCommand { get; }
        #endregion

        public RenameChannelViewModel(Channel channel)
        {
            Channel = channel;
            // Commands
            ApplyCommand = new DelegateCommand(() =>
            {
                if (string.IsNullOrEmpty(newChannelName))
                {
                    SUI.Dialogs.MessageBox.Show("Название канала не может быть пустым!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    if (!channel.Name.Equals(newChannelName))
                    {
                        channel.Name = newChannelName;
                    }
                    Close();
                }
            });
            CloseCommand = new DelegateCommand(() => Close());
        }
    }
}