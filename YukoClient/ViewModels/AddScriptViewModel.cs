using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using YukoClient.Models;
using YukoClientBase.Interfaces;

namespace YukoClient.ViewModels
{
    public class AddScriptViewModel : BindableBase, ICloseableView, IViewTitle
    {
        #region Fields
        private Models.ScriptMode selectedMode;
        #endregion

        #region Propirties
        public string Title { get => App.Name; }
        public Action Close { get; set; }
        public Server Server { get; set; }
        public Channel SelectedChannel { get; set; }
        public ObservableCollection<Models.ScriptMode> Modes
        {
            get
            {
                return new ObservableCollection<Models.ScriptMode> {
                    new Models.ScriptMode { Mode = Enums.ScriptMode.One },
                    new Models.ScriptMode { Mode = Enums.ScriptMode.After },
                    new Models.ScriptMode { Mode = Enums.ScriptMode.Before },
                    new Models.ScriptMode { Mode = Enums.ScriptMode.End },
                    new Models.ScriptMode { Mode = Enums.ScriptMode.All }
                };
            }
        }
        public Models.ScriptMode SelectedMode
        {
            set
            {
                selectedMode = value;
                RaisePropertyChanged("ModeDescription");
                RaisePropertyChanged("IsEnabledMessageId");
                RaisePropertyChanged("IsEnabledMessageCount");
            }
        }
        public string ModeDescription { get { return selectedMode == null ? string.Empty : selectedMode.Description; } }
        public bool IsEnabledMessageId
        {
            get
            {
                if (selectedMode != null)
                {
                    if (selectedMode.Mode == Enums.ScriptMode.All || selectedMode.Mode == Enums.ScriptMode.End)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public string MessageId { get; set; }
        public bool IsEnabledMessageCount
        {
            get
            {
                if (selectedMode != null)
                {
                    if (selectedMode.Mode == Enums.ScriptMode.All || selectedMode.Mode == Enums.ScriptMode.One)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public string MessageCount { get; set; }
        #endregion

        #region Commands
        public DelegateCommand CloseCommand { get; }
        public DelegateCommand ApplyCommand { get; }
        #endregion

        public AddScriptViewModel()
        {
            CloseCommand = new DelegateCommand(() => Close());
            ApplyCommand = new DelegateCommand(() =>
            {
                ulong messageId = 0;
                if (IsEnabledMessageId)
                {
                    if (string.IsNullOrEmpty(MessageId))
                    {
                        Models.Dialogs.MessageBox.Show("Поле \"Cообщение\" не может быть пустым!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (!ulong.TryParse(MessageId, out messageId))
                    {
                        Models.Dialogs.MessageBox.Show("В поле \"Сообщение\" введено некорректное значение! Поле должно содержать Id сообщения.", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                int messageCount = int.MaxValue;
                if (IsEnabledMessageCount)
                {
                    if (string.IsNullOrEmpty(MessageCount))
                    {
                        Models.Dialogs.MessageBox.Show("Поле \"Количество\" не может быть пустым!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (!int.TryParse(MessageCount, out messageCount))
                    {
                        Models.Dialogs.MessageBox.Show("В поле \"Количество\" введено некорректное значение! Поле должно содержать целое число от 0 до " + int.MaxValue + " включительно.", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                Server.Scripts.Add(new Script
                {
                    Channel = SelectedChannel,
                    Mode = selectedMode,
                    MessageId = messageId,
                    Count = messageCount
                });
                Close();
            });
        }
    }
}