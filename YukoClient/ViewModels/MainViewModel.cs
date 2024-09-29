using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using YukoClient.Models;
using YukoClient.Models.Progresses;
using YukoClientBase.Interfaces;
using YukoClientBase.Views;
using MessageBox = YukoClientBase.Dialogs.MessageBox;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using DialogResult = System.Windows.Forms.DialogResult;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace YukoClient.ViewModels
{
    public class MainViewModel : BindableBase, IFullscreenEvent
    {
        #region Fields
        private Server _selectedServer;
        private Script _selectedScript;
        private string _selectedUrl;
        #endregion

        #region Propirties
        public string Title => App.Name;
        public ImageBrush Avatar => Storage.Current.Avatar;
        public string Username => Storage.Current.Username;
        public string UserId => Storage.Current.UserId.ToString();
        public ObservableCollection<Server> Servers => Storage.Current.Servers;

        public Server SelectedServer
        {
            get => _selectedServer;
            set
            {
                _selectedServer = value;

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Scripts));
                RaisePropertyChanged(nameof(Urls));

                RemoveServerCommand.RaiseCanExecuteChanged();
                ServerSettingsCommand.RaiseCanExecuteChanged();

                AddScriptCommand.RaiseCanExecuteChanged();
                ClearScriptsCommand.RaiseCanExecuteChanged();
                ExportScriptsCommand.RaiseCanExecuteChanged();
                ImportScriptsCommand.RaiseCanExecuteChanged();
                RunScriptsCommand.RaiseCanExecuteChanged();

                ClearUrlsCommand.RaiseCanExecuteChanged();
                ExportUrlsCommand.RaiseCanExecuteChanged();
                ImportUrlsCommand.RaiseCanExecuteChanged();
                DownloadFilesCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<Script> Scripts => _selectedServer?.Scripts;

        public Script SelectedScript
        {
            get => _selectedScript;
            set
            {
                _selectedScript = value;

                RaisePropertyChanged();

                RemoveScriptCommand.RaiseCanExecuteChanged();
                ShowExecutionErrorsCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<string> Urls => _selectedServer?.Urls;

        public string SelectedUrl
        {
            get => _selectedUrl;
            set
            {
                _selectedUrl = value;

                RaisePropertyChanged();

                RemoveUrlCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Commands
        public DelegateCommand FullscreenCommand { get; }
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

        public event FullscreenEventHandler FullscreenEvent;

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public MainViewModel()
        {
            Storage.Current.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);

            FullscreenCommand = new DelegateCommand(() => FullscreenEvent?.Invoke());
            WindowLoadedCommand = new DelegateCommand(() =>
            {
                ProgressWindow progress = new ProgressWindow(Title, new StorageInitialization(), false);
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
                        new ProgressWindow(Title, new UpdateServers(messageResult == MessageBoxResult.Yes));
                    progress.ShowDialog();
                }
            });
            RemoveServerCommand = new DelegateCommand(
                () =>
                {
                    if (MessageBox.Show($"Удалить сервер {_selectedServer.Name} из списка?", App.Name,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Storage.Current.Servers.Remove(_selectedServer);
                    }
                },
                () => _selectedServer != null);
            ServerSettingsCommand = new DelegateCommand(
                () =>
                {
                    ServerSettingsWindow serverSettings = new ServerSettingsWindow(_selectedServer);
                    serverSettings.ShowDialog();
                    // save data
                    Task.Run(() => Storage.Current.Save());
                },
                () => _selectedServer != null);

            // Script Commands
            AddScriptCommand = new DelegateCommand(
                () =>
                {
                    AddScriptWindow addScript = new AddScriptWindow(_selectedServer);
                    addScript.ShowDialog();
                    RunScriptsCommand.RaiseCanExecuteChanged();
                    ClearScriptsCommand.RaiseCanExecuteChanged();
                },
                () => _selectedServer != null);
            RemoveScriptCommand = new DelegateCommand(
                () =>
                {
                    if (MessageBox.Show("Удалить выбранное правило?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedServer.Scripts.Remove(SelectedScript);
                        RunScriptsCommand.RaiseCanExecuteChanged();
                        ClearScriptsCommand.RaiseCanExecuteChanged();
                    }
                }, () => _selectedScript != null);
            ClearScriptsCommand = new DelegateCommand(
                () =>
                {
                    if (MessageBox.Show("Очистить список правил?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedServer.Scripts.Clear();
                        RunScriptsCommand.RaiseCanExecuteChanged();
                        ClearScriptsCommand.RaiseCanExecuteChanged();
                    }
                },
                () => _selectedServer != null);
            ExportScriptsCommand = new DelegateCommand(
                () =>
                {
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Yuko Script|*.yukoscript";
                        saveFileDialog.DefaultExt = "yukoscript";

                        if (saveFileDialog.ShowDialog() != DialogResult.OK)
                            return;

                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new ExportScripts(_selectedServer.Scripts, _selectedServer.Id, saveFileDialog.FileName));
                        progressWindow.ShowDialog();
                    }
                },
                () => _selectedServer != null);
            ImportScriptsCommand = new DelegateCommand(
                () =>
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filter = "Yuko Script|*.yukoscript";
                        openFileDialog.DefaultExt = "yukoscript";

                        if (openFileDialog.ShowDialog() != DialogResult.OK)
                            return;

                        if (_selectedServer.Scripts.Count > 0)
                        {
                            if (MessageBox.Show("Очистить список правил перед добавлением?", App.Name,
                                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                _selectedServer.Scripts.Clear();
                            }
                        }

                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new ImportScripts(_selectedServer.Scripts, _selectedServer.Id, openFileDialog.FileName));
                        progressWindow.ShowDialog();
                        RunScriptsCommand.RaiseCanExecuteChanged();
                        ClearScriptsCommand.RaiseCanExecuteChanged();
                    }
                },
                () => _selectedServer != null);
            RunScriptsCommand = new DelegateCommand(
                () =>
                {
                    ProgressWindow progress = new ProgressWindow(Title, new ExecuteScripts(_selectedServer));
                    progress.ShowDialog();

                    ClearUrlsCommand.RaiseCanExecuteChanged();
                    ExportUrlsCommand.RaiseCanExecuteChanged();
                    ImportUrlsCommand.RaiseCanExecuteChanged();
                    DownloadFilesCommand.RaiseCanExecuteChanged();

                    ShowExecutionErrorsCommand.RaiseCanExecuteChanged();
                },
                () => _selectedServer != null && _selectedServer.Scripts.Count > 0);
            ShowExecutionErrorsCommand = new DelegateCommand(
                () =>
                {
                    ExecutionErrorsWindow executionErrorsWindow = new ExecutionErrorsWindow(SelectedScript);
                    executionErrorsWindow.ShowDialog();
                }, () => _selectedScript != null && _selectedScript.CompletedWithErrors == true);

            // Url Command
            RemoveUrlCommand = new DelegateCommand(
                () =>
                {
                    if (MessageBox.Show($"Удалить \"{SelectedUrl}\" из списка?", App.Name,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedServer?.Urls.Remove(SelectedUrl);
                        DownloadFilesCommand.RaiseCanExecuteChanged();
                        ClearUrlsCommand.RaiseCanExecuteChanged();
                    }
                }, () => _selectedUrl != null);
            ClearUrlsCommand = new DelegateCommand(
                () =>
                {
                    if (MessageBox.Show("Очистить список сылок?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedServer.Urls.Clear();
                        DownloadFilesCommand.RaiseCanExecuteChanged();
                        ClearUrlsCommand.RaiseCanExecuteChanged();
                    }
                },
                () => _selectedServer != null);
            ExportUrlsCommand = new DelegateCommand(
                () =>
                {
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Текстовый докуент|*.txt";
                        saveFileDialog.DefaultExt = "txt";

                        if (saveFileDialog.ShowDialog() != DialogResult.OK)
                            return;

                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new ExportUrls(_selectedServer.Urls, saveFileDialog.FileName));
                        progressWindow.ShowDialog();
                    }
                },
                () => _selectedServer != null);
            ImportUrlsCommand = new DelegateCommand(
                () =>
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filter = "Текстовый докуент|*.txt";
                        openFileDialog.DefaultExt = "txt";

                        if (openFileDialog.ShowDialog() != DialogResult.OK)
                            return;

                        if (_selectedServer.Urls.Count > 0)
                        {
                            if (MessageBox.Show("Очистить список сылок перед добавлением?", App.Name,
                                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                _selectedServer.Urls.Clear();
                            }
                        }

                        ProgressWindow progressWindow =
                            new ProgressWindow(Title,
                                new ImportUrls(_selectedServer.Urls, openFileDialog.FileName));
                        progressWindow.ShowDialog();
                        DownloadFilesCommand.RaiseCanExecuteChanged();
                        ClearUrlsCommand.RaiseCanExecuteChanged();
                    }
                },
                () => _selectedServer != null);
            DownloadFilesCommand = new DelegateCommand(
                () =>
                {
                    using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                    {
                        folderBrowserDialog.ShowNewFolderButton = true;

                        if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                            return;

                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new Download(_selectedServer.Urls, folderBrowserDialog.SelectedPath), true);
                        progressWindow.ShowDialog();
                    }
                },
                () => _selectedServer != null && _selectedServer.Urls.Count > 0);
        }
    }
}