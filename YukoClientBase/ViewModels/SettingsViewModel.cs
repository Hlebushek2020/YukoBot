using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using Prism.Commands;
using Sergey.UI.Extension.Themes;
using YukoClientBase.Interfaces;
using YukoClientBase.Models;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoClientBase.ViewModels
{
    public class SettingsViewModel : ICloseableView, IViewTitle
    {
        #region Propirties
        public string Title { get; }
        public Action Close { get; set; }
        public List<DisplayTheme> Themes { get; }
        public DisplayTheme SelectTheme { get; set; }
        public List<int> MaxDownloadThreads { get; }
        public int SelectMaxDownloadThreads { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        #endregion

        #region Commands
        public DelegateCommand CloseCommand { get; }
        public DelegateCommand SaveAndApplyCommand { get; }
        #endregion

        public SettingsViewModel(string title)
        {
            Title = title;
            // fields
            Themes = DisplayTheme.GetList();
            SelectTheme = new DisplayTheme(Settings.Current.Theme);
            MaxDownloadThreads = Settings.GetListAllowedNumberDownloadThreads();
            SelectMaxDownloadThreads = Settings.Current.MaxDownloadThreads;
            Host = Settings.Current.Host;
            Port = Settings.Current.Port.ToString();
            // commands
            CloseCommand = new DelegateCommand(() => Close());
            SaveAndApplyCommand = new DelegateCommand(() =>
            {
                if (string.IsNullOrEmpty(Host) || string.IsNullOrEmpty(Port))
                {
                    MessageBox.Show("Все поля должны быть заполнены!", Title, MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (!IPAddress.TryParse(Host, out _))
                {
                    MessageBox.Show("Некорректный адрес хоста!", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(Port, out int port))
                {
                    MessageBox.Show(
                        "Недопустимое значение в поле \"Порт\". Значение должно быть больше чем 1023 и меньше чем 65536.",
                        Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    if (port < 1024 || port > 65535)
                    {
                        MessageBox.Show(
                            "Недопустимое значение в поле \"Порт\". Значение должно быть больше чем 1023 и меньше чем 65536.",
                            Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                Settings.Current.Host = Host;
                Settings.Current.Port = port;
                Settings.Current.Theme = SelectTheme.Value;
                Settings.Current.MaxDownloadThreads = SelectMaxDownloadThreads;
                Settings.Current.Save();

                Close();
            });
        }
    }
}