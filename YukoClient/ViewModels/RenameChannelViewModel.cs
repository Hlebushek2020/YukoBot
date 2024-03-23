using System;
using Prism.Commands;
using Prism.Mvvm;
using YukoClient.Models;

namespace YukoClient.ViewModels
{
    public class RenameChannelViewModel : BindableBase
    {
        private string _newChannelName;

        #region Propirties
        public string Title => App.Name;

        public string NewChannelName
        {
            get => _newChannelName;
            set
            {
                _newChannelName = value;
                RaisePropertyChanged();
                ApplyCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Commands
        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand CloseCommand { get; }
        #endregion

        public RenameChannelViewModel(Action closeAction, Channel channel)
        {
            ApplyCommand = new DelegateCommand(() =>
                {
                    if (!channel.Name.Equals(_newChannelName))
                        channel.Name = _newChannelName;

                    closeAction.Invoke();
                },
                () => !string.IsNullOrEmpty(_newChannelName));
            CloseCommand = new DelegateCommand(closeAction.Invoke);
        }
    }
}