using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using YukoClient.Interfaces.ViewModel;
using YukoClient.Models;

namespace YukoClient.ViewModels
{
    public class SettingsViewModel : BindableBase, ICloseableView, ITitle
    {
        #region Propirties
        public string Title { get => App.Name; }
        public Action Close { get; set; }
        public ObservableCollection<string> Themes { get; }
        public string SelectTheme { get; set; }
        public ObservableCollection<int> MaxDownloadThreads { get; }
        public int SelectMaxDownloadThreads { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        #endregion

        #region Commands
        public DelegateCommand CloseCommand { get; }
        public DelegateCommand SaveAndApplyCommand { get; }
        #endregion

        public SettingsViewModel()
        {
            // fields
            Themes = new ObservableCollection<string> { "Light", "Dark" };
            SelectTheme = Settings.Current.Theme;
            MaxDownloadThreads = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            SelectMaxDownloadThreads = Settings.Current.MaxDownloadThreads;
            Host = Settings.Current.Host;
            Port = Settings.Current.Port.ToString();
            // commands
            CloseCommand = new DelegateCommand(() => Close());
            SaveAndApplyCommand = new DelegateCommand(() =>
            {
                if (string.IsNullOrEmpty(Host) || string.IsNullOrEmpty(Port))
                {
                    UIC.MessageBox.Show("Все поля должны быть заполнены!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!IPAddress.TryParse(Host, out _))
                {
                    UIC.MessageBox.Show("Некорректный адрес хоста!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(Port, out int port))
                {
                    UIC.MessageBox.Show("Недопустимое значение в поле \"Порт\". Значение должно быть больше чем 1023 и меньше чем 65536.", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    if (port < 1024 || port > 65535)
                    {
                        UIC.MessageBox.Show("Недопустимое значение в поле \"Порт\". Значение должно быть больше чем 1023 и меньше чем 65536.", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                Settings.Current.Host = Host;
                Settings.Current.Port = port;
                if (!Settings.Current.Theme.Equals(SelectTheme))
                {
                    Settings.Current.Theme = SelectTheme;
                    App.SetTheme();
                }
                Settings.Current.MaxDownloadThreads = SelectMaxDownloadThreads;
                Settings.Current.Save();

                Close();
            });
        }
    }
}
