using System;
using System.Collections.ObjectModel;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using YukoClient.Models;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoClient.ViewModels
{
    public class AddScriptViewModel : BindableBase
    {
        private ScriptMode _selectedMode;

        #region Propirties
        public string Title => App.Name;
        public Server Server { get; set; }
        public Channel SelectedChannel { get; set; }

        public ObservableCollection<ScriptMode> Modes =>
            new ObservableCollection<ScriptMode>
            {
                new ScriptMode(Enums.ScriptMode.One),
                new ScriptMode(Enums.ScriptMode.After),
                new ScriptMode(Enums.ScriptMode.Before),
                new ScriptMode(Enums.ScriptMode.End),
                new ScriptMode(Enums.ScriptMode.All)
            };

        public ScriptMode SelectedMode
        {
            get => _selectedMode;
            set
            {
                _selectedMode = value;

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ModeDescription));
                RaisePropertyChanged(nameof(IsEnabledMessageId));
                RaisePropertyChanged(nameof(IsEnabledMessageCount));
            }
        }

        public string ModeDescription => _selectedMode == null ? string.Empty : _selectedMode.Description;

        public bool IsEnabledMessageId
        {
            get
            {
                if (_selectedMode == null)
                    return true;

                return _selectedMode.Mode != Enums.ScriptMode.All && _selectedMode.Mode != Enums.ScriptMode.End;
            }
        }

        public string MessageId { get; set; }

        public bool IsEnabledMessageCount
        {
            get
            {
                if (_selectedMode == null)
                    return true;

                return _selectedMode.Mode != Enums.ScriptMode.All && _selectedMode.Mode != Enums.ScriptMode.One;
            }
        }

        public string MessageCount { get; set; }
        #endregion

        #region Commands
        public DelegateCommand CloseCommand { get; }
        public DelegateCommand ApplyCommand { get; }
        #endregion

        public AddScriptViewModel(Action closeAction)
        {
            CloseCommand = new DelegateCommand(closeAction.Invoke);
            ApplyCommand = new DelegateCommand(() =>
            {
                ulong messageId = 0;
                if (IsEnabledMessageId)
                {
                    if (string.IsNullOrEmpty(MessageId))
                    {
                        MessageBox.Show("Поле \"Cообщение\" не может быть пустым!", App.Name, MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    if (!ulong.TryParse(MessageId, out messageId))
                    {
                        MessageBox.Show(
                            "В поле \"Сообщение\" введено некорректное значение! Поле должно содержать Id сообщения.",
                            App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                int messageCount = int.MaxValue;
                if (IsEnabledMessageCount)
                {
                    if (string.IsNullOrEmpty(MessageCount))
                    {
                        MessageBox.Show("Поле \"Количество\" не может быть пустым!", App.Name, MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    if (!int.TryParse(MessageCount, out messageCount))
                    {
                        MessageBox.Show(
                            "В поле \"Количество\" введено некорректное значение! Поле должно содержать целое число от 0 до " +
                            int.MaxValue + " включительно.", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                Server.Scripts.Add(new Script
                {
                    Channel = SelectedChannel,
                    Mode = _selectedMode,
                    MessageId = messageId,
                    Count = messageCount
                });

                closeAction.Invoke();
            });
        }
    }
}