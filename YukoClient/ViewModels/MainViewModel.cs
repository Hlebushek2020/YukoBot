using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Prism.Commands;
using Prism.Mvvm;
using YukoClient.Models;
using YukoClient.Models.Progress;
using YukoClientBase.Interfaces;
using YukoClientBase.Views;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;
using WinForm = System.Windows.Forms;

namespace YukoClient.ViewModels
{
    public class MainViewModel : BindableBase, ICloseableView, IViewTitle
    {
        private Server _selectedServer;

        #region Propirties
        public string Title => App.Name;
        public Action Close { get; set; }
        public ImageBrush Avatar => Storage.Current.Avatar;
        public string Username => Storage.Current.Username;
        public string UserId => Storage.Current.UserId.ToString();
        public ObservableCollection<Server> Servers => Storage.Current.Servers;

        public Server SelectedServer
        {
            get { return _selectedServer; }
            set
            {
                _selectedServer = value;
                if (_selectedServer != null)
                {
                    RaisePropertyChanged(nameof(Scripts));
                    RaisePropertyChanged(nameof(Urls));
                }
            }
        }

        public ObservableCollection<Script> Scripts => _selectedServer?.Scripts;
        public Script SelectedScript { get; set; }
        public ObservableCollection<string> Urls => _selectedServer?.Urls;
        public string SelectedUrl { get; set; }
        #endregion

        #region Commands
        public DelegateCommand WindowLoadedCommand { get; }

        // User Commands
        public DelegateCommand AppSettingsCommand { get; }

        // Server Commands
        public DelegateCommand ServerSettingsCommand { get; }
        public DelegateCommand RemoveServerCommand { get; }
        public DelegateCommand UpdateServerCollectionCommand { get; }

        // Script Commands
        public DelegateCommand AddScriptCommand { get; }
        public DelegateCommand RemoveScriptCommand { get; }
        public DelegateCommand ClearScriptsCommand { get; }
        public DelegateCommand ExportScriptsCommand { get; }
        public DelegateCommand ImportScriptsCommand { get; }
        public DelegateCommand RunScriptsCommand { get; }
        public DelegateCommand ShowExecutionErrorsCommand { get; }

        // Url Command
        public DelegateCommand RemoveUrlCommand { get; }
        public DelegateCommand ClearUrlsCommand { get; }
        public DelegateCommand ExportUrlsCommand { get; }
        public DelegateCommand ImportUrlsCommand { get; }
        public DelegateCommand DownloadFilesCommand { get; }
        #endregion

        public MainViewModel()
        {
            Storage.Current.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            WindowLoadedCommand = new DelegateCommand(() =>
            {
                // TODO: Update servers (oerride)
                ProgressWindow progress = new ProgressWindow(new StorageInitialization());
                progress.ShowDialog();
            });
            // User Commands
            AppSettingsCommand = new DelegateCommand(() =>
            {
                SettingsWindow settingsWindow = new SettingsWindow(App.Name);
                settingsWindow.ShowDialog();
            });
            // Server Commands
            UpdateServerCollectionCommand = new DelegateCommand(() =>
            {
                MessageBoxResult messageResult = MessageBox.Show(
                    "Перезаписать данные текущих серверов? Внимание! Это приведет к потере списка правил и ссылок.",
                    App.Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (messageResult != MessageBoxResult.Cancel)
                {
                    ProgressWindow progress =
                        new ProgressWindow(new UpdateServers(messageResult == MessageBoxResult.Yes));
                    progress.ShowDialog();
                }
            });
            RemoveServerCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null)
                {
                    if (MessageBox.Show($"Удалить сервер {_selectedServer.Name} из списка?", App.Name,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Storage.Current.Servers.Remove(_selectedServer);
                    }
                }
            });
            ServerSettingsCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null)
                {
                    ServerSettingsWindow serverSettings = new ServerSettingsWindow(_selectedServer);
                    serverSettings.ShowDialog();
                    // save data
                    Task.Run(() => Storage.Current.Save());
                }
            });
            // Script Commands
            AddScriptCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null)
                {
                    AddScriptWindow addScript = new AddScriptWindow(_selectedServer);
                    addScript.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Выберите сервер!", App.Name, MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            });
            RemoveScriptCommand = new DelegateCommand(() =>
            {
                if (SelectedScript != null)
                {
                    if (MessageBox.Show("Удалить выбранное правило?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedServer?.Scripts.Remove(SelectedScript);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите сервер!", App.Name, MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            });
            ClearScriptsCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null && _selectedServer.Scripts.Count > 0)
                {
                    if (MessageBox.Show("Очистить список правил?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedServer.Scripts.Clear();
                    }
                }
            });
            ExportScriptsCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null && _selectedServer.Scripts.Count > 0)
                {
                    using (WinForm.SaveFileDialog saveFileDialog = new WinForm.SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Yuko Script|*.yukoscript";
                        saveFileDialog.DefaultExt = "yukoscript";
                        if (saveFileDialog.ShowDialog() == WinForm.DialogResult.OK)
                        {
                            ProgressWindow progressWindow = new ProgressWindow(new ExportScripts(
                                _selectedServer.Scripts,
                                _selectedServer.Id, saveFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            ImportScriptsCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null)
                {
                    using (WinForm.OpenFileDialog openFileDialog = new WinForm.OpenFileDialog())
                    {
                        openFileDialog.Filter = "Yuko Script|*.yukoscript";
                        openFileDialog.DefaultExt = "yukoscript";
                        if (openFileDialog.ShowDialog() == WinForm.DialogResult.OK)
                        {
                            if (_selectedServer.Scripts.Count > 0)
                            {
                                if (MessageBox.Show("Очистить список правил перед добавлением?", App.Name,
                                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    _selectedServer.Scripts.Clear();
                                }
                            }
                            ProgressWindow progressWindow = new ProgressWindow(new ImportScripts(
                                _selectedServer.Scripts,
                                _selectedServer.Id, openFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            RunScriptsCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null && _selectedServer.Scripts.Count > 0)
                {
                    ProgressWindow progress = new ProgressWindow(new ExecuteScripts(_selectedServer));
                    progress.ShowDialog();
                }
            });
            ShowExecutionErrorsCommand = new DelegateCommand(() =>
            {
                if (SelectedScript != null)
                {
                    ExecutionErrorsWindow executionErrorsWindow = new ExecutionErrorsWindow(SelectedScript);
                    executionErrorsWindow.ShowDialog();
                }
            });
            // Url Command
            RemoveUrlCommand = new DelegateCommand(() =>
            {
                if (SelectedUrl != null)
                {
                    if (MessageBox.Show($"Удалить \"{SelectedUrl}\" из списка?", App.Name,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedServer?.Urls.Remove(SelectedUrl);
                    }
                }
            });
            ClearUrlsCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null && _selectedServer.Urls.Count > 0)
                {
                    if (MessageBox.Show("Очистить список сылок?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedServer.Urls.Clear();
                    }
                }
            });
            ExportUrlsCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null && _selectedServer.Urls.Count > 0)
                {
                    using (WinForm.SaveFileDialog saveFileDialog = new WinForm.SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Текстовый докуент|*.txt";
                        saveFileDialog.DefaultExt = "txt";
                        if (saveFileDialog.ShowDialog() == WinForm.DialogResult.OK)
                        {
                            ProgressWindow progressWindow =
                                new ProgressWindow(new ExportUrls(_selectedServer.Urls, saveFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            ImportUrlsCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null)
                {
                    using (WinForm.OpenFileDialog openFileDialog = new WinForm.OpenFileDialog())
                    {
                        openFileDialog.Filter = "Текстовый докуент|*.txt";
                        openFileDialog.DefaultExt = "txt";
                        if (openFileDialog.ShowDialog() == WinForm.DialogResult.OK)
                        {
                            if (_selectedServer.Urls.Count > 0)
                            {
                                if (MessageBox.Show("Очистить список сылок перед добавлением?", App.Name,
                                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    _selectedServer.Urls.Clear();
                                }
                            }
                            ProgressWindow progressWindow =
                                new ProgressWindow(new ImportUrls(_selectedServer.Urls, openFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            DownloadFilesCommand = new DelegateCommand(() =>
            {
                if (_selectedServer != null && _selectedServer.Urls.Count > 0)
                {
                    System.Windows.Forms.FolderBrowserDialog folderBrowserDialog =
                        new System.Windows.Forms.FolderBrowserDialog { ShowNewFolderButton = true };
                    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ProgressWindow progressWindow = new ProgressWindow(
                            new Download(_selectedServer.Urls, folderBrowserDialog.SelectedPath), true);
                        progressWindow.ShowDialog();
                    }
                }
            });
        }
    }
}